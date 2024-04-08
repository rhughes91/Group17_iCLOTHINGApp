using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Group17_iCLOTHINGApp.Models;

namespace Group17_iCLOTHINGApp.Controllers
{
    public class UserPasswordsController : Controller
    {
        private static String currentUser = null;
        private static String currentCust = null;

        public static bool Verified()
        {
            return currentUser != null && currentUser != "";
        }

        public static String CurrentUser()
        {
            return currentUser;
        }

        public static String CurrentCustomer()
        {
            return currentCust;
        }

        public class Account
        {
            public String password { get; set; }
            public Customer customer { get; set; }

            public bool valid()
            {
                return customer.customerID != "" && customer.customerName != "" && customer.userID != "" && customer.customerGender != "" && password != "";
            }
        }

        private Group17_iCLOTHINGDBEntities db = new Group17_iCLOTHINGDBEntities();

        // GET: UserPasswords
        public ActionResult Index(bool? status)
        {
            if(status == true)
            {
                currentUser = null;
                currentCust = null;
            }
            if(Verified())
            {
                return RedirectToAction("LoggedIn");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(UserPassword result)
        {
            var query = from user in db.UserPassword where user.userID == result.userID && user.userEncryptedPassword == result.userEncryptedPassword select user;
            if (query.Any())
            {
                currentUser = result.userID;
                currentCust = (from cust in db.Customer where cust.userID == currentUser select cust.customerID).FirstOrDefault();
                return View("~/Views/Home/Index.cshtml");
            }

            return View(result);
        }


        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Account result)
        {
            var query = from user in db.UserPassword where user.userID == result.customer.userID select user;
            if(result.valid() && query.Any())
            {
                System.Diagnostics.Debug.WriteLine("Username has already been taken");
                return View(result);
            }

            UserPassword up = new UserPassword();
            up.userID = result.customer.userID;
            up.userAccountName = result.customer.userID;
            up.userEncryptedPassword = result.password;
            up.passwordExpiryTime = 31;
            up.userAccountExpirtDate = DateTime.Now.AddDays(up.passwordExpiryTime);

            Customer cust = result.customer;
            cust.customerBillingAddress = "";
            cust.customerShippingAddress = "";
            switch(cust.customerGender.ToLower())
            {
                case "m":
                case "man":
                case "male":
                    cust.customerGender = "M";
                break;
                case "f":
                case "woman":
                case "female":
                    cust.customerGender = "F";
                break;
                default:
                    cust.customerGender = "X";
                break;
            }

            db.UserPassword.Add(up);
            db.Customer.Add(cust);
            try
            {
                db.SaveChanges();
            }
            catch(System.Data.Entity.Validation.DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    System.Diagnostics.Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
            }

            currentUser = cust.userID;
            currentCust = cust.customerID;

            return View("~/Views/Home/Index.cshtml");
        }

        public ActionResult LoggedIn()
        {
            if (!Verified())
                return RedirectToAction("Index");

            Account account = new Account();
            if (currentUser == "admin")
            {
                account.customer = null;
                return View(account);
            }

            account.customer = (from customer in db.Customer where customer.userID == currentUser select customer).FirstOrDefault();
            account.password = (from user in db.UserPassword where user.userID == currentUser select user.userEncryptedPassword).FirstOrDefault();
            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoggedIn(Account result)
        {
            System.Diagnostics.Debug.WriteLine("RIGHT HERE: " + result.customer.customerName);
            Customer cust = result.customer;
            switch (cust.customerGender.ToLower())
            {
                case "m":
                case "man":
                case "male":
                    cust.customerGender = "M";
                    break;
                case "f":
                case "woman":
                case "female":
                    cust.customerGender = "F";
                    break;
                default:
                    cust.customerGender = "X";
                    break;
            }

            db.Entry(cust).State = EntityState.Modified;
            db.SaveChanges();

            return View("~/Views/Home/Index.cshtml");
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
