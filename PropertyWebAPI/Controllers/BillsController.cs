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
    using WebDataDB;
    

    #region Local Helper Classes
    public class WaterBillDetails
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long?     requestId;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String   externalReferenceId;
        public int  statusTypeId;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Decimal?  billAmount;
    }

    public class TaxBillDetails
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long?    requestId;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String   externalReferenceId;
        public int  statusTypeId;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Decimal?  billAmount;
    }

    public class Bills
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WaterBillDetails    waterBill;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TaxBillDetails      taxBill;
    }
    #endregion

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
        /// <param name="needWaterBill">
        ///     Set this optional parameter to N if you do not want the waterbill. The default value for this parameter is Y
        /// </param>
        /// /// <param name="needTaxBill">
        ///     Set this optional parameter to N if you do not want the taxbill. The default value for this parameter is Y
        /// </param>
        /// <returns>
        ///     Returns all the bills requested or the ones available along with a list of request ids for ones that are not available. Null values are ignored
        /// </returns>
        [Route("api/bills/{propertyBBLE}")]
        [ResponseType(typeof(Bills))]
        public IHttpActionResult GetBills(string propertyBBLE, string externalRequestId = null, string needWaterBill = "Y", string needTaxBill = "Y")
        {
            Bills billsObj = new Bills();

            if (Regex.IsMatch(propertyBBLE, "^[1-5][0-9]{9}[A-Z]??$"))
            {
                if (needWaterBill == "Y")
                {
                    billsObj.waterBill = new WaterBillDetails();
                    billsObj.waterBill.externalReferenceId = externalRequestId;
                    billsObj.waterBill.statusTypeId = 0;
                    billsObj.waterBill.billAmount = null;

                    using (WebDataEntities webDBEntities = new WebDataEntities())
                    {
                        WaterBill waterBillObj = webDBEntities.WaterBills.FirstOrDefault(i => i.BBL == propertyBBLE);

                        Boolean sendANewRequest = false;
                        //check if data available
                        if (waterBillObj == null)
                        {
                            //check request in queue, then send same request id
                            DataRequestLog dataRequestLogObj = webDBEntities.DataRequestLogs.FirstOrDefault(i => i.RequestStatusTypeId == (int)RequestStatus.Pending
                                                                                                                && i.RequestTypeId == (int)RequestTypes.WaterBill
                                                                                                                && i.BBL == propertyBBLE);
                            if (dataRequestLogObj == null)
                                sendANewRequest = true;
                            else
                            {
                                billsObj.waterBill.statusTypeId = (int)RequestStatus.Pending;
                                billsObj.waterBill.requestId = 0; //to put valid value
                            }
                            //checked request in queue, then send same request id
                            //insert a record into RequestDatalog
                        }
                        else
                        {
                            if (DateTime.Today.Subtract(waterBillObj.LastUpdated).Days <= 30)
                            {
                                billsObj.waterBill.billAmount = waterBillObj.BillAmount;
                                //insert a record into RequestDatalog
                            }
                            else
                            {
                                //check request in queue, then send same request id
                                DataRequestLog dataRequestLogObj = webDBEntities.DataRequestLogs.FirstOrDefault(i => i.RequestStatusTypeId == (int)RequestStatus.Pending
                                                                                                                    && i.RequestTypeId == (int)RequestTypes.WaterBill
                                                                                                                    && i.BBL == propertyBBLE);
                                if (dataRequestLogObj == null)
                                    sendANewRequest = true;
                                else
                                {
                                    billsObj.waterBill.statusTypeId = (int)RequestStatus.Pending;
                                    billsObj.waterBill.requestId = dataRequestLogObj.RequestId;
                                }

                            }
                        }

                        if (sendANewRequest)
                        {

                        }
                    }
                }

                if (needTaxBill == "Y")
                {
                    billsObj.taxBill = new TaxBillDetails();
                    billsObj.taxBill.externalReferenceId = externalRequestId;
                    billsObj.taxBill.statusTypeId = 0;
                    billsObj.taxBill.billAmount = null;

                    using (WebDataEntities webDBEntities = new WebDataEntities())
                    {
                        TaxBill taxBillObj = webDBEntities.TaxBills.FirstOrDefault(i => i.BBL == propertyBBLE);

                        //check if data available
                        if (taxBillObj == null)
                        {
                            billsObj.taxBill.statusTypeId = 1;
                            billsObj.taxBill.requestId = 0; //to put valid value
                            //insert a record into RequestDatalog
                        }
                        else
                        {
                            if (DateTime.Today.Subtract(taxBillObj.LastUpdated).Days <= 30)
                            {
                                billsObj.taxBill.billAmount = taxBillObj.BillAmount;

                                //insert a record into RequestDatalog
                            }
                            else
                            {
                                billsObj.taxBill.statusTypeId = 1;
                                billsObj.taxBill.requestId = 0; //to put valid value
                                //insert a record into RequestDatalog
                            }
                        }
                    }
                }

                // return final result

                return Ok(billsObj);
                
            }

            return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");
        }
    }
}

    