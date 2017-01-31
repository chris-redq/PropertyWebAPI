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

    public class SuggestPropertPrices: vwSuggestedSubjectPrice
    {

    }

    public class SalesDataDetailsByMonth: vwSalesByMonthByNTA
    {

    }
    public class PricePerSqftDetailsByMonth: PricePerSqFtStatisticsByMonthByNTAMeanSmoothing
    {

    }

    public class PriceDetailsByMonth : SalePriceStatisticsByMonthByNTAMeanSmoothing
    {

    }

    public class SubjectDetails: ShowCMASubject_Result
    {

    }

    public class CMAResult: tfnGetCMA_Result
    {

    }

    public class CMA
    {

        public static SuggestPropertPrices GetSuggestedPricesForAProperty(string propertyBBL)
        {
            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                return Mapper.Map<SuggestPropertPrices>(nycmaE.vwSuggestedSubjectPrices.Where(i => i.SubjectBBL == propertyBBL).FirstOrDefault());
            }
        }

        public static Assessment GetAssessmentRecord(string propertyBBL)
        {
            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                return nycmaE.Assessments.Find(propertyBBL);
            }
        }

        public static List<SalesDataDetailsByMonth> GetSalesTrend(string ntaCode, int timeSpanInYears)
        {
            DateTime eDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month,1,0,0,0);
            DateTime sDate = new DateTime(DateTime.Today.Year- timeSpanInYears, DateTime.Today.Month, 1, 0, 0, 0);
            sDate=sDate.AddMonths(1);

            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                return Mapper.Map<List<vwSalesByMonthByNTA>,List<SalesDataDetailsByMonth>>(nycmaE.vwSalesByMonthByNTAs.Where(i => i.YearMonth >= sDate && i.YearMonth <= eDate && i.NTACode==ntaCode).ToList());
            }
        }

        public static List<PricePerSqftDetailsByMonth> GetPricePerSqftTrend(string ntaCode, int timeSpanInYears)
        {
            DateTime eDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1, 0, 0, 0);
            DateTime sDate = new DateTime(DateTime.Today.Year - timeSpanInYears, DateTime.Today.Month, 1, 0, 0, 0);
            sDate = sDate.AddMonths(1);

            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                return Mapper.Map<List<PricePerSqFtStatisticsByMonthByNTAMeanSmoothing>, List<PricePerSqftDetailsByMonth>>(nycmaE.PricePerSqFtStatisticsByMonthByNTAMeanSmoothings
                                                                                                                                   .Where(i => i.YearMonth >= sDate && 
                                                                                                                                               i.YearMonth <= eDate && 
                                                                                                                                               i.NTACode == ntaCode)
                                                                                                                                               .OrderBy(i => i.YearMonth).ToList());
            }
        }

        public static List<PriceDetailsByMonth> GetSalesPriceTrend(string ntaCode, int timeSpanInYears)
        {
            DateTime eDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1, 0, 0, 0);
            DateTime sDate = new DateTime(DateTime.Today.Year - timeSpanInYears, DateTime.Today.Month, 1, 0, 0, 0);
            sDate = sDate.AddMonths(1);

            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                return Mapper.Map<List<SalePriceStatisticsByMonthByNTAMeanSmoothing>, List<PriceDetailsByMonth>>(nycmaE.SalePriceStatisticsByMonthByNTAMeanSmoothings
                                                                                                                         .Where(i => i.YearMonth >= sDate &&
                                                                                                                                     i.YearMonth <= eDate &&
                                                                                                                                     i.NTACode == ntaCode)
                                                                                                                         .OrderBy(i => i.YearMonth).ToList());
            }
        }


        public static List<CMAResult> GetComaparables(string algorithmType, string propertyBBL, int? maxRecords, bool? sameNeighboorhood, bool? sameSchoolDistrict,
                                                         bool? sameZip, bool? sameBlock, bool? sameStreetName, int? monthOffset, double? minSalePrice, double? maxSalePrice,
                                                         int? classMatchType, bool? isNotIntraFamily, bool? isSelleraCompany, bool? isBuyeraCompany)
        {
            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                return Mapper.Map<List<tfnGetCMA_Result>, List<CMAResult>>(nycmaE.tfnGetCMA(algorithmType, propertyBBL, maxRecords, sameNeighboorhood, sameSchoolDistrict, 
                                                                                            sameZip, sameBlock, sameStreetName, monthOffset, minSalePrice, maxSalePrice, 
                                                                                            classMatchType, isNotIntraFamily, isSelleraCompany, isBuyeraCompany).ToList());
            }
        }


        public static SubjectDetails GetSubject(string propertyBBL)
        {
            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                return Mapper.Map<SubjectDetails>(nycmaE.ShowCMASubject(propertyBBL));
            }
        }

        public static List<SaleParty> GetDeedParties(string deedUniqueKey)
        {
            using (NYCMAEntities nycmaE = new NYCMAEntities())
            {
                return nycmaE.SaleParties.Where(i=> i.DeedUniqueKey== deedUniqueKey).ToList();
            }
        }
    }
}