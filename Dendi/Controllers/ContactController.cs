using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Dendi.Controllers
{
    /// <summary>
    /// Currently not avaliable
    /// </summary>
    public class ContactController : Controller
    {
        public ActionResult Contact()
        {
            ViewBag.Title = "Dendi App";

            return View();
        }
    }
}
