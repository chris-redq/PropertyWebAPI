//-----------------------------------------------------------------------
// <author>Raj Sethi, Gurpreet Singh</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI.BAL
{
    using System;
    using System.Collections.Generic;

    public class BasicCMAFilter
    {
        /// <summary>Maximum number of comparables to be returned</summary>
        public int? maxRecords { get; set; }
        /// <summary>Minimum similarity index score allowed for returned comparables. Default value is 85</summary>
        public int? minSimilarity { get; set; }
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
        public string algorithmType = "F";
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
        private const double MAX_REDQ_THRESHOLD = 0.12;
        private const double MAX_REHAB_THRESHOLD = 0.25;
        private const double MAX_SHORTSALE_THRESHOLD = 0.25;

        private const double GAUSSIAN_SIGMA = 12;
        
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

        public static double[] GetThresholdsForClusters(int cMAType, double[] derivatives)
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
                        maxAllowedThreshold = MAX_REHAB_THRESHOLD;
                        break;
                }
                if (val <= maxAllowedThreshold)
                    finalMinThresholdRequiredList.Add(val);
            }
            return finalMinThresholdRequiredList.ToArray();
        }

        public class Clusters
        {
            public int[] clusters;
            public double? minThresholdRequired;
        }

        private static Clusters FindClusters(int cMAType, double[] derivatives, double[] minThresholds)
        {
            Clusters c = new BAL.CMA.Clusters();
            Clusters optimalC = null;

            for (int ii = 0; ii < minThresholds.Length; ii++)
            {
                c.minThresholdRequired = minThresholds[ii];
                c.clusters = Common.Statistics.FindClusters(derivatives, minThresholds[ii]).ToArray();

                if (cMAType == (int)CMAType.Regular)
                {
                    if (c.clusters.Length <= 2)
                    {
                        if (optimalC == null)
                            optimalC = new BAL.CMA.Clusters();
                        optimalC.minThresholdRequired = c.minThresholdRequired;
                        optimalC.clusters = c.clusters;
                    }

                    if (c.clusters.Length == 2)
                        break;
                }
                else if (cMAType == (int)CMAType.ShortSale)
                {
                    if (optimalC == null)
                    {
                        optimalC = new BAL.CMA.Clusters();
                        optimalC.minThresholdRequired = c.minThresholdRequired;
                        optimalC.clusters = c.clusters;
                    }
                    else if (optimalC.clusters.Length < c.clusters.Length)
                    {
                        optimalC.minThresholdRequired = c.minThresholdRequired;
                        optimalC.clusters = c.clusters;
                    }
                }
                else
                {
                    if (optimalC == null)
                    {
                        optimalC = new BAL.CMA.Clusters();
                        optimalC.minThresholdRequired = c.minThresholdRequired;
                        optimalC.clusters = c.clusters;
                    }
                    else if (optimalC.clusters.Length < c.clusters.Length)
                    {
                        optimalC.minThresholdRequired = c.minThresholdRequired;
                        optimalC.clusters = c.clusters;
                    }
                }
            }
            return optimalC;
        }

        public class KDECluster
        {
            public double minValue;
            public double maxValue;
            public double threshold;
            public int samples;
            public string message;
        }

        private static List<KDECluster> FindClustersWithKDE(int cMAType, double[] kdeDensityPoints, double[] minThresholds, double[] priceArray)
        {
            List<KDECluster> clusters = new List<KDECluster>();

            for (int k = 0; k < kdeDensityPoints.Length; k++)
            {
                for (int ii = 0; ii < minThresholds.Length; ii++)
                {
                    KDECluster cluster = new KDECluster();
                    double maxVal = kdeDensityPoints[k] * (1 + minThresholds[ii]);
                    double minVal = kdeDensityPoints[k] * (1 - minThresholds[ii]);
                    cluster.samples = 0;
                    cluster.threshold = minThresholds[ii];
                    for (int j = 0; j < priceArray.Length; j++)
                    {
                        if (priceArray[j]>= minVal && priceArray[j]<= maxVal)
                        {
                            if (cluster.samples==0)
                            {
                                cluster.minValue = priceArray[j];
                                cluster.maxValue = priceArray[j];
                            }
                            else if (cluster.minValue > priceArray[j])
                                cluster.minValue = priceArray[j];
                            else if (cluster.maxValue < priceArray[j])
                                cluster.maxValue = priceArray[j];
                            cluster.samples++;
                        }              
                    }
                    if (cluster.samples >= 3)
                        clusters.Add(cluster);
                }
            }
            
            return clusters;
        }

        public class ClusterSelection
        {
            public int clusterValue;
            public KDECluster kDEcluster;
            public string message;
        }

        private static ClusterSelection SelectCluster(int cMAType, List<KDECluster> clusters, double medianPrice)
        {
            ClusterSelection cs = new ClusterSelection();
            cs.clusterValue = -1;
            KDECluster[] clustersArray = clusters.ToArray();

            switch (clustersArray.Length)
            {
                case 0:
                    cs.message = "No price clusters found";
                    break;
                case 1:
                    cs.message = "Single price cluster found";
                    cs.kDEcluster = clustersArray[0];
                    break;
                default:
                    cs.message = string.Format("{0} clusters found.", clusters.Count);
                    switch (cMAType)
                    {
                        case (int)CMAType.Regular:
                            cs.kDEcluster = null;
                            cs.message += " Picking the largest cluster containing median price with min value closest to the median Price";
                            for (int i = 0; i < clustersArray.Length; i++)
                            {   if (cs.kDEcluster == null)
                                {  if ((clustersArray[i].minValue <= medianPrice) && (clustersArray[i].maxValue >= medianPrice))
                                        cs.kDEcluster = clustersArray[i];
                                }
                                else if ((clustersArray[i].minValue <= medianPrice) && (clustersArray[i].maxValue >= medianPrice))
                                {
                                    if (clustersArray[i].minValue > cs.kDEcluster.minValue)
                                        cs.kDEcluster = clustersArray[i];
                                    else if (clustersArray[i].minValue == cs.kDEcluster.minValue)
                                    {
                                        if (cs.kDEcluster.samples < clustersArray[i].samples)
                                            cs.kDEcluster = clustersArray[i];
                                    }
                                }
                            }
                            if (cs.kDEcluster==null)
                            {
                                cs.message += " Picking the largest cluster  where median price  is less than with min value but closest to the median Price";
                                for (int i = 0; i < clustersArray.Length; i++)
                                {
                                    if (cs.kDEcluster == null)
                                    {  if (clustersArray[i].minValue >= medianPrice)
                                            cs.kDEcluster = clustersArray[i];
                                    }
                                    else if (clustersArray[i].minValue >= medianPrice)
                                    {
                                        if (clustersArray[i].minValue < cs.kDEcluster.minValue)
                                            cs.kDEcluster = clustersArray[i];
                                        else if (clustersArray[i].minValue == cs.kDEcluster.minValue)
                                        {
                                            if (cs.kDEcluster.samples < clustersArray[i].samples)
                                                cs.kDEcluster = clustersArray[i];
                                        }
                                    }
                                }
                            }
                            break;
                        case (int)CMAType.ShortSale:
                            cs.message += " Picking the smallest cluster with minimum minValue.";
                            cs.kDEcluster = null;
                            for (int i = 0; i < clustersArray.Length; i++)
                            {
                                if (clustersArray[i].maxValue <= medianPrice)
                                {
                                    if (cs.kDEcluster == null)
                                        cs.kDEcluster = clustersArray[i];
                                    else if (cs.kDEcluster.minValue > clustersArray[i].minValue)
                                        cs.kDEcluster = clustersArray[i];
                                    else if (cs.kDEcluster.minValue == clustersArray[i].minValue)
                                    {
                                        if (cs.kDEcluster.samples > clustersArray[i].samples)
                                            cs.kDEcluster = clustersArray[i];
                                    }
                                }
                            }
                            break;
                        case (int)CMAType.Rehab:
                            cs.message += " Picking the smallest cluster with maximum maxValue.";
                            cs.kDEcluster = null;
                            for (int i = 0; i < clustersArray.Length; i++)
                            {
                                if (clustersArray[i].minValue >= medianPrice)
                                {
                                    if (cs.kDEcluster == null)
                                        cs.kDEcluster = clustersArray[i];
                                    else if (cs.kDEcluster.maxValue < clustersArray[i].maxValue)
                                        cs.kDEcluster = clustersArray[i];
                                    else if (cs.kDEcluster.maxValue == clustersArray[i].maxValue)
                                    {
                                        if (cs.kDEcluster.samples > clustersArray[i].samples)
                                            cs.kDEcluster = clustersArray[i];
                                    }
                                }
                            }
                            break;
                    }
                    break;
            }
            if (cs.kDEcluster == null)
                cs.message += " Cannot find an appropriate price cluster";
            return cs;
        }

        private static ClusterSelection SelectCluster(int cMAType, Clusters clusterInfo)
        {
            ClusterSelection cs = new ClusterSelection();
            cs.clusterValue = -1;

            switch (clusterInfo.clusters.Length)
            {
                case 0:
                    cs.message = "No price clusters found";
                    break;
                case 1:
                    cs.message = "Single price cluster found";
                    cs.clusterValue = 0;
                    break;
                case 2:
                    cs.message = "Two price clusters found.";
                    switch (cMAType)
                    {
                        case (int)CMAType.Regular:
                            cs.message += " Picking the second cluster.";
                            cs.clusterValue = 1;
                            break;
                        case (int)CMAType.ShortSale:
                            cs.message += " Picking the first cluster.";
                            cs.clusterValue = 0;
                            break;
                        case (int)CMAType.Rehab:
                            cs.message += " Picking the second cluster.";
                            cs.clusterValue = 1;
                            break;
                    }
                    break;
                default:
                    cs.message = string.Format("{0} clusters found.", clusterInfo.clusters.Length);
                    switch (cMAType)
                    {
                        case (int)CMAType.Regular:
                            cs.message += " Cannot determine the right price cluster refine search.";
                            break;
                        case (int)CMAType.ShortSale:
                            cs.message += " Picking the first cluster.";
                            cs.clusterValue = 0;
                            break;
                        case (int)CMAType.Rehab:
                            cs.message += " Picking the last cluster.";
                            cs.clusterValue = clusterInfo.clusters.Length - 1;
                            break;
                    }
                    break;
            }

            return cs;
        }

        private static double[] ApplyGaussianKDE(double[] pricesArray)
        {
            double minValue, maxValue;
            int distance;
            int kernelSize;
            Common.DoubleList localDensityPoints = new Common.DoubleList();

            if (pricesArray.Length <= 1)
                localDensityPoints.ToArray();

            minValue = pricesArray[0];
            maxValue = pricesArray[pricesArray.Length - 1];
            distance = Convert.ToInt32(Math.Round(maxValue-minValue)+1);

            kernelSize = distance / (pricesArray.Length - 1);
            if (kernelSize % 2 == 0)
                kernelSize += 1;

            double[] SampleArray = new double[distance];

            for (int i = 0, j=0; i < distance; i++)
            {
                if (Convert.ToInt32(pricesArray[j]) - Convert.ToInt32(minValue) == i)
                {
                    SampleArray[i] = 1000;
                    j++;
                }
                else
                    SampleArray[i] = 0;

            }

            double[] densityArray = Common.Statistics.ApplyGaussianKDEV2(SampleArray, kernelSize, GAUSSIAN_SIGMA);

            double? localMaxima = 0;
            int? direction=null;
            for (int i = 0; i < distance; i++)
            {
                if (localMaxima == null)
                    localMaxima=densityArray[i];
                else if (densityArray[i]>= localMaxima)
                {
                    direction = 1;
                    localMaxima = densityArray[i];
                }
                else
                {   if (direction == 1)
                        localDensityPoints.Add(i + minValue);
                    direction = 0;
                    localMaxima = densityArray[i];
                }
            }

            return localDensityPoints.ToArray();
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

            if (results.Count == 0)
            {
                price.message = "No Comparables found";
                return price;
            }

            Common.DoubleList pricessqft = new Common.DoubleList();
            foreach (var comp in results)
                pricessqft.Add(Math.Round(comp.DeedAmount.GetValueOrDefault() / comp.GLA.GetValueOrDefault(),0));
            pricessqft.Sort();

            double medianPrice = Common.Statistics.Percentile(pricessqft, 50);

            double[] pricesArray = pricessqft.ToArray();
            //Round Prices
            for (int j = 0; j < pricesArray.Length; j++)
                pricesArray[j] = Math.Round(pricesArray[j], 0);

            double[] kDEDensityPoints = ApplyGaussianKDE(pricesArray);
            //double[] smoothValues = Common.Statistics.ApplyGaussianKDE(pricesArray, GAUSSIAN_KERNEL_SIZE, GAUSSIAN_SIGMA);

            double[] derivatives = Common.Statistics.DiscreteDerivative(pricesArray);
            for (int j = 0; j < derivatives.Length; j++)
            {
                double value = derivatives[j];
                if (Math.Round(value, 2)>= value)
                    derivatives[j] = Math.Round(value, 2);
                else
                    derivatives[j] = Math.Round(value + 0.005, 2);
            }

            //What is the minimum threshold required to get a cluster from our comparables
            double[] minThresholds = GetThresholdsForClusters(cMAType, derivatives);

            if (minThresholds.Length < 1)
            {
                price.message = "At least 3 comparables required for pricing";
                return price;
            }

            //Using minThresholds find clusters in comparables with KDE points
            List<KDECluster> clusters = FindClustersWithKDE(cMAType, kDEDensityPoints, minThresholds, pricesArray);
            //Using minThresholds find clusters in comparables
            Clusters clusterInfo = FindClusters(cMAType, derivatives, minThresholds);

            //Select the appropriate cluster based on CMAType
            ClusterSelection clusterSelection = SelectCluster(cMAType, clusterInfo);
            ClusterSelection clusterSelectionKDE = SelectCluster(cMAType, clusters, medianPrice);
            
            //mix and max values from the cluster
            double ? minClusterValue = null, maxClusterValue = null;

            if (clusterSelectionKDE.kDEcluster!=null)
            {
                minClusterValue = clusterSelectionKDE.kDEcluster.minValue;
                maxClusterValue = clusterSelectionKDE.kDEcluster.maxValue;
                price.message = clusterSelectionKDE.message;
            }
            else if (clusterSelection.clusterValue != -1)
            {
                minClusterValue = Math.Round(pricesArray[clusterInfo.clusters[clusterSelection.clusterValue]],0);
                maxClusterValue = Math.Round(pricesArray[clusterInfo.clusters[clusterSelection.clusterValue] + 1],0);
                int i = clusterInfo.clusters[clusterSelection.clusterValue] + 1;
                while (i < derivatives.Length)
                {
                    if (derivatives[i] <= clusterInfo.minThresholdRequired.GetValueOrDefault())
                        maxClusterValue = Math.Round(pricesArray[i + 1],0);
                    else
                        break;
                    i++;
                }
                price.message = clusterSelection.message;
            }
            
            //use min max values to compute subject price
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
                                                          int? classMatchType, bool? isNotIntraFamily, bool? isSelleraCompany, bool? isBuyeraCompany, int? minSimilarity)
        {
            AutomatedCMAResults cmaResult = new AutomatedCMAResults();
            
            cmaResult.results=DAL.CMA.GetComparables(algorithmType, subjectBBL, maxRecords, sameNeighborhood, sameSchoolDistrict, sameZip, sameBlock, sameStreet, 
                                                     monthOffset, minSalePrice, maxSalePrice, classMatchType, isNotIntraFamily, isSelleraCompany, isBuyeraCompany, minSimilarity);

            cmaResult.price = getSuggestedPropertyPrices((int)CMAType.Regular, subjectBBL, cmaResult.results);

            return cmaResult;
        }

        public static AutomatedCMAResults GetAutomatedShortSaleCMA(string subjectBBL, AutomatedCMAFilters fl)
        {
            AutomatedCMAResults cmaResult = new AutomatedCMAResults();

            cmaResult.results = DAL.CMA.GetComparables(fl.algorithmType, subjectBBL, fl.basicFilter.maxRecords, fl.basicFilter.sameNeighborhood, fl.basicFilter.sameSchoolDistrict,
                                                       fl.basicFilter.sameZip, fl.basicFilter.sameBlock, fl.basicFilter.sameStreet, fl.basicFilter.monthOffset, fl.basicFilter.minSalePrice,
                                                       fl.basicFilter.maxSalePrice, fl.basicFilter.classMatchType, fl.basicFilter.isNotIntraFamily, fl.basicFilter.isSelleraCompany, 
                                                       fl.basicFilter.isBuyeraCompany, fl.basicFilter.minSimilarity);

            cmaResult.price = getSuggestedPropertyPrices((int)CMAType.ShortSale, subjectBBL, cmaResult.results);

            return cmaResult;
        }

        public static AutomatedCMAResults GetAutomatedRehabCMA(string subjectBBL, AutomatedCMAFilters fl)
        {
            AutomatedCMAResults cmaResult = new AutomatedCMAResults();

            cmaResult.results = DAL.CMA.GetComparables(fl.algorithmType, subjectBBL, fl.basicFilter.maxRecords, fl.basicFilter.sameNeighborhood, fl.basicFilter.sameSchoolDistrict,
                                                       fl.basicFilter.sameZip, fl.basicFilter.sameBlock, fl.basicFilter.sameStreet, fl.basicFilter.monthOffset, fl.basicFilter.minSalePrice,
                                                       fl.basicFilter.maxSalePrice, fl.basicFilter.classMatchType, fl.basicFilter.isNotIntraFamily, fl.basicFilter.isSelleraCompany, 
                                                       fl.basicFilter.isBuyeraCompany, fl.basicFilter.minSimilarity);

            cmaResult.price = cmaResult.price = getSuggestedPropertyPrices((int)CMAType.Rehab, subjectBBL, cmaResult.results);

            return cmaResult;
        }
    }
}