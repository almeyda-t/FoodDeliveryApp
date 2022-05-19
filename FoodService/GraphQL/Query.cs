using FoodService.Models;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;

namespace FoodService.GraphQL
{
    public class Query
    {
        //FOOD
        [Authorize(Roles = new[] { "MANAGER","BUYER" })]
        public IQueryable<Food> ViewFoods([Service] FoodDeliveryAppContext context) =>
            context.Foods;

    }
}
