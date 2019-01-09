using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dendi.Models
{
    public class DiscountedProduct : Product
    {
        [Range(0,100)] [Required]
        public decimal Discount { get; set; }
        public decimal LastPrice { get; set; }
    }
}