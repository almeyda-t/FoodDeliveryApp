using System;
using System.Collections.Generic;

namespace UserService.Models
{
    public partial class Food
    {
        public Food()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Stock { get; set; }
        public double Price { get; set; }
        public DateTime Created { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
