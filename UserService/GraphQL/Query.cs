
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

    }
}
