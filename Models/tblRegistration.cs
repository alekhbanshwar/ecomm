//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ecomm.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblRegistration
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblRegistration()
        {
            this.tblOrders = new HashSet<tblOrder>();
        }
    
        public int id { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string profile { get; set; }
        public string MobileNumber { get; set; }
        public string Password { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Address { get; set; }
        public string UserType { get; set; }
        public Nullable<bool> isDel { get; set; }
        public Nullable<System.DateTime> addRegDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblOrder> tblOrders { get; set; }
    }
}
