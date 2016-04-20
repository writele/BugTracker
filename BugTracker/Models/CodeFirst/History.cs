using System;

namespace BugTracker.Models
{
    public class History
    {
        public int Id { get; set; }
        public int TicketId { get; set; }

        public DateTimeOffset Date { get; set; }
        public string Body { get; set; }

        public virtual Ticket Ticket { get; set; }
    }
}