namespace OrderService.GraphQL
{
    public class OrderDetailData
    {
        public int? Id { get; set; }
        public int? OrderId { get; set; }
        public int FoodId  { get; set; }
        public int Quantity { get; set; }
    }
}
