namespace OrderService.GraphQL
{
    public class UpdateOrder
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourierId { get; set; }

        public List<OrderDetailData> Details { get; set; }
    }
}
