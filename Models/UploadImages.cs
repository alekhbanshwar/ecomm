using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ecomm.Models
{
    public class UploadImages
    {
        public int proId { get; set; }
        public HttpPostedFileBase[] files { get; set; }
        public int[] colorId { get; set; }
        public int qty { get; set; }
        public string[] size { get; set; }

    }
}

