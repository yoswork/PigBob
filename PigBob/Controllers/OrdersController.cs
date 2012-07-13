using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PigBob.Models;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Web.Security;
using RestSharp;

namespace PigBob.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private PigDB db = new PigDB();

        //
        // GET: /Orders/
        public ViewResult Index()
        {
            return View(db.Orders.OrderByDescending(o => o.OrderDate).Take(10).ToList());
        }

        //
        // GET: /Orders/Create
        [Authorize(Roles = "Administrator")]
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Orders/Create
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult Create(Order order)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingOrder = db.Orders.Count(o => o.OrderDate == order.OrderDate);
                    if (existingOrder > 0)
                    {
                        throw new Exception("An order with the same date already exists");
                    }
                    db.Orders.Add(order);
                    db.SaveChanges();
                    SendEmail(order);
                    return RedirectToAction("Index");
                }
                catch (SqlException sqlex)
                {
                    ModelState.AddModelError("OrderDate", "Woah, there, cowboy, SQL says - " +
                                                            sqlex.GetBaseException().GetType().ToString() + " - " +
                                                            sqlex.GetBaseException().Message);
                    return View();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("OrderDate", "Woah, there, cowboy - " +
                                                            ex.GetBaseException().GetType().ToString() + " - " +
                                                            ex.GetBaseException().Message);
                    return View();
                }
            }

            return View(order);
        }

        private void SendEmail(Order order)
        {
            //var client = new RestClient();
            //client.BaseUrl = "https://api.mailgun.net/v2";
            //client.Authenticator = new HttpBasicAuthenticator("api", "key-46@qffnszp0xztzms0");
            //var request = new RestRequest();
            //request.AddParameter("domain", "pigbob.mailgun.org", ParameterType.UrlSegment);
            //request.Resource = "{domain}/messages";
            //request.AddParameter("from", "Pigbob <pigbob@reedonline.co.uk>");
            //request.AddParameter("subject", "New Pig Order!");
            //var messageBody = "<html>Hello, a fellow PigBobber has created a new Pig Run for <strong>" + order.OrderDate.ToLongDateString() + "</strong><br/><br/>" +
            //    "Please click <a href=\"http://pigbob.azurewebsites.net/Sandwiches/Index/" + order.OrderID + "\">here</a> to place your order.";
            //request.AddParameter("html", messageBody);
            //request.Method = Method.POST;

            //request.AddParameter("to", "yosnoor@hotmail.com");

            //var result = client.Execute(request);

            
            
            MailMessage message = new MailMessage();
            message.From = new MailAddress("pigbob@reedonline.co.uk");
            message.Subject = "New Pig Order!";
            message.IsBodyHtml = true;
            message.Body = "Hello, a fellow PigBobber has created a new Pig Run for <strong>" + order.OrderDate.ToLongDateString() + "</strong><br/><br/>" +
                "Please click <a href=\"http://pigbob.azurewebsites.net/Sandwiches/Index/" + order.OrderID + "\">here</a> to place your order.";
            var members = Membership.GetAllUsers();
            foreach (MembershipUser member in members)
            {
                message.To.Add(member.Email);
            }
            //message.To.Add("yosnoor@hotmail.com");
            SmtpClient smtp = new SmtpClient();
            smtp.Send(message);
        }

        private static bool UrlValidationCallback(string redirectionUrl)
        {
            return true;
        }

        //
        // GET: /Orders/Edit/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int id)
        {
            Order order = db.Orders.Find(id);
            return View(order);
        }

        //
        // POST: /Orders/Edit/5
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(order);
        }

        //
        // GET: /Orders/Delete/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int id)
        {
            Order order = db.Orders.Find(id);
            return View(order);
        }

        //
        // POST: /Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Find(id);
            db.Orders.Remove(order);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}