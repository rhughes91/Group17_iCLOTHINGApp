//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Group17_iCLOTHINGApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class OrderStatus
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public OrderStatus()
        {
            this.ShoppingCart = new HashSet<ShoppingCart>();
        }
    
        public string orderID { get; set; }
        public string status { get; set; }
        public System.DateTime statusDate { get; set; }
        public string adminID { get; set; }
    
        public virtual Administrator Administrator { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShoppingCart> ShoppingCart { get; set; }
    }
}
