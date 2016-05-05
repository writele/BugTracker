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
        public ActionResult UserProfile()
        {
            ApplicationUser user = db.Users.Find(TempData["UserId"]);
            if (user == null)
            {
                return RedirectToAction("Index");
            }
            UserProfileView model = new UserProfileView();
            Project project = db.Projects.Find(TempData["ProjectId"]);
            model.ProjectId = project.Id;
            UserRolesHelper helper = new UserRolesHelper(db);
            model.Name = user.FullName;
            model.Email = user.Email;
            model.PhoneNumber = user.PhoneNumber;
            model.ProjectCount = user.Projects.Count();
            model.Roles = helper.ListUserRoles(user.Id);
            var tickets = user.Projects.SelectMany(p => p.Tickets).ToList();
            model.TicketsAssigned = tickets.Where(t => t.AssigneeId == user.Id).Count();
            model.TicketsSubmitted = tickets.Where(t => t.OwnerId == user.Id).Count();
            model.TicketsResolved = tickets.Where(t => t.AssigneeId == user.Id).Where(t => t.Status == Status.Resolved || t.Status == Status.Closed).Count();
            return View(model);
        }

        [HttpPost]
        public ActionResult UserProfile(string userId, int? ProjectId)
        {
            var user = db.Users.Find(userId);
            TempData["UserId"] = userId;
            TempData["ProjectId"] = ProjectId;
            return RedirectToAction("UserProfile");
        }


        public ActionResult FAQ()
        {
            return View();
        }

    }
}