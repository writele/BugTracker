namespace BugTracker.Models
{
    public class Attachment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }

        public string AuthorId { get; set; }

        public string Description { get; set; }
        public string MediaURL { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser Author { get; set; }
    }
}