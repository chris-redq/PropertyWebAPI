//-----------------------------------------------------------------------
// <copyright file="CasesController.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web.Http;
    using System.Web.Http.Description;
    using NYCMADB;
    using AutoMapper;

  

    /// <summary>  
    /// This controller handles all api requests associated with CMA related information
    /// </summary>  
    [Authorize]
    public class CMAController : ApiController
    {
        #region Statistics

        /// <summary>  
        ///     Use this api to get suggested prices for a property in NYC
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>
        /// <returns>
        ///     Returns Average Low, Median and Avg High Price for a property in NYC
        /// </returns>
        [Route("api/cma/{propertyBBL}/SuggestedPrices")]
        [ResponseType(typeof(DAL.SuggestedPropertyPrices))]
        public IHttpActionResult GetSuggestedPrices(string propertyBBL)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            try
            {
                var result = DAL.CMA.GetSuggestedPricesForAProperty(propertyBBL);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL: {0}{1}", propertyBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>  
        ///     Use this api to get sales data by month for the neighborhood around a given property
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>
        /// <param name="timeSpanInYears">
        ///     Time span in Years over which the trend data is requested. Default Value is 1. Valid range is 1-10.
        /// </param>
        /// <returns>
        ///     Returns sales data by month for the neighborhood around a given property
        /// </returns>
        [Route("api/cma/{propertyBBL}/salestrend")]
        [ResponseType(typeof(List<DAL.SalesDataDetailsByMonth>))]
        public IHttpActionResult GetSalesTrendAroundProperty(string propertyBBL, int timeSpanInYears=1)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            if (timeSpanInYears < 1 || timeSpanInYears > 10)
                return this.BadRequest("Incorrect time span - Valid values 1-5 years");

            try
            {
                var assessment = DAL.CMA.GetAssessmentRecord(propertyBBL);
                if (assessment==null)
                    return this.BadRequest("Incorrect BBL - Borough Block Lot number not found");

                var result = DAL.CMA.GetSalesTrend(assessment.NeighborhoodName, timeSpanInYears);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL: {0}{1}", propertyBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>  
        ///     Use this api to get sales price data by month for the neighborhood around a given property
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>
        /// <param name="timeSpanInYears">
        ///     Time span in Years over which the trend data is requested. Default Value is 1. Valid range is 1-10.
        /// </param>
        /// <returns>
        ///     Returns sales price data by month for the neighborhood around a given property
        /// </returns>
        [Route("api/cma/{propertyBBL}/salespricetrend")]
        [ResponseType(typeof(List<DAL.PriceDetailsByMonth>))]
        public IHttpActionResult GetSalesPriceTrendAroundProperty(string propertyBBL, int timeSpanInYears = 1)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            if (timeSpanInYears < 1 || timeSpanInYears > 10)
                return this.BadRequest("Incorrect time span - Valid values 1-5 years");

            try
            {
                var assessment = DAL.CMA.GetAssessmentRecord(propertyBBL);
                if (assessment == null)
                    return this.BadRequest("Incorrect BBL - Borough Block Lot number not found");

                var result = DAL.CMA.GetSalesPriceTrend(assessment.NeighborhoodName, assessment.BuildingClass, timeSpanInYears);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL: {0}{1}", propertyBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>  
        ///     Use this api to get price per sq-ft data by month for the neighborhood around a given property
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>
        /// <param name="timeSpanInYears">
        ///     Time span in Years over which the trend data is requested. Default Value is 1. Valid range is 1-10.
        /// </param>
        /// <returns>
        ///     Returns sales price per sq-ft data by month for the neighborhood around a given property
        /// </returns>
        [Route("api/cma/{propertyBBL}/salespricepersqfttrend")]
        [ResponseType(typeof(List<DAL.PricePerSqftDetailsByMonth>))]
        public IHttpActionResult GetPricePerSqFtTrendAroundProperty(string propertyBBL, int timeSpanInYears = 1)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            if (timeSpanInYears < 1 || timeSpanInYears > 10)
                return this.BadRequest("Incorrect time span - Valid values 1-5 years");

            try
            {
                var assessment = DAL.CMA.GetAssessmentRecord(propertyBBL);
                if (assessment == null)
                    return this.BadRequest("Incorrect BBL - Borough Block Lot number not found");

                var result = DAL.CMA.GetPricePerSqftTrend(assessment.NeighborhoodName, assessment.BuildingClass, timeSpanInYears);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL: {0}{1}", propertyBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
        #endregion

        #region Comparables Old Methods
        /// <summary>  
        ///     Use this api to get a list of comparables for the given property and associated parameters
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>
        /// <param name="algorithmType">Valid Option are G, O and E. T returns all sales ordered by distance</param>
        /// <param name="maxRecords">Total number of comps to be returned</param>
        /// <param name="monthOffset">Month in past, always negative, default value is -12</param>
        /// <param name="classMatchType">0 - For same BuildingClass, 1  - For Expanded Building Classes based on Subject Building Class, 2 - To ignore Building Class</param>
        /// <param name="sameNeighboorhood">0 or 1. 0 means ignore neighborhood and 1 means search in same neighborhood</param>
        /// <param name="sameSchoolDistrict">0 or 1. 0 means ignore school district and 1 means search in same school district. Default is 1</param>
        /// <param name="sameZip">0 or 1. 0 means ignore zip code and 1 means search in same zip code. Default is 0</param>
        /// <param name="sameBlock">0 or 1. 0 means ignore address block and 1 means search in same block. Default is 0</param>
        /// <param name="sameStreetName">0 or 1. 0 means ignore street and 1 means search in same street. Default is 0</param>
        /// <param name="minSalePrice">Minimum sale price for consideration. Default is 10000</param>
        /// <param name="maxSalePrice">Maximum Sale price for consideration. Default is ignore max sale price</param>
        /// <param name="isNotIntraFamily">Null or 1. 1 means only arm length sales are considered.</param>
        /// <param name="isBuyeraCompany">Null or 1. 1 means buyer is company.</param>
        /// <param name="isSelleraCompany">Null or 1. 1 means seller is a company</param>
        /// <param name="minSimilarity"></param>
        /// <returns>
        ///     Returns sales price per a list of comparables for the given property and associated parameters data by month for the neighborhood around a given property
        /// </returns>
        [Route("api/cma/{propertyBBL}/comparables")]
        [ResponseType(typeof(List<DAL.CMAResult>))]
        public IHttpActionResult GetComparables(string propertyBBL, string algorithmType="O", int? maxRecords=null, bool? sameNeighboorhood=null, bool? sameSchoolDistrict=null,
                                                bool? sameZip=null, bool? sameBlock=null, bool? sameStreetName=null, int? monthOffset=null, double? minSalePrice=null, 
                                                double? maxSalePrice=null, int? classMatchType=null, bool? isNotIntraFamily=null, bool? isSelleraCompany=null, bool? isBuyeraCompany=null,
                                                int? minSimilarity=null)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            try
            {
                var result = DAL.CMA.GetComparables(algorithmType, propertyBBL, maxRecords, sameNeighboorhood, sameSchoolDistrict, sameZip, sameBlock, sameStreetName, monthOffset,
                                                     minSalePrice, maxSalePrice, classMatchType, isNotIntraFamily, isSelleraCompany, isBuyeraCompany, minSimilarity);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL: {0}{1}", propertyBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>  
        ///     Use this api is deprecated use the other POST based api to get a list of comparables for the given property and associated parameters
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>
        /// <param name="maxRecs">Total number of comps to be returned.</param>
        /// <param name="monthOffset">Month in past, always negative, default value is -12</param>
        /// <param name="classMatch">0 - For same BuildingClass, 1  - For Expanded Building Classes based on Subject Building Class, 2 - To ignore Building Class</param>
        /// <param name="smNeighborhood">0 or 1. 0 means ignore neighborhood and 1 means search in same neighborhood. Default is 0</param>
        /// <param name="smSchDist">0 or 1. 0 means ignore school district and 1 means search in same school district. Default is 1</param>
        /// <param name="smZip">0 or 1. 0 means ignore zip code and 1 means search in same zip code. Default is 0</param>
        /// <param name="smBlock">0 or 1. 0 means ignore address block and 1 means search in same block. Default is 0</param>
        /// <param name="smStreet">0 or 1. 0 means ignore street and 1 means search in same street. Default is 0</param>
        /// <param name="minP">Minimum sale price for consideration. Default is 10000</param>
        /// <param name="maxP">Maximum Sale price for consideration. Default is ignore max sale price</param>
        /// <param name="NotIntraFamily">Null or 1. 1 means only arm length sales are considered.</param>
        /// <param name="BComp">Null or 1. 1 means buyer is company.</param>
        /// <param name="SComp">Null or 1. 1 means seller is a company</param>
        /// <param name="dist">Search distance in miles. Values can be decimals. Default is 1.0</param>
        /// <param name="GLARng">Range of GLA Values for consideration based on percentage difference from subject. Format is CSV, only positive Values. Single values are considered as upper limit. Example: 10 or 10,20</param>
        /// <param name="LARng">Range of LA Values for consideration based on percentage difference from subject. Format is CSV, only positive Values. Single values are considered as upper limit. Example: 15 or 15,25</param>
        /// <param name="BldgFrontRng">Range of Building Frontage Values for consideration in ft. Format is CSV. Single values are considered as upper limit. Example: 20 or Null,20 or 16,NULL or 16,20</param>
        /// <param name="BldgDepthRng">Range of Building Depth Values for consideration in ft. Format is CSV. Single values are considered as upper limit. Example: 45 or Null,45 or 30,NULL or 30,45</param>
        /// <param name="lotFrontRng">Range of Lot Frontage Values for consideration in ft. Format is CSV. Single values are considered as upper limit. Example: 25 or Null,25 or 20,NULL or 20,25</param>
        /// <param name="lotDepthRng">Range of Building Depth Values for consideration in ft. Format is CSV. Single values are considered as upper limit. Example: 80 or Null,80 or 65,NULL or 65,80</param>
        /// <returns>
        ///     Returns sales price per a list of comparables for the given property and associated parameters data by month for the neighborhood around a given property
        /// </returns>
        [Route("api/cma/{propertyBBL}/manualcomparables")]
        [ResponseType(typeof(List<DAL.CMAManualResult>))]
        public IHttpActionResult GetManualComparables(string propertyBBL, int? maxRecs = null, bool? smNeighborhood = null, bool? smSchDist = null, bool? smZip = null, bool? smBlock = null, 
                                                      bool? smStreet = null, int? monthOffset = null, double? minP = null, double? maxP = null, int? classMatch = null, bool? NotIntraFamily = null, 
                                                      bool? SComp = null, bool? BComp = null, double? dist = null, string GLARng = null, string LARng = null, string BldgFrontRng = null, 
                                                      string BldgDepthRng = null, string lotFrontRng = null, string lotDepthRng = null)
        {
            int? gLAHiRange = null, gLALoRange = null, lAHiRange = null, lALoRange = null;
            int? buildingFrontageHiRange = null, buildingFrontageLoRange = null, buildingDepthHiRange = null, buildingDepthLoRange = null;
            int? lotFrontageHiRange = null, lotFrontageLoRange = null, lotDepthHiRange = null, lotDepthLoRange = null;

            if (!BAL.BBL.IsValidFormat(propertyBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            if (!InitializePercentageRangeValues(GLARng, ref gLALoRange, ref gLAHiRange))
                return this.BadRequest("Incorrect GLA Range. Correct format 99, 99,99");

            if (!InitializePercentageRangeValues(LARng, ref lALoRange, ref lAHiRange))
                return this.BadRequest("Incorrect GLA Range. Correct format 99, 99,99");

            if (!InitializeRangeValues(BldgFrontRng, ref buildingFrontageLoRange, ref buildingFrontageHiRange))
                return this.BadRequest("Incorrect Building Frontage Range. Correct format 9999 or Null, 9999 or 9999, NULL or 9999, 9999");

            if (!InitializeRangeValues(BldgDepthRng, ref buildingDepthLoRange, ref buildingDepthHiRange))
                return this.BadRequest("Incorrect Building Depth Range. Correct format 9999 or Null, 9999 or 9999, NULL or 9999, 9999");

            if (!InitializeRangeValues(lotFrontRng, ref lotFrontageLoRange, ref lotFrontageHiRange))
                return this.BadRequest("Incorrect Lot Frontage Range. Correct format 9999 or Null, 9999 or 9999, NULL or 9999, 9999");

            if (!InitializeRangeValues(lotDepthRng, ref lotDepthLoRange, ref lotDepthHiRange))
                return this.BadRequest("Incorrect Lot Depth Range. Correct format 9999 or Null, 9999 or 9999, NULL or 9999, 9999");

            try
            {
                var result = DAL.CMA.GetManualComparables(propertyBBL, maxRecs, smNeighborhood, smSchDist, smZip, smBlock, smStreet, monthOffset, minP, maxP, classMatch, NotIntraFamily, 
                                                          SComp, BComp, dist, gLAHiRange, gLALoRange, lAHiRange, lALoRange, buildingFrontageHiRange, buildingFrontageLoRange, buildingDepthHiRange,
                                                          buildingDepthLoRange, lotFrontageHiRange, lotFrontageLoRange, lotDepthHiRange, lotDepthLoRange);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL: {0}{1}", propertyBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        private bool InitializeRangeValues(string inStr, ref int? LoRangeValue, ref int? HiRangeValue)
        {
            string[] strVals = Common.Conversions.ParseCSV(inStr);
            if (strVals != null)
            {
                switch (strVals.Length)
                {
                    case 0:
                        break;
                    case 1:
                        HiRangeValue = Common.Conversions.ToIntorNull(strVals[0]);
                        break;
                    case 2:
                        LoRangeValue = Common.Conversions.ToIntorNull(strVals[0]);
                        HiRangeValue = Common.Conversions.ToIntorNull(strVals[1]);
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }

        private bool InitializePercentageRangeValues(string inStr, ref int? LoRangeValue, ref int? HiRangeValue)
        {
            string[] strVals = Common.Conversions.ParseCSV(inStr);
            if (strVals != null)
            {
                switch (strVals.Length)
                {
                    case 0:
                        break;
                    case 1:
                        HiRangeValue = Common.Conversions.ToIntorNull(strVals[0]);
                        LoRangeValue = Common.Conversions.ToIntorNull(strVals[0]);
                        if (HiRangeValue == null)
                            return false;
                        break;
                    case 2:
                        LoRangeValue = Common.Conversions.ToIntorNull(strVals[0]);
                        HiRangeValue = Common.Conversions.ToIntorNull(strVals[1]);
                        if (LoRangeValue == null)
                            LoRangeValue = HiRangeValue;
                        if (HiRangeValue == null)
                            HiRangeValue = LoRangeValue;
                        if ((LoRangeValue == null) || (LoRangeValue == null))
                            return false;
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }
        #endregion

        #region New Comparable Methods
        /// <summary>Use this api to get a subject property's details</summary>  
        /// <param name="subjectBBL">Borough Block Lot Number of the subject property. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number</param>
        /// <returns>Returns subject property's details</returns>
        [Route("api/cma/{subjectBBL}")]
        [ResponseType(typeof(ShowCMASubject_Result))]
        public IHttpActionResult GetSubjectDetails(string subjectBBL)
        {
            if (!BAL.BBL.IsValidFormat(subjectBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            try
            {
                var result = DAL.CMA.GetSubject(subjectBBL);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL: {0}{1}", subjectBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>  
        ///     Deprecated - Use this api to get a list of comparables for the given property and associated parameters
        /// </summary>  
        /// <param name="subjectBBL">Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number</param>
        /// <param name="filters">Filters to refine comparable</param>
        /// <returns>Returns sales price per a list of comparables for the given property and associated parameters data by month for the neighborhood around a given property</returns>
        [Route("api/cma/{subjectBBL}/comparables")]
        [ResponseType(typeof(List<DAL.CMAResult>))]
        [HttpPost]
        public IHttpActionResult GetComparables(string subjectBBL, BAL.AutomatedCMAFilters filters)
        {
            if (!BAL.BBL.IsValidFormat(subjectBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            try
            {
                var result = DAL.CMA.GetComparables(filters.algorithmType, subjectBBL, filters.basicFilter.maxRecords, filters.basicFilter.sameNeighborhood, 
                                                    filters.basicFilter.sameSchoolDistrict, filters.basicFilter.sameZip, filters.basicFilter.sameBlock, 
                                                    filters.basicFilter.sameStreet, filters.basicFilter.monthOffset, filters.basicFilter.minSalePrice,
                                                    filters.basicFilter.maxSalePrice, filters.basicFilter.classMatchType, filters.basicFilter.isNotIntraFamily, 
                                                    filters.basicFilter.isSelleraCompany, filters.basicFilter.isBuyeraCompany, filters.basicFilter.minSimilarity);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL: {0}{1}", subjectBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>  
        ///     Use this api to get an automated CMA for a given property and associated parameters
        /// </summary>  
        /// <param name="subjectBBL">Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number</param>
        /// <param name="filters">Filters to refine comparables</param>
        /// <returns>Returns a list of comparables and price range for the subject property</returns>
        [Route("api/cma/{subjectBBL}/automatedCMA")]
        [ResponseType(typeof(BAL.AutomatedCMAResults))]
        [HttpPost]
        public IHttpActionResult RunAutomatedCMA(string subjectBBL, BAL.AutomatedCMAFilters filters)
        {
            if (!BAL.BBL.IsValidFormat(subjectBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            if (filters==null)
                return this.BadRequest("Malformed filter - Check the api body");

            try
            {
                BAL.AutomatedCMAResults result = null;
                switch(filters.intent)
                {
                    case (int)CMAType.Regular:
                        result = BAL.CMA.GetAutomatedCMA(filters.algorithmType, subjectBBL, filters.basicFilter.maxRecords, filters.basicFilter.sameNeighborhood,
                                                         filters.basicFilter.sameSchoolDistrict, filters.basicFilter.sameZip, filters.basicFilter.sameBlock,
                                                         filters.basicFilter.sameStreet, filters.basicFilter.monthOffset, filters.basicFilter.minSalePrice,
                                                         filters.basicFilter.maxSalePrice, filters.basicFilter.classMatchType, filters.basicFilter.isNotIntraFamily,
                                                         filters.basicFilter.isSelleraCompany, filters.basicFilter.isBuyeraCompany, filters.basicFilter.minSimilarity);
                        break;
                    case (int)CMAType.ShortSale:
                        result = BAL.CMA.GetAutomatedShortSaleCMA(subjectBBL, filters);
                        break;
                    case (int)CMAType.Rehab:
                        result = BAL.CMA.GetAutomatedRehabCMA(subjectBBL, filters);
                        break;
                    default:
                        return this.BadRequest("Incorrect Type of CMA request - Valid values for intent are 1, 2, 3"); ;
                }
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered while running automated for BBL: {0}{1}", subjectBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }


        /// <summary>  
        ///     Use this api to get an automated CMA for a given property and associated parameters
        /// </summary>  
        /// <param name="subjectBBL">Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number</param>
        /// <param name="filters">Filters to refine comparables</param>
        /// <returns>Returns a list of comparables and price range for the subject property</returns>
        [Route("api/cma/{subjectBBL}/multipleautomatedCMA")]
        [ResponseType(typeof(List<BAL.AutomatedBatchCMAResults>))]
        [HttpPost]
        public IHttpActionResult RunMultipleAutomatedCMAs(string subjectBBL, BAL.AutomatedCMAFilters[] filters)
        {
            if (!BAL.BBL.IsValidFormat(subjectBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            if (filters == null)
                return this.BadRequest("Malformed filter - Check the api body");

            List<BAL.AutomatedBatchCMAResults> resultList = new List<BAL.AutomatedBatchCMAResults>();

            for (int i=0; i < filters.Length; i++)
            {
                try
                {
                    BAL.AutomatedCMAResults result = null;
                    switch (filters[i].intent)
                    {
                        case (int)CMAType.Regular:
                            result = BAL.CMA.GetAutomatedCMA(filters[i].algorithmType, subjectBBL, filters[i].basicFilter.maxRecords, filters[i].basicFilter.sameNeighborhood,
                                                             filters[i].basicFilter.sameSchoolDistrict, filters[i].basicFilter.sameZip, filters[i].basicFilter.sameBlock,
                                                             filters[i].basicFilter.sameStreet, filters[i].basicFilter.monthOffset, filters[i].basicFilter.minSalePrice,
                                                             filters[i].basicFilter.maxSalePrice, filters[i].basicFilter.classMatchType, filters[i].basicFilter.isNotIntraFamily,
                                                             filters[i].basicFilter.isSelleraCompany, filters[i].basicFilter.isBuyeraCompany, filters[i].basicFilter.minSimilarity);
                            break;
                        case (int)CMAType.ShortSale:
                            result = BAL.CMA.GetAutomatedShortSaleCMA(subjectBBL, filters[i]);
                            break;
                        case (int)CMAType.Rehab:
                            result = BAL.CMA.GetAutomatedRehabCMA(subjectBBL, filters[i]);
                            break;
                        default:
                            return this.BadRequest("Incorrect Type of CMA request - Valid values for intent are 1, 2, 3"); ;
                    }
                    BAL.AutomatedBatchCMAResults batchresult = new BAL.AutomatedBatchCMAResults();
                    batchresult.cma = result;
                    batchresult.filter = filters[i];
                    resultList.Add(batchresult);
                }
                catch (Exception e)
                {
                    Common.Logs.log().Error(string.Format("Exception encountered while running automated for BBL: {0}{1}", subjectBBL, Common.Logs.FormatException(e)));
                    return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
                }
            }
            return Ok(resultList);
        }


        /// <summary>
        /// Use this api to get a list of sales comparables for the given property and associated filters
        /// </summary>
        /// <param name="subjectBBL"></param>
        /// <param name="filters">Filters to refine comparable sales</param>
        /// <returns>Return a list of sales conforming with subject property and filters provided</returns>
        [Route("api/cma/{subjectBBL}/comparablesales")]
        [ResponseType(typeof(List<DAL.CMAManualResult>))]
        [HttpPost]
        public IHttpActionResult GetManualComparables(string subjectBBL, [FromBody] BAL.ManualCMAFilters filters)
        {
            if (!BAL.BBL.IsValidFormat(subjectBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            if ((filters.additionalFilter.gLAMin == null) && (filters.additionalFilter.gLAMax != null))
                filters.additionalFilter.gLAMin = filters.additionalFilter.gLAMax;
            if ((filters.additionalFilter.gLAMin != null) && (filters.additionalFilter.gLAMax == null))
                filters.additionalFilter.gLAMax = filters.additionalFilter.gLAMin;

            if ((filters.additionalFilter.lotAreaMin == null) && (filters.additionalFilter.lotAreaMax != null))
                filters.additionalFilter.lotAreaMin = filters.additionalFilter.lotAreaMax;
            if ((filters.additionalFilter.lotAreaMin != null) && (filters.additionalFilter.lotAreaMax == null))
                filters.additionalFilter.lotAreaMax = filters.additionalFilter.lotAreaMin;

            try
            {
                var result = DAL.CMA.GetManualComparables(subjectBBL, filters.basicFilter.minSimilarity, filters.basicFilter.sameNeighborhood, filters.basicFilter.sameSchoolDistrict, 
                                                          filters.basicFilter.sameZip, filters.basicFilter.sameBlock, filters.basicFilter.sameStreet, filters.basicFilter.monthOffset, 
                                                          filters.basicFilter.minSalePrice, filters.basicFilter.maxSalePrice, filters.basicFilter.classMatchType, filters.basicFilter.isNotIntraFamily, 
                                                          filters.basicFilter.isSelleraCompany, filters.basicFilter.isBuyeraCompany, filters.additionalFilter.distance, filters.additionalFilter.gLAMax, 
                                                          filters.additionalFilter.gLAMin, filters.additionalFilter.lotAreaMax, filters.additionalFilter.lotAreaMin, filters.additionalFilter.buildingFrontageMax, 
                                                          filters.additionalFilter.buildingFrontageMin, filters.additionalFilter.buildingDepthMax, filters.additionalFilter.buildingDepthMin,
                                                          filters.additionalFilter.lotFrontageMax, filters.additionalFilter.lotFrontageMin, filters.additionalFilter.lotDepthMax, filters.additionalFilter.lotDepthMin);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL: {0}{1}", subjectBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>
        /// Use this API to get price estimates for a manual selection of comparables 
        /// </summary>
        /// <param name="subjectBBL">Borough Block Lot Number of the subject property. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number</param>
        /// <param name="manualCMA">Filters used for initial comparable sales</param>
        /// <returns>Estimated price for the subject property based on comparables selected</returns>
        [Route("api/cma/{subjectBBL}/estimatedpropertyprice")]
        [ResponseType(typeof(BAL.ManualCMAResult))]
        public IHttpActionResult PostManualComparables(string subjectBBL, [FromBody] BAL.ManualCMASelection manualCMA)
        {
            if (manualCMA==null)
                return this.BadRequest("Malformed body");

            if (String.IsNullOrEmpty(manualCMA.username))
                return this.BadRequest("Username cannot be null or empty");

            if (!BAL.BBL.IsValidFormat(subjectBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");
            if (manualCMA.comparables==null || manualCMA.comparables.Count==0)
                return this.BadRequest("No sales selected. Please select one or more sales");

            if (manualCMA.additionalFilter != null)
            {
                if ((manualCMA.additionalFilter.gLAMin == null) && (manualCMA.additionalFilter.gLAMax != null))
                    manualCMA.additionalFilter.gLAMin = manualCMA.additionalFilter.gLAMax;
                if ((manualCMA.additionalFilter.gLAMin != null) && (manualCMA.additionalFilter.gLAMax == null))
                    manualCMA.additionalFilter.gLAMax = manualCMA.additionalFilter.gLAMin;

                if ((manualCMA.additionalFilter.lotAreaMin == null) && (manualCMA.additionalFilter.lotAreaMax != null))
                    manualCMA.additionalFilter.lotAreaMin = manualCMA.additionalFilter.lotAreaMax;
                if ((manualCMA.additionalFilter.lotAreaMin != null) && (manualCMA.additionalFilter.lotAreaMax == null))
                    manualCMA.additionalFilter.lotAreaMax = manualCMA.additionalFilter.lotAreaMin;
            }
            else
               Common.Logs.log().Warn(string.Format("Additional Filter for Manual CMA for {0} by {1} is NULL", subjectBBL, manualCMA.username));

            if (manualCMA.basicFilter == null)
                Common.Logs.log().Warn(string.Format("Basic Filter for Manual CMA for {0} by {1} is NULL", subjectBBL, manualCMA.username));

            try
            {   
                var result = BAL.CMA.SaveManualComparables(subjectBBL, manualCMA);
                if (result == null)
                    throw new Exception("Internal Error function should not return null"); 
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL: {0}{1}", subjectBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>  
        ///     Use this api to get parties associated with a deed (buyers and sellers)
        /// </summary>  
        /// <param name="deedUniqueKey">
        ///     Document Unique Key associated with a Deed document in ACRIS
        /// </param>
        /// <returns>
        ///     Returns parties associated with a deed (buyers and sellers)
        /// </returns>
        [Route("api/cma/{deedUniqueKey}/parties")]
        [ResponseType(typeof(List<SaleParty>))]
        public IHttpActionResult GetDeedParties(string deedUniqueKey)
        {
            try
            {
                var result = DAL.CMA.GetDeedParties(deedUniqueKey);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for Deed: {0}{1}", deedUniqueKey, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
        #endregion
    }
}
