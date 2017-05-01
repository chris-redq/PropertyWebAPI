//-----------------------------------------------------------------------
// <copyright file="WaterBill.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI.BAL
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using WebDataDB;
    using System.IO;
    using System.Text;
    using System.Runtime.Serialization.Json;
    using System.Runtime.Serialization;
    using RequestResponseBuilder.RequestObjects;
    using RequestResponseBuilder.ResponseObjects;
    using System.Collections.Generic;
    using System.Net;


    #region Local Helper Classes
    /// <summary>
    /// Helper class used to capture Water bill details and used for serialization into JSON object 
    /// </summary>
    public class WaterBillDetails: NYCBaseResult
    {
        /// <summary>
        /// Amount owed in water bill
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Decimal? billAmount;
    }
    
    #endregion

    /// <summary>
    ///     This class deals with all the details associated with either returning water bill details or creating the 
    ///     request for getting data to be scrapped from the web 
    /// </summary>
    public static class WaterBill
    {
        private const int RequestTypeId = (int)RequestTypes.NYCWaterBill;

        /// <summary>
        /// Helper class used for serialization and deserialization of parameters necessary to get Water bill 
        /// </summary>
        [DataContract]
        private class Parameters
        {   [DataMember]
            public string BBL;
        }

        /// <summary>
        ///     This methods converts all parameters required for Water Bill into a JSON object
        /// </summary>
        /// <param name="propertyBBL"></param>
        /// <returns>JSON string</returns>
        private static string ParametersToJSON(string propertyBBL)
        {
            Parameters waterParams = new Parameters();
            waterParams.BBL = propertyBBL;
            return JsonConvert.SerializeObject(waterParams);
        }

        /// <summary>
        ///     This method converts a JSON back into WaterBillParameters Object
        /// </summary>
        /// <param name="jsonParameters"></param>
        /// <returns>waterBillParameters</returns>
        private static Parameters JSONToParameters(string jsonParameters)
        {
            return JsonConvert.DeserializeObject<Parameters>(jsonParameters);
        }

        /// <summary>
        ///     This method calls back portal for every log record in the list
        /// </summary>
        private static void MakeCallBacks(List<DataRequestLog> logs, Decimal? billAmount)
        {
            var resultObj = new BAL.Results();
            resultObj.waterBill = new WaterBillDetails();
            resultObj.waterBill.billAmount = billAmount;

            foreach (var rec in logs)
            {
                var cb = CallingSystem.isAnyCallBack(rec.AccountId);
                if (cb == null)
                    continue;

                resultObj.waterBill.BBL = rec.BBL;
                resultObj.waterBill.requestId = rec.RequestId;
                resultObj.waterBill.status = ((RequestStatus)rec.RequestStatusTypeId).ToString();
                resultObj.waterBill.externalReferenceId = rec.ExternalReferenceId;
                CallingSystem.PostCallBack(rec.AccountId, cb, resultObj);
            }
        }

        /// <summary>
        ///     Use this method in the controller to log failures that are processed before calling any 
        ///     other business methods of this class
        /// </summary>
        public static void LogFailure(string propertyBBL, string externalReferenceId, string jobId, int httpErrorCode)
        {
            DAL.DataRequestLog.InsertForFailure(propertyBBL, RequestTypeId, externalReferenceId, jobId, "Error Code: " + ((HttpStatusCode)httpErrorCode).ToString());
        }

        /// <summary>
        ///     This method deals with all the details associated with either returning the water bill details or creating the 
        ///     request for getting is scrapped from the web 
        /// </summary>
        public static WaterBillDetails Get(Common.Context appContext, string propertyBBL, string externalReferenceId)
        {
            return Get(appContext, propertyBBL, externalReferenceId, DAL.Request.MEDIUMPRIORITY, null);
        }

        /// <summary>
        ///     This method deals with all the details associated with either returning the water bill details or creating the 
        ///     request for getting is scrapped from the web 
        /// </summary>
        public static WaterBillDetails Get(Common.Context appContext, string propertyBBL, string externalReferenceId, int priority, string jobId)
        {
            WaterBillDetails waterBill = new WaterBillDetails();
            waterBill.externalReferenceId = externalReferenceId;
            waterBill.status = RequestStatus.Pending.ToString();
            waterBill.BBL = propertyBBL;
            waterBill.billAmount = null;

            using (WebDataEntities webDBEntities = new WebDataEntities())
            {
                using (var webDBEntitiestransaction = webDBEntities.Database.BeginTransaction())
                {
                    string parameters = ParametersToJSON(propertyBBL);

                    try
                    {
                        //check if data available
                        WebDataDB.WaterBill waterBillObj = webDBEntities.WaterBills.FirstOrDefault(i => i.BBL == propertyBBL);

                        // record in database and data is not stale
                        if (waterBillObj != null && DateTime.UtcNow.Subtract(waterBillObj.LastUpdated).Days <= DAL.RequestType.GetDaysToRefresh(RequestTypeId))
                        {
                            waterBill.billAmount = waterBillObj.BillAmount;
                            waterBill.status = RequestStatus.Success.ToString();

                            DAL.DataRequestLog.InsertForCacheAccess(webDBEntities, propertyBBL, RequestTypeId, externalReferenceId, jobId, parameters);
                        }
                        else
                        {   //check if pending request in queue
                            DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetPendingRequest(webDBEntities, propertyBBL, RequestTypeId, parameters);

                            if (dataRequestLogObj == null) //No Pending Request Create New Request
                            {
                                string requestStr = RequestData.WaterBill(propertyBBL); 

                                Request requestObj = DAL.Request.Insert(webDBEntities, requestStr, RequestTypeId, priority, jobId);

                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, RequestTypeId, requestObj.RequestId,
                                                                                               externalReferenceId, jobId, appContext.getAccountId(), parameters);

                                waterBill.status = RequestStatus.Pending.ToString();
                                waterBill.requestId = requestObj.RequestId;
                            }
                            else //Pending request in queue
                            {
                                waterBill.status = RequestStatus.Pending.ToString();
                                //Send the RequestId for the pending request back
                                waterBill.requestId = dataRequestLogObj.RequestId;
                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, RequestTypeId, dataRequestLogObj.RequestId.GetValueOrDefault(), 
                                                                                               externalReferenceId, jobId, appContext.getAccountId(), parameters);
                            }
                        }
                        webDBEntitiestransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        waterBill.status = RequestStatus.Error.ToString();
                        DAL.DataRequestLog.InsertForFailure(propertyBBL, RequestTypeId, externalReferenceId, jobId, parameters);
                        Common.Logs.log().Error(string.Format("Exception encountered processing {0} with externalRefId {1}: {2}", 
                                                propertyBBL, externalReferenceId, Common.Logs.FormatException(e)));
                    }
                }
            }
            return waterBill;
        }

        /// <summary>
        ///     This method gets the data or current status for a request 
        /// </summary>
        /// <param name="dataRequestLogObj"></param>
        /// <returns></returns>
        public static WaterBillDetails ReRun(DataRequestLog dataRequestLogObj)
        {
            WaterBillDetails waterBill = new WaterBillDetails();
            waterBill.BBL = dataRequestLogObj.BBL;
            waterBill.requestId = dataRequestLogObj.RequestId;
            waterBill.externalReferenceId = dataRequestLogObj.ExternalReferenceId;
            waterBill.status = ((RequestStatus)dataRequestLogObj.RequestStatusTypeId).ToString();
            waterBill.billAmount = null;

            try
            {
                using (WebDataEntities webDBEntities = new WebDataEntities())
                {
                    if (dataRequestLogObj.RequestStatusTypeId == (int)RequestStatus.Success)
                    {
                        Parameters waterParams = JSONToParameters(dataRequestLogObj.RequestParameters);
                        //check if data available
                        WebDataDB.WaterBill waterBillObj = webDBEntities.WaterBills.FirstOrDefault(i => i.BBL == waterParams.BBL);

                        if (waterBillObj != null && DateTime.UtcNow.Subtract(waterBillObj.LastUpdated).Days <= DAL.RequestType.GetDaysToRefresh(RequestTypeId))
                            waterBill.billAmount = waterBillObj.BillAmount;
                        else
                            waterBill.status = RequestStatus.Error.ToString();
                    }
                }
                return waterBill;
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered precessing request log for {0} with externalRefId {1}{2}",
                                                       dataRequestLogObj.BBL, dataRequestLogObj.ExternalReferenceId, Common.Logs.FormatException(e)));
                return null;
            }
        }
     
        /// <summary>
        ///     This method updates the WaterBill table based on the information received from the Request Object
        /// </summary>
        public static bool UpdateData(Common.Context appContext, Request requestObj)
        {
            using (WebDataEntities webDBEntities = new WebDataEntities())
            {
                using (var webDBEntitiestransaction = webDBEntities.Database.BeginTransaction())
                {
                    try
                    {
                        List<DataRequestLog> logs = null;
                        Decimal? billAmount = null;

                        switch (requestObj.RequestStatusTypeId)
                        {
                            case (int)RequestStatus.Error:
                                DAL.DataRequestLog.SetAsError(webDBEntities, requestObj.RequestId);
                                break;
                            case (int)RequestStatus.Success:
                                {
                                    DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetFirst(webDBEntities, requestObj.RequestId);
                                    if (dataRequestLogObj != null)
                                    {
                                        var resultObj = ResponseData.ParseWaterBill(requestObj.ResponseData);

                                        billAmount = resultObj.Amount;

                                        Parameters waterBillParams = JSONToParameters(dataRequestLogObj.RequestParameters);
                                        //check if data available
                                        WebDataDB.WaterBill waterBillObj = webDBEntities.WaterBills.FirstOrDefault(i => i.BBL == waterBillParams.BBL);
                                        if (waterBillObj != null)
                                        {
                                            waterBillObj.BillAmount = resultObj.Amount;
                                            waterBillObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();
                                        }
                                        else
                                        {
                                            waterBillObj = new WebDataDB.WaterBill();
                                            waterBillObj.BBL = waterBillParams.BBL;
                                            waterBillObj.BillAmount = resultObj.Amount;
                                            waterBillObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();

                                            webDBEntities.WaterBills.Add(waterBillObj);
                                        }

                                        webDBEntities.SaveChanges();
                                        logs=DAL.DataRequestLog.SetAsSuccess(webDBEntities, requestObj.RequestId);
                                    }
                                    else
                                        throw (new Exception("Cannot locate Request Log Record(s)"));

                                    break;
                                }
                            default:
                                Common.Logs.log().Warn(String.Format("Update called for a Request Id {0} with incorrect Status Id {2}", requestObj.RequestId, requestObj.RequestStatusTypeId));
                                break;
                        }
                        webDBEntitiestransaction.Commit();
                        if (logs != null)
                            MakeCallBacks(logs, billAmount);
                        return true;
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        Common.Logs.log().Error(string.Format("Exception encountered updating request with id {0}{1}", requestObj.RequestId, Common.Logs.FormatException(e)));
                        return false;
                    }
                }
            }
        }
    }
}