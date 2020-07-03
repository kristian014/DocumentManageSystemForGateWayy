using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DocumentManageSystemForGateWay.Models;
using DocumentManageSystemForGateWay.ViewModels;
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
            DocumentViewModel model = new DocumentViewModel();
            var documents = db.Documents.Include(d => d.Category);

            if (!String.IsNullOrEmpty(search))
            {
                documents = documents.Where(p =>
                    p.Name.Contains(search) || p.Description.Contains(search) || p.Category.Name.Contains(search));
                model.Search = search;
               // model.Search = search;
            }

            var categories = documents.OrderBy(p => p.Category.Name).Select(p => p.Category.Name).Distinct();

            if (!String.IsNullOrEmpty(category))
            {
                documents = documents.Where(p => p.Category.Name == category);
                model.Categories = category;
            }
            ViewBag.Category = new SelectList(categories);
            model.Documents = documents;
            return View(model);
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
        public ActionResult Create([Bind(Include = "DocumentID,Name,SelectedStartDate,SelectedEndDate,Description,CategoryID")] Document document, HttpPostedFileBase files)
        {

            bool allValid = true;
            string inValidFiles = "";

            //if (ModelState.IsValid)
            //{
            //New Files
            List<FileUpload> fileUploads = new List<FileUpload>();
            //for (int i = 0; i < Request.Files.Count; i++)
            //{
            //var files = Request.Files[i];

            if (files != null)
            {
                if (files.ContentLength > 0)
                {

                    if (!ValidateFile(files))
                    {
                        allValid = false;
                        inValidFiles += ", " + files.FileName;
                    }



                    if (allValid)
                    {

                        try
                        {
                            //SaveFileToDisk(file);
                            var fileName = Path.GetFileName(files.FileName);
                            FileUpload fileUpload = new FileUpload()
                            {
                                FileName = fileName,
                                Extension = Path.GetExtension(fileName),
                                Id = Guid.NewGuid()
                            };
                            fileUploads.Add(fileUpload);


                            var path = Path.Combine(Server.MapPath(Constant.Documentpath),
                                fileUpload.Id + fileUpload.Extension);
                            files.SaveAs(path);

                        }
                        catch (Exception)
                        {
                            ModelState.AddModelError("FileName",
                                "Sorry an error occurred saving the files to disk, please try again");
                        }


                    }

                    //else add an error listing out the invalid files            
                    else
                    {
                        ModelState.AddModelError("FileName",
                            "All files must be pdf, doc and less than 2MB in size.The following files" +
                            inValidFiles + " are not valid");
                    }

                }

                //the user has entered more than 10 files
                else
                {
                    ModelState.AddModelError("FileName", "Please only upload up to ten files at a time");
                }


            }
            else
            {
                //if the user has not entered a file return an error message
                ModelState.AddModelError("FileName", "Please choose a file");
            }


            if (ModelState.IsValid)
            {
                bool duplicates = false;
                bool otherDbError = false;
                string duplicateFiles = "";

                var Document = new FileUpload { FileName = files.FileName };
                try
                {
                    document.FileUploads = fileUploads;
                    db.Documents.Add(document);
                    db.SaveChanges();

                }
                catch (DbUpdateException ex)
                {
                    SqlException innerException = ex.InnerException.InnerException as SqlException;
                    if (innerException != null && innerException.Number == 2601)
                    {
                        duplicateFiles += ", " + files.FileName;
                        duplicates = true;
                        db.Entry(document).State = EntityState.Detached;
                    }
                    else
                    {
                        otherDbError = true;
                    }
                }


                if (duplicates)
                {
                    ModelState.AddModelError("FileName",
                        "All files uploaded except the files" + duplicateFiles +
                        ", which already exist in the system." +
                        " Please delete them and try again if you wish to re-add them");
                    return View();
                }

                if (otherDbError)
                {
                    ModelState.AddModelError("FileName",
                        "Sorry an error has occurred saving to the database, please try again");
                    return View();
                }

                ViewBag.CategoryID = new SelectList(db.Categories, "CategoryId", "Name", document.CategoryID);
                return RedirectToAction("Index");

            }

            //}


            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryId", "Name", document.CategoryID);
            return View();


            // return View(document);

        }



























        //bool allValid = true;
        //string inValidFiles = "";

        //if (ModelState.IsValid)
        //{
        //    List<FileUpload> fileUploads = new List<FileUpload>();
        //    for (int i = 0; i < Request.Files.Count; i++)
        //    {

        //        var file = Request.Files[i];

        //        if (file != null && file.ContentLength > 0)
        //        {
        //            var fileName = Path.GetFileName(file.FileName);
        //            FileUpload fileUpload = new FileUpload()
        //            {
        //                FileName = fileName,
        //                Extension = Path.GetExtension(fileName),
        //                Id = Guid.NewGuid()
        //            };
        //            fileUploads.Add(fileUpload);

        //            var path = Path.Combine(Server.MapPath("~/Content/Upload/"), fileUpload.Id + fileUpload.Extension);
        //            file.SaveAs(path);
        //        }
        //    }

        //    document.FileUploads = fileUploads;
        //    db.Documents.Add(document);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        //ViewBag.CategoryID = new SelectList(db.Categories, "CategoryId", "Name", document.CategoryID);
        //return View(document);
    

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
                      var path = Path.Combine(Server.MapPath(Constant.Documentpath), fileUpload.Id + fileUpload.Extension);
                        file.SaveAs(path);
 
                      db.Entry(fileUpload).State = EntityState.Modified;
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
                var path = Path.Combine(Server.MapPath(Constant.Documentpath), fileUpload.Id + fileUpload.Extension);
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

                
                String path = Path.Combine(Server.MapPath(Constant.Documentpath), Convert.ToString(id) + "/");
                //if (System.IO.File.Exists(path))
                //{
                    System.IO.File.Delete(path);
              //  }
          

                db.Documents.Remove(document);
                db.SaveChanges();

               // var dirPath = Path.Combine(Directory.GetCurrentDirectory(), Constant.Documentpath + Convert.ToString(id) + "/");

               // Directory.Delete(dirPath, true);

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

        private bool ValidateFile(HttpPostedFileBase file)
        {
            string fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();
            string[] allowedFileTypes = {  ".pdf" , "doc" , "docx" };
            if ((file.ContentLength > 0 && file.ContentLength < 2097152) && allowedFileTypes.Contains(fileExtension))
            {
                return true;
            }
            return false;
        }

    }
}
