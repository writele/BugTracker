using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class DashboardViewModel
    {
        public string Name { get; set; }
        public string Id { get; set; }

        public List<Project> Projects { get; set; }
        public List<Ticket> Tickets { get; set; }
    }
}