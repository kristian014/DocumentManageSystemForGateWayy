using IdentitySample.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace DocumentManageSystemForGateWay.Controllers
{
   
    public class DocumentdController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Documentd
        //public ActionResult Index()
        //{
        //    return View();
        //}

        public FileResult Download(String p, String d)
        {
            return File(Path.Combine(Server.MapPath("~/Content/Upload/"), p), System.Net.Mime.MediaTypeNames.Application.Octet, d);
        }

    }
}