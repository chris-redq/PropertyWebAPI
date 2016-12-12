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
    using Newtonsoft.Json;

    #region Helper classes

    public class Violations
    {
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
        // ../api/violations/3001670091/?externalRequestId=135&ecb=Y&dob=Y
        /// <summary>  
        ///     Use this method to get all violations associated with a property
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, followed by 0 padded 5 digit block number, 
        ///     followed by 0 padded 4 digit lot number and finally ending with optional alpha character indicating the easement associated with the property
        /// </param>  
        /// <param name="externalReferenceId">
        ///     The user of the API can provide their own reference number for a request. This refeernce number is sent back along with results to the caller when their request is furnished later asynchronously.
        /// </param>
        /// <param name="dobPenaltiesAndViolations">
        ///     Set this optional parameter to N if you do not want the DOB Civil Penalties. The default value for this parameter is Y
        /// </param>
        /// <returns>
        ///     Returns all the violation amounts and penalties requested or the ones available along with a list of request ids for ones that are not available. Null values are ignored
        /// </returns>
        [Route("api/violations/{propertyBBLE}")]
        [ResponseType(typeof(Violations))]
        public IHttpActionResult GetBills(string propertyBBL, string externalReferenceId = "", string dobPenaltiesAndViolations = "Y")
        {
            Violations violationsObj = new Violations();

            if (!Regex.IsMatch(propertyBBL, "^[1-5][0-9]{9}[A-Z]??$"))
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");

            if (dobPenaltiesAndViolations == "Y")
                violationsObj.dobPenaltiesAndViolations = BAL.DOBPenaltiesAndViolationsSummary.Get(propertyBBL, externalReferenceId);
                
            return Ok(violationsObj);
        }
    }
}

