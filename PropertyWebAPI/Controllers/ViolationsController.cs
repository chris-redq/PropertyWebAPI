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
        ///     Use this method to get all violations associated with a property
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
        ///     Returns all the violation amounts and penalties requested or the ones available along with a list of request ids for ones that are not available. Null values are ignored
        /// </returns>
        [Route("api/violations/{propertyBBL}")]
        [ResponseType(typeof(Violations))]
        public IHttpActionResult GetViolations(string propertyBBL, string externalReferenceId = null, string dobPenaltiesAndViolationsSummary = "Y")
        {
            if (Common.BBL.IsValid(propertyBBL))
            {
                BAL.DOBPenaltiesAndViolationsSummary.LogFailure(propertyBBL, externalReferenceId, (int)HttpStatusCode.BadRequest);
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");
            }
            
            Violations violationsObj = new Violations();

            if (dobPenaltiesAndViolationsSummary == "Y")
                violationsObj.dobPenaltiesAndViolations = BAL.DOBPenaltiesAndViolationsSummary.Get(propertyBBL, externalReferenceId);
                
            return Ok(violationsObj);
        }
    }
}

