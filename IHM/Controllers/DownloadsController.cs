using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IHM.Controllers
{
    public class DownloadsController : Controller
    {
        // GET: Download
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Download()
        {
            string path = Server.MapPath("~/Images");
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            FileInfo[] files = dirInfo.GetFiles("*.*");
            List<string> lst = new List<string>(files.Length);
            foreach (var item in files)
            {
                lst.Add(item.Name);
            }
            return View(lst);
        }
        public ActionResult DownloadFiles(string filename)
        {
            if (Path.GetExtension(filename) == ".png")
            {
                string fullpath = Path.Combine(Server.MapPath("~/Images"), filename);
                return File(fullpath, "Images/png");
            }
            else
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
        }
    }
}