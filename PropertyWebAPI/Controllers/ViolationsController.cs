using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

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

    public class ECBViolations
    {
        public int RequestId;
        public String externalReferenceId;
        public int StatusTypeId;
        public Double violationAmount;
    }

    public class DOBViolations
    {
        public int RequestId;
        public String externalReferenceId;
        public int StatusTypeId;
        public Double violationAmount;
    }

    public class Violations
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ECBViolations ecbViolations;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DOBViolations dobViolations;
    }

    /// <summary>  
    /// This controller handles all api requests associated with property violations
    /// </summary>  
    [Authorize]
    public class ViolationsController : ApiController
    {
        // ../api/violations/3001670091/?externalRequestId=135&ecb=Y&dob=Y
        /// <summary>  
        ///     Use this method to get tax bill for a property
        /// </summary>  
        /// <param name="propertyBBLE">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, followed by 0 padded 5 digit block number, 
        ///     followed by 0 padded 4 digit lot number and finally ending with optional alpha character indicating the easement associated with the property
        /// </param>  
        /// <param name="externalRequestId">
        ///     The user of the API can provide their own reference number for a request. This refeernce number is sent back along with results to the caller when their request is furnished later asynchronously.
        /// </param>
        /// <param name="ecb">
        ///     Set this optional parameter to N if you do not want the ECB Violation Amount. The default value for this parameter is Y
        /// </param>
        /// /// <param name="dob">
        ///     Set this optional parameter to N if you do not want the DOB Civil Penalties. The default value for this parameter is Y
        /// </param>
        /// <returns>
        ///     Returns all the violation amounts and penalties requested or the ones available along with a list of request ids for ones that are not available. Null values are ignored
        /// </returns>
        [Route("api/violations/{propertyBBLE}")]
        [ResponseType(typeof(Violations))]
        public IHttpActionResult GetBills(string propertyBBLE, string externalRequestId = "", string ecb = "Y", string dob = "Y")
        {
            Violations violationsObj = new Violations();

            if (Regex.IsMatch(propertyBBLE, "^[1-5][0-9]{9}[A-Z]??$"))
            {
                if (ecb == "Y")
                {
                    violationsObj.ecbViolations = new ECBViolations();
                    violationsObj.ecbViolations.externalReferenceId = externalRequestId;
                    violationsObj.ecbViolations.StatusTypeId = 0;
                    violationsObj.ecbViolations.violationAmount = 0;

                    //check if data available
                    //if not 
                    //   send request
                    //   return with request details 

                    //if stale 
                    //   send request
                    //   return with request details 

                    //store partial result
                }

                if (dob == "Y")
                {
                    violationsObj.dobViolations = new DOBViolations();
                    violationsObj.dobViolations.externalReferenceId = externalRequestId;
                    violationsObj.dobViolations.StatusTypeId = 0;
                    violationsObj.dobViolations.violationAmount = 0;
                    //check if data available
                    //if not 
                    //   send request
                    //   return with request details 

                    //if stale 
                    //   send request
                    //   return with request details 

                    //store partial result
                }

                // return final result

                return Ok(violationsObj);

            }

            return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");
        }
    }
}

