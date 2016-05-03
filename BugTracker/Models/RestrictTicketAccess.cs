using Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace BugTracker.Models
{
    public class RestrictTicketAccess : AuthorizeAttribute
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = db.Users.Find(httpContext.User.Identity.GetUserId());
            var ticket = db.Tickets.Find(14);
            if (httpContext.User.IsInRole("Project Manager") && user.Projects.SelectMany(p => p.Tickets).Contains(ticket)) 
            {
                return true;
            }

            return false; 
        }
    }
}