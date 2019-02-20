using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Dendi.Controllers
{
    /// <summary>
    /// Currently not avaliable and useless :D
    /// </summary>
    public class ContactController : Controller
    {
        public ActionResult Contact()
        {
            ViewBag.Title = "Dendi App Contact!";

            return View();
        }
    }
}
