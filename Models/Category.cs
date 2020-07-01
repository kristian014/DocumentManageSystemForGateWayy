using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DocumentManageSystemForGateWay.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        public string Name { get; set; }

        public virtual ICollection<Document> Documents { get; set; }

    }
}