using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dendi.Models
{
    public class User
    {
        public int ID { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public Budget Budget { get; set; }        
        public bool IsAdmin { get; set; }
    }
}