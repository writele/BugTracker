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
    [RequireHttps]
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Projects
        public ActionResult Index(string UserId)
        {
            var userId = User.Identity.GetUserId();
            var projects = new List<Project>();
            ProjectUsersHelper helper = new ProjectUsersHelper(db);
            projects = helper.ListProjects(userId);

            return View(projects);
        }

        //GET: Projects/ViewAll
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult ViewAll()
        {
            var projects = db.Projects.ToList();
            return View(projects);
        }

        // GET: Projects/Details/5
        public ActionResult Details(int? id)
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
            var userId = User.Identity.GetUserId();
            UserRolesHelper userHelper = new UserRolesHelper(db);
            ProjectUsersHelper projectHelper = new ProjectUsersHelper(db);
            var userRoles = userHelper.ListUserRoles(userId);
            var tickets = new List<Ticket>();
            if (userRoles.Contains("Admin") || (userRoles.Contains("Project Manager")))
            {
                tickets = project.Tickets.ToList();
            }
            else if (userRoles.Contains("Developer") && userRoles.Contains("Submitter"))
            {
                tickets = project.Tickets.Where(t => t.AssigneeId == userId || t.OwnerId == userId).ToList();
            }
            else if (userRoles.Contains("Developer"))
            {
                tickets = project.Tickets.Where(t => t.AssigneeId == userId).ToList();
            }
            else if (userRoles.Contains("Submitter"))
            {
                tickets = project.Tickets.Where(t => t.OwnerId == userId).ToList();
            }

            ViewBag.TicketList = tickets;
            return View(project);
        }

        [Authorize(Roles = "Admin, Project Manager")]
        // GET: Projects/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Project Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,Created,Deadline")] Project project)
        {
            if (ModelState.IsValid)
            {
                ProjectUsersHelper helper = new ProjectUsersHelper(db);
                project.Created = System.DateTimeOffset.Now;
                var userId = User.Identity.GetUserId();
                db.Projects.Add(project);
                helper.AssignUser(userId, project.Id);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Edit(int? id)
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
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Project Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,Created,Deadline")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        //GET: Project/AssignUsers
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AssignUsers(int? id)
        {
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

                AssignProjectUsersViewModel model = new AssignProjectUsersViewModel();
                ProjectUsersHelper helper = new ProjectUsersHelper(db);
                model.ProjectId = project.Id;
                model.ProjectTitle = project.Title;
                var currentUsers = helper.ListUsers(model.ProjectId);
                model.UsersList = currentUsers;
                model.CurrentUsers = new SelectList(currentUsers, "Id", "FullName");
                var absentUsers = helper.ListAbsentUsers(model.ProjectId);
                model.AbsentUsers = new SelectList(absentUsers, "Id", "FullName");
                return View(model);
            }
        }

        ////POST: Project/AddUser
        [Authorize(Roles = "Admin, Project Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUser(string AddUserId, int ProjectId)
        {
            ProjectUsersHelper helper = new ProjectUsersHelper(db);
            helper.AssignUser(AddUserId, ProjectId);
            db.SaveChanges();
            return RedirectToAction("AssignUsers", new { id = ProjectId });
        }

        ////POST: Project/RemoveUser
        [Authorize(Roles = "Admin, Project Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveUser(string RemoveUserId, int ProjectId)
        {
            ProjectUsersHelper helper = new ProjectUsersHelper(db);
            var tickets = db.Tickets.Where(t => t.AssigneeId == RemoveUserId && t.ProjectId == ProjectId).ToList();
            foreach (var ticket in tickets)
            {
                ticket.AssigneeId = null;
                ticket.Status = Status.Open;
                db.Tickets.Attach(ticket);
                db.Entry(ticket).Property("AssigneeId").IsModified = true;
                db.Entry(ticket).Property("Status").IsModified = true;
                db.SaveChanges();
            }
            helper.RemoveUser(RemoveUserId, ProjectId);
            db.SaveChanges();
            return RedirectToAction("AssignUsers", new { id = ProjectId });
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Delete(int? id)
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
            return View(project);
        }

        // POST: Projects/Delete/5
        [Authorize(Roles = "Admin, Project Manager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
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
