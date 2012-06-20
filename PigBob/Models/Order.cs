using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PigBob.Models
{
    public class Order
    {
        public int OrderID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }

    public class OrderItem
    {
        public int ID { get; set; }

        public bool Bacon { get; set; }
        public bool Sausage { get; set; }
        public bool Egg { get; set; }
        [MaxLength(200, ErrorMessage = "Too Long! Please stop trying to hack me.")]
        public string Other { get; set; }
        public int OrderID { get; set; }

        public virtual PigEater Eater { get; set; }
        public virtual Order Order { get; set; }
    }
}