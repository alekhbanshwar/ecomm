using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ecomm.Models
{
    public class CategoryViewModel
    {
        public int CatId { get; set; }
        public string CatName { get; set; }
        public List<SubCategoryViewModel> Subcategories { get; set; }
    }
}