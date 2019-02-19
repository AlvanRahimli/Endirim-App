using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dendi.Models
{
    /// <summary>
    /// Product Model.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Product's ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// The Adder User's ID
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// Product's Name
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// Product's Shop
        /// </summary>
        [Required]
        public string Shop { get; set; }
        /// <summary>
        /// Product's price
        /// </summary>
        [Required] [Range(0,100000000)]
        public decimal Price { get; set; }
        /// <summary>
        /// Product's Addition Time
        /// </summary>
        public string AdditionTime { get; set; }
    }
}
