using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DocumentManageSystemForGateWay.Models;

namespace DocumentManageSystemForGateWay.ViewModels
{
    public class DocumentViewModel
    {
        [Key]
        public int DocumentID { get; set; }

        [Required(ErrorMessage = "Please Enter Your Company Name")]
        [Display(Name = "Company Name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? SelectedStartDate { get; set; }


        [Required]
        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? SelectedEndDate { get; set; }


        [Required(ErrorMessage = "Please Enter Contract Description")]
        [Display(Name = "Contract Description")]
        [MaxLength(500)]
        public string Description { get; set; }

        public int? CategoryID { get; set; }

        public virtual Category Category { get; set; }

       // public virtual ICollection<FileUpload> FileUploads { get; set; }
        // public string Search { get; set; }

        public string Categories { get; set; }

        public IQueryable<Document> Documents { get; set; }
        public string Search { get; set; }

       // public virtual ICollection<FileUpload> FileUploads { get; set; }


    }
}