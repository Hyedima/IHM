using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using IHM.Models;
using System.IO;
using Microsoft.AspNet.Membership.OpenAuth;
using System.Collections.Specialized;
using DotNetOpenAuth.GoogleOAuth2;
using System.Web.Security;


namespace IHM.Controllers
{
    public class UserAccountsController : Controller
    {
        private IHMEntities db = new IHMEntities();

        public ActionResult Index()
        {
            return View(db.UserAccounts.ToList());
        }

        [HttpPost]
        public ActionResult Register(FormCollection form, HttpPostedFileBase file)
        {
            string UserID = Setup.GenerateID.GetID();
            string Password = form["Password"];
            string EncPass = Setup.CryptoEngine.Encrypt(Password);
            var NewUser = new UserAccount
            {
                UserID = UserID,
                UserName = form["Email"],
                Password = EncPass,
                IsActive = false,
                UserType = "USER",
                DateRegistered = DateTime.Now
            };
            var NewWallet = new Wallet
            {
                WalletID = Setup.GenerateID.GetID(),
                UserID = UserID,
                Amount = 0,
                LastDepositeAmount = 0,
                LastDepositeDate = DateTime.Today
            };

            try
            {
                if (file.ContentLength > 0)
                {
                    //string _FileName = Path.GetFileName(file.FileName);
                    string fileExtention = System.IO.Path.GetExtension(file.FileName);
                    //creating filename to avoid file name conflicts.
                    string fileName = UserID;
                    //saving file in savedImage folder.
                    //string savePath = savelocation + fileName + fileExtention;
                    string _path = Path.Combine(Server.MapPath("~/Images/Users"), fileName + fileExtention);
                    file.SaveAs(_path);
                    NewUser.photo = "Images/Users/" + fileName + fileExtention;
                    db.UserAccounts.Add(NewUser);
                    db.Wallets.Add(NewWallet);
                    db.SaveChanges();
                    Session["userid"] = UserID;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    db.UserAccounts.Add(NewUser);
                    db.Wallets.Add(NewWallet);
                    db.SaveChanges();
                    Session["userid"] = UserID;
                    return RedirectToAction("Index", "Home");
                }
            }
            catch
            {
                ViewBag.Message = "File upload failed!!";
                //return View();
                //Session["userid"] = UserID;
                return RedirectToAction("Index", "Home");
            }

        }
        [HttpPost]
        public ActionResult Login(FormCollection form)
        {
            string UserName = form["Email"];
            string Password = form["Password"];
            string EncPass = Setup.CryptoEngine.Encrypt(Password);

            var User = db.UserAccounts.FirstOrDefault(p => p.UserName == UserName);
            if (User != null)
            {
                if (User.UserType == "ADMIN" || User.UserType == "Admin")
                {
                    Session["userid"] = User.UserID;
                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    Session["userid"] = User.UserID;
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        public ActionResult Logout()
        {
            Session["userid"] = null;
            return RedirectToAction("Index", "Home");
        }


        public ActionResult RedirectToGoogle()
        {
            string provider = "google";
            string returnUrl = "";
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
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
                OpenAuth.RequestAuthentication(Provider, ReturnUrl);
            }
        }
        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            string ProviderName = OpenAuth.GetProviderNameFromCurrentRequest();

            if (ProviderName == null || ProviderName == "")
            {
                NameValueCollection nvs = Request.QueryString;
                if (nvs.Count > 0)
                {
                    if (nvs["state"] != null)
                    {
                        NameValueCollection provideritem = HttpUtility.ParseQueryString(nvs["state"]);
                        if (provideritem["__provider__"] != null)
                        {
                            ProviderName = provideritem["__provider__"];
                        }
                    }
                }
            }

            GoogleOAuth2Client.RewriteRequest();

            var redirectUrl = Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl });
            var retUrl = returnUrl;
            var authResult = OpenAuth.VerifyAuthentication(redirectUrl);

            if (!authResult.IsSuccessful)
            {
                return Redirect(Url.Action("Index", "Home"));
            }

