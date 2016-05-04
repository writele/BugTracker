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

                TicketsHelper helper = new TicketsHelper(db);
                model.Tickets = helper.GetUserTickets(model.Id).OrderByDescending(t => t.Created).Take(5).ToList();
                return View(model);
            }
        }

        // GET: User Profile
        public ActionResult UserProfile(string slug)
        {
            if (String.IsNullOrWhiteSpace(slug))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = db.Users.FirstOrDefault(u => u.Email == slug);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult UserProfile(int userId)
        {
            var user = db.Users.Find(userId);
            var userSlug = user.UserName;
            return RedirectToAction("UserProfile", new { slug = userSlug });
        }


        public ActionResult FAQ()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}