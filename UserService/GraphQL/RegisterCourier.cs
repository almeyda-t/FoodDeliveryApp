namespace OrderService.GraphQL
{
    public record RegisterCourier
    (
        int? Id,
        string CourierName,
        string PhoneNumber,
        bool? Availability,
        int UserId
    );
}
