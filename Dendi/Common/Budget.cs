using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dendi.Common
{
    public class Budget
    {
        /// <summary>
        /// User's Budget's Amount, decimal.
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// User's Budget's Currency, can be 'Azn', 'Dollar', 'Euro'
        /// </summary>
        public string Currency { get; set; }
    }
}