using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocumentManageSystemForGateWay.Models
{
    public class FileUpload
    {

        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public int DocumentId { get; set; }
        public virtual Document Document { get; set; }
    }
}