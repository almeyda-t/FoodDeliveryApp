using GeoCoordinatePortable;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OrderService.Models;
using System.Security.Claims;

namespace OrderService.GraphQL
{
    public class Mutation
    {
        //ORDER
        //add order
        [Authorize(Roles = new[] { "BUYER" })]
        public async Task<OrderOutput> AddOrderAsync(
            OrderData input,
            ClaimsPrincipal claimsPrincipal,
            [Service] FoodDeliveryAppContext context)
        {
            using var transaction = context.Database.BeginTransaction();
            var userName = claimsPrincipal.Identity.Name;

            try
            {
                var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
                var courier = context.Couriers.Where(o => o.Id == input.CourierId).FirstOrDefault();

                if (user != null && courier != null)
                {
                    // EF
                    var order = new Order
                    {
                        Code = Guid.NewGuid().ToString(), // generate random chars using GUID
                        UserId = user.Id,
                        CourierId = input.CourierId,
                        Status = "Restaurant is preparing the foods"
                    };

                    foreach (var item in input.Details)
                    {
                        var detail = new OrderDetail
                        {
                            OrderId = order.Id,
                            FoodId = item.FoodId,
                            Quantity = item.Quantity
                        };
                        order.OrderDetails.Add(detail);
                    }
                    context.Orders.Add(order);
                    context.SaveChanges();
                    await transaction.CommitAsync();
                    return new OrderOutput
                    {
                        Message = "Order is received",
                        TransactionDate = DateTime.Now.ToString()
                    };
                }
                else
                {
                    return new OrderOutput
                    {
                        Message = "Order is rejected",
                        TransactionDate = DateTime.Now.ToString()
                    };
                }
            }
            catch (Exception err)
            {
                transaction.Rollback();
            }
            return new OrderOutput
            {
                Message = "Order is rejected",
                TransactionDate = DateTime.Now.ToString()
            };
        }

        //update order
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<string> UpdateOrderAsync(
            UpdateOrder input,
            [Service] FoodDeliveryAppContext context)
        {
            var order = context.Orders.Where(o => o.Id == input.OrderId).FirstOrDefault();
            var courier = context.Couriers.Where(o => o.Id == input.CourierId).FirstOrDefault();
            
            if (order == null) return "Order not found";
            else if(courier == null) return "Courier not found";
                order.CourierId = input.CourierId;

                context.Orders.Update(order);
                context.SaveChanges(); 

            return "Order is updated";
        }

        //delete
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Order> DeleteOrderByIdAsync(
            int id,
            [Service] FoodDeliveryAppContext context)
        {
            var order = context.Orders.Where(o => o.Id == id).FirstOrDefault();
            var orderDetail = context.OrderDetails.Where(o => o.OrderId == order.Id).ToList();
            if (order != null)
            {
                context.OrderDetails.RemoveRange(orderDetail);
                context.Orders.Remove(order);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(order);
        }

        //tracking
        [Authorize(Roles = new[] { "COURIER" })]
        public async Task<OrderOutput> TrackingOrderAsync(
            TrackingData input,
            [Service] FoodDeliveryAppContext context)
        {
            var order = context.Orders.Where(o => o.Id == input.OrderId).FirstOrDefault();
            if (order == null)
            {
                return new OrderOutput
                {
                    Message = "Order is not found",
                    TransactionDate = DateTime.Now.ToString()
                };
            }
                
            order.Latitude = input.Latitude;
            order.Longitude = input.Longitude;
            order.Status = "Order is on the way";
            context.Orders.Update(order);
            await context.SaveChangesAsync();
           
            return new OrderOutput
            {
                Message = "Tracking is successfully added",
                TransactionDate = DateTime.Now.ToString()
            };
        }

        //complete order
        [Authorize(Roles = new[] { "COURIER" })]
        public async Task<OrderOutput> CompleteOrderAsync(
            int id,
            [Service] FoodDeliveryAppContext context)
        {
            var order = context.Orders.Where(o => o.Id == id).FirstOrDefault();
            if (order == null) return new OrderOutput
            {
                Message = "Order is not found",
                TransactionDate = DateTime.Now.ToString()
            };
            order.Status = "Order is completed";
            context.Orders.Update(order);
            await context.SaveChangesAsync();

            return new OrderOutput
            {
                Message = "Order is finished",
                TransactionDate = DateTime.Now.ToString()
            };
        }
    }
}
