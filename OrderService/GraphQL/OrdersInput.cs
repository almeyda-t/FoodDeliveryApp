using OrderService.Models;
using System.Collections.Generic;

namespace OrderService.GraphQL
{
    public record OrdersInput
    (
        int? Id,
        string Code,
        int UserId,
        int CourierId,
        List<OrderDetail> OrderDetails
    );
}
