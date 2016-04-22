using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin
        public ActionResult Index()
        {
            return RedirectToAction("ListUsers");
        }

        [Authorize(Roles = "Admin")]
        //
        // GET: /Admin/ListUsers
        public ActionResult ListUsers()
        {
            UsersListViewModel UserModel = new UsersListViewModel();
            UserRolesHelper helper = new UserRolesHelper(db);
            var users = db.Users.ToList();
            foreach (var user in users)
            {
                var id = user.Id;
                var name = user.FirstName + " " + user.LastName;
                var roles = helper.ListUserRoles(id);
                var projects = new List<string>();
                UserModel.Users = new List<UsersViewModel>();
                foreach (var project in user.Projects)
                {
                    projects.Add(project.Title);
                }
                UserModel.Users.Add(new UsersViewModel(id, name, roles, projects));
            }

            return View(UserModel);
        }

        //
        // GET: /Admin/EditUser
        [Authorize(Roles = "Admin")]
        public ActionResult EditUser(string id)
        {
            var user = db.Users.Find(id);
            AdminUserViewModel AdminModel = new AdminUserViewModel();
            UserRolesHelper helper = new UserRolesHelper(db);
            var currentRoles = helper.ListUserRoles(id);
            var absentRoles = helper.ListAbsentUserRoles(id);
            AdminModel.AbsentRoles = new MultiSelectList(absentRoles);
            AdminModel.Roles = new MultiSelectList(currentRoles);
            AdminModel.Id = user.Id;
            AdminModel.Name = user.FirstName + " " + user.LastName;

            return View(AdminModel);
        }

        //
        // POST: Add User Role
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AddRole(string Id, List<string> SelectedAbsentRoles)
        {
            if (ModelState.IsValid)
            {  
                UserRolesHelper helper = new UserRolesHelper(db);
                var user = db.Users.Find(Id);
                foreach (var role in SelectedAbsentRoles)
                {
                    helper.AddUserToRole(Id, role);
                }

                db.Entry(user).State = EntityState.Modified;
                db.Users.Attach(user);
                db.SaveChanges();
                return RedirectToAction("ListUsers");
            }
            return View(Id);
        }

        //
        // POST: Remove User Role
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult RemoveRole(string Id, List<string> SelectedCurrentRoles)
        {
            if (ModelState.IsValid)
            {
                UserRolesHelper helper = new UserRolesHelper(db);
                var user = db.Users.Find(Id);
                foreach (var role in SelectedCurrentRoles)
                {
                    helper.RemoveUserFromRole(Id, role);
                }

                db.Entry(user).State = EntityState.Modified;
                db.Users.Attach(user);
                db.SaveChanges();
                return RedirectToAction("ListUsers");
            }
            return View(Id);
        }
    }
}