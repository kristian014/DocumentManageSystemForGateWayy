using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DocumentManageSystemForGateWay.Models;
using IdentitySample.Models;

namespace DocumentManageSystemForGateWay.Controllers
{
    [Authorize(Roles = CustomRoles.Administrator + "," + CustomRoles.User)]
    public class DocumentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Documents
        public ActionResult Index(string category, string search)
        {
            var documents = db.Documents.Include(d => d.Category);

            if (!String.IsNullOrEmpty(search))
            {
                documents = documents.Where(p =>
                    p.Name.Contains(search) || p.Description.Contains(search) || p.Category.Name.Contains(search));
                ViewBag.Search = search;
               // model.Search = search;
            }

            var categories = documents.OrderBy(p => p.Category.Name).Select(p => p.Category.Name).Distinct();

            if (!String.IsNullOrEmpty(category))
            {
                documents = documents.Where(p => p.Category.Name == category);
                ViewBag.Categories = category;
            }
            ViewBag.Category = new SelectList(categories);

            return View(documents.ToList());
        }

        // GET: Documents/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Document document = db.Documents.Find(id);
            if (document == null)
            {
                return HttpNotFound();
            }
            return View(document);
        }

        // GET: Documents/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Documents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DocumentID,Name,SelectedStartDate,SelectedEndDate,Description,CategoryID")] Document document)
        {
            if (ModelState.IsValid)
            {
                List<FileUpload> fileUploads = new List<FileUpload>();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        FileUpload fileUpload = new FileUpload()
                        {
                            FileName = fileName,
                            Extension = Path.GetExtension(fileName),
                            Id = Guid.NewGuid()
                        };
                        fileUploads.Add(fileUpload);

                        var path = Path.Combine(Server.MapPath("~/Content/Upload/"), fileUpload.Id + fileUpload.Extension);
                        file.SaveAs(path);
                    }
                }

                document.FileUploads = fileUploads;
                db.Documents.Add(document);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryId", "Name", document.CategoryID);
            return View(document);
        }

        // GET: Documents/Edit/5
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Document document = db.Documents.Include(s => s.FileUploads).SingleOrDefault(x => x.DocumentID == id);
           // Document document = db.Documents.Find(id);
            if (document == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryId", "Name", document.CategoryID);
            return View(document);
        }

        // POST: Documents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DocumentID,Name,SelectedStartDate,SelectedEndDate,Description,CategoryID")] Document document)
        {
            if (ModelState.IsValid)
            {
                //New Files
              for (int i = 0; i < Request.Files.Count; i++)
              {
                  var file = Request.Files[i];
 
                  if (file != null && file.ContentLength > 0)
                  {
                      var fileName = Path.GetFileName(file.FileName);
                      FileUpload fileUpload = new FileUpload()
                        {
                          FileName = fileName,
                          Extension = Path.GetExtension(fileName),
                          Id = Guid.NewGuid(),
                          DocumentId = document.DocumentID
                      };
                      var path = Path.Combine(Server.MapPath("~/Content/Upload/"), fileUpload.Id + fileUpload.Extension);
                        file.SaveAs(path);
 
                      db.Entry(fileUpload).State = EntityState.Added;
                  }
              }
 
              db.Entry(document).State = EntityState.Modified;
              db.SaveChanges();
              return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryId", "Name", document.CategoryID);
            return View(document);
        }


        [HttpPost]
        public JsonResult DeleteFile(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { Result = "Error" });
            }
            try
            {
                Guid guid = new Guid(id);
                FileUpload fileUpload = db.FileUploads.Find(guid);

                if (fileUpload == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { Result = "Error" });
                }

                //Remove from database
                db.FileUploads.Remove(fileUpload);
                db.SaveChanges();

                //Delete file from the file system
                var path = Path.Combine(Server.MapPath("~/Content/Upload/"), fileUpload.Id + fileUpload.Extension);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }



        // GET: Documents/Delete/5
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Document document = db.Documents.Find(id);
            if (document == null)
            {
                return HttpNotFound();
            }
            return View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {

                Document document = db.Documents.Find(id);
                if (document == null)
                {
                    Response.StatusCode = (int) HttpStatusCode.NotFound;
                    return Json(new {Result = "Error"});
                }

                //delete files from the file system

                foreach (var item in document.FileUploads)
                {
                    String path = Path.Combine(Server.MapPath("~/Content/Upload/"), item.Id + item.Extension);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                db.Documents.Remove(document);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
            //return RedirectToAction("Index");
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
