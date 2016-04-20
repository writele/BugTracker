using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class Ticket
    {

        public Ticket()
        {
            this.Comments = new HashSet<Comment>();
            this.Attachments = new HashSet<Attachment>();
            this.History = new HashSet<History>();
        }

        public int Id { get; set; }

        public int ProjectId { get; set; }
        public Priority Priority { get; set; }
        public int TypeId { get; set; }
        public Status Status { get; set; }

        public string OwnerId { get; set; }
        public string AssigneeId { get; set; }

        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Modified { get; set; }

        public string Title { get; set; }
        [Required]
        public string Body { get; set; }

        public virtual ApplicationUser Owner { get; set; }
        public virtual ApplicationUser Assignee { get; set; }

        public virtual string Type { get; set; }

        public virtual Project Project { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Attachment> Attachments { get; set; }
        public virtual ICollection<History> History { get; set; }

    }

    public enum Priority
    {
        Low,
        Medium,
        High
    }

    public enum Status
    {
        Open,
        Pending,
        Resolved,
        Closed
    }
}