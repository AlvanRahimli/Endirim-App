using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Dendi.Common;

namespace Dendi.Models
{
    /// <summary>
    /// User's model. UserName and Password is required.
    /// </summary>
    public class User
    {
        /// <summary>
        /// User's ID. Required for actions.
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// User's Username.
        /// </summary>
        [Required(ErrorMessage ="UserName is not fulfilled.")]
        public string UserName { get; set; }
        /// <summary>
        /// User's Password. While registering, it is plain. After Logging in it becomes Hashed string.
        /// </summary>
        [Required(ErrorMessage = "Password is not fulfilled.")]
        public string Password { get; set; }
        /// <summary>
        /// User's Phone number. Not required.
        /// </summary>
        public string Phone { get; set; }  
        /// <summary>
        /// User's Email.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// User's Budget. For example, 17.5 Azn.
        /// </summary>
        public Budget Budget { get; set; }
        /// <summary>
        /// Determines if user is Administrator.
        /// </summary>
        public bool IsAdmin { get; set; }
    }
}