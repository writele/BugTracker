using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string AuthorId { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTimeOffset Date { get; set; }
        public DateTimeOffset? Modified { get; set; }

        public virtual ApplicationUser Author { get; set; }
        public virtual Ticket Ticket { get; set; }
    }
}