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
        private static String filter = "";

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
                case 2:
                    sortedProducts = products.OrderBy(o => o.productPrice).ToList();
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
            {
                filter = null;
                lastSort = -1;
                sort = 0;
            }

            List<Product> sortedProducts = SortProducts(db.Product.ToList(), sort);
            if (filter != null && filter != "")
            {
                sortedProducts = (from sp in sortedProducts where sp.productName.ToLower().Contains(filter) select sp).ToList();
            }

            if (UserPasswordsController.CurrentUser() == "admin")
            {
                lastSort = -1;
                return RedirectToAction("Admin", new {srt = sort});
            }
            return View(new CatalogInformation(db.Department.ToList(), db.Category.ToList(), sortedProducts, db.Brand.ToList()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(String search)
        {
            filter = search.ToLower();
            return Index(-1);
        }


        public ActionResult Filter(String id, int? sort)
        {
            if (UserPasswordsController.CurrentUser() == "admin")
            {
                lastSort = -1;
                return RedirectToAction("AdminFilter", new { srt = sort });
            }

            if (id == null)
            {
                return RedirectToAction("Index", sort);
            }

            if (sort == null)
            {
                filter = null;
                lastSort = -1;
                sort = -1;
            }

            Category category = (from categ in db.Category where categ.categoryID == id select categ).FirstOrDefault();
            List<Category> cats = (from cat in db.Category where cat.parentID == id select cat).ToList();
            cats.Insert(0, category);

            List<String> catIDs = (from c in cats select c.categoryID).ToList();

            List<Product> sortedProducts = (from prod in db.Product where catIDs.Contains(prod.categoryID) select prod).ToList();
            if (filter != null && filter != "")
            {
                sortedProducts = (from sp in sortedProducts where sp.productName.ToLower().Contains(filter) select sp).ToList();
            }
            return View(new CatalogInformation(
                (from dep in db.Department where dep.departmentID == category.departmentID select dep).ToList(),
                cats,
                SortProducts(sortedProducts, sort),
                db.Brand.ToList()
            ));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Filter(String id, String search)
        {
            filter = search.ToLower();
            return Filter(id, -1);
        }


        public ActionResult Admin(int? srt)
        {
            if (srt == null)
            {
                filter = null;
                lastSort = -1;
                srt = 0;
            }

            List<Product> sortedProducts = SortProducts(db.Product.ToList(), srt);
            if (filter != null && filter != "")
            {
                sortedProducts = (from sp in sortedProducts where sp.productName.ToLower().Contains(filter) select sp).ToList();
            }

            if (UserPasswordsController.CurrentUser() != "admin")
            {
                lastSort = -1;
                return RedirectToAction("Index", new { sort = srt });
            }
            return View(new CatalogInformation(db.Department.ToList(), db.Category.ToList(), sortedProducts, db.Brand.ToList()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Admin(String search)
        {
            filter = search.ToLower();
            return Admin(-1);
        }


        public ActionResult AdminFilter(String id, int? srt)
        {
            if (UserPasswordsController.CurrentUser() != "admin")
            {
                lastSort = -1;
                return RedirectToAction("Index", new { sort = srt });
            }

            if (id == null)
            {
                return RedirectToAction("Admin", srt);
            }

            if (srt == null)
            {
                filter = null;
                lastSort = -1;
                srt = -1;
            }

            Category category = (from categ in db.Category where categ.categoryID == id select categ).FirstOrDefault();
            List<Category> cats = (from cat in db.Category where cat.parentID == id select cat).ToList();
            cats.Insert(0, category);

            List<String> catIDs = (from c in cats select c.categoryID).ToList();

            List<Product> sortedProducts = SortProducts((from prod in db.Product where catIDs.Contains(prod.categoryID) select prod).ToList(), srt);
            if (filter != null && filter != "")
            {
                sortedProducts = (from sp in sortedProducts where sp.productName.ToLower().Contains(filter) select sp).ToList();
            }
            return View(new CatalogInformation(
                (from dep in db.Department where dep.departmentID == category.departmentID select dep).ToList(),
                cats,
                sortedProducts,
                db.Brand.ToList()
            ));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminFilter(String id, String search)
        {
            filter = search.ToLower();
            return AdminFilter(id, -1);
        }


        // GET: Departments/Create
        public ActionResult Create()
        {
            if (UserPasswordsController.CurrentUser() != "admin")
            {
                lastSort = -1;
                return RedirectToAction("Index");
            }
            return View();
        }

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
            if (UserPasswordsController.CurrentUser() != "admin")
            {
                lastSort = -1;
                return RedirectToAction("Index");
            }

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
            if (UserPasswordsController.CurrentUser() != "admin")
            {
                lastSort = -1;
                return RedirectToAction("Index");
            }

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
