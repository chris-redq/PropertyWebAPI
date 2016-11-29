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

    public class WaterBill
    {
        public int      requestId;
        public String   externalReferenceId;
        public int      statusTypeId;
        public Double   billAmount;
    }

    public class TaxBill
    {
        public int      requestId;
        public String   externalReferenceId;
        public int      statusTypeId;
        public Double   billAmount;
    }

    public class Bills
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WaterBill    waterBill;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TaxBill      taxBill;
    }

    /// <summary>  
    /// This controller handles all api requests associated with property bills
    /// </summary>  
    [Authorize]
    public class BillsController : ApiController
    {

        // ../api/Bills/3001670091/?externalRequestId=135&waterbill=Y&taxbill=Y
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
        /// <param name="waterBill">
        ///     Set this optional parameter to N if you do not want the waterbill. The default value for this parameter is Y
        /// </param>
        /// /// <param name="taxBill">
        ///     Set this optional parameter to N if you do not want the taxbill. The default value for this parameter is Y
        /// </param>
        /// <returns>
        ///     Returns all the bills requested or the ones available along with a list of request ids for ones that are not available. Null values are ignored
        /// </returns>
        [Route("api/bills/{propertyBBLE}")]
        [ResponseType(typeof(Bills))]
        public IHttpActionResult GetBills(string propertyBBLE, string externalRequestId = "", string waterBill = "Y", string taxBill = "Y")
        {
            Bills billsObj = new Bills();

            if (Regex.IsMatch(propertyBBLE, "^[1-5][0-9]{9}[A-Z]??$"))
            {
                if (waterBill == "Y")
                {
                    billsObj.waterBill = new WaterBill();
                    billsObj.waterBill.externalReferenceId = externalRequestId;
                    billsObj.waterBill.statusTypeId = 0;
                    billsObj.waterBill.billAmount = 0;


                    //check if data available
                    //if not 
                    //   send request
                    //   return with request details 

                    //if stale 
                    //   send request
                    //   return with request details 

                    //store partial result
                }

                if (taxBill == "Y")
                {
                    billsObj.taxBill = new TaxBill();
                    billsObj.taxBill.externalReferenceId = externalRequestId;
                    billsObj.taxBill.statusTypeId = 0;
                    billsObj.taxBill.billAmount = 0;
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

                return Ok(billsObj);
                
            }

            return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");
        }
    }
}

    