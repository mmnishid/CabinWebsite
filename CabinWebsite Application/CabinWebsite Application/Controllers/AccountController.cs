using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using CabinWebsite_Application.Filters;
using CabinWebsite_Application.Models;
using CabinWebsite_Application.DB_Data;
using System.Collections;
using System.Text.RegularExpressions;

namespace CabinWebsite_Application.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
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

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {


                //return RedirectToLocal(returnUrl);
                return RedirectToAction("Overview");
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();
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

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                /*
                location location;
                List<int> existingVendorCodes = new List<int>();
                // Check if signup code is valid.
                using (var db = new CabinWebsiteEntities())
                {
                    //existingVendorCodes = db.users.Where(u => u.vendorCode != -1).Select(u => u.vendorCode).ToList();
                    
                    var keyCheck = db.locations.Where(s => s.signupKey.Equals(model.Token));
                    location = keyCheck.FirstOrDefault();
                    if (keyCheck.Any() == false)
                    {
                        ModelState.AddModelError("", "Invalid signup code.");
                        return View(model);
                    }
                    
                }
                 
                */

                // Attempt to register the user
                try
                {
                    string sanitizedun = Regex.Replace(model.UserName, @"\s+", "");
                    WebSecurity.CreateUserAndAccount(
                        sanitizedun, 
                        model.Password,
                        new
                        {
                            firstName = model.FirstName,
                            lastName = model.LastName,
                            email = model.Email,
                            status = 1,
                        });
                    WebSecurity.Login(sanitizedun, model.Password);
                    return RedirectToAction("Overview", "Account");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", User.Identity.Name));
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /*
        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "User name already exists. Please enter a different user name.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }
        */


        // GET: /Account/Overview
        // Overview of transactions for the current user
        public ActionResult Overview()
        {
            
            CabinOverviewModel model = new CabinOverviewModel();
            using (var db = new CabinWebsiteEntities())
            {
                
                int userId = WebSecurity.CurrentUserId;
                model.messages = db.messages.Select(t => new MessageModel
                {/*
                    messageID = t.messageID,
                    userID = t.userID,
                    date = t.date,
                    title = t.title,
                    body = t.body,*/
                }).OrderByDescending(t => t.date).ToList();

            }
            return View(model);
           
            //return View();
        }




        // GET: /Account/Create
        // Create a new transaction whether producer or consumer. Just returns the view
        public ActionResult AddMessage()
        {
            NewMessageModel model = new NewMessageModel();
            return View(model);
        }


        // POST: /Account/Create
        // Create a new transaction. Needs to take a model that matchs the form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMessage(NewMessageModel model)
        {
            /*
            using (var db = new CabinWebsiteEntities())
            {
                
                message newMess = new message();
                newMess.messageID = Guid.NewGuid();
                newMess.userID = WebSecurity.CurrentUserId;
                newMess.date = DateTime.Now;
                newMess.title = model.title;
                newMess.body = model.body;
                db.messages.Add(newMess);
                db.SaveChanges();
                
            }*/
            return RedirectToAction("Overview");
        }
         /**/
        /*
        public ActionResult Tasks()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult AddTask()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }
         * *
        /*
        public ActionResult AddTask(int model)
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }
        */

        //
        //
        public ActionResult Calendar()
        {
            CalendarModel model = new CalendarModel();

            using (var db = new CabinWebsiteEntities())
            {/*
                int userId = WebSecurity.CurrentUserId;
                string myFirstName = db.users.Where(i => i.userID == userId).Select(u => u.firstname).FirstOrDefault();
                string myLastName = db.users.Where(i => i.userID == userId).Select(u => u.lastname).FirstOrDefault();
                model.myReservedDates = db.calendars.Where(
                    a => 
                    a.userID == userId
                ).Select(t => new ReservedModel
                {
                    calendarID = t.calendarID,
                    userID = t.userID,
                    userFirstName = myFirstName,
                    userLastName = myLastName,
                    dateAdded = t.dateAdded,
                    reservedDate = t.reservedDate,
                }).OrderByDescending(t => t.reservedDate).ToList();

                model.otherReservedDates = db.calendars.Where(
                    a =>
                    a.userID != userId
                ).Select(t => new ReservedModel
                {
                    calendarID = t.calendarID,
                    userID = t.userID,
                    userFirstName = db.users.Where(i => i.userID == t.userID).Select(u => u.firstname).FirstOrDefault(),
                    userLastName = db.users.Where(i => i.userID == t.userID).Select(u => u.lastname).FirstOrDefault(),
                    dateAdded = t.dateAdded,
                    reservedDate = t.reservedDate,
                }).OrderByDescending(t => t.reservedDate).ToList();*/
            }
            return View(model);
        }

        //
        //
        public ActionResult AddDate()
        {
            //ReservedModel dateReserved = new ReservedModel();
            NewReservedModel newReservation = new NewReservedModel();
            /*
            using (var db = new CabinWebsiteEntities())
            {
                dateReserved.userID = WebSecurity.CurrentUserId;

                int year = 2015; //Int32.Parse(id.Substring(0, 4));
                int month = 10;  //Int32.Parse(id.Substring(4, 2));
                int day = 1; //Int32.Parse(id.Substring(6, 2));
                dateReserved.reservedDate = new DateTime(year, month, day);
                dateReserved.dateAdded = DateTime.Now.Date;
            }
            */
            return View(newReservation);
        }

        //
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDate(NewReservedModel m)
        {
            if (ModelState.IsValid)
            {
                /*
                using (var db = new CabinWebsiteEntities())
                {
                    calendar newCal = new calendar();
                    newCal.calendarID = Guid.NewGuid();
                    newCal.userID = WebSecurity.CurrentUserId;
                    newCal.reservedDate = new DateTime(2015, 10, 1);
                    newCal.dateAdded = DateTime.Now;
                }*/
                return RedirectToAction("Calendar");
            }
            else
            {
                return View();
            }
        }
/*
        public ActionResult Rules()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
 */

        public ActionResult GetInfo()
        {
            return View();
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

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
