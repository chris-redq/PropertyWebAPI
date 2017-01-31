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
                Common.Logs.log().Error(string.Format("Exception encountered for BBL: {0}{1}", propertyBBL, Common.Utilities.FormatException(e)));
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
                Common.Logs.log().Error(string.Format("Exception encountered for BBL: {0}{1}", propertyBBL, Common.Utilities.FormatException(e)));
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

                var result = DAL.CMA.GetSalesPriceTrend(assessment.NTACode, timeSpanInYears);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL: {0}{1}", propertyBBL, Common.Utilities.FormatException(e)));
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

                var result = DAL.CMA.GetPricePerSqftTrend(assessment.NTACode, timeSpanInYears);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL: {0}{1}", propertyBBL, Common.Utilities.FormatException(e)));
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
                var result = DAL.CMA.GetComaparables(algorithmType, propertyBBL, maxRecords, sameNeighboorhood, sameSchoolDistrict, sameZip, sameBlock, sameStreetName, monthOffset,
                                                     minSalePrice, maxSalePrice, classMatchType, isNotIntraFamily, isSelleraCompany, isBuyeraCompany);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL: {0}{1}", propertyBBL, Common.Utilities.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
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
                Common.Logs.log().Error(string.Format("Exception encountered for BBL: {0}{1}", propertyBBL, Common.Utilities.FormatException(e)));
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
                Common.Logs.log().Error(string.Format("Exception encountered for Deed: {0}{1}", deedUniqueKey, Common.Utilities.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
        #endregion
    }
}
