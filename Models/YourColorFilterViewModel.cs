using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ecomm.Models
{
    public class YourColorFilterViewModel
    {
        public List<tblProduct> Products { get; set; }
        public string ColorFilter { get; set; }
        public List<string> ColorFilterArr { get; set; }
    }
}