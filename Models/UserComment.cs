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
    
    public partial class UserComment
    {
        public string commentNo { get; set; }
        public System.DateTime commentDate { get; set; }
        public string commentDescription { get; set; }
        public string userID { get; set; }
    
        public virtual Customer Customer { get; set; }
    }
}
