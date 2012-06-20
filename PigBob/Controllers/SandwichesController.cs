using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PigBob.Models;

namespace PigBob.Controllers
{ 
    [Authorize]
    public class SandwichesController : Controller
    {
        private PigDB db = new PigDB();

        //
        // GET: /Sandwiches/
        public ActionResult Index(int id)
        {
            ViewBag.UserId = Membership.GetUser().ProviderUserKey.ToString();
            ViewBag.OrderId = id;
            var order = db.Orders.Find(id);

            if (order == null)
            {
                return RedirectToAction("Error");
            }

            ViewBag.OrderDate = db.Orders.Find(id).OrderDate;
            var orderitems = db.OrderItems
                .Where(o => o.OrderID == id)
                .Include(o => o.Order);
            ViewBag.Sausage = db.OrderItems
                .Where(o => o.OrderID == id && o.Sausage && o.Egg == false && o.Bacon == false && (o.Other == null || o.Other.Trim().Length == 0))
                .Count();
            ViewBag.Egg = db.OrderItems
                .Where(o => o.OrderID == id && o.Egg && o.Sausage == false && o.Bacon == false && (o.Other == null || o.Other.Trim().Length == 0))
                .Count();
            ViewBag.Bacon = db.OrderItems
                .Where(o => o.OrderID == id && o.Bacon && o.Egg == false && o.Sausage == false && (o.Other == null || o.Other.Trim().Length == 0))
                .Count();
            ViewBag.SausageEgg = db.OrderItems
                .Where(o => o.OrderID == id && o.Sausage && o.Egg && o.Bacon == false && (o.Other == null || o.Other.Trim().Length == 0))
                .Count();
            ViewBag.BaconEgg = db.OrderItems
                .Where(o => o.OrderID == id && o.Sausage == false && o.Egg && o.Bacon && (o.Other == null || o.Other.Trim().Length == 0))
                .Count();
            ViewBag.SausageBacon = db.OrderItems
                .Where(o => o.OrderID == id && o.Sausage && o.Egg == false && o.Bacon && (o.Other == null || o.Other.Trim().Length == 0))
                .Count();
            ViewBag.SausageBaconEgg = db.OrderItems
                .Where(o => o.OrderID == id && o.Sausage && o.Egg && o.Bacon && (o.Other == null || o.Other.Trim().Length == 0))
                .Count();
            ViewBag.Other = db.OrderItems
                .Where(o => o.OrderID == id && o.Other != null && o.Other.Length > 0)
                .Count();
            ViewBag.Total = db.OrderItems
                .Where(o => o.OrderID == id)
                .Count();
            if (ViewBag.Other > 0)
            {
                GetOtherItems(id);
            }
            return View(orderitems.ToList());
        }

        public ViewResult Error()
        {
            return View();
        }

        private void GetOtherItems(int id)
        {
            var otherItems = db.OrderItems.Where(o => o.OrderID == id && o.Other != null && o.Other.Length > 0);
            List<string> otherItemStrings = new List<string>();
            string itemString = string.Empty;

            foreach (var item in otherItems)
            {
                itemString = (item.Bacon ? "Bacon and " : "");
                itemString += (item.Sausage ? "Sausage and " : "");
                itemString += (item.Egg ? "Egg and " : "");
                itemString += item.Other;

                otherItemStrings.Add(itemString);
            }

            ViewBag.OtherItems = otherItemStrings;
        }

        //
        // GET: /Sandwiches/Details/5
        public ActionResult Details(int id)
        {
            ViewBag.UserId = Membership.GetUser().ProviderUserKey.ToString();
            OrderItem orderitem = db.OrderItems.Find(id);
            if (orderitem == null)
            {
                return RedirectToAction("Error");
            }
            return View(orderitem);
        }

        //
        // GET: /Sandwiches/Create
        public ActionResult Create(int id)
        {
            var sandwich = new OrderItem();
            sandwich.OrderID = id;
            if (db.Orders.Find(id) == null)
            {
                return RedirectToAction("Error");
            }
            ViewBag.OrderDate = db.Orders.Find(id).OrderDate;
            if (ViewBag.OrderDate < DateTime.Now.AddHours(-8))
            {
                return RedirectToAction("Error");
            }
            return View(sandwich);
        }

        [HttpPost]
        public void Add()
        {
            var bodyText = Request["body-plain"].ToLower();
        }

        //
        // POST: /Sandwiches/Create
        [HttpPost]
        public ActionResult Create(OrderItem orderitem)
        {
            if (ModelState.IsValid)
            {
                var user = Membership.GetUser();
                var eater = new PigEater();
                var profile = UserProfile.GetUserProfile();
                eater.UserId = user.ProviderUserKey.ToString();
                eater.FirstName = profile.FirstName;
                eater.LastName = profile.LastName;
                orderitem.Eater = eater;
                if (user.UserName.ToLower().Contains("joe"))
                {
                    orderitem.Other = null;
                }
                db.OrderItems.Add(orderitem);
                db.SaveChanges();
                return RedirectToAction("Index", new { id=orderitem.OrderID});
            }

            ViewBag.OrderID = new SelectList(db.Orders, "OrderID", "OrderID", orderitem.OrderID);
            return View(orderitem);
        }
        
        //
        // GET: /Sandwiches/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.UserId = Membership.GetUser().ProviderUserKey.ToString();
            OrderItem orderitem = db.OrderItems.Find(id);
            if (orderitem == null)
            {
                return RedirectToAction("Error");
            }
            ViewBag.OrderID = new SelectList(db.Orders, "OrderID", "OrderID", orderitem.OrderID);
            return View(orderitem);
        }

        //
        // POST: /Sandwiches/Edit/5
        [HttpPost]
        public ActionResult Edit(OrderItem orderitem)
        {
            if (ModelState.IsValid)
            {
                var user = Membership.GetUser();
                var eater = new PigEater();
                var profile = UserProfile.GetUserProfile();
                eater.UserId = user.ProviderUserKey.ToString();
                eater.FirstName = profile.FirstName;
                eater.LastName = profile.LastName;
                orderitem.Eater = eater;
                if (user.UserName.ToLower().Contains("joe"))
                {
                    orderitem.Other = null;
                }
                db.Entry(orderitem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new {id = orderitem.OrderID});
            }
            ViewBag.OrderID = new SelectList(db.Orders, "OrderID", "OrderID", orderitem.OrderID);
            return View(orderitem);
        }

        //
        // GET: /Sandwiches/Delete/5
        public ActionResult Delete(int id)
        {
            var user = Membership.GetUser();
            ViewBag.UserId = user.ProviderUserKey.ToString();
            OrderItem orderitem = db.OrderItems.Find(id);
            if (orderitem == null)
            {
                return RedirectToAction("Error");
            }
            ViewBag.OrderId = orderitem.OrderID;
            return View(orderitem);
        }

        //
        // POST: /Sandwiches/Delete/5
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            OrderItem orderitem = db.OrderItems.Find(id);
            int orderId = orderitem.OrderID;
            db.OrderItems.Remove(orderitem);
            db.SaveChanges();
            return RedirectToAction("Index", new {id = orderId});
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}