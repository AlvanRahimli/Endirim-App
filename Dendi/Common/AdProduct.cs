using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dendi.Models
{
    /// <summary>
    /// Currently not completed
    /// </summary>
    public class AdProduct : Product
    {
        /// <summary>
        /// Will be UserID of Advertiser
        /// </summary>
        public string Owner { get; set; }
        /// <summary>
        /// There will be types of Ads
        /// </summary>
        public string AdType { get; set; }
    }
}