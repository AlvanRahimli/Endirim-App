using Dendi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dendi.Common
{
    /// <summary>
    /// Leaving User
    /// </summary>
    public class LeavingUser
    {
        /// <summary>
        /// ID of User
        /// </summary>
        [Required(ErrorMessage =("Id is not specified."))]
        public int ID { get; set; }
        /// <summary>
        /// Reason of leaving
        /// </summary>
        public LeavingReason Reason { get; set; }
        /// <summary>
        /// Given stars to app.
        /// </summary>
        public int Stars { get; set; }
    }
}