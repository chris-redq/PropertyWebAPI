//-----------------------------------------------------------------------
// <copyright file="cma.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using NYCMADB;
    using AutoMapper;
    using System.Data.Entity.Validation;

    public class SuggestedPropertyPrices: vwSuggestedSubjectPrice
    {

    }

    public class AutomatedSuggestedPropertyPrices : vwSuggestedSubjectPrice
    {
        public string message;
        public double? minClusterValue;
        public double? maxClusterValue;
    }

    public class SalesDataDetailsByMonth: vwSalesStatisticsByMonthByNeighborhood
    {

    }
    public class PricePerSqftDetailsByMonth: PricePerSqFtStatisticsByMonthByNeighborhoodMeanSmoothing
    {

    }

    public class PriceDetailsByMonth : SalePriceStatisticsByMonthByNeighborhoodMeanSmoothing
    {

    }

    public class SubjectDetails: ShowCMASubject_Result
    {

    }

    public class CMAResult: tfnGetCMA_Result
    {
        public int? GLAppsqft;

    }
    public class CMAManualResult : tfnGetManualCMA_Result
    {
        public int? GLAppsqft;
    }

    public class CMA
    {

        public static SuggestedPropertyPrices GetSuggestedPricesForAProperty(string propertyBBL)
        {
            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                return Mapper.Map<SuggestedPropertyPrices>(nycmaE.vwSuggestedSubjectPrices.Where(i => i.SubjectBBL == propertyBBL).FirstOrDefault());
            }
        }

        public static Assessment GetAssessmentRecord(string propertyBBL)
        {
            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                return nycmaE.Assessments.Find(propertyBBL);
            }
        }

        public static List<SalesDataDetailsByMonth> GetSalesTrend(string neighborhood, string buildingClass, int timeSpanInYears)
        {
            DateTime eDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month,1,0,0,0);
            DateTime sDate = new DateTime(DateTime.Today.Year- timeSpanInYears, DateTime.Today.Month, 1, 0, 0, 0);
            sDate=sDate.AddMonths(1);

            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                if (buildingClass==null)
                    return Mapper.Map<List<vwSalesStatisticsByMonthByNeighborhood>,List<SalesDataDetailsByMonth>>
                           (nycmaE.vwSalesStatisticsByMonthByNeighborhoods.Where(i => i.YearMonth >= sDate 
                                                                                 && i.YearMonth <= eDate 
                                                                                 && i.Neighborhood== neighborhood)
                                                                          .OrderBy(i => i.YearMonth)
                                                                          .ToList());
                else
                    return Mapper.Map<List<vwSalesStatisticsByMonthByNeighborhoodByBuildingClass>, List<SalesDataDetailsByMonth>>
                           (nycmaE.vwSalesStatisticsByMonthByNeighborhoodByBuildingClasses.Where(i => i.YearMonth >= sDate
                                                                                                 && i.YearMonth <= eDate
                                                                                                 && i.Neighborhood == neighborhood
                                                                                                 && i.BuildingClass == buildingClass)
                                                                                          .OrderBy(i => i.YearMonth)
                                                                                          .ToList());
            }
        }

        public static List<PricePerSqftDetailsByMonth> GetPricePerSqftTrend(string neighborhood, string buildingClass, int timeSpanInYears)
        {
            DateTime eDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1, 0, 0, 0);
            DateTime sDate = new DateTime(DateTime.Today.Year - timeSpanInYears, DateTime.Today.Month, 1, 0, 0, 0);
            sDate = sDate.AddMonths(1);

            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                return Mapper.Map<List<PricePerSqFtStatisticsByMonthByNeighborhoodMeanSmoothing>, 
                                  List<PricePerSqftDetailsByMonth>>(nycmaE.PricePerSqFtStatisticsByMonthByNeighborhoodMeanSmoothings
                                                                          .Where(i => i.YearMonth >= sDate && i.YearMonth <= eDate 
                                                                                 && i.NeighborhoodName == neighborhood
                                                                                 && i.BuildingClass == buildingClass)
                                                                          .OrderBy(i => i.YearMonth).ToList());
            }
        }

        public static List<PriceDetailsByMonth> GetSalesPriceTrend(string neighborhood, string buildingClass, int timeSpanInYears)
        {
            DateTime eDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1, 0, 0, 0);
            DateTime sDate = new DateTime(DateTime.Today.Year - timeSpanInYears, DateTime.Today.Month, 1, 0, 0, 0);
            sDate = sDate.AddMonths(1);

            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                return Mapper.Map<List<SalePriceStatisticsByMonthByNeighborhoodMeanSmoothing>, 
                                  List<PriceDetailsByMonth>>(nycmaE.SalePriceStatisticsByMonthByNeighborhoodMeanSmoothings
                                                                   .Where(i => i.YearMonth >= sDate && i.YearMonth <= eDate
                                                                          && i.NeighborhoodName == neighborhood
                                                                          && i.BuildingClass == buildingClass)
                                                                   .OrderBy(i => i.YearMonth).ToList());
            }
        }


        public static List<CMAResult> GetComparables(string algorithmType, string propertyBBL, int? maxRecords, bool? sameNeighboorhood, bool? sameSchoolDistrict,
                                                         bool? sameZip, bool? sameBlock, bool? sameStreetName, int? monthOffset, double? minSalePrice, double? maxSalePrice,
                                                         int? classMatchType, bool? isNotIntraFamily, bool? isSelleraCompany, bool? isBuyeraCompany, int? minSimilarity)
        {
            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                var resultList = Mapper.Map<List<tfnGetCMA_Result>, List<CMAResult>>(nycmaE.tfnGetCMA(algorithmType, propertyBBL, maxRecords, sameNeighboorhood, sameSchoolDistrict, 
                                                                                     sameZip, sameBlock, sameStreetName, monthOffset, minSalePrice, maxSalePrice, 
                                                                                     classMatchType, isNotIntraFamily, isSelleraCompany, isBuyeraCompany, minSimilarity)
                                                                                     .OrderBy(o=> o.SortOrder)
                                                                                     .ThenByDescending(o=>o.Similarity)
                                                                                     .ToList());
                foreach (var res in resultList)
                    if (res.GLA.GetValueOrDefault()>0)
                        res.GLAppsqft = Convert.ToInt32(Math.Round(res.DeedAmount.GetValueOrDefault()/ res.GLA.GetValueOrDefault(), 0));
                return resultList;
            }
        }


        public static List<CMAManualResult> GetManualComparables(string propertyBBL, int? minSimilarity, bool? sameNeighboorhood, bool? sameSchoolDistrict,
                                                        bool? sameZip, bool? sameBlock, bool? sameStreetName, int? monthOffset, double? minSalePrice, double? maxSalePrice,
                                                        int? classMatchType, bool? isNotIntraFamily, bool? isSelleraCompany, bool? isBuyeraCompany, double? distanceInMiles, int? gLAHiRange,
                                                        int? gLALoRange, int? lAHiRange, int? lALoRange, int? buildingFrontageHiRange, int? buildingFrontageLoRange, int? buildingDepthHiRange,
                                                        int? buildingDepthLoRange, int? lotFrontageHiRange, int? lotFrontageLoRange, int? lotDepthHiRange, int? lotDepthLoRange)
        {
            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                var resultList = Mapper.Map<List<tfnGetManualCMA_Result>, List<CMAManualResult>>(nycmaE.tfnGetManualCMA(propertyBBL, minSimilarity, sameNeighboorhood, sameSchoolDistrict,
                                                                                                sameZip, sameBlock, sameStreetName, monthOffset, minSalePrice, maxSalePrice,
                                                                                                classMatchType, isNotIntraFamily, isSelleraCompany, isBuyeraCompany, distanceInMiles, gLAHiRange,
                                                                                                gLALoRange, lAHiRange, lALoRange, buildingFrontageHiRange, buildingFrontageLoRange, buildingDepthHiRange,
                                                                                                buildingDepthLoRange, lotFrontageHiRange, lotFrontageLoRange, lotDepthHiRange, lotDepthLoRange)
                                                                                                .OrderBy(o=> o.SortOrder)
                                                                                                .ThenByDescending(o=>o.Similarity)
                                                                                                .ToList());
                foreach (var res in resultList)
                    if (res.GLA.GetValueOrDefault() > 0)
                        res.GLAppsqft = Convert.ToInt32(Math.Round(res.DeedAmount.GetValueOrDefault() / res.GLA.GetValueOrDefault(), 0));
                return resultList;
            }
        }

        public static SubjectDetails GetSubject(string propertyBBL)
        {
            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                return Mapper.Map<SubjectDetails>(nycmaE.ShowCMASubject(propertyBBL).FirstOrDefault());
            }
        }

        public static List<SaleParty> GetDeedParties(string deedUniqueKey)
        {
            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                return nycmaE.SaleParties.Where(i=> i.DeedUniqueKey== deedUniqueKey).ToList();
            }
        }

        public static long SaveCMARun(string subjectBBL, BAL.ManualCMASelection manualCMA )
        {
            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                using (var nycmaEntitiestransaction = nycmaE.Database.BeginTransaction())
                {
                    NYCMADB.CMARun cmaRun = new NYCMADB.CMARun();

                    cmaRun.UserName = manualCMA.username;
                    cmaRun.RunDateTime = DateTime.UtcNow;
                    cmaRun.RunType = (int)CMARunType.Manual;
                    cmaRun.CMAType = manualCMA.intent;
                    cmaRun.BBL = subjectBBL;

                    cmaRun.AlgorithmType = "F";
                    if (manualCMA.basicFilter != null)
                    {
                        cmaRun.MonthsOffset = manualCMA.basicFilter.monthOffset;
                        cmaRun.BuildingClassMatch = manualCMA.basicFilter.classMatchType;
                        cmaRun.MinSalePrice = Convert.ToDecimal(manualCMA.basicFilter.minSalePrice);
                        cmaRun.MaxSalePrice = Convert.ToDecimal(manualCMA.basicFilter.maxSalePrice);
                        cmaRun.SameNeighborhood = manualCMA.basicFilter.sameNeighborhood;
                        cmaRun.SameSchoolDistrict = manualCMA.basicFilter.sameSchoolDistrict;
                        cmaRun.SameZip = manualCMA.basicFilter.sameZip;
                        cmaRun.SameStreet = manualCMA.basicFilter.sameStreet;
                        cmaRun.SameBlock = manualCMA.basicFilter.sameBlock;
                        cmaRun.IsNotIntraFamily = manualCMA.basicFilter.isNotIntraFamily;
                        cmaRun.BuyerIsCompany = manualCMA.basicFilter.isBuyeraCompany;
                        cmaRun.SellerIsCompany = manualCMA.basicFilter.isSelleraCompany;
                    }

                    if (manualCMA.additionalFilter != null)
                    {
                        cmaRun.Distance = Convert.ToDecimal(manualCMA.additionalFilter.distance);
                        cmaRun.GLAHiPercentage = manualCMA.additionalFilter.gLAMax;
                        cmaRun.GLALoPercentage = manualCMA.additionalFilter.gLAMin;
                        cmaRun.LAHiPercentage = manualCMA.additionalFilter.lotAreaMax;
                        cmaRun.LALoPercentage = manualCMA.additionalFilter.lotAreaMin;
                        cmaRun.BuildingFrontageHiRange = manualCMA.additionalFilter.buildingFrontageMax;
                        cmaRun.BuildingFrontageLoRange = manualCMA.additionalFilter.buildingFrontageMin;
                        cmaRun.BuildingDepthHiRange = manualCMA.additionalFilter.buildingDepthMax;
                        cmaRun.BuildingDepthLoiRange = manualCMA.additionalFilter.buildingDepthMin;
                        cmaRun.LotFrontageHiRange = manualCMA.additionalFilter.lotFrontageMax;
                        cmaRun.LotFrontageLoRange = manualCMA.additionalFilter.lotFrontageMin;
                        cmaRun.LotDepthHiRange = manualCMA.additionalFilter.lotDepthMax;
                        cmaRun.LotDepthLoRange = manualCMA.additionalFilter.lotDepthMin;
                    }

                    try
                    {
                        cmaRun = nycmaE.CMARuns.Add(cmaRun);
                        nycmaE.SaveChanges();

                        foreach (var comp in manualCMA.comparables)
                        {
                            NYCMADB.CMAComparable cmaComp = new NYCMADB.CMAComparable();

                            cmaComp.CMARunId = cmaRun.CMARunId;
                            cmaComp.ComparableBBL = comp.BBLE;
                            cmaComp.SaleDate = comp.DeedDate.GetValueOrDefault();
                            cmaComp = nycmaE.CMAComparables.Add(cmaComp);
                        }
                        nycmaE.SaveChanges();

                        nycmaEntitiestransaction.Commit();

                        return cmaRun.CMARunId;
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                Common.Logs.log().Error(string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage));
                            }
                        }
                        return 0;
                    }
                    catch (Exception e)
                    {
                        nycmaEntitiestransaction.Rollback();
                        Common.Logs.log().Error(string.Format("Exception encountered saving manual CMARun for {} run by {2}", subjectBBL, manualCMA.username, Common.Logs.FormatException(e)));
                        return 0;
                    }
                }
            }
        }
    }
}