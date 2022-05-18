using HotChocolate.AspNetCore.Authorization;
using OrderService.Models;

namespace OrderService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "MANAGER", "BUYER" })]
        public IQueryable<Order> ViewOrders([Service] FoodDeliveryAppContext context) =>
            context.Orders;
    }
}
