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
        /// <param name="propertyBBL">
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
        [Route("api/bills/{propertyBBL}")]
        [ResponseType(typeof(Bills))]
        public IHttpActionResult GetBills(string propertyBBL, string externalRequestId = null, string needWaterBill = "Y", string needTaxBill = "Y")
        {
            Bills billsObj = new Bills();

            if (!Regex.IsMatch(propertyBBL, "^[1-5][0-9]{9}[A-Z]??$"))
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");

            if (needWaterBill == "Y")
            {
                billsObj.waterBill = new WaterBillDetails();
                billsObj.waterBill.externalReferenceId = externalRequestId;
                billsObj.waterBill.statusTypeId = 0;
                billsObj.waterBill.billAmount = null;

                using (WebDataEntities webDBEntities = new WebDataEntities())
                {
                    using (var webDBEntitiestransaction = webDBEntities.Database.BeginTransaction())
                    {
                        try
                        {
                            //check if data available
                            WaterBill waterBillObj = webDBEntities.WaterBills.FirstOrDefault(i => i.BBL == propertyBBL);

                            // record in database and data is not stale
                            if (waterBillObj != null && DateTime.UtcNow.Subtract(waterBillObj.LastUpdated).Days <= 30)
                            {
                                billsObj.waterBill.billAmount = waterBillObj.BillAmount;

                                DAL.DataRequestLog.InsertForCacheAccess(webDBEntities, propertyBBL, (int)RequestTypes.WaterBill, externalRequestId);
                            }
                            else
                            {   //check if pending request in queue
                                DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetPendingRequest(webDBEntities, propertyBBL, (int)RequestTypes.WaterBill);

                                if (dataRequestLogObj == null) //No Pending Request Create New Request
                                {
                                    string requestStr = propertyBBL; // we need a helper class to convert propertyBBL into a correct format so that the webscrapping service can read

                                    Request requestObj = DAL.Request.Insert(webDBEntities, requestStr, (int)RequestTypes.WaterBill, null);

                                    dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, (int)RequestTypes.WaterBill, requestObj.RequestId, externalRequestId);

                                    billsObj.waterBill.statusTypeId = (int)RequestStatus.Pending;
                                    billsObj.waterBill.requestId = requestObj.RequestId;
                                }
                                else //Pending request in queue
                                {
                                    billsObj.waterBill.statusTypeId = (int)RequestStatus.Pending;
                                    //Send the RequestId for the pending request back
                                    billsObj.waterBill.requestId = dataRequestLogObj.RequestId;
                                }
                            }
                            webDBEntitiestransaction.Commit();
                        }
                        catch (Exception e)
                        {   
                            webDBEntitiestransaction.Rollback();
                            billsObj.waterBill.statusTypeId = (int)RequestStatus.Error;
                            //log externalrequestid error from exception
                        }
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
                    using (var webDBEntitiestransaction = webDBEntities.Database.BeginTransaction())
                    {
                        try
                        {
                            //check if data available
                            TaxBill taxBillObj = webDBEntities.TaxBills.FirstOrDefault(i => i.BBL == propertyBBL);

                            // record in database and data is not stale
                            if (taxBillObj != null && DateTime.UtcNow.Subtract(taxBillObj.LastUpdated).Days <= 30)
                            {
                                billsObj.taxBill.billAmount = taxBillObj.BillAmount;

                                DAL.DataRequestLog.InsertForCacheAccess(webDBEntities, propertyBBL, (int)RequestTypes.TaxBill, externalRequestId);
                            }
                            else
                            {   //check if pending request in queue
                                DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetPendingRequest(webDBEntities, propertyBBL, (int)RequestTypes.TaxBill);

                                if (dataRequestLogObj == null) //No Pending Request Create New Request
                                {
                                    string requestStr = propertyBBL; // we need a helper class to convert propertyBBL into a correct format so that the webscrapping service can read

                                    Request requestObj = DAL.Request.Insert(webDBEntities, requestStr, (int)RequestTypes.TaxBill, null);

                                    dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, (int)RequestTypes.TaxBill, requestObj.RequestId, externalRequestId);

                                    billsObj.taxBill.statusTypeId = (int)RequestStatus.Pending;
                                    billsObj.taxBill.requestId = requestObj.RequestId;
                                }
                                else //Pending request in queue
                                {
                                    billsObj.taxBill.statusTypeId = (int)RequestStatus.Pending;
                                    //Send the RequestId for the pending request back
                                    billsObj.taxBill.requestId = dataRequestLogObj.RequestId;
                                }
                            }
                            webDBEntitiestransaction.Commit();
                        }
                        catch (Exception e)
                        {
                            webDBEntitiestransaction.Rollback();
                            billsObj.taxBill.statusTypeId = (int)RequestStatus.Error;
                            //log externalrequestid error from exception
                        }
                    }
                }
            }

            // return final result

            return Ok(billsObj);

        }

    }
}

    