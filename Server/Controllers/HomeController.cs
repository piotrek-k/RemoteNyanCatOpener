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
        public ActionResult ControlPanel(string password)
        {
            string generatedPassword = "banan"+(DateTime.UtcNow.Hour+1+6)+""+(DateTime.UtcNow.Minute + 9);

            if (password == generatedPassword)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
            
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}