//-----------------------------------------------------------------------
// <copyright file="ViolationsController.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI.Controllers
{
    using System.Text.RegularExpressions;
    using System.Web.Http;
    using System.Web.Http.Description;
    using Newtonsoft.Json;
    using System.Net;
    using System.Collections.Generic;
    using System;
    using NYCDOB;
    using NYCVNL;

    #region Helper classes
    /// <summary>
    ///     This class is designed to capture all violations associated with a property. Currently, it only captures
    ///     DOB Civil Penalties and DOB ECB Violations Summary (total amounts)
    /// </summary>
    /// 
    public class Violations
    {
        /// <summary>
        ///     Captures sum of all DOB Civil Penalties and sum of all ECB Violations
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BAL.DOBPenaltiesAndViolationsSummaryData dobPenaltiesAndViolations;
    }
    #endregion

    /// <summary>  
    /// This controller handles all api requests associated with property violations
    /// </summary>  
    [Authorize]
    public class ViolationsController : ApiController
    {
        // ../api/violations/3001670091/?externalRequestId=135&dobPenaltiesAndViolationsSummary=Y
        /// <summary>  
        ///     Use this method to get a total of all violations/civil penalty amounts etc. associated with a property
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, 
        ///     followed by 0 padded 5 digit block number and followed by 0 padded 4 digit lot number
        /// </param>  
        /// <param name="externalReferenceId">
        ///     This is an optional parameter. The user of the API can provide their own reference number for a request. This reference number is sent 
        ///     back along with results to the caller when their request is furnished asynchronously.
        /// </param>
        /// <param name="dobPenaltiesAndViolationsSummary">
        ///     Set this optional parameter to N if you do not want the DOB Civil Penalties. The default value for this parameter is Y
        /// </param>
        /// <returns>
        ///     Returns totals for all violation/Penalties amounts associated with a property
        /// </returns>
        [Route("api/violations/{propertyBBL}")]
        [ResponseType(typeof(Violations))]
        public IHttpActionResult GetViolations(string propertyBBL, string externalReferenceId = null, string dobPenaltiesAndViolationsSummary = "Y")
        {
            if (!BAL.BBL.IsValidFormat(propertyBBL))
            {
                BAL.DOBPenaltiesAndViolationsSummary.LogFailure(propertyBBL, externalReferenceId, (int)HttpStatusCode.BadRequest);
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");
            }

            if (!BAL.BBL.IsValid(propertyBBL))
            {
                BAL.DOBPenaltiesAndViolationsSummary.LogFailure(propertyBBL, externalReferenceId, (int)HttpStatusCode.BadRequest);
                return BadRequest("BBLE - Borough Block Lot & Easement number, not found");
            }

            Violations violationsObj = new Violations();

            if (dobPenaltiesAndViolationsSummary == "Y")
                violationsObj.dobPenaltiesAndViolations = BAL.DOBPenaltiesAndViolationsSummary.Get(propertyBBL, externalReferenceId);
                
            return Ok(violationsObj);
        }

        #region DOB Violations
        // ../api/violations/3001670091/DOBViolations
        /// <summary>  
        ///     Use this method to get a list of all (active,closed or both) DOB Violations associated with a property
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, 
        ///     followed by 0 padded 5 digit block number and followed by 0 padded 4 digit lot number
        /// </param>  
        /// <param name="active">
        ///     Set this optional parameter to N if you want the all closed DOB Violations or set the parameter to ignore if you want all violations.
        ///     The default value for this parameter is Y
        /// </param>
        /// <returns>
        ///     Returns a list of DOB Violations associated with a property
        /// </returns>
        [Route("api/violations/{propertyBBL}/DOBViolations")]
        [ResponseType(typeof(List<Violation>))]
        public IHttpActionResult GetDOBViolations(string propertyBBL, string active ="Y")
        {
            if (!BAL.BBL.IsValid(propertyBBL))
               return BadRequest("BBLE - Borough Block Lot & Easement number, not found");

            try
            {
                var resultList = DAL.Violations.GetDOBViolations(propertyBBL, active);
                if (resultList == null || resultList.Count == 0)
                    return NotFound();
                return Ok(resultList);

            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL {0}{1}", propertyBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        // ../api/violations/3001670091/DOBViolationsSummary
        /// <summary>  
        ///     Use this method to get a summary of DOB Violations associated with a property
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, 
        ///     followed by 0 padded 5 digit block number and followed by 0 padded 4 digit lot number
        /// </param>  
        /// <returns>
        ///     Returns summary of DOB Violations associated with a property
        /// </returns>
        [Route("api/violations/{propertyBBL}/DOBViolationsSummary")]
        [ResponseType(typeof(DAL.ViolationSummary))]
        public IHttpActionResult GetDOBViolationsSummary(string propertyBBL)
        {
            if (!BAL.BBL.IsValid(propertyBBL))
                return BadRequest("BBLE - Borough Block Lot & Easement number, not found");

            try
            {
                var resultList = DAL.Violations.GetDOBViolationsSummary(propertyBBL);
                if (resultList == null)
                    return NotFound();
                return Ok(resultList);

            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL {0}{1}", propertyBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
        #endregion

        #region ECB Violations

        // ../api/violations/3001670091/ECBViolations
        /// <summary>  
        ///     Use this method to get a list of all (active, closed or both) ECB Violations associated with a property
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, 
        ///     followed by 0 padded 5 digit block number and followed by 0 padded 4 digit lot number
        /// </param>  
        /// <param name="active">
        ///     Set this optional parameter to N if you want the all closed ECB Violations or set the parameter to ignore if you want all violations.
        ///     The default value for this parameter is Y
        /// </param>
        /// <returns>
        ///     Returns a list of ECB Violations associated with a property
        /// </returns>
        [Route("api/violations/{propertyBBL}/ECBViolations")]
        [ResponseType(typeof(List<DAL.ECBViolationDetail>))]
        public IHttpActionResult GetECBViolations(string propertyBBL, string active = "Y")
        {
            if (!BAL.BBL.IsValid(propertyBBL))
                return BadRequest("BBLE - Borough Block Lot & Easement number, not found");

            try
            {
                var resultList = DAL.Violations.GetECBViolations(propertyBBL, active);
                if (resultList == null || resultList.Count == 0)
                    return NotFound();
                return Ok(resultList);

            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL {0}{1}", propertyBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        // ../api/violations/3001670091/ECBViolationsSummary
        /// <summary>  
        ///     Use this method to get a summary of ECB Violations (across multiple agencies) associated with a property
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, 
        ///     followed by 0 padded 5 digit block number and followed by 0 padded 4 digit lot number
        /// </param>  
        /// <returns>
        ///     Returns summary of ECB Violations (across multiple agencies) associated with a property
        /// </returns>
        [Route("api/violations/{propertyBBL}/ECBViolationsSummary")]
        [ResponseType(typeof(ECBViolationsSummary))]
        public IHttpActionResult GetECBViolationsSummary(string propertyBBL)
        {
            if (!BAL.BBL.IsValid(propertyBBL))
                return BadRequest("BBLE - Borough Block Lot & Easement number, not found");

            try
            {
                var resultList = DAL.Violations.GetECBViolationsSummary(propertyBBL);
                if (resultList == null)
                    return NotFound();
                return Ok(resultList);

            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL {0}{1}", propertyBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        // ../api/violations/3001670091/ECBViolationsSummaryByAgency
        /// <summary>  
        ///     Use this method to get a summary of ECB Violations by agency associated with a property
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, 
        ///     followed by 0 padded 5 digit block number and followed by 0 padded 4 digit lot number
        /// </param>  
        /// <returns>
        ///     Returns summary of ECB Violations by agency associated with a property
        /// </returns>
        [Route("api/violations/{propertyBBL}/ECBViolationsSummaryByAgency")]
        [ResponseType(typeof(List<ECBViolationsSummaryByAgency>))]
        public IHttpActionResult GetECBViolationsSummaryByAgency(string propertyBBL)
        {
            if (!BAL.BBL.IsValid(propertyBBL))
                return BadRequest("BBLE - Borough Block Lot & Easement number, not found");

            try
            {
                var resultList = DAL.Violations.GetECBViolationsSummaryByAgency(propertyBBL);
                if (resultList == null)
                    return NotFound();
                return Ok(resultList);

            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL {0}{1}", propertyBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
        #endregion

        #region DOB Complaints
        // ../api/violations/3001670091/DOBComplaints
        /// <summary>  
        ///     Use this method to get a list of all active DOB Complaints associated with a property
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, 
        ///     followed by 0 padded 5 digit block number and followed by 0 padded 4 digit lot number
        /// </param>  
        /// <param name="active">
        ///     Set this optional parameter to N if you want the all closed DOB Complaints or set the parameter to ignore if you want all violations.
        ///     The default value for this parameter is Y
        /// </param>
        /// <returns>
        ///     Returns a list of DOB Complaints associated with a property
        /// </returns>
        [Route("api/violations/{propertyBBL}/DOBComplaints")]
        [ResponseType(typeof(List<Complaint>))]
        public IHttpActionResult GetDOBCompliants(string propertyBBL, string active = "Y")
        {
            if (!BAL.BBL.IsValid(propertyBBL))
                return BadRequest("BBLE - Borough Block Lot & Easement number, not found");

            try
            {
                var resultList = DAL.Violations.GetDOBCompliants(propertyBBL, active);
                if (resultList == null || resultList.Count == 0)
                    return NotFound();
                return Ok(resultList);

            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL {0}{1}", propertyBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }


        // ../api/violations/3001670091/DOBComplaintsSummary
        /// <summary>  
        ///     Use this method to get a summary of DOB Complaints associated with a property
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, 
        ///     followed by 0 padded 5 digit block number and followed by 0 padded 4 digit lot number
        /// </param>  
        /// <returns>
        ///     Returns summary of DOB Complaints associated with a property
        /// </returns>
        [Route("api/violations/{propertyBBL}/DOBComplaintsSummary")]
        [ResponseType(typeof(DAL.DOBComplaintsSummary))]
        public IHttpActionResult GetDOBComplaintsSummary(string propertyBBL)
        {
            if (!BAL.BBL.IsValid(propertyBBL))
                return BadRequest("BBLE - Borough Block Lot & Easement number, not found");

            try
            {
                var resultList = DAL.Violations.GetDOBComplaintsSummary(propertyBBL);
                if (resultList == null)
                    return NotFound();
                return Ok(resultList);

            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL {0}{1}", propertyBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        #endregion
    }

}

