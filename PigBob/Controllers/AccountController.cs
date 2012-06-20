using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;
using PigBob.Models;

namespace PigBob.Controllers
{
    public class AccountController : Controller
    {

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserName, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Index
        [Authorize]
        public ActionResult Index()
        {
            var user = Membership.GetUser(User.Identity.Name);
            var profile = UserProfile.GetUserProfile(User.Identity.Name);
            var reg = new RegisterModel();

            reg.UserName = user.UserName;
            reg.Email = user.Email;
            reg.FirstName = profile.FirstName;
            reg.LastName = profile.LastName;

            return View(reg);
        }

        //
        // POST: /Account/Index
        [Authorize]
        [HttpPost]
        public ActionResult Index(RegisterModel model)
        {
            var user = Membership.GetUser(User.Identity.Name);
            user.Email = model.UserName;
            Membership.UpdateUser(user);

            var profile = UserProfile.GetUserProfile(User.Identity.Name);
            profile.FirstName = model.FirstName;
            profile.LastName = model.LastName;
            profile.Save();

            return View(model);
        }

        //
        // GET: /Account/LogOff
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                Membership.CreateUser(model.UserName, model.Password, model.Email, passwordQuestion: null, passwordAnswer: null, isApproved: true, providerUserKey: null, status: out createStatus);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, createPersistentCookie: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, userIsOnline: true);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePasswordSuccess
        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        private IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }

        //
        // GET: /Account/Delete
        [Authorize]
        public ActionResult Delete()
        {
            return View();
        }

        //
        // POST: /Account/Delete
        [Authorize]
        [HttpPost]
        public ActionResult Delete(string username)
        {
            FormsAuthentication.SignOut();
            if (Membership.DeleteUser(User.Identity.Name, true))
            {
                return RedirectToAction("DeleteSuccess");
            }
            else
            {
                ModelState.AddModelError("Message", "There was a problem deleting your account, please contact your pig administrator.");
                ViewBag.Message = "There was a problem deleting your account, please contact your pig administrator.";
                return View();
            }
        }

        //
        // GET: /Account/DeleteSuccess
        public ActionResult DeleteSuccess()
        {
            return View();
        }

        //
        // GET, POST: /Account/OpenIdLogOn
        [AcceptVerbs(HttpVerbs.Post | HttpVerbs.Get), ValidateInput(false)]
        public ActionResult OpenIdLogOn(string returnUrl)
        {
            var openid = new OpenIdRelyingParty();
            var response = openid.GetResponse();

            if (response == null)  // Initial operation
            {
                try
                {
                    var req = openid.CreateRequest("https://www.google.com/accounts/o8/id");
                    var fields = new ClaimsRequest();
                    var fetch = new FetchRequest();
                    fetch.Attributes.AddRequired(WellKnownAttributes.Contact.Email);
                    fetch.Attributes.AddRequired(WellKnownAttributes.Name.First);
                    fetch.Attributes.AddRequired(WellKnownAttributes.Name.Last);
                    fields.Email = DemandLevel.Require;
                    req.AddExtension(fetch);
                    return req.RedirectingResponse.AsActionResult();
                }
                catch (ProtocolException ex)
                {
                    // display error by showing original LogOn view
                    ModelState.AddModelError("", "Unable to authenticate: " + ex.Message);
                    return View("Login");
                }
            }
            else  // OpenId redirection callback
            {
                // Step 2: OpenID Provider sending assertion response
                switch (response.Status)
                {
                    case AuthenticationStatus.Authenticated:
                        string identifier = response.ClaimedIdentifier;
                        //                        var fetch = response.GetExtension<ClaimsResponse>();
                        //                        var email = fetch.Email;
                        var fetch = response.GetExtension<FetchResponse>();
                        var email = fetch.GetAttributeValue(WellKnownAttributes.Contact.Email);

                        //                        var users = Membership.FindUsersByEmail(email);
                        var user = Membership.GetUser(email);

                        // OpenId lookup fails - Id doesn't exist for login - login first)
                        if (user != null)
                        {
                            FormsAuthentication.SetAuthCookie(user.UserName, false);
                            var profile = UserProfile.GetUserProfile(user.UserName);
                            if (profile.FirstName == null || profile.FirstName == string.Empty)
                            {
                                profile.FirstName = fetch.GetAttributeValue(WellKnownAttributes.Name.First);
                                profile.LastName = fetch.GetAttributeValue(WellKnownAttributes.Name.Last);
                                profile.Save();
                            }
                        }
                        else
                        {
                            // User not found create a new user
                            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@£$%&";
                            var passwordChars = new char[8];
                            var rand = new Random();
                            var newUser = new RegisterModel
                            {
                                Email = email,
                                UserName = email
                            };
                            newUser.FirstName = fetch.GetAttributeValue(WellKnownAttributes.Name.First);
                            newUser.LastName = fetch.GetAttributeValue(WellKnownAttributes.Name.Last);
                            for (var i = 0; i < 8; i++)
                            {
                                passwordChars[i] = allowedChars[rand.Next(0, allowedChars.Length)];
                            }
                            newUser.Password = new string(passwordChars);
                            newUser.ConfirmPassword = newUser.Password;

                            //Create the user and sign in
                            MembershipCreateStatus createStatus;
                            Membership.CreateUser(newUser.UserName, newUser.Password, newUser.Email, null, null, true, null, out createStatus);

                            if (createStatus == MembershipCreateStatus.Success)
                            {
                                FormsAuthentication.SetAuthCookie(newUser.UserName, false /* createPersistentCookie */);

                                var profile = UserProfile.GetUserProfile(newUser.UserName);
                                profile.FirstName = newUser.FirstName;
                                profile.LastName = newUser.LastName;
                                profile.Save();
                            }
                            else
                            {
                                ModelState.AddModelError("", ErrorCodeToString(createStatus));
                                return View("Register");
                            }

                        }

                        if (!string.IsNullOrEmpty(returnUrl))
                            return Redirect(returnUrl);

                        return RedirectToAction("Index", "Home");

                    case AuthenticationStatus.Canceled:
                        ModelState.AddModelError("", "Authentication cancelled at google");
                        return View("Login");
                    case AuthenticationStatus.Failed:
                        ModelState.AddModelError("", "Authentication FAILED");
                        return View("Login");
                }
            }
            return new EmptyResult();
        }

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
