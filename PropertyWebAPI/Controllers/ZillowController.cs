//-----------------------------------------------------------------------
// <copyright file="BillsController.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI.Controllers
{
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Web.Http;
    using System.Web.Http.Description;


    /// <summary>  
    /// This controller handles all api requests associated with property bills
    /// </summary>  
    [Authorize]
    public class ZillowController : ApiController
    {
        // ../api/Bills/3001670091/?externalReferenceId=135
        /// <summary>  
        ///     Use this method to get tax bill for a property
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, followed by 0 padded 5 digit block number, 
        ///     followed by 0 padded 4 digit lot number and finally ending with optional alpha character indicating the easement associated with the property
        /// </param>  
        /// <param name="externalReferenceId">
        ///     The user of the API can provide their own reference number for a request. This reference number is sent back along with results to the caller when their request is furnished later asynchronously.
        /// </param>
        /// <returns>
        ///     Returns all the bills requested or the ones available along with a list of request ids for ones that are not available. Null values are ignored
        /// </returns>
        [Route("api/zillow/{propertyBBL}")]
        [ResponseType(typeof(BAL.ZillowPropertyDetails))]
        public IHttpActionResult Get(string propertyBBL, string externalReferenceId = null)
        {
            if (!Regex.IsMatch(propertyBBL, "^[1-5][0-9]{9}[A-Z]??$"))
            {
                BAL.Zillow.LogFailure(propertyBBL, externalReferenceId, (int)HttpStatusCode.BadRequest);
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");
            }
            BAL.GeneralPropertyInformation propInfo = BAL.NYCPhysicalPropertyData.Get(propertyBBL,true);
            if (propInfo == null)
            {   BAL.Zillow.LogFailure(propertyBBL, externalReferenceId, (int)HttpStatusCode.NotFound);
                return NotFound();
            }

            BAL.ZillowPropertyDetails propertyObj = BAL.Zillow.Get(propertyBBL, propInfo.address.ToString(), externalReferenceId);
            if (propertyObj == null)
                return NotFound();
           
            // return final result
            return Ok(propertyObj);
        }
    }
}
