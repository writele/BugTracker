using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Controllers
{
    public class ProjectUsersHelper
    {
        private ApplicationDbContext db;

        public ProjectUsersHelper(ApplicationDbContext context)
        {
            this.db = context;
        }

        public void AssignUser(string userId, int projectId)
        {
            if(!HasProject(userId, projectId))
            {
                var user = db.Users.Find(userId);
                var project = db.Projects.Find(projectId);
                project.Users.Add(user);
            }
        }

        public bool HasProject(string userId, int projectId)
        {
            var user = db.Users.Find(userId);
            var project = db.Projects.Find(projectId);
            if (project.Users.Contains(user)) //Or Any(u=> u.Id ==userId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RemoveUser(string userId, int projectId)
        {
            if (HasProject(userId, projectId))
            {
                var user = db.Users.Find(userId);
                var project = db.Projects.Find(projectId);
                project.Users.Remove(user);
            }
        }

        public List<Project> ListProjects(string userId)
        {
            var user = db.Users.Find(userId);
            return user.Projects.ToList();
        }

        public List<ApplicationUser> ListUsers(int projectId)
        {
            var project = db.Projects.Find(projectId);
            return project.Users.ToList();
        }

        public List<ApplicationUser> ListAbsentUsers(int projectId)
        {
            var project = db.Projects.Find(projectId);
            var users = db.Users.ToList();
            var absentUsers = new List<ApplicationUser>();
            foreach(var user in users)
            {
                if(!HasProject(user.Id, projectId))
                {
                    absentUsers.Add(user);
                }
            }
            return absentUsers;
        }

        public List<string> ListProjectManagers(int projectId)
        {
            var projectManagers = new List<string>();
            var project = db.Projects.Find(projectId);
            var projectUsers = project.Users.ToList();
            UserRolesHelper helper = new UserRolesHelper(db);
            foreach (var user in projectUsers)
            {
                if (helper.IsUserInRole(user.Id, "Project Manager"))
                {
                    projectManagers.Add(user.Email);
                }
            }
            return projectManagers;
        }

    }
}