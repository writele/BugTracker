using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class AssignProjectUsersViewModel
    {
        public string ProjectTitle { get; set; }
        public int ProjectId { get; set; }

        public List<ApplicationUser> UsersList { get; set; }
        public SelectList CurrentUsers { get; set; }
        public SelectList AbsentUsers { get; set; }

        public string AddUserId { get; set; }
        public string RemoveUserId { get; set; }

    }
}