using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dendi.Models
{
    public class LoginUser
    {
        [Required]
        public string Username { get; set; }        
        public string Password { get; set; }
    }
}