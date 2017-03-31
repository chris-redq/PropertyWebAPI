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
        public DAL.AutomatedSuggestedPropertyPrices price;
        public List<DAL.CMAResult> results;
    }
    
    public class CMA
    {
        private static double[] THRESHOLD = new double[] { 0.08, 0.12, 0.25 };

        private const double MAX_REDQ_THRESHOLD = 0.12;
        private const double MAX_REHAB_THRESHOLD = 0.25;
        private const double MAX_SHORTSALE_THRESHOLD = 0.25;
        private const double MAX_THRESHOLD_PAD = 0.02;

        private const double GAUSSIAN_SIGMA = 0.8;
        private const int GAUSSIAN_KERNEL_SIZE = 3;

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

        private static double[] GetThresholdsForClusters(int cMAType, double[] derivatives)
        {
            Common.DoubleList minThresholdRequiredList = new Common.DoubleList();
            Common.DoubleList finalMinThresholdRequiredList = new Common.DoubleList();

            double? minThresholdRequired = 0;
            double? localMinThresholdRequired = 0;
            while (minThresholdRequired != null)
            {
                minThresholdRequired = null;
                if (derivatives.Length >= 2)
                {
                    for (int j = 0; j < derivatives.Length - 1; j++)
                    {
                        double localThreshold = derivatives[j] > derivatives[j + 1] ? derivatives[j] : derivatives[j + 1];
                        if (localThreshold > localMinThresholdRequired)
                        {
                            if (minThresholdRequired == null)
                                minThresholdRequired = localThreshold;
                            else if (minThresholdRequired > localThreshold)
                                minThresholdRequired = localThreshold;
                        }
                    }
                    if (minThresholdRequired != null)
                    {
                        localMinThresholdRequired = minThresholdRequired;
                        minThresholdRequiredList.Add(minThresholdRequired.GetValueOrDefault());
                    }
                }
            }

            foreach (var val in minThresholdRequiredList)
            {
                double? maxAllowedThreshold = null;
                switch (cMAType)
                {
                    case (int)CMAType.Regular:
                        maxAllowedThreshold = MAX_REDQ_THRESHOLD;
                        break;
                    case (int)CMAType.ShortSale:
                        maxAllowedThreshold = MAX_SHORTSALE_THRESHOLD;
                        break;
                    case (int)CMAType.Rehab:
                        maxAllowedThreshold = MAX_REDQ_THRESHOLD;
                        break;
                }
                if (val < maxAllowedThreshold)
                {
                    if (val + MAX_THRESHOLD_PAD <= maxAllowedThreshold)
                    {
                        if (finalMinThresholdRequiredList.IndexOf(val + MAX_THRESHOLD_PAD)==-1)
                            finalMinThresholdRequiredList.Add(val + MAX_THRESHOLD_PAD);
                    }
                    else
                    {
                        if (finalMinThresholdRequiredList.IndexOf(maxAllowedThreshold.GetValueOrDefault()) == -1)
                            finalMinThresholdRequiredList.Add(maxAllowedThreshold.GetValueOrDefault());
                    }
                }
            }
            return finalMinThresholdRequiredList.ToArray();
        }

        private static double? GetMinimumThresholdForCluster(int cMAType, double[] derivatives)
        {
            double? minThresholdRequired = null;
            if (derivatives.Length >= 2)
            {
                for (int j = 0; j < derivatives.Length - 1; j++)
                {
                    double localThreshold = derivatives[j] > derivatives[j + 1] ? derivatives[j] : derivatives[j + 1];
                    if (minThresholdRequired == null)
                        minThresholdRequired = localThreshold;
                    else if (minThresholdRequired > localThreshold)
                        minThresholdRequired = localThreshold;
                }
            }
            if (minThresholdRequired != null)
            {
                double? maxAllowedThreshold = null;
                switch (cMAType)
                {
                    case (int)CMAType.Regular:
                        maxAllowedThreshold = MAX_REDQ_THRESHOLD;
                        break;
                    case (int)CMAType.ShortSale:
                        maxAllowedThreshold = MAX_SHORTSALE_THRESHOLD;
                        break;
                    case (int)CMAType.Rehab:
                        maxAllowedThreshold = MAX_REDQ_THRESHOLD;
                        break;
                }
                if (minThresholdRequired + MAX_THRESHOLD_PAD <= maxAllowedThreshold)
                    minThresholdRequired += MAX_THRESHOLD_PAD;
                else
                    minThresholdRequired = maxAllowedThreshold;
            }
            return minThresholdRequired;
        }

        private static int[] FindClusters(int cMAType, double[] derivatives, double minThresholdInData)
        {
            int[] mins = Common.Statistics.FindClusters(derivatives, minThresholdInData).ToArray();
            return mins;
        }


        private static DAL.AutomatedSuggestedPropertyPrices getSuggestedPropertyPrices(int cMAType, string subjectBBL, List<DAL.CMAResult> results)
        {
            DAL.AutomatedSuggestedPropertyPrices price = new DAL.AutomatedSuggestedPropertyPrices();
            price.SubjectBBL = subjectBBL;

            var subject = DAL.CMA.GetSubject(subjectBBL);
            if (subject == null)
            {   price.message = "Error locating Subject Information";
                return price;
            }

            double? GLA = subject.GLA.GetValueOrDefault();
            if (GLA == null || GLA==0)
            {
                price.message = "GLA is null or zero for Subject";
                return price;
            }

            Common.DoubleList pricessqft = new Common.DoubleList();
            foreach (var comp in results)
                pricessqft.Add(comp.DeedAmount.GetValueOrDefault() / comp.GLA.GetValueOrDefault());

            pricessqft.Sort();
            double[] pricesArray = pricessqft.ToArray();
            double[] smoothValues = Common.Statistics.ApplyGaussianKDE(pricessqft.ToArray(), GAUSSIAN_KERNEL_SIZE, GAUSSIAN_SIGMA);
            double[] derivatives = Common.Statistics.DiscreteDerivative(smoothValues);
            for (int j = 0; j < derivatives.Length; j++)
                derivatives[j] = Math.Round(derivatives[j],0);

            //What is the minimum threshold required to get a cluster from our comparables
            double? minThresholdRequired = GetMinimumThresholdForCluster(cMAType, derivatives);

            if (minThresholdRequired == null)
            {
                price.message = "At least 3 comparables required for pricing";
                return price;
            }

            //Using minThresholdRequired find clusters in comparables
            int[] mins = FindClusters(cMAType, derivatives, minThresholdRequired.GetValueOrDefault());

            int clusterValue=-1;
            double? minClusterValue = null, maxClusterValue = null;
            switch (mins.Length)
            {
                case 0:
                    price.message = "No price clusters found";
                    break;
                case 1:
                    price.message = "Single price cluster found";
                    clusterValue = 0;
                    break;
                case 2:
                    price.message = "Two price clusters found.";
                    switch (cMAType)
                    {
                        case (int)CMAType.Regular:
                            price.message += " Picking the second cluster.";
                            clusterValue = 1;
                            break;
                        case (int)CMAType.ShortSale:
                            price.message += " Picking the first cluster.";
                            clusterValue = 0;
                            break;
                        case (int)CMAType.Rehab:
                            price.message += " Picking the second cluster.";
                            clusterValue = 1;
                            break;
                    }
                    break;
                default:
                    price.message = string.Format("{0} clusters found.", mins.Length);
                    switch (cMAType)
                    {
                        case (int)CMAType.Regular:
                            price.message += " Cannot determine the right price cluster refine search.";
                            break;
                        case (int)CMAType.ShortSale:
                            price.message += " Picking the first cluster.";
                            clusterValue = 0;
                            break;
                        case (int)CMAType.Rehab:
                            price.message += " Picking the last cluster.";
                            clusterValue = mins.Length - 1;
                            break;
                    }
                    break;
            }

            if (clusterValue!=-1)
            {
                minClusterValue = pricesArray[mins[clusterValue]];
                maxClusterValue = pricesArray[mins[clusterValue] + 1];
                int i = mins[clusterValue] + 1;
                while (i < derivatives.Length)
                {
                    if (derivatives[i] <= minThresholdRequired.GetValueOrDefault())
                        maxClusterValue = pricesArray[i + 1];
                    else
                        break;
                    i++;
                }
            }

            if (minClusterValue != null && maxClusterValue != null)
            {
                price.SubjectBBL = subjectBBL;
                price.minClusterValue = minClusterValue;
                price.maxClusterValue = maxClusterValue;

                price.LowPrice = Math.Round(pricessqft.Min(x => x >= minClusterValue && x <= maxClusterValue) * GLA.GetValueOrDefault(), 0);
                price.AVGPrice = Math.Round(pricessqft.Average(x => x >= minClusterValue && x <= maxClusterValue) * GLA.GetValueOrDefault(), 0);
                price.MedianPrice = Math.Round(Common.Statistics.Percentile(pricessqft.SubList(x => x >= minClusterValue && x <= maxClusterValue), 50) * GLA.GetValueOrDefault(), 0);
                price.HighPrice = Math.Round(pricessqft.Max(x => x >= minClusterValue && x <= maxClusterValue) * GLA.GetValueOrDefault(), 0);
            }

            return price;
        }

        public static AutomatedCMAResults GetAutomatedCMA(string algorithmType, string subjectBBL, int? maxRecords, bool? sameNeighborhood, bool? sameSchoolDistrict,
                                                          bool? sameZip, bool? sameBlock, bool? sameStreet, int? monthOffset, double? minSalePrice, double? maxSalePrice,
                                                          int? classMatchType, bool? isNotIntraFamily, bool? isSelleraCompany, bool? isBuyeraCompany)
        {
            AutomatedCMAResults cmaResult = new AutomatedCMAResults();
            
            cmaResult.results=DAL.CMA.GetComparables(algorithmType, subjectBBL, maxRecords, sameNeighborhood, sameSchoolDistrict, sameZip, sameBlock, sameStreet, 
                                                     monthOffset, minSalePrice, maxSalePrice, classMatchType, isNotIntraFamily, isSelleraCompany, isBuyeraCompany);

            cmaResult.price = getSuggestedPropertyPrices((int)CMAType.Regular, subjectBBL, cmaResult.results);

            return cmaResult;
        }

        public static AutomatedCMAResults GetAutomatedShortSaleCMA(string subjectBBL, AutomatedCMAFilters fl)
        {
            AutomatedCMAResults cmaResult = new AutomatedCMAResults();

            cmaResult.results = DAL.CMA.GetComparables(fl.algorithmType, subjectBBL, fl.basicFilter.maxRecords, fl.basicFilter.sameNeighborhood, fl.basicFilter.sameSchoolDistrict,
                                                       fl.basicFilter.sameZip, fl.basicFilter.sameBlock, fl.basicFilter.sameStreet, fl.basicFilter.monthOffset, fl.basicFilter.minSalePrice,
                                                       fl.basicFilter.maxSalePrice, fl.basicFilter.classMatchType, fl.basicFilter.isNotIntraFamily, fl.basicFilter.isSelleraCompany, 
                                                       fl.basicFilter.isBuyeraCompany);

            cmaResult.price = getSuggestedPropertyPrices((int)CMAType.ShortSale, subjectBBL, cmaResult.results);

            return cmaResult;
        }

        public static AutomatedCMAResults GetAutomatedRehabCMA(string subjectBBL, AutomatedCMAFilters fl)
        {
            AutomatedCMAResults cmaResult = new AutomatedCMAResults();

            cmaResult.results = DAL.CMA.GetComparables(fl.algorithmType, subjectBBL, fl.basicFilter.maxRecords, fl.basicFilter.sameNeighborhood, fl.basicFilter.sameSchoolDistrict,
                                                       fl.basicFilter.sameZip, fl.basicFilter.sameBlock, fl.basicFilter.sameStreet, fl.basicFilter.monthOffset, fl.basicFilter.minSalePrice,
                                                       fl.basicFilter.maxSalePrice, fl.basicFilter.classMatchType, fl.basicFilter.isNotIntraFamily, fl.basicFilter.isSelleraCompany, 
                                                       fl.basicFilter.isBuyeraCompany);

            cmaResult.price = cmaResult.price = getSuggestedPropertyPrices((int)CMAType.Rehab, subjectBBL, cmaResult.results);

            return cmaResult;
        }
    }
}