//-----------------------------------------------------------------------
// <copyright file="WebDataController.cs" company="Redq Technologies, Inc.">
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
    using System.Web.Http;
    using System.Web.Http.Description;
    using Newtonsoft.Json;
    using WebDataDB;


    /// <summary>
    ///     This controller handles callbacks from Web Scrapping Job Managers when requests are processed. It also provides
    ///     current state of the request
    /// </summary>
    [Authorize]
    public class WebDataController : ApiController
    {
        /// <summary>  
        /// The DJM uses this API to inform WebData service that the given request has been processed
        /// </summary>  
        /// <param name="requestId">
        ///     Id of the request whose results need to be processed 
        /// </param>  
        /// 
        [Security.APIAuthorize(Roles = Security.Roles.CALLBACK)]
        [Route("api/webdata/djm/request/{requestId}/processresult")]
        [ResponseType(typeof(Boolean))]
        public IHttpActionResult PostProcessRequestResult(long requestId)
        {
            Common.Context appContext = new Common.Context(RequestContext, Request);

            using (WebDataEntities webDataE = new WebDataEntities())
            {
                Request requestObj = webDataE.Requests.Find(requestId);
                if (requestObj == null)
                    return NotFound();
                
                bool success = false;

                switch (requestObj.RequestTypeId)
                {
                    case (int)RequestTypes.NYCTaxBill:
                        success = BAL.TaxBill.UpdateData(appContext, requestObj);
                        break;
                    case (int)RequestTypes.NYCWaterBill:
                        success = BAL.WaterBill.UpdateData(appContext, requestObj);
                        break;
                    case (int)RequestTypes.NYCDOBPenaltiesAndViolations:
                        success = BAL.DOBPenaltiesAndViolationsSummary.UpdateData(appContext, requestObj);
                        break;
                    case (int)RequestTypes.NYCMortgageServicer:
                        success = BAL.MortgageServicer.UpdateData(appContext, requestObj);
                        break;
                    case (int)RequestTypes.Zillow:
                        success = BAL.Zillow.UpdateData(appContext, requestObj);
                        break;
                    case (int)RequestTypes.NYCNoticeOfPropertyValue:
                        success = BAL.NoticeOfPropertyValueDocument.UpdateData(appContext, requestObj);
                        break;
                    case (int)RequestTypes.NYCMortgageDocumentDetails:
                        success = BAL.MortgageDocument.UpdateData(appContext, requestObj);
                        break;
                    default:
                        String msg = String.Format("Cannot process request - Invalid Request Type: {0} for Request Id {1}", requestObj.RequestTypeId, requestObj.RequestId);
                        Common.Logs.log().Warn(msg);
                        return Common.HttpResponse.InternalError(Request, msg);
                }

                if (!success)
                    return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");

                return Ok(true);
            }
        }

        // ../api/webdata/request/501
        /// <summary>  
        ///     Use this method to get tax bill for a property
        /// </summary>  
        /// <param name="requestId">
        ///     requestId sent back in the response when request was made for a specific data.
        /// </param>  
        /// <returns>
        ///     Returns data associated with the requestId
        /// </returns>
        [Security.APIAuthorize(Roles = Security.Roles.DATAREQUEST)]
        [Route("api/webdata/request/{requestId}")]
        [ResponseType(typeof(BAL.Results))]
        public IHttpActionResult GetRequest(long requestId)
        {
            using (WebDataEntities webDataE = new WebDataEntities())
            {
                WebDataDB.DataRequestLog requestLogObj = DAL.DataRequestLog.GetFirst(webDataE, requestId);
                if (requestLogObj == null)
                    return NotFound();
                var result = new BAL.Results();
                switch (requestLogObj.RequestTypeId)
                {
                    case (int)RequestTypes.NYCTaxBill:
                        result.taxBill = BAL.TaxBill.ReRun(requestLogObj);
                        return Ok(result);
                    case (int)RequestTypes.NYCWaterBill:
                        result.waterBill = BAL.WaterBill.ReRun(requestLogObj);
                        return Ok(result);
                    case (int)RequestTypes.NYCMortgageServicer:
                        result.mortgageServicer = BAL.MortgageServicer.ReRun(requestLogObj);
                        return Ok(result);
                    case (int)RequestTypes.NYCDOBPenaltiesAndViolations:
                        result.dobPenaltiesAndViolationsSummary = BAL.DOBPenaltiesAndViolationsSummary.ReRun(requestLogObj);
                        return Ok(result);
                    case (int)RequestTypes.Zillow:
                        result.zillowProperty = BAL.Zillow.ReRun(requestLogObj);
                        return Ok(result);
                    case (int)RequestTypes.NYCNoticeOfPropertyValue:
                        result.noticeOfPropertyValueResult = BAL.NoticeOfPropertyValueDocument.ReRun(requestLogObj);
                        return Ok(result);
                    case (int)RequestTypes.NYCMortgageDocumentDetails:
                        result.mortgageDocumentResult = BAL.MortgageDocument.ReRun(requestLogObj);
                        return Ok(result);
                    default:
                        return BadRequest("Bad Request - Incorrect Request Type");
                }
            }
        }

        // ../api/webdata/request/externalreference/ABC123
        /// <summary>  
        ///     Use this method to get tax bill for a property
        /// </summary>  
        /// <param name="externalReferenceId">
        ///     The user of the API can provide their own reference number for a request. This reference number is sent back along with results to the caller when their request is furnished later asynchronously.
        /// </param>
        /// <returns>
        ///     Returns data associated with the requestId
        /// </returns>
        [Security.APIAuthorize(Roles = Security.Roles.DATAREQUEST)]
        [Route("api/webdata/request/externalreference/{externalReferenceId}")]
        [ResponseType(typeof(BAL.Results))]
        public IHttpActionResult GetRequestByExternalReferenceId(string externalReferenceId)
        {
            using (WebDataEntities webDataE = new WebDataEntities())
            {
                List<DataRequestLog> requestLogList = DAL.DataRequestLog.GetAll(webDataE, externalReferenceId);
                if (requestLogList == null)
                    return NotFound();

                var result = new BAL.Results();
                foreach (var requestLogObj in requestLogList)
                {   switch (requestLogObj.RequestTypeId)
                    {
                        case (int)RequestTypes.NYCTaxBill:
                            result.taxBill = BAL.TaxBill.ReRun(requestLogObj);
                            break;
                        case (int)RequestTypes.NYCWaterBill:
                            result.waterBill = BAL.WaterBill.ReRun(requestLogObj);
                            break;
                        case (int)RequestTypes.NYCMortgageServicer:
                            result.mortgageServicer = BAL.MortgageServicer.ReRun(requestLogObj);
                            break;
                        case (int)RequestTypes.NYCDOBPenaltiesAndViolations:
                            result.dobPenaltiesAndViolationsSummary = BAL.DOBPenaltiesAndViolationsSummary.ReRun(requestLogObj);
                            break;
                        case (int)RequestTypes.Zillow:
                            result.zillowProperty = BAL.Zillow.ReRun(requestLogObj);
                            break;
                        case (int)RequestTypes.NYCNoticeOfPropertyValue:
                            result.noticeOfPropertyValueResult = BAL.NoticeOfPropertyValueDocument.ReRun(requestLogObj);
                            break;
                        case (int)RequestTypes.NYCMortgageDocumentDetails:
                            result.mortgageDocumentResult = BAL.MortgageDocument.ReRun(requestLogObj);
                            break;
                        default:
                            break;
                    }
                }
                return Ok(result);
            }
        }

        // ../api/webdata/request/checkprocessed
        /// <summary>  
        ///     Use this method after the service comes up to process any requests that may be processed by DJM but not data services. This would 
        ///     happen in situations when the data services are down and the DJM is processing requests 
        /// </summary>  
        /// <returns>
        ///     void
        /// </returns>
        [Security.APIAuthorize(Roles = Security.Roles.DATAREQUEST)]
        [Route("api/webdata/request/checkprocessed")]
        [HttpPost]
        public void CheckIfRequestsProcessed()
        {
            Common.Context appContext = new Common.Context(RequestContext, Request);

            using (WebDataEntities webDataE = new WebDataEntities())
            {
                List<DataRequestLog> requestLogList = DAL.DataRequestLog.GetAllUnprocessed(webDataE);
                if (requestLogList == null)
                    return;

                foreach (var requestLogObj in requestLogList)
                {
                    Request requestObj = webDataE.Requests.Find(requestLogObj.RequestId);

                    if (requestObj == null)
                        continue;

                    switch (requestObj.RequestTypeId)
                    {
                        case (int)RequestTypes.NYCTaxBill:
                            BAL.TaxBill.UpdateData(appContext, requestObj);
                            break;
                        case (int)RequestTypes.NYCWaterBill:
                            BAL.WaterBill.UpdateData(appContext, requestObj);
                            break;
                        case (int)RequestTypes.NYCDOBPenaltiesAndViolations:
                            BAL.DOBPenaltiesAndViolationsSummary.UpdateData(appContext, requestObj);
                            break;
                        case (int)RequestTypes.NYCMortgageServicer:
                            BAL.MortgageServicer.UpdateData(appContext, requestObj);
                            break;
                        case (int)RequestTypes.Zillow:
                            BAL.Zillow.UpdateData(appContext, requestObj);
                            break;
                        case (int)RequestTypes.NYCNoticeOfPropertyValue:
                            BAL.NoticeOfPropertyValueDocument.UpdateData(appContext, requestObj);
                            break;
                        case (int)RequestTypes.NYCMortgageDocumentDetails:
                            BAL.MortgageDocument.UpdateData(appContext, requestObj);
                            break;
                        default:
                            Common.Logs.log().Warn(string.Format("Cannot process request - Invalid Request Type: {0} for Request Id {1}", requestObj.RequestTypeId, requestObj.RequestId));
                            break;
                    }
                }
            }
        }

        /// <summary>  
        /// Uses this API to start data updates for all data that is stale. This api requires specific authorization
        /// </summary>  
        [Security.APIAuthorize(Roles = Security.Roles.SCHEDULING)]
        [Route("api/webdata/updatestaledata")]
        [HttpPost]
        public void UpdateStaleData()
        {
            string jobId = "SCH-"+ DateTime.UtcNow.ToString() + " UTC" ;

            List<WebDataDB.vwStaleDataBBL> bblList = null;

            using (WebDataEntities webDBE = new WebDataEntities())
            {
                bblList = webDBE.vwStaleDataBBLs.OrderBy(m => m.BBL).ToList();
            }
            if (bblList == null || bblList.Count < 0)
                return;

            foreach (var bbl in bblList)
            {
                switch (bbl.RequestTypeId)
                {
                    case (int)RequestTypes.NYCTaxBill:
                        BAL.TaxBill.Get(bbl.BBL, null, DAL.Request.STALEDATAPRIORITY, jobId);
                        break;
                    case (int)RequestTypes.NYCMortgageServicer:
                        BAL.MortgageServicer.Get(bbl.BBL, null, DAL.Request.STALEDATAPRIORITY, jobId);
                        break;
                    case (int)RequestTypes.Zillow:
                        var propInfo = BAL.NYCPhysicalPropertyData.Get(bbl.BBL, true);
                        if (propInfo != null)
                            BAL.Zillow.LogFailure(bbl.BBL, null, jobId, (int)HttpStatusCode.BadRequest);
                        else
                            BAL.Zillow.Get(bbl.BBL, propInfo.address.ToString(), null, DAL.Request.STALEDATAPRIORITY, jobId);
                        break;
                    case (int)RequestTypes.NYCNoticeOfPropertyValue:
                        BAL.NoticeOfPropertyValueDocument.GetDetails(bbl.BBL, null, DAL.Request.STALEDATAPRIORITY, jobId);
                        break;
                    case (int)RequestTypes.NYCMortgageDocumentDetails:
                        BAL.MortgageDocument.GetDetailsAllUnstaisfiedMortgages(bbl.BBL, null, DAL.Request.STALEDATAPRIORITY, jobId);
                        break;
                    default:
                        String msg = String.Format("Cannot process request - Invalid Request Type: {0} for BBL {1}", bbl.RequestTypeId, bbl.BBL);
                        Common.Logs.log().Warn(msg);
                        break;
                }
            }
        }

        /// <summary>  
        /// Uses this API to do bulk load
        /// Requires specific authorization to use this api
        /// </summary>  
        [Security.APIAuthorize(Roles = Security.Roles.DATAREQUEST)]
        [Route("api/webdata/bulkretrieve")]
        public void PostBulkRetrieve([FromBody]List<string> bblList, int requestTypeId = 0)
        {
            string jobId = "SCH-" + DateTime.UtcNow.ToString() + " UTC";

            if (bblList == null || bblList.Count <= 0)
                return;
            
            foreach (var bbl in bblList)
            {
                if (!BAL.BBL.IsValidFormat(bbl))
                {                
                    Common.Logs.log().Warn(string.Format("Incorrect BBL {0} in Bulk Retrieve with Job Id {1}", bbl, jobId));
                    continue;
                }

                if (!BAL.BBL.IsValid(bbl))
                {
                    Common.Logs.log().Warn(string.Format("Incorrect BBL {0} in Bulk Retrieve with Job Id {1}", bbl, jobId));
                    continue;
                }

                switch (requestTypeId)
                {
                    case (int)RequestTypes.NYCTaxBill:
                        BAL.TaxBill.Get(bbl, null, DAL.Request.BULKDATAPRIORITY, jobId);
                        break;
                    case (int)RequestTypes.NYCMortgageServicer:
                        BAL.MortgageServicer.Get(bbl, null, DAL.Request.BULKDATAPRIORITY, jobId);
                        break;
                    case (int)RequestTypes.Zillow:
                        {
                            var propInfo = BAL.NYCPhysicalPropertyData.Get(bbl, true);
                            if (propInfo != null)
                                BAL.Zillow.LogFailure(bbl, null, jobId, (int)HttpStatusCode.BadRequest);
                            else
                                BAL.Zillow.Get(bbl, propInfo.address.ToString(), null, DAL.Request.BULKDATAPRIORITY, jobId);
                            break;
                        }
                    case (int)RequestTypes.NYCNoticeOfPropertyValue:
                        BAL.NoticeOfPropertyValueDocument.GetDetails(bbl, null, DAL.Request.BULKDATAPRIORITY, jobId);
                        break;
                    case (int)RequestTypes.NYCMortgageDocumentDetails:
                        BAL.MortgageDocument.GetDetailsAllUnstaisfiedMortgages(bbl, null, DAL.Request.STALEDATAPRIORITY, jobId);
                        break;
                    case 0:
                        {
                            BAL.TaxBill.Get(bbl, null, DAL.Request.BULKDATAPRIORITY, jobId);
                            BAL.MortgageServicer.Get(bbl, null, DAL.Request.BULKDATAPRIORITY, jobId);
                            var propInfo = BAL.NYCPhysicalPropertyData.Get(bbl, true);
                            if (propInfo != null)
                                BAL.Zillow.LogFailure(bbl, null, jobId, (int)HttpStatusCode.BadRequest);
                            else
                                BAL.Zillow.Get(bbl, propInfo.address.ToString(), null, DAL.Request.BULKDATAPRIORITY, jobId);
                            BAL.NoticeOfPropertyValueDocument.GetDetails(bbl, null, DAL.Request.BULKDATAPRIORITY, jobId);
                            BAL.MortgageDocument.GetDetailsAllUnstaisfiedMortgages(bbl, null, DAL.Request.STALEDATAPRIORITY, jobId);
                            break;
                        }
                    default:
                        String msg = String.Format("Cannot process request - Invalid Request Type: {0}", requestTypeId);
                        Common.Logs.log().Warn(msg);
                        return;
                }
            }
        }
    }
}
