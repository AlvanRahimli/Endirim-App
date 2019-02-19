using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dendi.Models
{
    /// <summary>
    /// Inherited from Product class
    /// </summary>
    public class DiscountedProduct : Product
    {
        /// <summary>
        /// Discount percentage
        /// </summary>
        [Range(0,100)]
        [Required]
        public decimal Discount { get; set; }
        /// <summary>
        /// Price after discount
        /// </summary>
        public decimal LastPrice { get; set; }
    }
}