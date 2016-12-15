//-----------------------------------------------------------------------
// <copyright file="BillsController.cs" company="Redq Technologies, Inc.">
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
    using WebDataDB;
    

    #region Local Helper Classes
    public class Bills
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BAL.WaterBillDetails  waterBill;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BAL.TaxBillDetails   taxBill;
    }
    #endregion

    /// <summary>  
    /// This controller handles all api requests associated with property bills
    /// </summary>  
    [Authorize]
    public class BillsController : ApiController
    {

        // ../api/Bills/3001670091/?externalReferenceId=135&&needTaxBill=Y
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
        /// <param name="needTaxBill">
        ///     Set this optional parameter to N if you do not want the tax bill. The default value for this parameter is Y
        /// </param>
        /// <returns>
        ///     Returns all the bills requested or the ones available along with a list of request ids for ones that are not available. Null values are ignored
        /// </returns>
        [Route("api/bills/{propertyBBL}")]
        [ResponseType(typeof(Bills))]
        public IHttpActionResult GetBills(string propertyBBL, string externalReferenceId = null, string needTaxBill = "Y")
        {                                                            
            Bills billsObj = new Bills();

            if (!Common.BBL.IsValid(propertyBBL))
            {   if (needTaxBill == "Y")
                    BAL.TaxBill.LogFailure(propertyBBL, externalReferenceId, (int)HttpStatusCode.BadRequest);
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");
            }

            //if (needWaterBill == "Y")
            //    billsObj.waterBill = BAL.WaterBill.Get(propertyBBL, externalReferenceId);

            if (needTaxBill == "Y")
                billsObj.taxBill = BAL.TaxBill.Get(propertyBBL, externalReferenceId);

            // return final result
            return Ok(billsObj);
        }
    }
}

    