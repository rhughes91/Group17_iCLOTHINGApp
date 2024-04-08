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
    public class ShoppingCartsController : Controller
    {
        private Group17_iCLOTHINGDBEntities db = new Group17_iCLOTHINGDBEntities();

        private static readonly Random random = new Random();
        public int GenerateUniqueCartID()
        {
            int id = random.Next(10000, 100000);

            // Generate a random number within the range of 10000 to 99999
            while (db.ShoppingCart.Find(id.ToString()) != null) id = random.Next(10000, 100000);
            return id;
        }



        // GET: ShoppingCarts
        public ActionResult Index()
        {
            if (UserPasswordsController.Verified())//If the user is logged in
            {
                var shoppingCart = db.ShoppingCart.Include(s => s.Customer).Include(s => s.Product);//show their stuff
                return View(shoppingCart.ToList());
            }
            else
            {
                return View("~/Views/Home/Index.cshtml");
            }
        }

        // GET: ShoppingCarts/Create
        public ActionResult Create(string id)
        {
            ViewBag.customerID = new SelectList(db.Customer, "customerID", "customerName");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.product = product;
            return View();
        }

        // POST: ShoppingCarts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "cartID,productPrice,productQuantity,productID,customerID")] ShoppingCart shoppingCart)
        {
            if (ModelState.IsValid)
            {
                if (!UserPasswordsController.Verified()) return RedirectToAction("Index", "UserPasswords");

                String userID = UserPasswordsController.CurrentUser();
                shoppingCart.customerID = db.Customer.FirstOrDefault(c => c.userID == userID)?.customerID;
                shoppingCart.productPrice = db.Product.Find(shoppingCart.productID).productPrice;
                shoppingCart.cartID = GenerateUniqueCartID().ToString();
                shoppingCart.OrderID = null;

                int productQuantity = shoppingCart.productQuantity;
                if (db.Product.Find(shoppingCart.productID).productQty < productQuantity)
                {
                    ViewBag.ErrorMessage = "Not enough of this item in stock";
                }
                else
                {
                    db.ShoppingCart.Add(shoppingCart);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            ViewBag.customerID = new SelectList(db.Customer, "customerID", "customerName", shoppingCart.customerID);
            ViewBag.productID = new SelectList(db.Product, "productID", "productName", shoppingCart.productID);
            return View(shoppingCart);
        }

        // GET: ShoppingCarts/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShoppingCart shoppingCart = db.ShoppingCart.Find(id);
            if (shoppingCart == null)
            {
                return HttpNotFound();
            }
            ViewBag.customerID = new SelectList(db.Customer, "customerID", "customerName", shoppingCart.customerID);
            ViewBag.productID = new SelectList(db.Product, "productID", "productName", shoppingCart.productID);
            return View(shoppingCart);
        }

        // POST: ShoppingCarts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "cartID,productPrice,productQuantity,productID,customerID")] ShoppingCart shoppingCart)
        {
            if (ModelState.IsValid)
            {
                int productQuantity = shoppingCart.productQuantity;
                if (db.Product.Find(shoppingCart.productID).productQty < productQuantity)
                {
                    ViewBag.ErrorMessage = "Not enough of this item in stock";
                }
                else
                {
                    db.Entry(shoppingCart).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            ViewBag.customerID = new SelectList(db.Customer, "customerID", "customerName", shoppingCart.customerID);
            ViewBag.productID = new SelectList(db.Product, "productID", "productName", shoppingCart.productID);
            return View(shoppingCart);
        }

        // GET: ShoppingCarts/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShoppingCart shoppingCart = db.ShoppingCart.Find(id);
            if (shoppingCart == null)
            {
                return HttpNotFound();
            }
            return View(shoppingCart);
        }

        // POST: ShoppingCarts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ShoppingCart shoppingCart = db.ShoppingCart.Find(id);
            db.ShoppingCart.Remove(shoppingCart);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Checkout()
        {
            return View();
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
