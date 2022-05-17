using System;
using System.Collections.Generic;

namespace OrderService.Models
{
    public partial class Profile
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }

        public virtual User? User { get; set; }
    }
}
