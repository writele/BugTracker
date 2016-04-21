using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public enum Status
    {
        [Display(Name = "Open")]
        Open,
        [Display(Name = "Pending")]
        Pending,
        [Display(Name = "Resolved")]
        Resolved,
        [Display(Name = "Closed")]
        Closed
    }
}