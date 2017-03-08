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
        [ResponseType(typeof(DAL.SuggestPropertPrices))]
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

                var result = DAL.CMA.GetSalesTrend(assessment.NTACode, timeSpanInYears);
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
        ///     Use this api to get price per sqft data by month for the neighborhood around a given property
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>
        /// <param name="timeSpanInYears">
        ///     Time span in Years over which the trend data is requested. Default Value is 1. Valid range is 1-10.
        /// </param>
        /// <returns>
        ///     Returns sales price per sqft data by month for the neighborhood around a given property
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

        #region Comparables
        /// <summary>  
        ///     Use this api to get a list of comparables for the given property and associated parameters
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>
        /// <param name="algorithmType"></param>
        /// <param name="maxRecords"></param>
        /// <param name="monthOffset"></param>
        /// <param name="classMatchType"></param>
        /// <param name="sameNeighboorhood"></param>
        /// <param name="sameSchoolDistrict"></param>
        /// <param name="sameZip"></param>
        /// <param name="sameBlock"></param>
        /// <param name="sameStreetName"></param>
        /// <param name="minSalePrice"></param>
        /// <param name="maxSalePrice"></param>
        /// <param name="isNotIntraFamily"></param>
        /// <param name="isBuyeraCompany"></param>
        /// <param name="isSelleraCompany"></param>
        /// <returns>
        ///     Returns sales price per a list of comparables for the given property and associated parameters data by month for the neighborhood around a given property
        /// </returns>
        [Route("api/cma/{propertyBBL}/comparables")]
        [ResponseType(typeof(List<DAL.CMAResult>))]
        public IHttpActionResult GetComparables(string propertyBBL, string algorithmType="O", int? maxRecords=null, bool? sameNeighboorhood=null, bool? sameSchoolDistrict=null,
                                                bool? sameZip=null, bool? sameBlock=null, bool? sameStreetName=null, int? monthOffset=null, double? minSalePrice=null, 
                                                double? maxSalePrice=null, int? classMatchType=null, bool? isNotIntraFamily=null, bool? isSelleraCompany=null, bool? isBuyeraCompany=null)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            try
            {
                var result = DAL.CMA.GetComparables(algorithmType, propertyBBL, maxRecords, sameNeighboorhood, sameSchoolDistrict, sameZip, sameBlock, sameStreetName, monthOffset,
                                                     minSalePrice, maxSalePrice, classMatchType, isNotIntraFamily, isSelleraCompany, isBuyeraCompany);
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
        ///     Use this api to get a list of comparables for the given property and associated parameters
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>
        /// <param name="maxRecs">Total number of comps to be returned.</param>
        /// <param name="monthOffset">Month in past, always negative, default value is -12</param>
        /// <param name="classMatch">0 - same BuildingClass, 1 Expended BuildingClass, 2 Ignore BuildingClass</param>
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
        /// <param name="GLARng">Range of GLA Values for consideration in sqft. Format is CSV. Single values are considered as upper limit. Example: 3800 or Null,3800 or 3000,NULL or 3000,3800</param>
        /// <param name="LARng">Range of LA Values for consideration in sqft. Format is CSV. Single values are considered as upper limit. Example: 1600 or Null,1600 or 1200,NULL or 1200,1600</param>
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

            if (!InitializeRangeValues(GLARng, ref gLALoRange, ref gLAHiRange))
                return this.BadRequest("Incorrect GLA Range. Correct format 9999 or Null, 9999 or 9999, NULL or 9999, 9999");

            if (!InitializeRangeValues(LARng, ref lALoRange, ref lAHiRange))
                return this.BadRequest("Incorrect LA Range. Correct format 9999 or Null, 9999 or 9999, NULL or 9999, 9999");

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

        /// <summary>  
        ///     Use this api to get a subject property's details
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>
        /// <returns>
        ///     Returns subject property's details
        /// </returns>
        [Route("api/cma/{propertyBBL}")]
        [ResponseType(typeof(DAL.SubjectDetails))]
        public IHttpActionResult GetSubjectDetails(string propertyBBL)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            try
            {
                var result = DAL.CMA.GetSubject(propertyBBL);
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
