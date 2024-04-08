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
    public class UserCommentsController : Controller
    {
        private Group17_iCLOTHINGDBEntities db = new Group17_iCLOTHINGDBEntities();

        private static readonly Random random = new Random();
        public int GenerateUniqueQueryID()
        {
            int id = random.Next(10000, 100000);

            // Generate a random number within the range of 10000 to 99999
            while (db.ShoppingCart.Find(id.ToString()) != null) id = random.Next(10000, 100000);
            return id;
        }


        // GET: UserQueries
        public ActionResult Index()
        {
            var userComment = db.UserComment.Include(u => u.Customer);
            return View(userComment.ToList());
        }

        // GET: UserQueries/Create
        public ActionResult Create()
        {
            ViewBag.searchID = new SelectList(db.Customer, "customerID", "customerName");
            return View();
        }

        // POST: UserQueries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "commentNo,commentDate,commentDescription,userID")] UserComment userComment)
        {
            if (!UserPasswordsController.Verified())
            {
                ViewBag.ErrorMessage = "You must be logged in to submit feedback!";
            }
            else
            {
                String userID = UserPasswordsController.CurrentUser();

                if (userID == "admin")
                {
                    ViewBag.ErrorMessage = "Administrators cannot submit feedback!";
                }
                else
                {
                    String custID = db.Customer.FirstOrDefault(c => c.userID == userID)?.customerID;

                    if (ModelState.IsValid)
                    {
                        userComment.userID = custID;
                        userComment.commentNo = GenerateUniqueQueryID().ToString();
                        userComment.commentDate = DateTime.Now;

                        db.UserComment.Add(userComment);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
            }

            ViewBag.searchID = new SelectList(db.Customer, "customerID", "customerName", userComment.userID);
            return View(userComment);
        }

        // GET: UserQueries/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserComment userComment = db.UserComment.Find(id);
            if (userComment == null)
            {
                return HttpNotFound();
            }
            return View(userComment);
        }

        // POST: UserQueries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            UserComment userComment = db.UserComment.Find(id);
            db.UserComment.Remove(userComment);
            db.SaveChanges();
            return RedirectToAction("Index");
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
