//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SGPHOOKAPI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TrackingOrder
    {
        public long Id { get; set; }
        public Nullable<long> OrderId { get; set; }
        public Nullable<int> Status { get; set; }
        public string StatusName { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public string CurrentPost { get; set; }
        public string ProvinceId { get; set; }
        public string DistrictId { get; set; }
    }
}