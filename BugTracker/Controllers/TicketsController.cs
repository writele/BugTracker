using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using BugTracker.Controllers;

namespace BugTracker
{
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            UserRolesHelper userHelper = new UserRolesHelper(db);
            ProjectUsersHelper projectHelper = new ProjectUsersHelper(db);
            var userRoles = userHelper.ListUserRoles(userId);
            var tickets = new List<Ticket>();
            if (userRoles.Contains("Admin"))
            {   
                tickets = db.Tickets.Include(t => t.Assignee).Include(t => t.Owner).Include(t => t.Project).ToList();
            }
            else if (userRoles.Contains("Project Manager"))
            {
                //tickets in projects where projects.users includes user
                var projects = projectHelper.ListProjects(userId);
                foreach(var project in projects)
                {
                    foreach(var ticket in project.Tickets)
                    {
                        tickets.Add(ticket);
                    }
                }

            }
            else if (userRoles.Contains("Developer") && userRoles.Contains("Submitter"))
            {
                tickets = db.Tickets.Where(t => t.AssigneeId == userId || t.OwnerId == userId).Include(t => t.Assignee).Include(t => t.Owner).Include(t => t.Project).ToList();

            }
            else if (userRoles.Contains("Developer"))
            {
                //tickets where AssigneedId == userId
                tickets = db.Tickets.Where(t => t.AssigneeId == userId).Include(t => t.Assignee).Include(t => t.Owner).Include(t => t.Project).ToList();
            }
            else if (userRoles.Contains("Submitter"))
            {
                //tickets where OwnerId == userID
                tickets = db.Tickets.Where(t => t.OwnerId == userId).Include(t => t.Assignee).Include(t => t.Owner).Include(t => t.Project).ToList();
            }

            return View(tickets);
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = User.Identity.GetUserId(); 
            return View(ticket);
        }

        [Authorize(Roles="Submitter")]
        // GET: Tickets/Create
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }       
            ViewBag.ProjectId = id;
            ViewBag.ProjectTitle = project.Title;
            return View();
        }
        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Submitter")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ProjectId,Title,Body,Priority,Type")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                if(ticket.Title == null)
                {
                    ticket.Title = StringUtilities.Shorten(ticket.Body, 50);
                }
                ticket.Created = System.DateTimeOffset.Now;
                ticket.OwnerId = User.Identity.GetUserId();
                ticket.Status = 0;
                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            var project = db.Projects.FirstOrDefault(p => p.Id == ticket.ProjectId);
            var ProjectTitle = project.Title;
            var type = db.Types.Find(ticket.TypeId);
            var TicketType = type.Name;
            ViewBag.ProjectTitle = ProjectTitle;
            ViewBag.TicketType = TicketType;
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Created,Title,Body,Priority,Type")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                if (ticket.Title == null)
                {
                    ticket.Title = StringUtilities.Shorten(ticket.Body, 50);
                }
                ticket.Modified = System.DateTimeOffset.Now;
                db.Tickets.Attach(ticket);
                //db.Entry(ticket).State = EntityState.Modified;
                db.Entry(ticket).Property("Modified").IsModified = true;
                db.Entry(ticket).Property("Title").IsModified = true;
                db.Entry(ticket).Property("Body").IsModified = true;
                db.Entry(ticket).Property("Priority").IsModified = true;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ticket);
        }

        //GET Tickets/Assign Users
        [Authorize(Roles ="Admin,Project Manager")]
        public ActionResult AssignUser(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            AssignTicketUserViewModel AssignModel = new AssignTicketUserViewModel();
            AssignModel.TicketId = ticket.Id;
            AssignModel.TicketTitle = ticket.Title;
            ProjectUsersHelper helper = new ProjectUsersHelper(db);
            UserRolesHelper userHelper = new UserRolesHelper(db);
            var projectUsers = helper.ListUsers(ticket.ProjectId);
            var projectDevelopers = new List<ApplicationUser>();
            foreach (var user in projectUsers)
            {
                if(userHelper.IsUserInRole(user.Id, "Developer"))
                {
                    projectDevelopers.Add(user);
                }
            }
            if(ticket.Assignee != null) {
                AssignModel.TicketAssignedTo = ticket.Assignee.FullName;
            }  
            AssignModel.UsersList = new SelectList(projectDevelopers, "Id", "FullName");
            return View(AssignModel);
        }

        ////POST: Tickets/AssignUser
        [Authorize(Roles = "Admin, Project Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignUser(string UserId, int TicketId)
        {
                var ticket = db.Tickets.Find(TicketId);
                ticket.AssigneeId = UserId;
                ticket.Status = Status.Pending;
                ticket.Modified = System.DateTimeOffset.Now;
                db.Tickets.Attach(ticket);
                db.Entry(ticket).Property("AssigneeId").IsModified = true;
                db.Entry(ticket).Property("Status").IsModified = true;
                db.Entry(ticket).Property("Modified").IsModified = true;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = TicketId });
        }

        // GET: Tickets/Close/5
        public ActionResult Close(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Close
        [Authorize(Roles = "Admin, Project Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Close(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            ticket.Status = Status.Closed;
            db.Tickets.Attach(ticket);
            db.Entry(ticket).Property("Status").IsModified = true;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
