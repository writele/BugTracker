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
using System.IO;

namespace BugTracker
{
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        [Authorize]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            TicketsHelper helper = new TicketsHelper(db);
            var tickets = helper.GetUserTickets(userId);

            return View(tickets);
        }

        // GET: Tickets/Details/5
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        [Authorize(Roles = "Admin, Project Manager")]
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
            ticket.Modified = System.DateTimeOffset.Now;
            db.Tickets.Attach(ticket);
            db.Entry(ticket).Property("Status").IsModified = true;
            db.Entry(ticket).Property("Modified").IsModified = true;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Tickets/Resolve/5
        [Authorize(Roles = "Developer")]
        public ActionResult Resolve(int? id)
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

        // POST: Tickets/Resolve
        [Authorize(Roles = "Developer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Resolve(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            ticket.Status = Status.Resolved;
            ticket.Modified = System.DateTimeOffset.Now;
            db.Tickets.Attach(ticket);
            db.Entry(ticket).Property("Status").IsModified = true;
            db.Entry(ticket).Property("Modified").IsModified = true;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //GET: Tickets/AddComment
        [Authorize]
        public ActionResult AddComment(int? id)
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
            ViewBag.TicketId = id;
            ViewBag.TicketTitle = ticket.Title;
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment([Bind(Include = "Id,TicketId,Content")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.AuthorId = User.Identity.GetUserId();
                comment.Date = System.DateTimeOffset.Now;
                db.Comments.Add(comment);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = comment.TicketId});
            }
                return View();
        }

        //GET: Tickets/AddAttachment
        [Authorize]
        public ActionResult AddAttachment(int? id)
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
            ViewBag.TicketId = id;
            ViewBag.TicketTitle = ticket.Title;
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAttachment([Bind(Include = "Id,TicketId,Description,MediaURL")] Attachment attachment, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                    attachment.MediaURL = "/Uploads/" + fileName;
                }
                db.Attachments.Add(attachment);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = attachment.TicketId });
            }
            return View();
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
