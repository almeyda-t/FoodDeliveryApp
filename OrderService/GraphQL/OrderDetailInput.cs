namespace OrderService.GraphQL
{
    public record OrderDetailInput
    (
        int? Id,
        int? OrderId,
        int FoodId,
        int Quantity
    );
}
