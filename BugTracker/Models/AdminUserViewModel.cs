using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class AdminUserViewModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public MultiSelectList Roles { get; set; }
        public MultiSelectList AbsentRoles { get; set; }
        public string[] SelectedCurrentRoles { get; set; }
        public string[] SelectedAbsentRoles { get; set; }

    }

    public class UsersModel
    {
        public List<ApplicationUser> Users { get; set; }

    }
}