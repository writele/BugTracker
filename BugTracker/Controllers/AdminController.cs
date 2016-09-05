using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    [RequireHttps]
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
            UserModel.Users = new List<UsersViewModel>();
            foreach (var user in users)
            {
                var id = user.Id;
                var name = user.FirstName + " " + user.LastName;
                var roles = helper.ListUserRoles(id);
                var projects = new List<string>();
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
            AdminModel.User = user;

            return View(AdminModel);
        }

        //
        // POST: Add User Role
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AddRole(string AddId, List<string> SelectedAbsentRoles)
        {
            if (ModelState.IsValid)
            {  
                UserRolesHelper helper = new UserRolesHelper(db);
                var user = db.Users.Find(AddId);
                foreach (var role in SelectedAbsentRoles)
                {
                    helper.AddUserToRole(AddId, role);
                }

                db.Entry(user).State = EntityState.Modified;
                db.Users.Attach(user);
                db.SaveChanges();
                return RedirectToAction("ListUsers");
            }
            return View(AddId);
        }

        //
        // POST: Remove User Role
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult RemoveRole(string RemoveId, List<string> SelectedCurrentRoles)
        {
            if (ModelState.IsValid)
            {
                UserRolesHelper helper = new UserRolesHelper(db);
                var user = db.Users.Find(RemoveId);
                foreach (var role in SelectedCurrentRoles)
                {
                    helper.RemoveUserFromRole(RemoveId, role);
                }

                db.Entry(user).State = EntityState.Modified;
                db.Users.Attach(user);
                db.SaveChanges();
                return RedirectToAction("ListUsers");
            }
            return View(RemoveId);
        }
    }
}