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
    
    public partial class MM_CallPrice_getMatrix_test_Result
    {
        public string PriceMatrixID { get; set; }
        public string PostOfficeID { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> EffectDateBegin { get; set; }
        public string ZoneID { get; set; }
        public Nullable<System.DateTime> EffectDateEnd { get; set; }
        public Nullable<bool> ApplyAllServiceType { get; set; }
        public Nullable<bool> ApplyAllCustomer { get; set; }
        public string PriceMatrixType { get; set; }
        public string PriceType { get; set; }
        public string CurrencyID { get; set; }
        public Nullable<double> RoundOff { get; set; }
        public Nullable<int> RoundType { get; set; }
        public bool Multiply { get; set; }
        public double ExchangeRate { get; set; }
        public Nullable<double> NextValue { get; set; }
        public string CustomerID { get; set; }
        public string ServiceTypeID { get; set; }
        public bool IncludeService { get; set; }
        public string RangeWeightByCust { get; set; }
        public string DistanceGroupID { get; set; }
    }
}
