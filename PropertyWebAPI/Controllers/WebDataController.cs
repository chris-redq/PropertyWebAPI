﻿//-----------------------------------------------------------------------
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
    using System.Text.RegularExpressions;
    using System.Web.Http;
    using System.Web.Http.Description;
    using Newtonsoft.Json;
    using WebDataDB;

    #region Local Helper Classes
    public class Results
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BAL.WaterBillDetails waterBill;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BAL.TaxBillDetails taxBill;
    }
    #endregion

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
        [Route("api/webdata/djm/request/{requestId}/processresult")]
        public void PostProcessRequestResult(long requestId)
        {
            using (WebDataEntities webDataE = new WebDataEntities())
            {
                Request requestObj = webDataE.Requests.Find(requestId);
                if (requestObj != null)
                {
                    switch (requestObj.RequestTypeId)
                    {
                        case (int)RequestTypes.TaxBill:
                            BAL.TaxBill.UpdateData(requestObj);
                            break;
                        case (int)RequestTypes.WaterBill:
                            BAL.WaterBill.UpdateData(requestObj);
                            break;
                        default:
                            // log something
                            break;
                     }
                }
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
        [Route("api/webdata/request/{requestId}")]
        [ResponseType(typeof(Results))]
        public IHttpActionResult GetRequest(long requestId)
        {
            using (WebDataEntities webDataE = new WebDataEntities())
            {
                WebDataDB.DataRequestLog requestLogObj = DAL.DataRequestLog.GetFirst(webDataE, requestId);
                if (requestLogObj == null)
                    return NotFound();
                Results result = new Results();
                switch (requestLogObj.RequestTypeId)
                {
                    case (int)RequestTypes.TaxBill:
                            result.taxBill = BAL.TaxBill.ReRun(requestLogObj);
                            return Ok(result);
                    case (int)RequestTypes.WaterBill:
                            result.waterBill = BAL.WaterBill.ReRun(requestLogObj);
                            return Ok(result);
                     default:
                        return BadRequest("Bad Request - Incorrect Resquest Type");
                }
            }
        }

        // ../api/webdata/request/externalreference/ABC123
        /// <summary>  
        ///     Use this method to get tax bill for a property
        /// </summary>  
        /// <param name="externalReferenceId">
        ///     The user of the API can provide their own reference number for a request. This refeernce number is sent back along with results to the caller when their request is furnished later asynchronously.
        /// </param>
        /// <returns>
        ///     Returns data associated with the requestId
        /// </returns>
        [Route("api/webdata/request/externalreference/{externalReferenceId}")]
        [ResponseType(typeof(Results))]
        public IHttpActionResult GetRequestByExternalReferenceId(string externalReferenceId)
        {
            using (WebDataEntities webDataE = new WebDataEntities())
            {
                List<DataRequestLog> requestLogList = DAL.DataRequestLog.GetAll(webDataE, externalReferenceId);
                if (requestLogList == null)
                    return NotFound();

                Results result = new Results();
                foreach (var requestLogObj in requestLogList)
                {   switch (requestLogObj.RequestTypeId)
                    {
                        case (int)RequestTypes.TaxBill:
                            result.taxBill = BAL.TaxBill.ReRun(requestLogObj);
                            break;
                        case (int)RequestTypes.WaterBill:
                            result.waterBill = BAL.WaterBill.ReRun(requestLogObj);
                            break;
                        default:
                            break;
                    }
                }
                return Ok(result);
            }
        }
    }
}