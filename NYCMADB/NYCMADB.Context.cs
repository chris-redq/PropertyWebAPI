﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NYCMADB
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class NYCMAEntities : DbContext
    {
        public NYCMAEntities()
            : base("name=NYCMAEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Assessment> Assessments { get; set; }
        public virtual DbSet<SaleParty> SaleParties { get; set; }
        public virtual DbSet<PricePerSqFtStatisticsByMonthByNeighborhoodMeanSmoothing> PricePerSqFtStatisticsByMonthByNeighborhoodMeanSmoothings { get; set; }
        public virtual DbSet<SalePriceStatisticsByMonthByNeighborhoodMeanSmoothing> SalePriceStatisticsByMonthByNeighborhoodMeanSmoothings { get; set; }
        public virtual DbSet<vwSalesByMonthByNeighborhood> vwSalesByMonthByNeighborhoods { get; set; }
        public virtual DbSet<CMAComparable> CMAComparables { get; set; }
        public virtual DbSet<CMARun> CMARuns { get; set; }
        public virtual DbSet<vwSuggestedSubjectPrice> vwSuggestedSubjectPrices { get; set; }
    
        public virtual int GetCMA(string algorithmType, string sbjBBLE, Nullable<int> maxRecords, Nullable<bool> sameNeighborhood, Nullable<bool> sameSchoolDistrict, Nullable<bool> sameZip, Nullable<bool> sameBlock, Nullable<bool> sameStreetName, Nullable<int> monthOffset, Nullable<double> minSalePrice, Nullable<double> maxSalePrice, Nullable<int> classMatchType, Nullable<bool> isNotIntraFamily, Nullable<bool> isSelleraCompany, Nullable<bool> isBuyeraCompany, Nullable<bool> isDebugMode)
        {
            var algorithmTypeParameter = algorithmType != null ?
                new ObjectParameter("AlgorithmType", algorithmType) :
                new ObjectParameter("AlgorithmType", typeof(string));
    
            var sbjBBLEParameter = sbjBBLE != null ?
                new ObjectParameter("sbjBBLE", sbjBBLE) :
                new ObjectParameter("sbjBBLE", typeof(string));
    
            var maxRecordsParameter = maxRecords.HasValue ?
                new ObjectParameter("MaxRecords", maxRecords) :
                new ObjectParameter("MaxRecords", typeof(int));
    
            var sameNeighborhoodParameter = sameNeighborhood.HasValue ?
                new ObjectParameter("SameNeighborhood", sameNeighborhood) :
                new ObjectParameter("SameNeighborhood", typeof(bool));
    
            var sameSchoolDistrictParameter = sameSchoolDistrict.HasValue ?
                new ObjectParameter("SameSchoolDistrict", sameSchoolDistrict) :
                new ObjectParameter("SameSchoolDistrict", typeof(bool));
    
            var sameZipParameter = sameZip.HasValue ?
                new ObjectParameter("SameZip", sameZip) :
                new ObjectParameter("SameZip", typeof(bool));
    
            var sameBlockParameter = sameBlock.HasValue ?
                new ObjectParameter("SameBlock", sameBlock) :
                new ObjectParameter("SameBlock", typeof(bool));
    
            var sameStreetNameParameter = sameStreetName.HasValue ?
                new ObjectParameter("SameStreetName", sameStreetName) :
                new ObjectParameter("SameStreetName", typeof(bool));
    
            var monthOffsetParameter = monthOffset.HasValue ?
                new ObjectParameter("MonthOffset", monthOffset) :
                new ObjectParameter("MonthOffset", typeof(int));
    
            var minSalePriceParameter = minSalePrice.HasValue ?
                new ObjectParameter("MinSalePrice", minSalePrice) :
                new ObjectParameter("MinSalePrice", typeof(double));
    
            var maxSalePriceParameter = maxSalePrice.HasValue ?
                new ObjectParameter("MaxSalePrice", maxSalePrice) :
                new ObjectParameter("MaxSalePrice", typeof(double));
    
            var classMatchTypeParameter = classMatchType.HasValue ?
                new ObjectParameter("ClassMatchType", classMatchType) :
                new ObjectParameter("ClassMatchType", typeof(int));
    
            var isNotIntraFamilyParameter = isNotIntraFamily.HasValue ?
                new ObjectParameter("IsNotIntraFamily", isNotIntraFamily) :
                new ObjectParameter("IsNotIntraFamily", typeof(bool));
    
            var isSelleraCompanyParameter = isSelleraCompany.HasValue ?
                new ObjectParameter("IsSelleraCompany", isSelleraCompany) :
                new ObjectParameter("IsSelleraCompany", typeof(bool));
    
            var isBuyeraCompanyParameter = isBuyeraCompany.HasValue ?
                new ObjectParameter("IsBuyeraCompany", isBuyeraCompany) :
                new ObjectParameter("IsBuyeraCompany", typeof(bool));
    
            var isDebugModeParameter = isDebugMode.HasValue ?
                new ObjectParameter("IsDebugMode", isDebugMode) :
                new ObjectParameter("IsDebugMode", typeof(bool));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("GetCMA", algorithmTypeParameter, sbjBBLEParameter, maxRecordsParameter, sameNeighborhoodParameter, sameSchoolDistrictParameter, sameZipParameter, sameBlockParameter, sameStreetNameParameter, monthOffsetParameter, minSalePriceParameter, maxSalePriceParameter, classMatchTypeParameter, isNotIntraFamilyParameter, isSelleraCompanyParameter, isBuyeraCompanyParameter, isDebugModeParameter);
        }
    
        public virtual ObjectResult<ShowCMASubject_Result> ShowCMASubject(string sbjBBLE)
        {
            var sbjBBLEParameter = sbjBBLE != null ?
                new ObjectParameter("sbjBBLE", sbjBBLE) :
                new ObjectParameter("sbjBBLE", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<ShowCMASubject_Result>("ShowCMASubject", sbjBBLEParameter);
        }
    
        [DbFunction("NYCMAEntities", "tfnGetCMA")]
        public virtual IQueryable<tfnGetCMA_Result> tfnGetCMA(string algorithmType, string sbjBBLE, Nullable<int> maxRecords, Nullable<bool> sameNeighborhood, Nullable<bool> sameSchoolDistrict, Nullable<bool> sameZip, Nullable<bool> sameBlock, Nullable<bool> sameStreetName, Nullable<int> monthOffset, Nullable<double> minSalePrice, Nullable<double> maxSalePrice, Nullable<int> classMatchType, Nullable<bool> isNotIntraFamily, Nullable<bool> isSelleraCompany, Nullable<bool> isBuyeraCompany, Nullable<int> minSimilarity)
        {
            var algorithmTypeParameter = algorithmType != null ?
                new ObjectParameter("AlgorithmType", algorithmType) :
                new ObjectParameter("AlgorithmType", typeof(string));
    
            var sbjBBLEParameter = sbjBBLE != null ?
                new ObjectParameter("sbjBBLE", sbjBBLE) :
                new ObjectParameter("sbjBBLE", typeof(string));
    
            var maxRecordsParameter = maxRecords.HasValue ?
                new ObjectParameter("MaxRecords", maxRecords) :
                new ObjectParameter("MaxRecords", typeof(int));
    
            var sameNeighborhoodParameter = sameNeighborhood.HasValue ?
                new ObjectParameter("SameNeighborhood", sameNeighborhood) :
                new ObjectParameter("SameNeighborhood", typeof(bool));
    
            var sameSchoolDistrictParameter = sameSchoolDistrict.HasValue ?
                new ObjectParameter("SameSchoolDistrict", sameSchoolDistrict) :
                new ObjectParameter("SameSchoolDistrict", typeof(bool));
    
            var sameZipParameter = sameZip.HasValue ?
                new ObjectParameter("SameZip", sameZip) :
                new ObjectParameter("SameZip", typeof(bool));
    
            var sameBlockParameter = sameBlock.HasValue ?
                new ObjectParameter("SameBlock", sameBlock) :
                new ObjectParameter("SameBlock", typeof(bool));
    
            var sameStreetNameParameter = sameStreetName.HasValue ?
                new ObjectParameter("SameStreetName", sameStreetName) :
                new ObjectParameter("SameStreetName", typeof(bool));
    
            var monthOffsetParameter = monthOffset.HasValue ?
                new ObjectParameter("MonthOffset", monthOffset) :
                new ObjectParameter("MonthOffset", typeof(int));
    
            var minSalePriceParameter = minSalePrice.HasValue ?
                new ObjectParameter("MinSalePrice", minSalePrice) :
                new ObjectParameter("MinSalePrice", typeof(double));
    
            var maxSalePriceParameter = maxSalePrice.HasValue ?
                new ObjectParameter("MaxSalePrice", maxSalePrice) :
                new ObjectParameter("MaxSalePrice", typeof(double));
    
            var classMatchTypeParameter = classMatchType.HasValue ?
                new ObjectParameter("ClassMatchType", classMatchType) :
                new ObjectParameter("ClassMatchType", typeof(int));
    
            var isNotIntraFamilyParameter = isNotIntraFamily.HasValue ?
                new ObjectParameter("IsNotIntraFamily", isNotIntraFamily) :
                new ObjectParameter("IsNotIntraFamily", typeof(bool));
    
            var isSelleraCompanyParameter = isSelleraCompany.HasValue ?
                new ObjectParameter("IsSelleraCompany", isSelleraCompany) :
                new ObjectParameter("IsSelleraCompany", typeof(bool));
    
            var isBuyeraCompanyParameter = isBuyeraCompany.HasValue ?
                new ObjectParameter("IsBuyeraCompany", isBuyeraCompany) :
                new ObjectParameter("IsBuyeraCompany", typeof(bool));
    
            var minSimilarityParameter = minSimilarity.HasValue ?
                new ObjectParameter("MinSimilarity", minSimilarity) :
                new ObjectParameter("MinSimilarity", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<tfnGetCMA_Result>("[NYCMAEntities].[tfnGetCMA](@AlgorithmType, @sbjBBLE, @MaxRecords, @SameNeighborhood, @SameSchoolDistrict, @SameZip, @SameBlock, @SameStreetName, @MonthOffset, @MinSalePrice, @MaxSalePrice, @ClassMatchType, @IsNotIntraFamily, @IsSelleraCompany, @IsBuyeraCompany, @MinSimilarity)", algorithmTypeParameter, sbjBBLEParameter, maxRecordsParameter, sameNeighborhoodParameter, sameSchoolDistrictParameter, sameZipParameter, sameBlockParameter, sameStreetNameParameter, monthOffsetParameter, minSalePriceParameter, maxSalePriceParameter, classMatchTypeParameter, isNotIntraFamilyParameter, isSelleraCompanyParameter, isBuyeraCompanyParameter, minSimilarityParameter);
        }
    
        [DbFunction("NYCMAEntities", "tfnGetManualCMA")]
        public virtual IQueryable<tfnGetManualCMA_Result> tfnGetManualCMA(string sbjBBLE, Nullable<int> minSimilarity, Nullable<bool> sameNeighborhood, Nullable<bool> sameSchoolDistrict, Nullable<bool> sameZip, Nullable<bool> sameBlock, Nullable<bool> sameStreetName, Nullable<int> monthOffset, Nullable<double> minSalePrice, Nullable<double> maxSalePrice, Nullable<int> classMatchType, Nullable<bool> isNotIntraFamily, Nullable<bool> isSelleraCompany, Nullable<bool> isBuyeraCompany, Nullable<double> distanceInMiles, Nullable<int> gLAHiRange, Nullable<int> gLALoRange, Nullable<int> lAHiRange, Nullable<int> lALoRange, Nullable<int> buildingFrontageHiRange, Nullable<int> buildingFrontageLoRange, Nullable<int> buildingDepthHiRange, Nullable<int> buildingDepthLoRange, Nullable<int> lotFrontageHiRange, Nullable<int> lotFrontageLoRange, Nullable<int> lotDepthHiRange, Nullable<int> lotDepthLoRange)
        {
            var sbjBBLEParameter = sbjBBLE != null ?
                new ObjectParameter("sbjBBLE", sbjBBLE) :
                new ObjectParameter("sbjBBLE", typeof(string));
    
            var minSimilarityParameter = minSimilarity.HasValue ?
                new ObjectParameter("MinSimilarity", minSimilarity) :
                new ObjectParameter("MinSimilarity", typeof(int));
    
            var sameNeighborhoodParameter = sameNeighborhood.HasValue ?
                new ObjectParameter("SameNeighborhood", sameNeighborhood) :
                new ObjectParameter("SameNeighborhood", typeof(bool));
    
            var sameSchoolDistrictParameter = sameSchoolDistrict.HasValue ?
                new ObjectParameter("SameSchoolDistrict", sameSchoolDistrict) :
                new ObjectParameter("SameSchoolDistrict", typeof(bool));
    
            var sameZipParameter = sameZip.HasValue ?
                new ObjectParameter("SameZip", sameZip) :
                new ObjectParameter("SameZip", typeof(bool));
    
            var sameBlockParameter = sameBlock.HasValue ?
                new ObjectParameter("SameBlock", sameBlock) :
                new ObjectParameter("SameBlock", typeof(bool));
    
            var sameStreetNameParameter = sameStreetName.HasValue ?
                new ObjectParameter("SameStreetName", sameStreetName) :
                new ObjectParameter("SameStreetName", typeof(bool));
    
            var monthOffsetParameter = monthOffset.HasValue ?
                new ObjectParameter("MonthOffset", monthOffset) :
                new ObjectParameter("MonthOffset", typeof(int));
    
            var minSalePriceParameter = minSalePrice.HasValue ?
                new ObjectParameter("MinSalePrice", minSalePrice) :
                new ObjectParameter("MinSalePrice", typeof(double));
    
            var maxSalePriceParameter = maxSalePrice.HasValue ?
                new ObjectParameter("MaxSalePrice", maxSalePrice) :
                new ObjectParameter("MaxSalePrice", typeof(double));
    
            var classMatchTypeParameter = classMatchType.HasValue ?
                new ObjectParameter("ClassMatchType", classMatchType) :
                new ObjectParameter("ClassMatchType", typeof(int));
    
            var isNotIntraFamilyParameter = isNotIntraFamily.HasValue ?
                new ObjectParameter("IsNotIntraFamily", isNotIntraFamily) :
                new ObjectParameter("IsNotIntraFamily", typeof(bool));
    
            var isSelleraCompanyParameter = isSelleraCompany.HasValue ?
                new ObjectParameter("IsSelleraCompany", isSelleraCompany) :
                new ObjectParameter("IsSelleraCompany", typeof(bool));
    
            var isBuyeraCompanyParameter = isBuyeraCompany.HasValue ?
                new ObjectParameter("IsBuyeraCompany", isBuyeraCompany) :
                new ObjectParameter("IsBuyeraCompany", typeof(bool));
    
            var distanceInMilesParameter = distanceInMiles.HasValue ?
                new ObjectParameter("DistanceInMiles", distanceInMiles) :
                new ObjectParameter("DistanceInMiles", typeof(double));
    
            var gLAHiRangeParameter = gLAHiRange.HasValue ?
                new ObjectParameter("GLAHiRange", gLAHiRange) :
                new ObjectParameter("GLAHiRange", typeof(int));
    
            var gLALoRangeParameter = gLALoRange.HasValue ?
                new ObjectParameter("GLALoRange", gLALoRange) :
                new ObjectParameter("GLALoRange", typeof(int));
    
            var lAHiRangeParameter = lAHiRange.HasValue ?
                new ObjectParameter("LAHiRange", lAHiRange) :
                new ObjectParameter("LAHiRange", typeof(int));
    
            var lALoRangeParameter = lALoRange.HasValue ?
                new ObjectParameter("LALoRange", lALoRange) :
                new ObjectParameter("LALoRange", typeof(int));
    
            var buildingFrontageHiRangeParameter = buildingFrontageHiRange.HasValue ?
                new ObjectParameter("BuildingFrontageHiRange", buildingFrontageHiRange) :
                new ObjectParameter("BuildingFrontageHiRange", typeof(int));
    
            var buildingFrontageLoRangeParameter = buildingFrontageLoRange.HasValue ?
                new ObjectParameter("BuildingFrontageLoRange", buildingFrontageLoRange) :
                new ObjectParameter("BuildingFrontageLoRange", typeof(int));
    
            var buildingDepthHiRangeParameter = buildingDepthHiRange.HasValue ?
                new ObjectParameter("BuildingDepthHiRange", buildingDepthHiRange) :
                new ObjectParameter("BuildingDepthHiRange", typeof(int));
    
            var buildingDepthLoRangeParameter = buildingDepthLoRange.HasValue ?
                new ObjectParameter("BuildingDepthLoRange", buildingDepthLoRange) :
                new ObjectParameter("BuildingDepthLoRange", typeof(int));
    
            var lotFrontageHiRangeParameter = lotFrontageHiRange.HasValue ?
                new ObjectParameter("LotFrontageHiRange", lotFrontageHiRange) :
                new ObjectParameter("LotFrontageHiRange", typeof(int));
    
            var lotFrontageLoRangeParameter = lotFrontageLoRange.HasValue ?
                new ObjectParameter("LotFrontageLoRange", lotFrontageLoRange) :
                new ObjectParameter("LotFrontageLoRange", typeof(int));
    
            var lotDepthHiRangeParameter = lotDepthHiRange.HasValue ?
                new ObjectParameter("LotDepthHiRange", lotDepthHiRange) :
                new ObjectParameter("LotDepthHiRange", typeof(int));
    
            var lotDepthLoRangeParameter = lotDepthLoRange.HasValue ?
                new ObjectParameter("LotDepthLoRange", lotDepthLoRange) :
                new ObjectParameter("LotDepthLoRange", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<tfnGetManualCMA_Result>("[NYCMAEntities].[tfnGetManualCMA](@sbjBBLE, @MinSimilarity, @SameNeighborhood, @SameSchoolDistrict, @SameZip, @SameBlock, @SameStreetName, @MonthOffset, @MinSalePrice, @MaxSalePrice, @ClassMatchType, @IsNotIntraFamily, @IsSelleraCompany, @IsBuyeraCompany, @DistanceInMiles, @GLAHiRange, @GLALoRange, @LAHiRange, @LALoRange, @BuildingFrontageHiRange, @BuildingFrontageLoRange, @BuildingDepthHiRange, @BuildingDepthLoRange, @LotFrontageHiRange, @LotFrontageLoRange, @LotDepthHiRange, @LotDepthLoRange)", sbjBBLEParameter, minSimilarityParameter, sameNeighborhoodParameter, sameSchoolDistrictParameter, sameZipParameter, sameBlockParameter, sameStreetNameParameter, monthOffsetParameter, minSalePriceParameter, maxSalePriceParameter, classMatchTypeParameter, isNotIntraFamilyParameter, isSelleraCompanyParameter, isBuyeraCompanyParameter, distanceInMilesParameter, gLAHiRangeParameter, gLALoRangeParameter, lAHiRangeParameter, lALoRangeParameter, buildingFrontageHiRangeParameter, buildingFrontageLoRangeParameter, buildingDepthHiRangeParameter, buildingDepthLoRangeParameter, lotFrontageHiRangeParameter, lotFrontageLoRangeParameter, lotDepthHiRangeParameter, lotDepthLoRangeParameter);
        }
    }
}
