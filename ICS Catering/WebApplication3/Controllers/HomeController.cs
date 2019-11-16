using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {

    
        public ActionResult Index()
        {
            
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Get in touch with us on 01924 455492 to enquire about our range.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.TheMessage = "Give us a call on 01924 455492!";

            return View();
        }

        [HttpPost]
        public ActionResult Contact(String message)
        {
            ViewBag.TheMessage = "Thanks, your message was sent";

            return View();
        }

    }
}