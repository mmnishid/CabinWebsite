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
    
    public partial class reservation
    {
        public int reservationID { get; set; }
        public System.DateTime reservationDate { get; set; }
        public System.DateTime createdDate { get; set; }
        public int createdBy { get; set; }
    
        public virtual user user { get; set; }
    }
}