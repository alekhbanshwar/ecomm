using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;


namespace ecomm.Models
{
    public class YourDbContext : DbContext
    {
        public DbSet<tblProduct> Products { get; set; }
        public DbSet<tblCategory> Categories { get; set; }
        public DbSet<tblProAttr> ProductsAttrs { get; set; }
        public DbSet<tblColor> Colors { get; set; }
    }
}