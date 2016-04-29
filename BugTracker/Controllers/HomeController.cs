using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        
        [Authorize]
        public ActionResult Index()
        {
            {
                DashboardViewModel model = new DashboardViewModel();
                model.Id = User.Identity.GetUserId();
                var user = db.Users.Find(model.Id);
                model.Name = user.FirstName + " " + user.LastName;

                model.Projects = user.Projects.Take(5).ToList();
                //Select the first 5 projects
                model.Tickets = user.Projects.SelectMany(p => p.Tickets).Take(5).ToList();
                //Select the first 5 tickets

                return View(model);
            }
        }

        public ActionResult UserProfile(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View();
        }

        [HttpPost]
        public ActionResult UserProfile()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}