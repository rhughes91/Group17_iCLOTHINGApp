using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Group17_iCLOTHINGApp.Models;

namespace Group17_iCLOTHINGApp.Controllers
{
    public class OrderStatusController : Controller
    {
        private Group17_iCLOTHINGDBEntities db = new Group17_iCLOTHINGDBEntities();

        private static readonly Random random = new Random();
        public int GenerateUniqueOrderID()
        {
            int id = random.Next(10000, 100000);

            // Generate a random number within the range of 10000 to 99999
            while (db.OrderStatus.Find(id.ToString()) != null) id = random.Next(10000, 100000);
            return id;
        }

        public int GenerateUniqueEmailID()
        {
            int id = random.Next(10000, 100000);

            // Generate a random number within the range of 10000 to 99999
            while (db.Email.Find(id.ToString()) != null) id = random.Next(10000, 100000);
            return id;
        }

        public int GenerateUniqueStickerID()
        {
            int id = random.Next(10000, 100000);

            // Generate a random number within the range of 10000 to 99999
            while (db.ItemDelivery.Find(id.ToString()) != null) id = random.Next(10000, 100000);
            return id;
        }


        // GET: OrderStatus
        public ActionResult Index()
        {
            var orderStatus = db.OrderStatus.Include(o => o.ShoppingCart);
            return View(orderStatus.ToList());
        }

        // GET: OrderStatus/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderStatus orderStatus = db.OrderStatus.Find(id);
            if (orderStatus == null)
            {
                return HttpNotFound();
            }
            return View(orderStatus);
        }

        // GET: OrderStatus/Create
        public ActionResult Create()
        {
            ViewBag.adminID = new SelectList(db.Administrator, "adminID", "adminName");
            ViewBag.cartID = new SelectList(db.ShoppingCart, "cartID", "productID");
            return View();
        }

        // POST: OrderStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "orderID,status,statusDate,adminID")] OrderStatus orderStatus)
        {
            if (ModelState.IsValid)
            {
                String orderID = GenerateUniqueOrderID().ToString();


                var shoppingCart = db.ShoppingCart.Include(s => s.Customer).Include(s => s.Product);

                String userID = UserPasswordsController.CurrentUser();
                String custID = UserPasswordsController.CurrentCustomer();

                bool canComplete = true;
                bool isEmpty = true;

                foreach (var item in db.ShoppingCart)
                {
                    if (item.OrderID == null && item.customerID == custID)
                    {
                        isEmpty = false;

                        string productID = item.productID;
                        if (db.Product.Find(productID).productQty < item.productQuantity)
                        {
                            //handle last second out of stock error message
                            ViewBag.ErrorMessage = "Error purchasing item number " + productID + ". ";

                            canComplete = false;
                        }
                    }
                }

                if (isEmpty)
                {
                    //handle cart empty error
                    ViewBag.ErrorMessage = "Error: Cart is empty";

                    canComplete = false;
                }

                if (!canComplete) return View(orderStatus);


                orderStatus.adminID = null;
                orderStatus.status = "Confirmed";
                orderStatus.orderID = orderID;
                orderStatus.statusDate = DateTime.Now;

                foreach (var item in db.ShoppingCart)
                {
                    if (item.OrderID == null && item.customerID == custID)
                    {
                        item.OrderID = orderID;

                        string productID = item.productID;

                        int updatedItemQty = db.Product.Find(productID).productQty - item.productQuantity;

                        db.Product.Find(productID).productQty = updatedItemQty;

                        if (updatedItemQty <= 5)
                        {
                            Email validationEmail = new Email();
                            validationEmail.emailNo = GenerateUniqueEmailID().ToString();
                            validationEmail.emailDate = DateTime.Now;
                            validationEmail.emailBody = "Product number " + item.productID + " has " + updatedItemQty + " in stock!";
                            validationEmail.emailSubject = "Product Stock Low";
                            validationEmail.customerID = custID;
                            validationEmail.adminID = "administrator0";

                            db.Email.Add(validationEmail);
                        }
                    }
                }

                db.OrderStatus.Add(orderStatus);
                db.SaveChanges();

                return RedirectToAction("Index", "Home");
            }

            ViewBag.adminID = new SelectList(db.Administrator, "adminID", "adminName", orderStatus.adminID);
            return View(orderStatus);
        }

        // GET: OrderStatus/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderStatus orderStatus = db.OrderStatus.Find(id);
            if (orderStatus == null)
            {
                return HttpNotFound();
            }
            return View(orderStatus);
        }

        // POST: OrderStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            OrderStatus orderStatus = db.OrderStatus.Find(id);
            db.OrderStatus.Remove(orderStatus);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: OrderStatus/Edit/5
        public ActionResult Validate(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderStatus orderStatus = db.OrderStatus.Find(id);
            if (orderStatus == null)
            {
                return HttpNotFound();
            }

            orderStatus.status = "Validated";
            orderStatus.adminID = "administrator0";

            Email validationEmail = new Email();
            validationEmail.emailNo = GenerateUniqueEmailID().ToString();
            validationEmail.emailDate = DateTime.Now;
            validationEmail.emailBody = "Your order number " + orderStatus.orderID + " has been validated!";
            validationEmail.emailSubject = "Order Confirmation";
            validationEmail.customerID = db.ShoppingCart.FirstOrDefault(c => c.OrderID == orderStatus.orderID)?.customerID;//Very roundabout way to find it
            validationEmail.adminID = "administrator0";

            db.Email.Add(validationEmail);


            foreach (var item in db.ShoppingCart)
            {
                if (item.OrderID == orderStatus.orderID)
                {
                    ItemDelivery itemDelivery = new ItemDelivery();
                    itemDelivery.stickerID = GenerateUniqueStickerID().ToString();
                    itemDelivery.cartID = item.cartID;
                    itemDelivery.stickerDate = DateTime.Now;

                    db.ItemDelivery.Add(itemDelivery);
                }
            }

            db.SaveChanges();

            ViewBag.adminID = new SelectList(db.Administrator, "adminID", "adminName", orderStatus.adminID);
            return View(orderStatus);
        }

        // POST: OrderStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Validate([Bind(Include = "orderID,status,statusDate,adminID")] OrderStatus orderStatus)
        {
            if (ModelState.IsValid)
            {
                orderStatus.status = "Validated";
                orderStatus.adminID = "administrator0";

                Email validationEmail = new Email();
                validationEmail.emailNo = GenerateUniqueEmailID().ToString();
                validationEmail.emailDate = DateTime.Now;
                validationEmail.emailBody = "Your order number " + orderStatus.orderID + " has been validated!";
                validationEmail.emailSubject = "Order Confirmation";

                validationEmail.customerID = db.ShoppingCart.FirstOrDefault(c => c.OrderID == orderStatus.orderID)?.customerID;//Very roundabout way to find it "Jaered"
                validationEmail.adminID = "administrator0";

                db.Email.Add(validationEmail);

                foreach (var item in db.ShoppingCart)
                {
                    if (item.OrderID == orderStatus.orderID)
                    {
                        ItemDelivery itemDelivery = new ItemDelivery();
                        itemDelivery.stickerID = GenerateUniqueStickerID().ToString();
                        itemDelivery.cartID = item.cartID;
                        itemDelivery.stickerDate = DateTime.Now;

                        db.ItemDelivery.Add(itemDelivery);
                    }
                }

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.adminID = new SelectList(db.Administrator, "adminID", "adminName", orderStatus.adminID);
            return View(orderStatus);
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
