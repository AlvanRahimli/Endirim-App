using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dendi.Models
{
    /// <summary>
    /// Logging user's full defined model.
    /// </summary>
    public class LoginUser
    {
        /// <summary>
        /// User's Username.
        /// </summary>
        [Required]
        public string Username { get; set; } 
        /// <summary>
        /// User's Password.
        /// </summary>
        public string Password { get; set; }
    }
}