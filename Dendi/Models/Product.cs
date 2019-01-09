using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dendi.Models
{
    public class Product
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Shop { get; set; }
        [Required] [Range(0,100000000)]
        public decimal Price { get; set; }
        public DateTime AdditionTime { get; set; }
    }
}
