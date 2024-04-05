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


        // GET: OrderStatus
        public ActionResult Index()
        {
            var orderStatus = db.OrderStatus.Include(o => o.Administrator).Include(o => o.ShoppingCart);
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
        public ActionResult Create([Bind(Include = "orderID,status,statusDate,cartID,adminID")] OrderStatus orderStatus)
        {
            if (ModelState.IsValid)
            {
                orderStatus.adminID = "admin";
                orderStatus.status = "Confirmed";
                orderStatus.orderID = GenerateUniqueOrderID().ToString();

                var shoppingCart = db.ShoppingCart.Include(s => s.Customer).Include(s => s.Product);
                String curCartID = shoppingCart.ToList().First().cartID;

                orderStatus.cartID = curCartID;
                orderStatus.statusDate = DateTime.Now.AddDays(14);

                db.OrderStatus.Add(orderStatus);

                foreach (var item in db.ShoppingCart)
                {
                    string productID = item.productID;
                    db.Product.Find(productID).productQty -= item.productQuantity;
                    //db.ShoppingCart.Remove(item);
                }

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.adminID = new SelectList(db.Administrator, "adminID", "adminName", orderStatus.adminID);
            ViewBag.cartID = new SelectList(db.ShoppingCart, "cartID", "productID", orderStatus.cartID);
            return View(orderStatus);
        }

        // GET: OrderStatus/Edit/5
        public ActionResult Edit(string id)
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
            ViewBag.adminID = new SelectList(db.Administrator, "adminID", "adminName", orderStatus.adminID);
            ViewBag.cartID = new SelectList(db.ShoppingCart, "cartID", "productID", orderStatus.cartID);
            return View(orderStatus);
        }

        // POST: OrderStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "orderID,status,statusDate,cartID,adminID")] OrderStatus orderStatus)
        {
            if (ModelState.IsValid)
            {
                db.Entry(orderStatus).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.adminID = new SelectList(db.Administrator, "adminID", "adminName", orderStatus.adminID);
            ViewBag.cartID = new SelectList(db.ShoppingCart, "cartID", "productID", orderStatus.cartID);
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
