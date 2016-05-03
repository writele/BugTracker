using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BugTracker.Controllers
{
    public class TicketsHelper
    {
        private ApplicationDbContext db;

        public TicketsHelper(ApplicationDbContext context)
        {
            this.db = context;
        }

        public List<Ticket> GetUserTickets(string userId)
        {

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
                tickets = user.Projects.SelectMany(p => p.Tickets).ToList();
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
            return tickets;
        }

        public bool HasTicketPermission(string userId, int ticketId)
        {
            var user = db.Users.Find(userId);
            var ticket = db.Tickets.Find(ticketId);
            UserRolesHelper helper = new UserRolesHelper(db);
            var userRoles = helper.ListUserRoles(userId);
            if (userRoles.Contains("Admin"))
            {
                return true;
            }
            else if (userRoles.Contains("Project Manager") && user.Projects.SelectMany(p => p.Tickets).ToList().Contains(ticket))
            {
                return true;
            }
            else if (userRoles.Contains("Submitter") && userRoles.Contains("Developer"))
            {
                if (ticket.AssigneeId == userId || ticket.OwnerId == userId)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (userRoles.Contains("Developer") && ticket.AssigneeId == userId)
            {
                return true;
            }
            else if (userRoles.Contains("Submitter") && ticket.OwnerId == userId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}