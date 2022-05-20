namespace OrderService.GraphQL
{
    public partial class OrderData
    {
        public int? Id { get; set; }
        public string? Code { get; set; } = null!;
        public int UserId { get; set; }
        public int CourierId { get; set; }


        public List<OrderDetailData> Details { get; set; }
    }
}
