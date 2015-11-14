using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Server.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult ControlPanel()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}