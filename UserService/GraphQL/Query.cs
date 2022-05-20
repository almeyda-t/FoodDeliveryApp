
using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.AspNetCore.Authorization;

using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using UserService.Models;
using OrderService.GraphQL;

namespace UserService.QraphQL
{
    public class Query
    {
        //USER
        [Authorize(Roles = new[] { "ADMIN" })] // dapat diakses kalau sudah login
        public IQueryable<UserData> GetUsers([Service] FoodDeliveryAppContext context) =>
            context.Users.Select(p => new UserData()
            {
                Id = p.Id,
                FullName = p.FullName,
                Email = p.Email,
                Username = p.Username

            });

        //PROFILE
        //get others' profiles by token 
        [Authorize]
        public IQueryable<Profile> GetProfilesbyToken([Service] FoodDeliveryAppContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user != null)
            {
                var profiles = context.Profiles.Where(o => o.UserId == user.Id);
                return profiles.AsQueryable();
            }
            return new List<Profile>().AsQueryable();
        }

        //get couriers
        [Authorize(Roles = new[] { "MANAGER" })]
        public IQueryable<Courier> GetCourierbyProfiles([Service] FoodDeliveryAppContext context) =>
            context.Couriers.Select(p => new Courier()
            {
                Id = p.Id,
                CourierName = p.CourierName,
                UserId = p.UserId,
                
            });
        public IQueryable<User> GetCouriers([Service] FoodDeliveryAppContext context)
        {
            var roleCourier = context.Roles.Where(k => k.Name == "COURIER").FirstOrDefault();
            var couriers = context.Users.Where(k => k.UserRoles.Any(o => o.RoleId == roleCourier.Id));
            return couriers.AsQueryable();
        }

    }
}
