using FoodService.Models;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;

namespace FoodService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "MANAGER","BUYER" })]
        public IQueryable<Food> ViewFoods([Service] FoodDeliveryAppContext context) =>
            context.Foods;

        //[Authorize]
        //public IQueryable<Food> ViewFoods([Service] FoodDeliveryAppContext context) =>
        //    context.Foods;

        //[Authorize]
        //public async Task<Food> ViewFoodByIdAsync(
        //int id,
        //[Service] FoodDeliveryAppContext context)
        //{
        //    var food = context.Foods.Where(o => o.Id == id).FirstOrDefault();

        //    return await Task.FromResult(food);
        //}
    }
}
