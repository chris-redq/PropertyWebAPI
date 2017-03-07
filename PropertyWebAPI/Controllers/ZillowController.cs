//-----------------------------------------------------------------------
// <copyright file="ZillowController.cs" company="Redq Technologies, Inc.">
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
    ///     This controller handles all api requests associated with Zillow website
    /// </summary>  
    [Authorize]
    public class ZillowController : ApiController
    {
        // ../api/zillow/3001670091/?externalReferenceId=135
        /// <summary>  
        ///     Use this method to get property information from Zillow
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block and Lot. The first character is a number between 1-5 indicating the borough associated with the property, 
        ///     followed by 0 padded 5 digit block number and followed by 0 padded 4 digit lot number.
        /// </param>  
        /// <param name="externalReferenceId">
        ///     This is a optional parameter. The user of the API can provide their own reference number for a request. This reference number is sent 
        ///     back along with results to the caller when their request is furnished asynchronously.
        /// </param>
        /// <returns>
        ///     Returns zEstimate of the property
        /// </returns>
        [Route("api/zillow/{propertyBBL}")]
        [ResponseType(typeof(BAL.ZillowPropertyDetails))]
        public IHttpActionResult Get(string propertyBBL, string externalReferenceId = null)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBL))
            {
                BAL.Zillow.LogFailure(propertyBBL, externalReferenceId, null, (int)HttpStatusCode.BadRequest);
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");
            }

            //Get Property address using BBL
            var propInfo = BAL.NYCPhysicalPropertyData.Get(propertyBBL,true);
            if (propInfo == null)
            {
                BAL.Zillow.LogFailure(propertyBBL, externalReferenceId, null, (int)HttpStatusCode.BadRequest);
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number, not found");
            }

            BAL.ZillowPropertyDetails propertyObj = BAL.Zillow.Get(propertyBBL, propInfo.address.ToString(), externalReferenceId);
            if (propertyObj == null)
                return NotFound();
           
            // return final result
            return Ok(propertyObj);
        }
    }
}
