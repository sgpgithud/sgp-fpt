﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class PMSSGPDBEntities : DbContext
    {
        public PMSSGPDBEntities()
            : base("name=PMSSGPDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<MM_Distances> MM_Distances { get; set; }
    
        public virtual ObjectResult<MM_CallPrice_getMatrix_test_Result> MM_CallPrice_getMatrix_test(Nullable<System.DateTime> documentDate, string postOfficeID, string serviceTypeID, string customerID, string provinceID, Nullable<int> distanceID, string priceMatrixType, string priceType, string zoneID)
        {
            var documentDateParameter = documentDate.HasValue ?
                new ObjectParameter("DocumentDate", documentDate) :
                new ObjectParameter("DocumentDate", typeof(System.DateTime));
    
            var postOfficeIDParameter = postOfficeID != null ?
                new ObjectParameter("PostOfficeID", postOfficeID) :
                new ObjectParameter("PostOfficeID", typeof(string));
    
            var serviceTypeIDParameter = serviceTypeID != null ?
                new ObjectParameter("ServiceTypeID", serviceTypeID) :
                new ObjectParameter("ServiceTypeID", typeof(string));
    
            var customerIDParameter = customerID != null ?
                new ObjectParameter("CustomerID", customerID) :
                new ObjectParameter("CustomerID", typeof(string));
    
            var provinceIDParameter = provinceID != null ?
                new ObjectParameter("ProvinceID", provinceID) :
                new ObjectParameter("ProvinceID", typeof(string));
    
            var distanceIDParameter = distanceID.HasValue ?
                new ObjectParameter("DistanceID", distanceID) :
                new ObjectParameter("DistanceID", typeof(int));
    
            var priceMatrixTypeParameter = priceMatrixType != null ?
                new ObjectParameter("PriceMatrixType", priceMatrixType) :
                new ObjectParameter("PriceMatrixType", typeof(string));
    
            var priceTypeParameter = priceType != null ?
                new ObjectParameter("PriceType", priceType) :
                new ObjectParameter("PriceType", typeof(string));
    
            var zoneIDParameter = zoneID != null ?
                new ObjectParameter("ZoneID", zoneID) :
                new ObjectParameter("ZoneID", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<MM_CallPrice_getMatrix_test_Result>("MM_CallPrice_getMatrix_test", documentDateParameter, postOfficeIDParameter, serviceTypeIDParameter, customerIDParameter, provinceIDParameter, distanceIDParameter, priceMatrixTypeParameter, priceTypeParameter, zoneIDParameter);
        }
    
        public virtual ObjectResult<MM_CallPrice_Test_Result> MM_CallPrice_Test(string priceMatrixID, Nullable<System.DateTime> documentDate, string postOfficeID, string serviceTypeID, string customerID, string provinceID, Nullable<int> distanceID, string priceMatrixType, string priceType, string zoneID)
        {
            var priceMatrixIDParameter = priceMatrixID != null ?
                new ObjectParameter("PriceMatrixID", priceMatrixID) :
                new ObjectParameter("PriceMatrixID", typeof(string));
    
            var documentDateParameter = documentDate.HasValue ?
                new ObjectParameter("DocumentDate", documentDate) :
                new ObjectParameter("DocumentDate", typeof(System.DateTime));
    
            var postOfficeIDParameter = postOfficeID != null ?
                new ObjectParameter("PostOfficeID", postOfficeID) :
                new ObjectParameter("PostOfficeID", typeof(string));
    
            var serviceTypeIDParameter = serviceTypeID != null ?
                new ObjectParameter("ServiceTypeID", serviceTypeID) :
                new ObjectParameter("ServiceTypeID", typeof(string));
    
            var customerIDParameter = customerID != null ?
                new ObjectParameter("CustomerID", customerID) :
                new ObjectParameter("CustomerID", typeof(string));
    
            var provinceIDParameter = provinceID != null ?
                new ObjectParameter("ProvinceID", provinceID) :
                new ObjectParameter("ProvinceID", typeof(string));
    
            var distanceIDParameter = distanceID.HasValue ?
                new ObjectParameter("DistanceID", distanceID) :
                new ObjectParameter("DistanceID", typeof(int));
    
            var priceMatrixTypeParameter = priceMatrixType != null ?
                new ObjectParameter("PriceMatrixType", priceMatrixType) :
                new ObjectParameter("PriceMatrixType", typeof(string));
    
            var priceTypeParameter = priceType != null ?
                new ObjectParameter("PriceType", priceType) :
                new ObjectParameter("PriceType", typeof(string));
    
            var zoneIDParameter = zoneID != null ?
                new ObjectParameter("ZoneID", zoneID) :
                new ObjectParameter("ZoneID", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<MM_CallPrice_Test_Result>("MM_CallPrice_Test", priceMatrixIDParameter, documentDateParameter, postOfficeIDParameter, serviceTypeIDParameter, customerIDParameter, provinceIDParameter, distanceIDParameter, priceMatrixTypeParameter, priceTypeParameter, zoneIDParameter);
        }
    }
}
