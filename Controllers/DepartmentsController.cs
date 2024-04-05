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
    public class DepartmentsController : Controller
    {
        private static int? lastSort = -1;

        private Group17_iCLOTHINGDBEntities db = new Group17_iCLOTHINGDBEntities();

        public class CatalogInformation
        {
            public List<Department> departments { get; set; }
            public List<Category> categories { get; set; }
            public List<Product> products { get; set; }

            public List<Brand> brands { get; set; }            

            public CatalogInformation(List<Department> deps, List<Category> cats, List<Product> prods, List<Brand> brds)
            {
                departments = deps;
                categories = cats;
                products = prods;
                brands = brds;
            }
        }

        private List<Product> SortProducts(List<Product> products, int? sort)
        {
            List<Product> sortedProducts = new List<Product>();
            switch (sort)
            {
                case 1:
                    sortedProducts = (from prod in products
                                      join brand in db.Brand on prod.brandID equals brand.brandID
                                      orderby brand.brandName
                                      select prod).ToList();
                    break;
                default:
                    sortedProducts = products.OrderBy(o => o.productName).ToList();
                    sort = 0;
                    break;
            }
            if (lastSort == sort)
            {
                sortedProducts.Reverse();
                lastSort = -1;
            }
            else
            {
                lastSort = sort;
            }
            return sortedProducts;
        }

        // GET: Departments
        public ActionResult Index(int? sort)
        {
            if (sort == null)
                sort = 0;

            List<Product> sortedProducts = SortProducts(db.Product.ToList(), sort);            

            if (UserPasswordsController.CurrentUser() == "admin")
            {
                return RedirectToAction("Admin", new {srt = sort});
            }
            return View(new CatalogInformation(db.Department.ToList(), db.Category.ToList(), sortedProducts, db.Brand.ToList()));
        }

        public ActionResult Filter(String id, int? sort)
        {
            if (id == null)
            {
                return RedirectToAction("Index", sort);
            }

            if (sort == null)
                sort = -1;

            Category category = (from categ in db.Category where categ.categoryID == id select categ).FirstOrDefault();
            List<Category> cats = (from cat in db.Category where cat.parentID == id select cat).ToList();
            cats.Insert(0, category);

            List<String> catIDs = (from c in cats select c.categoryID).ToList();

            return View(new CatalogInformation(
                (from dep in db.Department where dep.departmentID == category.departmentID select dep).ToList(),
                cats,
                SortProducts((from prod in db.Product where catIDs.Contains(prod.categoryID) select prod).ToList(), sort),
                db.Brand.ToList()
            ));
        }

        public ActionResult Admin(int? srt)
        {
            if (srt == null)
                srt = -1;

            List<Product> sortedProducts = SortProducts(db.Product.ToList(), srt);
            if (UserPasswordsController.CurrentUser() != "admin")
            {
                return RedirectToAction("Index");
            }
            return View(new CatalogInformation(db.Department.ToList(), db.Category.ToList(), sortedProducts, db.Brand.ToList()));
        }

        // GET: Departments/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Departments/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "productID,productName,productDescription,productPrice,productQty,categoryID,brandID")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Product.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(product);
        }

        // GET: Departments/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Departments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "productID,productName,productDescription,productPrice,productQty,categoryID,brandID")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // GET: Departments/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Product product = db.Product.Find(id);
            db.Product.Remove(product);
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
