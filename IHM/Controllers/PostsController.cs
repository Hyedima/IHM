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

namespace IHM.Controllers
{
    public class PostsController : Controller
    {
        private IHMEntities db = new IHMEntities();

        // GET: Posts
        public ActionResult Index()
        {
            var posts = db.Posts.Include(p => p.Category).Include(p => p.UserAccount);
            return View(posts.ToList());
        }
        public ActionResult MyPosts(string ID)
        {
            var posts = db.Posts.Include(p => p.Category).Include(p => p.UserAccount).Where(p=>p.UserID == ID);
            return View(posts.ToList());
        }
        public ActionResult MediaPost(string ID)
        {
            var posts = db.Posts.FirstOrDefault(p => p.PostID == ID);
            return View(posts);
        }
        [HttpPost]
        public ActionResult MediaPost(FormCollection form, HttpPostedFileBase file)
        {
            string UserID = form["UserID"];
            string PostID = form["PostID"];
            var post = db.Posts.FirstOrDefault(p => p.PostID == PostID);

            if (file.ContentLength > 0)
            {
                //string _FileName = Path.GetFileName(file.FileName);
                string fileExtention = System.IO.Path.GetExtension(file.FileName);
                //creating filename to avoid file name conflicts.
                string fileName ="Ihypemedia_"+ UserID+"_"+ post.Title;
                //saving file in savedImage folder.
                //string savePath = savelocation + fileName + fileExtention;
                string _path = Path.Combine(Server.MapPath("~/Images/Posts/Media"), fileName + fileExtention);
                file.SaveAs(_path);
                post.Url = "/Images/Posts/Media" + fileName + fileExtention;
                db.SaveChanges();
                return RedirectToAction("Dashboard","Home");
            }
            else
            {
                return View(UserID);
            }
        }
        public ActionResult Post(string ID)
        {
            var posts = db.Posts.FirstOrDefault(p=>p.PostID == ID);
            return View(posts);
        }

        // GET: Posts/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // GET: Posts/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name");
            ViewBag.UserID = new SelectList(db.UserAccounts, "UserID", "FirstName");
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PostID,UserID,Artist,Title,Post1,CategoryID,Verified,Photo,PostDate")] Post post, HttpPostedFileBase file)
        {
            string UserID = Session["userid"].ToString();
            post.PostID = Setup.GenerateID.GetID();
            post.PostDate = DateTime.Now;
            post.Photo = "";
            post.UserID = UserID;
            post.Verified = false;

                if (file.ContentLength > 0)
                {
                    //string _FileName = Path.GetFileName(file.FileName);
                    string fileExtention = System.IO.Path.GetExtension(file.FileName);
                    //creating filename to avoid file name conflicts.
                    string fileName = UserID;
                    //saving file in savedImage folder.
                    //string savePath = savelocation + fileName + fileExtention;
                    string _path = Path.Combine(Server.MapPath("~/Images/Posts"), fileName + fileExtention);
                    file.SaveAs(_path);
                    post.Photo = "/Images/Posts/" + fileName + fileExtention;
                    db.Posts.Add(post);
                    db.SaveChanges();
                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    db.Posts.Add(post);
                    db.SaveChanges();
                    return RedirectToAction("Dashboard", "Home");
                }
        }

        // GET: Posts/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name", post.CategoryID);
            ViewBag.UserID = new SelectList(db.UserAccounts, "UserID", "FirstName", post.UserID);
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PostID,UserID,Artist,Title,Post1,CategoryID,Verified,Photo,PostDate")] Post post)
        {
            if (ModelState.IsValid)
            {
                db.Entry(post).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name", post.CategoryID);
            ViewBag.UserID = new SelectList(db.UserAccounts, "UserID", "FirstName", post.UserID);
            return View(post);
        }

        // GET: Posts/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Post post = db.Posts.Find(id);
            db.Posts.Remove(post);
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
