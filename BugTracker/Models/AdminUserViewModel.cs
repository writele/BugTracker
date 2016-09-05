using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class AdminUserViewModel
    {
        public ApplicationUser User { get; set; }
        public MultiSelectList Roles { get; set; }
        public MultiSelectList AbsentRoles { get; set; }
        public string[] SelectedCurrentRoles { get; set; }
        public string[] SelectedAbsentRoles { get; set; }
    }

    public class UsersListViewModel
    {
        public List<UsersViewModel> Users { get; set; }

    }

    public class UsersViewModel
    {
        public UsersViewModel (string id, string name, IList<string> roles, List<string> projects)
        {
            this.Id = id;
            this.Name = name;
            this.Roles = roles;
            this.Projects = projects;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public IList<string> Roles { get; set; }
        public List<string> Projects { get; set; }
    }
}