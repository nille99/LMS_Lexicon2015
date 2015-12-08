﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LMS_Lexicon2015.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;


namespace LMS_Lexicon2015.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        public static bool FromPartitialView;
        //private ApplicationSignInManager _signInManager;
        //private ApplicationUserManager _userManager;

        //public UsersController()
        //{
        //}

        //public UsersController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        //{
        //    UserManager = userManager;
        //    SignInManager = signInManager;
        //}

        //public ApplicationSignInManager SignInManager
        //{
        //    get
        //    {
        //        return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
        //    }
        //    private set
        //    {
        //        _signInManager = value;
        //    }
        //}

        //public ApplicationUserManager UserManager
        //{
        //    get
        //    {
        //        return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        //    }
        //    private set
        //    {
        //        _userManager = value;
        //    }
        //}

        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Users
        public ActionResult Index(string searchString)
        {
            //var applicationUsers = db.Users.ToList();

            //ViewBag.Roles = db.Roles.ToList();
            //FromPartitialView = false;

            //var model = db.Users.Select(r => new UserListViewModel
            //    {
            //        Id = r.Id,
            //        FirstName = r.FirstName,
            //        LastName = r.LastName,
            //        Email = r.Email,
            //        Role = db.Roles.Where(R => R.Id == r.Roles.FirstOrDefault().RoleId).FirstOrDefault().Name,
            //        Group = db.Groups.Where(G => G.Id == r.GroupId).FirstOrDefault().Name,
            //        PhoneNumber = r.PhoneNumber
            //    }).ToList();

            //return View(model);



            //__________________________________________

            ViewBag.searchString = searchString;
            var Users = from s in db.Users select s;
            if (!String.IsNullOrEmpty(searchString))
        {
                Users = Users.Where(s => s.FirstName.Contains(searchString)
                || (s.LastName.ToString()).Contains(searchString)
                || (s.FirstName + " " + s.LastName).Contains(searchString)
                    //|| (s.Role.ToString()).Contains(searchString)
                || s.Group.Name.Contains(searchString)
                || s.Email.Contains(searchString)
                || s.PhoneNumber.Contains(searchString)
                 );
            }

            Users = Users.OrderByDescending(s => s.LastName);
            //return View(Users.ToList());



            var applicationUsers = Users.ToList();

            ViewBag.Roles = db.Roles.ToList();
            FromPartitialView = false;

            var model = Users.Select(r => new UserListViewModel
                {
                Id = r.Id,
                FirstName = r.FirstName,
                LastName = r.LastName,
                Email = r.Email,
                Role = db.Roles.Where(R => R.Id == r.Roles.FirstOrDefault().RoleId).FirstOrDefault().Name,
                Group = db.Groups.Where(G => G.Id == r.GroupId).FirstOrDefault().Name,
                GroupId = db.Groups.Where(G => G.Id == r.GroupId).FirstOrDefault().Id,
                PhoneNumber = r.PhoneNumber
    }).ToList();

            return View(model);


        }

        // GET: Groups
        //       public ActionResult DelGroup(int id)
        public ActionResult PartitialGroup(int? id)
        {
            ViewBag.Line1 = "/";
            ViewBag.Line2 = "-";

            var model = db.Users.Where(r => r.GroupId == id).ToList();
            if (model.Any())
            {
                ViewBag.GroupName = model.First().Group.Name;
            }
            else ViewBag.GroupName = "Tom Grupp";
            FromPartitialView = true;
            return View(model);
        }

        // GET: Users/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ApplicationUser applicationUser = db.Users.Find(id);

            ViewBag.Role = db.Roles.Find((applicationUser.Roles).First().RoleId).Name;
            ViewBag.GroupName = "Gruppnamn";
            ViewBag.GroupStart = "Startdatum";
            ViewBag.GroupEnd = "Slutdatum";
            ViewBag.RoleHeader = "Roll";
            ViewBag.EmailHeader = "Epost";
            ViewBag.PhoneHeader = "Mobilnummer";
            ViewBag.UserName = "Användarnamn";

            if (applicationUser == null)
            {
                return HttpNotFound();
            }

            return View(applicationUser);
        }

        // GET: Users/Create
        [Authorize(Roles = "Lärare")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,GroupId,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(applicationUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(applicationUser);
        }
        [Authorize(Roles = "Lärare")] //var används denna action?
        public ActionResult Register()
        {
            return View();
        }

        // GET: Users/Edit/5
        [Authorize(Roles = "Lärare")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model =
            db.Users.Where(u => u.Id == id).Select(r => new UserListViewModel
            {
                Id = r.Id,
                FirstName = r.FirstName,
                LastName = r.LastName,
                Email = r.Email,
                Role = db.Roles.Where(R => R.Id == r.Roles.FirstOrDefault().RoleId).FirstOrDefault().Name,
                Group = db.Groups.Where(G => G.Id == r.GroupId).FirstOrDefault().Name,
                GroupId = db.Groups.Where(G => G.Id == r.GroupId).FirstOrDefault().Id,
                PhoneNumber = r.PhoneNumber,
                UserName = r.UserName

            }).FirstOrDefault();

           // if (String.IsNullOrEmpty(model.Group)) model.Group = "c"; //test ck

            if (model == null)
            {
                return HttpNotFound();
            }

            List<Group> g = db.Groups.ToList();
            g.Insert(0, null);


            //lägg till en gång till "If we got this far, something failed, redisplay form"
            ViewBag.Role = new SelectList(db.Roles, "Name", "Name");//en bäg för rullningslistan på formuläret 
            ViewBag.GroupTeacher = new SelectList(g, "Id", "Name");//en bäg för rullningslistan på formuläret
            ViewBag.GroupId = new SelectList(g, "Id", "Name",model.GroupId);//en bäg för rullningslistan på formuläret
            //return View();


            return View(model);
        }

        //vi har ändratedit pga lössenord ändras vid ändringar

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Lärare")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,GroupId,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] ApplicationUser applicationUser)

        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,Email,Role,GroupId,PhoneNumber,UserName,GroupStudent,GroupTeacher")] UserListViewModel listUser)
        {

            //var currentUser = applicationUser.Id;
            var userInDb = db.Users.Where(u => u.Id == listUser.Id).FirstOrDefault();

            if (listUser.LastName != userInDb.LastName && !String.IsNullOrEmpty(listUser.LastName))
            {
                userInDb.LastName = listUser.LastName;
            }


            if (listUser.FirstName != userInDb.FirstName && !String.IsNullOrEmpty(listUser.FirstName))
            {
                userInDb.FirstName = listUser.FirstName;
            }


            //lärare kan vara utan grupp 
            //  både före och efter ändring FIXA. GroupId=12, Group=NULL
            if (!listUser.GroupId.HasValue && listUser.Role == "Lärare")
            {
                userInDb.GroupId = listUser.GroupId;
            }

            //userInDb.Group.Name.

            if (listUser.GroupId.HasValue)
            {
                userInDb.GroupId = listUser.GroupId; //testar inte på förändring. Blir ingen skillnad. Mindre krångligt då Lärare byter från ingen grupp till en grupp
                
            }

            if (!listUser.GroupId.HasValue && listUser.Role == "Elev")
            {
                ModelState.AddModelError("", "Du försökte att ta bort grupp från en elev. Elev kan bara byta grupp.");
                //return RedirectToAction("Edit");


                List<Group> g = db.Groups.ToList();
                g.Insert(0, null);


                //lägg till en gång till "If we got this far, something failed, redisplay form"
                ViewBag.Role = new SelectList(db.Roles, "Name", "Name");//en bäg för rullningslistan på formuläret 
                ViewBag.GroupTeacher = new SelectList(g, "Id", "Name");//en bäg för rullningslistan på formuläret
                ViewBag.GroupId = new SelectList(g, "Id", "Name", userInDb.GroupId);//en bäg för rullningslistan på formuläret
                //return View();

                return View(listUser);
                
            }

            if (listUser.Email != userInDb.Email && !String.IsNullOrEmpty(listUser.Email))
            {
                userInDb.Email = listUser.Email;
            }
            if (listUser.PhoneNumber != userInDb.PhoneNumber)
            {
                userInDb.PhoneNumber = listUser.PhoneNumber;
            }


            db.SaveChanges();

            if (ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            // i fall det knasar måste nedan finnas så att det visas om
            return RedirectToAction("Edit");


        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Lärare")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = db.Users.Find(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            return View(applicationUser);
        }

        // POST: Users/Delete/5
        [Authorize(Roles = "Lärare")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ApplicationUser applicationUser = db.Users.Find(id);
            db.Users.Remove(applicationUser);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
