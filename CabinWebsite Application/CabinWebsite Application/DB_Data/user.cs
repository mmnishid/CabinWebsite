//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CabinWebsite_Application.DB_Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class user
    {
        public user()
        {
            this.messages = new HashSet<message>();
            this.messages1 = new HashSet<message>();
            this.newsboards = new HashSet<newsboard>();
            this.reservations = new HashSet<reservation>();
        }
    
        public int userID { get; set; }
        public int vendorCode { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int status { get; set; }
        public Nullable<System.DateTime> lastLogin { get; set; }
        public Nullable<System.DateTime> prevLastLogin { get; set; }
    
        public virtual ICollection<message> messages { get; set; }
        public virtual ICollection<message> messages1 { get; set; }
        public virtual ICollection<newsboard> newsboards { get; set; }
        public virtual ICollection<reservation> reservations { get; set; }
    }
}
