using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderService.GraphQL;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Models;
using UserService.QraphQL;

namespace UserService.GraphQL
{
    public class Mutation
    {
        //USER
        //register
        public async Task<UserData> RegisterUserAsync(
        RegisterUser input,
        [Service] FoodDeliveryAppContext context)
        {
            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                Username = input.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password), // encrypt password
                Role = input.Role,
                Verification = false
            };
            var memberRole = context.Roles.Where(m => m.Name == input.Role).FirstOrDefault();
            if (memberRole == null)
                throw new Exception("Invalid Role");
            var userRole = new UserRole
            {
                RoleId = memberRole.Id,
                UserId = newUser.Id
            };
            newUser.UserRoles.Add(userRole);
            // EF
            var ret = context.Users.Add(newUser);
            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                FullName = newUser.FullName
            });
        }

        //login
        public async Task<UserToken> LoginAsync(
            LoginUser input,
            [Service] IOptions<TokenSettings> tokenSettings, 
            [Service] FoodDeliveryAppContext context) 
        {
            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }
            else if (!user.Verification)
            {
                return await Task.FromResult(new UserToken(null, null, "Account not verified"));
            }

            bool valid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);
            if (valid)
            {
                // generate jwt token
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                // jwt payload
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.Username));

                var userRoles = context.UserRoles.Where(o => o.UserId == user.Id).ToList(); //diganti userid
                foreach (var userRole in userRoles)
                {
                    var role = context.Roles.Where(o => o.Id == userRole.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var expired = DateTime.Now.AddHours(15);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    expires: expired,
                    claims: claims, // jwt payload
                    signingCredentials: credentials // signature
                );

                return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expired.ToString(), null));
            }
            return await Task.FromResult(new UserToken(null, null, Message: "Username or password was invalid"));
        }

        //verification
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<string> VerifAsync(
            string username,
            [Service] FoodDeliveryAppContext context)
        {
            var user = context.Users.FirstOrDefault(o => o.Username == username);
            if (user == null)
            {
                return "User not found";
            }
            using var transaction = context.Database.BeginTransaction(); //update dua table
            try
            {
                user.Verification = true;
                context.Users.Update(user);
                if(user.Role == "COURIER")
                {
                    Courier courier = new Courier
                    {
                        UserId = user.Id,
                        CourierName = user.FullName
                    };
                    context.Couriers.Add(courier);
                }
                context.SaveChanges();
                await transaction.CommitAsync();
            }
            catch(Exception ex)
            {
                transaction.Rollback();
            }
            return "Verification success";
        }

        //update users and couriers
        [Authorize(Roles = new[] { "ADMIN","MANAGER" })]
        public async Task<UpdateUser> UpdateUserAsync(
            UserData input, ClaimsPrincipal claimsPrincipal,
            [Service] FoodDeliveryAppContext context)
        {
            var username = claimsPrincipal.Identity.Name;
            var userUpdate = context.Users.Where(o => o.Username == username).FirstOrDefault();
            var user = context.Users.Where(o => o.Id == input.Id).FirstOrDefault();
            if (user != null)
            {
                if((userUpdate.Role == "ADMIN" && user.Role != "COURIER") || (userUpdate.Role == "MANAGER" && user.Role == "COURIER"))
                {
                    user.FullName = input.FullName;
                    user.Email = input.Email;
                    user.Username = input.Username;

                    context.Users.Update(user);
                    await context.SaveChangesAsync();

                    return new UpdateUser
                    {
                        Message = "Success"
                    };
                }
            }
            return new UpdateUser
            {
                Message = "Failed"
            };
        }

        //delete users by admin
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<User> DeleteUserByIdAsync(
            int id,
            [Service] FoodDeliveryAppContext context)
        {
            var userRole = context.UserRoles.FirstOrDefault(o => o.UserId == id);
            var user = context.Users.Where(o => o.Id == id).FirstOrDefault();
            if (user != null)
            {
                context.UserRoles.Remove(userRole);
                context.Users.Remove(user);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(user);
        }

        //delete courier by manager
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<User> DeleteCourierByIdAsync(
            int id,
            [Service] FoodDeliveryAppContext context)
        {
            var user = context.Users.Where(o => o.Id == id && o.Role == "COURIER").Include(o => o.UserRoles).FirstOrDefault();
            var courier = context.Couriers.Where(o => o.UserId == id).FirstOrDefault();
            var userRole = context.UserRoles.FirstOrDefault(o => o.UserId == id);
            if (user != null)
            {
                context.Couriers.Remove(courier);
                context.UserRoles.Remove(userRole);
                context.Users.Remove(user);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(user);
        }

        //change pass by token
        [Authorize]
        public async Task<User> ChangePasswordByTokenAsync(
            ChangePassword input, ClaimsPrincipal claimsPrincipal,
            [Service] FoodDeliveryAppContext context)
        {
            var username = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == username).FirstOrDefault();
            bool valid = BCrypt.Net.BCrypt.Verify(input.OldPassword, user.Password);
            if (user != null && valid)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(input.NewPassword);

                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(user);
        }

        //PROFILE
        [Authorize]
        public async Task<Profile> AddProfileAsync(
            ProfilesInput input,
            [Service] FoodDeliveryAppContext context)
        {
            var profile = new Profile
            {
                UserId = input.UserId,
                Name = input.Name,
                Address = input.Address,
                City = input.City,
                Phone = input.Phone
            };

            var ret = context.Profiles.Add(profile);
            await context.SaveChangesAsync();
            return ret.Entity;
        }

        //COURIER
        ////add couriers
        //[Authorize(Roles = new[] { "MANAGER" })]
        //public async Task<Courier> AddCourierAsync(
        //    RegisterCourier input,
        //    [Service] FoodDeliveryAppContext context)
        //{

        //    var courier = new Courier
        //    {
        //        CourierName = input.CourierName,
        //        PhoneNumber = input.PhoneNumber
        //    };

        //    var ret = context.Couriers.Add(courier);
        //    await context.SaveChangesAsync();
        //    return ret.Entity;
        //}

        ////update couriers
        //[Authorize(Roles = new[] { "MANAGER" })]
        //public async Task<Courier> UpdateCourierAsync(
        //    RegisterCourier input,
        //    [Service] FoodDeliveryAppContext context)
        //{
        //    var courier = context.Couriers.Where(o => o.Id == input.Id).FirstOrDefault();
        //    if (courier != null)
        //    {
        //        courier.CourierName = input.CourierName;
        //        courier.PhoneNumber = input.PhoneNumber;

        //        context.Couriers.Update(courier);
        //        await context.SaveChangesAsync();
        //    }
        //    return await Task.FromResult(courier);
        //}

        ////delete couriers
        //[Authorize(Roles = new[] { "MANAGER" })]
        //public async Task<Courier> DeleteCourierByIdAsync(
        //   int id,
        //   [Service] FoodDeliveryAppContext context)
        //{
        //    var courier = context.Couriers.Where(o => o.Id == id).FirstOrDefault();
        //    if (courier != null)
        //    {
        //        context.Couriers.Remove(courier);
        //        await context.SaveChangesAsync();
        //    }
        //    return await Task.FromResult(courier);
        //}

        //[Authorize(Roles = new[] { "MANAGER" })]
        //public async Task<UserData> AddCourierAsync(
        //    RegisterUser input,
        //    [Service] FoodDeliveryAppContext context)
        //{
        //    var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
        //    if (user != null)
        //    {
        //        return await Task.FromResult(new UserData());
        //    }
        //    var newUser = new User
        //    {
        //        FullName = input.FullName,
        //        Email = input.Email,
        //        Username = input.Username,
        //        Password = BCrypt.Net.BCrypt.HashPassword(input.Password) 
        //    };
        //    var memberRole = context.Roles.Where(m => m.Name == "COURIER").FirstOrDefault();
        //    if (memberRole == null)
        //        throw new Exception("Invalid Role");
        //    var userRole = new UserRole
        //    {
        //        RoleId = memberRole.Id,
        //        UserId = newUser.Id
        //    };
        //    newUser.UserRoles.Add(userRole);
        //    // EF
        //    var ret = context.Users.Add(newUser);
        //    await context.SaveChangesAsync();

        //    return await Task.FromResult(new UserData
        //    {
        //        Id = newUser.Id,
        //        Username = newUser.Username,
        //        Email = newUser.Email,
        //        FullName = newUser.FullName
        //    });
        //}

        //[Authorize(Roles = new[] { "MANAGER" })]
        //public async Task<Courier> AddCourierProfileAsync(
        //    RegisterCourier input,
        //    [Service] FoodDeliveryAppContext context)
        //{
        //    // EF
        //    var courier = new Courier
        //    {
        //        CourierName = input.CourierName,
        //        UserId = input.UserId
        //    };

        //    var ret = context.Couriers.Add(courier);
        //    await context.SaveChangesAsync();

        //    return ret.Entity;
        //}
        //public async Task<Courier> UpdateCourierProfileAsync(
        //    RegisterCourier input,
        //    [Service] FoodDeliveryAppContext context)
        //{
        //    var courier = context.Couriers.Where(o => o.Id == input.Id).FirstOrDefault();
        //    if (courier != null)
        //    {
        //        courier.CourierName = input.CourierName;

        //        context.Couriers.Update(courier);
        //        await context.SaveChangesAsync();
        //    }

        //    return await Task.FromResult(courier);
        //}

        

        ////Delete CourierProfile
        //[Authorize(Roles = new[] { "MANAGER" })]
        //public async Task<Courier> DeleteCourierProfileAsync(
        //    int id,
        //    [Service] FoodDeliveryAppContext context)
        //{
        //    var courier = context.Couriers.Where(o => o.Id == id).FirstOrDefault();
        //    if (courier != null)
        //    {
        //        context.Couriers.Remove(courier);
        //        await context.SaveChangesAsync();
        //    }

        //    return await Task.FromResult(courier);
        //}

    }
}
