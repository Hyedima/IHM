using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IHM.Models;

namespace IHM.Controllers
{
    public class WalletsController : Controller
    {
        IHMEntities db = new IHMEntities();
        // GET: Wallets
        public ActionResult Index()
        {
            return View(db.Wallets.ToList());
        }
        public ActionResult MyWallet(string ID)
        {
            var wallet = db.Wallets.FirstOrDefault(w => w.UserID == ID);
            return View(wallet);
        }
        public ActionResult Log(string ID)
        {
            var wallet = db.Wallets.FirstOrDefault(w => w.UserID == ID);
            return View(wallet);
        }
    }
}