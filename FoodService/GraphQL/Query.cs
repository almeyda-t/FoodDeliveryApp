using FoodService.Models;
using HotChocolate.AspNetCore.Authorization;

namespace FoodService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "BUYER" })]
        public IQueryable<Food> ViewFoods([Service] FoodDeliveryAppContext context) =>
            context.Foods;

        [Authorize(Roles = new[] { "BUYER" })]
        public async Task<Food> ViewFoodByIdAsync(
        int id,
        [Service] FoodDeliveryAppContext context)
        {
            var food = context.Foods.Where(o => o.Id == id).FirstOrDefault();

            return await Task.FromResult(food);
        }
    }
}
