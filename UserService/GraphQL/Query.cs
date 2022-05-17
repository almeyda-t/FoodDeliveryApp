
using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.AspNetCore.Authorization;

using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using UserService.Models;

namespace UserService.QraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "ADMIN" })] // dapat diakses kalau sudah login
        public IQueryable<UserData> GetUsers([Service] FoodDeliveryAppContext context) =>
            context.Users.Select(p => new UserData()
            {
                Id = p.Id,
                FullName = p.FullName,
                Email = p.Email,
                Username = p.Username
            });

        //[Authorize]
        //public IQueryable<Profile> GetProfilesbyToken([Service] FoodDeliveryAppContext context, ClaimsPrincipal claimsPrincipal)
        //{
        //    var userName = claimsPrincipal.Identity.Name;
        //    var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
        //    if (user != null)
        //    {
        //        var profiles = context.Profiles.Where(o => o.UserId == user.Id);
        //        return profiles.AsQueryable();
        //    }
        //    return new List<Profile>().AsQueryable();
        //}

        [Authorize]
        public IQueryable<Profile> GetProfiles([Service] FoodDeliveryAppContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;

            // check admin role ?
            var adminRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role && o.Value == "ADMIN").FirstOrDefault();
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user != null)
            {
                if (adminRole != null)
                {
                    return context.Profiles;
                }
                var profiles = context.Profiles.Where(o => o.UserId == user.Id);
                return profiles.AsQueryable();
            }
            return new List<Profile>().AsQueryable();
        }

    }
}