            // User has logged in with provider successfully
            // Check if user is already registered locally
            //You can call you user data access method to check and create users based on your model
            //if (OpenAuth.Login(authResult.Provider, authResult.ProviderUserId, createPersistentCookie: false))
            //{
            //    return Redirect(Url.Action("Index", "Home"));
            //}

            //Get provider user details
            string ProviderUserId = authResult.ProviderUserId;
            string ProviderUserName = authResult.UserName;

            string Email = null;
            if (Email == null && authResult.ExtraData.ContainsKey("email"))
            {
                Email = authResult.ExtraData["email"];
            }

            var User = db.UserAccounts.FirstOrDefault(p => p.UserName == Email && p.UserID == ProviderUserId);
            if (User == null)
            {
                //its a new user
                UserAccount NewUser = new UserAccount
                {
                    UserID = ProviderUserId,
                    UserName = Email,
                    Password = Setup.CryptoEngine.Encrypt("1234"),
                    IsActive = true,
                    UserType = "USER",
                    DateRegistered = DateTime.Now
                };
                db.UserAccounts.Add(NewUser);
                db.SaveChanges();
                Session["userid"] = ProviderUserId;

                return RedirectToAction("index", "Home");
            }
            else
            {
                Session["userid"] = ProviderUserId;
                return RedirectToAction("index", "Home");
            }

            //string Email = null;
            //if (Email == null && authResult.ExtraData.ContainsKey("email"))
            //{
            //    Email = authResult.ExtraData["email"];
            //}

            //if (User.Identity.IsAuthenticated)
            //{
            //    // User is already authenticated, add the external login and redirect to return url
            //    OpenAuth.AddAccountToExistingUser(ProviderName, ProviderUserId, ProviderUserName, User.Identity.Name);
            //    return Redirect(Url.Action("Index", "Home"));
            //}
            //else
            //{
            //    // User is new, save email as username
            //    string membershipUserName = Email ?? ProviderUserId;
            //    var createResult = OpenAuth.CreateUser(ProviderName, ProviderUserId, ProviderUserName, membershipUserName);

            //    if (!createResult.IsSuccessful)
            //    {
            //        ViewBag.Message = "User cannot be created";
            //        return View();
            //    }
            //    else
            //    {
            //        // User created
            //        if (OpenAuth.Login(ProviderName, ProviderUserId, createPersistentCookie: false))
            //        {
            //            return Redirect(Url.Action("Index", "Home"));
            //        }
            //    }
            //}
        }
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            //Call https://www.google.com/accounts/Logout if you want to logoff at provider
            return Redirect(Url.Action("Index", "Home"));
        }
        //Facebook Authentication

        // GET: UserAccounts/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserAccount userAccount = db.UserAccounts.Find(id);
            if (userAccount == null)
            {
                return HttpNotFound();
            }
            return View(userAccount);
        }

        // GET: UserAccounts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserID,FirstName,LastName,OtherNames,NickName,UserName,Password,Birthday,Address,About,IsActive,UserType,AccountType,photo,DateRegistered")] UserAccount userAccount)
        {
            if (ModelState.IsValid)
            {
                db.UserAccounts.Add(userAccount);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(userAccount);
        }

        // GET: UserAccounts/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserAccount userAccount = db.UserAccounts.Find(id);
            if (userAccount == null)
            {
                return HttpNotFound();
            }
            return View(userAccount);
        }

        // POST: UserAccounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserID,FirstName,LastName,OtherNames,NickName,UserName,Password,Birthday,Address,About,IsActive,UserType,AccountType,photo,DateRegistered")] UserAccount userAccount)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userAccount).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Dashboard", "Home");
            }
            return View(userAccount);
        }

        // GET: UserAccounts/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserAccount userAccount = db.UserAccounts.Find(id);
            if (userAccount == null)
            {
                return HttpNotFound();
            }
            return View(userAccount);
        }

        // POST: UserAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            UserAccount userAccount = db.UserAccounts.Find(id);
            db.UserAccounts.Remove(userAccount);
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
