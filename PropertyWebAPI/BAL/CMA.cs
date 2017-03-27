//-----------------------------------------------------------------------
// <copyright file="cma.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI.BAL
{
    using System;
    using System.Collections.Generic;

    public class BasicCMAFilter
    {
        /// <summary>Maximum number of comparables to be returned</summary>
        public int? maxRecords { get; set; }
        /// <summary>Valid values are 0, 1 or NULL. 0 means ignore neighborhood and 1 means search in same neighborhood. Default is 0</summary>
        public bool? sameNeighborhood { get; set; }
        /// <summary>Valid values are 0, 1 or NULL. 0 means ignore school district and 1 means search in same school district. Default is 1</summary>
        public bool? sameSchoolDistrict { get; set; }
        /// <summary>Valid values are 0, 1 or NULL. 0 means ignore zip code and 1 means search in same zip code. Default is 0</summary>
        public bool? sameZip { get; set; }
        /// <summary>Valid values are 0, 1 or NULL. 0 means ignore address block and 1 means search in same block. Default is 0</summary>
        public bool? sameBlock { get; set; }
        /// <summary>Valid values are 0, 1 or NULL. 0 means ignore street and 1 means search in same street. Default is 0</summary>
        public bool? sameStreet { get; set; }
        /// <summary>Month in past to inspect for comparables, always negative, default value is -12 (1 Year)</summary>
        public int? monthOffset { get; set; }
        /// <summary>Minimum sale price for consideration. Default is 10000</summary>
        public double? minSalePrice { get; set; }
        /// <summary>Maximum Sale price for consideration. Default is ignore max sale price</summary>
        public double? maxSalePrice { get; set; }
        /// <summary>0 - For same BuildingClass, 1  - For Expanded Building Classes based on Subject Building Class, 2 - To ignore Building Class</summary>
        public int? classMatchType { get; set; }
        /// <summary>Null or 1. 1 - means only arm length sales are considered.</summary>
        public bool? isNotIntraFamily { get; set; }
        /// <summary>Null or 1. 1 - means seller is a company</summary>
        public bool? isSelleraCompany { get; set; }
        /// <summary>Null or 1. 1 - means buyer is a company.</summary>
        public bool? isBuyeraCompany { get; set; }
    }

    public class AdditionalCMAFilter
    {
        /// <summary>Search distance in miles. Values can be decimals. Default is 1.0</summary>
        public double distance { get; set; }
        /// <summary>Percentage difference in GLA for properties smaller than subject</summary>
        public int? gLAMin { get; set; }
        /// <summary>Percentage difference in GLA for properties larger than subject</summary>
        public int? gLAMax { get; set; }
        /// <summary>Percentage difference in Lot Area for properties smaller than subject</summary>
        public int? lotAreaMin { get; set; }
        /// <summary>Percentage difference in Lot Area for properties larger than subject</summary>
        public int? lotAreaMax { get; set; }
        /// <summary>Minimum Building Frontage in ft for consideration</summary>
        public int? buildingFrontageMin { get; set; }
        /// <summary>Maximum Building Frontage in ft for consideration</summary>
        public int? buildingFrontageMax { get; set; }
        /// <summary>Minimum Building Depth in ft for consideration</summary>
        public int? buildingDepthMin { get; set; }
        /// <summary>Maximum Building Depth in ft for consideration</summary>
        public int? buildingDepthMax { get; set; }
        /// <summary>Minimum Lot Frontage in ft for consideration</summary>
        public int? lotFrontageMin { get; set; }
        /// <summary>Maximum Lot Frontage in ft for consideration</summary>
        public int? lotFrontageMax { get; set; }
        /// <summary>Minimum Lot Depth in ft for consideration</summary>
        public int? lotDepthMin { get; set; }
        /// <summary>Maximum Lot Depth in ft for consideration</summary>
        public int? lotDepthMax { get; set; }
    }

    public class ManualCMASelection
    {
        /// <summary>Name of the user requesting CMA</summary>
        public string username;
        /// <summary>Valid values are 1 - Regular CMA, 2 - Short Sale, 3 - Rehab</summary>
        public int intent;
        /// <summary>Filters associated with result set size, ownership, location,price etc.</summary>
        public BasicCMAFilter basicFilter;
        /// <summary>Filters associated with distance, building and lot dimensions</summary>
        public AdditionalCMAFilter additionalFilter;
        /// <summary>List of comparables selected</summary>
        public List<DAL.CMAManualResult> comparables;

    }

    public class ManualCMAResult
    {
        public long? minValue;
        public long? maxValue;
        public long? averageValue;
    }

    public class ManualCMAFilters
    {
        /// <summary>Filters associated with result set size, ownership, location,price etc.</summary>
        public BasicCMAFilter basicFilter;
        /// <summary>Filters associated with distance, building and lot dimensions</summary>
        public AdditionalCMAFilter additionalFilter;
    }

    public class AutomatedCMAFilters
    {
        /// <summary>Valid values are 1 - Regular CMA, 2 - Short Sale, 3 - Rehab</summary>
        public int intent;
        /// <summary>Valid values are G - Farther th property tighter the constraints, O - Iterative constraint relaxation with fixed upper bounds, E - Euclidean distance based ranking within set constraints</summary>
        public string algorithmType = "O";
        /// <summary>Filters associated with result set size, ownership, location,price etc.</summary>
        public BasicCMAFilter basicFilter;
    }

    public class AutomatedCMAResults
    {
        public DAL.SuggestedPropertyPrices price;
        public List<DAL.CMAResult> results;
    }

    public class CMA
    {
        public static ManualCMAResult SaveManualComparables(string propertyBBL, ManualCMASelection manualCMA)
        {
            DAL.CMA.SaveCMARun(propertyBBL, manualCMA);
            return computeManualCMANumbers(manualCMA.comparables);
        }

        private static ManualCMAResult computeManualCMANumbers(List<PropertyWebAPI.DAL.CMAManualResult> comparables)
        {
            ManualCMAResult mCMAResult = new ManualCMAResult();
            long sum = 0, i = 0;

            foreach (var comp in comparables)
            {
                i += 1;
                sum += Convert.ToInt64(comp.DeedAmount);
                if (mCMAResult.minValue == null || mCMAResult.minValue > comp.DeedAmount)
                    mCMAResult.minValue = Convert.ToInt64(comp.DeedAmount);
                if (mCMAResult.maxValue == null || mCMAResult.maxValue < comp.DeedAmount)
                    mCMAResult.maxValue = Convert.ToInt64(comp.DeedAmount);
                if (mCMAResult.averageValue == null)
                    mCMAResult.averageValue = Convert.ToInt64(comp.DeedAmount);
                else
                    mCMAResult.averageValue = Convert.ToInt64(sum / i);
            }
            return mCMAResult;
        }

        private static DAL.SuggestedPropertyPrices getSuggestedPropertyPrice(string subjectBBL, List<DAL.CMAResult> results)
        {
            DAL.SuggestedPropertyPrices price = new DAL.SuggestedPropertyPrices();

            Common.DoubleList pricessqft = new Common.DoubleList();
            foreach (var comp in results)
                pricessqft.Add(comp.DeedAmount.GetValueOrDefault()/comp.GLA.GetValueOrDefault());

            double min = Common.Statistics.Percentile(pricessqft, 0);
            double percentile7 = Common.Statistics.Percentile(pricessqft, 7);
            double q1 = Common.Statistics.Percentile(pricessqft, 25);
            double median = Common.Statistics.Percentile(pricessqft, 50);
            double q3 = Common.Statistics.Percentile(pricessqft, 75);
            double percentile95 = Common.Statistics.Percentile(pricessqft, 95);
            double max = Common.Statistics.Percentile(pricessqft, 100);

            double avgLowValueNoLP = pricessqft.Average(x => x<= q1 && !Common.Statistics.IsOutLier(x, q1, q3));
            double avgHighValue = pricessqft.Average(x => x>=q3);


            var subject = DAL.CMA.GetSubject(subjectBBL);
            double GLA = subject.GLA.GetValueOrDefault();

            price.SubjectBBL = subjectBBL;
            price.AVGLowPrice = Math.Round(percentile7 * GLA, 0);
            price.AVGLowPriceNoLP = Math.Round(avgLowValueNoLP * GLA,0);
            price.MedianPrice = Math.Round(median * GLA, 0);
            price.AVGHighPice = Math.Round(avgHighValue * GLA, 0);

            return price;
        }

        public static AutomatedCMAResults GetAutomatedCMA(string algorithmType, string subjectBBL, int? maxRecords, bool? sameNeighborhood, bool? sameSchoolDistrict,
                                                          bool? sameZip, bool? sameBlock, bool? sameStreet, int? monthOffset, double? minSalePrice, double? maxSalePrice,
                                                          int? classMatchType, bool? isNotIntraFamily, bool? isSelleraCompany, bool? isBuyeraCompany)
        {
            AutomatedCMAResults cmaResult = new AutomatedCMAResults();
            
            cmaResult.results=DAL.CMA.GetComparables(algorithmType, subjectBBL, maxRecords, sameNeighborhood, sameSchoolDistrict, sameZip, sameBlock, sameStreet, 
                                                     monthOffset, minSalePrice, maxSalePrice, classMatchType, isNotIntraFamily, isSelleraCompany, isBuyeraCompany);

            cmaResult.price = getSuggestedPropertyPrice(subjectBBL, cmaResult.results);

            return cmaResult;
        }
    }
}