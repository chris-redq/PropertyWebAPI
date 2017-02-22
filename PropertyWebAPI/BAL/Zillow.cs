//-----------------------------------------------------------------------
// <copyright file="Zillow.cs" company="Redq Technologies, Inc.">
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
    using System.Net;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using DexiRobotRequestResponseBuilder.Response;
    using DexiRobotRequestResponseBuilder.Request;

    #region Local Helper Classes
    /// <summary>
    /// Helper class used to capture Tax bill details and used for serialization into JSON object 
    /// </summary>
    public class ZillowPropertyDetails: NYCBaseResult
    {
        /// <summary>
        /// zEstimate of the property
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? zEstimate;
    }

    #endregion

    /// <summary>
    /// This class deals with all the details associated with either returning Zillow details or creating the 
    /// request to get data scrapped from the web 
    /// </summary>
    public static class Zillow
    {
        private const int RequestTypeId = (int)RequestTypes.Zillow;
        /// <summary>
        /// Helper class used for serialization and deserialization of parameters necessary to get Zillow information 
        /// </summary>
        [DataContract]
        private class Parameters
        {   [DataMember]
            public string BBL;
            [DataMember]
            public string address;
        }


        /// <summary>
        ///     This methods converts all parameters required for Tax Bills into a JSON object
        /// </summary>
        /// <param name="BBL"></param>
        /// <param name="address"></param>
        /// <returns>JSON string</returns>
        private static string ParametersToJSON(string BBL, string address)
        {
            Parameters parameters = new Parameters();
            parameters.BBL = BBL;
            parameters.address = address;
            return JsonConvert.SerializeObject(parameters);
        }

        /// <summary>
        ///     This method converts a JSON back into Parameters Object
        /// </summary>
        /// <param name="jsonParameters"></param>
        /// <returns>Parameters</returns>
        private static Parameters JSONToParameters(string jsonParameters)
        {
            return JsonConvert.DeserializeObject<Parameters>(jsonParameters);
        }

        /// <summary>
        ///     Use this method in the controller to log failures that are processed before calling any 
        ///     other business methods of this class
        /// </summary>
        /// <param name="propertyBBL"></param>
        /// <param name="externalReferenceId"></param>
        /// <param name="httpErrorCode"></param>
        /// <returns></returns>
        public static void LogFailure(string propertyBBL, string externalReferenceId, int httpErrorCode)
        {
            DAL.DataRequestLog.InsertForFailure(propertyBBL, RequestTypeId, externalReferenceId, "Error Code: "+((HttpStatusCode)httpErrorCode).ToString());
        }

        /// <summary>
        ///     This method calls back portal for every log record in the list
        /// </summary>
        /// <param name="zEstimate"></param>
        /// <param name="logs">List or Request Log Records</param>
        private static void MakePortalCallBacks(List<DataRequestLog> logs, Decimal? zEstimate)
        {
            var resultObj = new BAL.Results();
            resultObj.zillowProperty = new ZillowPropertyDetails();
            resultObj.zillowProperty.zEstimate = zEstimate;

            foreach (var rec in logs)
            {
                resultObj.zillowProperty.BBL = rec.BBL;
                resultObj.zillowProperty.requestId = rec.RequestId;
                resultObj.zillowProperty.status = ((RequestStatus)rec.RequestStatusTypeId).ToString();
                resultObj.zillowProperty.externalReferenceId = rec.ExternalReferenceId;
                CallingSystem.PostCallBack(resultObj);
            }
        }

        /// <summary>
        ///     This method deals with all the details associated with either returning the Zillow details or creating the 
        ///     request for getting it scrapped from the web 
        /// </summary>
        /// <param name="propertyBBL"></param>
        /// <param name="address"></param>
        /// <param name="externalReferenceId"></param>
        /// <returns></returns>
        public static ZillowPropertyDetails Get(string propertyBBL, string address, string externalReferenceId)
        {
            ZillowPropertyDetails zPropertyDetails = new ZillowPropertyDetails();
            zPropertyDetails.BBL = propertyBBL;
            zPropertyDetails.externalReferenceId = externalReferenceId;
            zPropertyDetails.status = RequestStatus.Pending.ToString();
            zPropertyDetails.zEstimate = null;

            string parameters = ParametersToJSON(propertyBBL, address);

            using (WebDataEntities webDBEntities = new WebDataEntities())
            {
                using (var webDBEntitiestransaction = webDBEntities.Database.BeginTransaction())
                {
                    try
                    {
                        string jsonBillParams = ParametersToJSON(propertyBBL,address);

                        //check if data available
                        WebDataDB.Zillow zillowObj = webDBEntities.Zillows.FirstOrDefault(i => i.BBL == propertyBBL);

                        // record in database and data is not stale
                        if (zillowObj != null && DateTime.UtcNow.Subtract(zillowObj.LastUpdated).Days <= 30)
                        {
                            zPropertyDetails.zEstimate = zillowObj.zEstimate;
                            zPropertyDetails.status = RequestStatus.Success.ToString();

                            DAL.DataRequestLog.InsertForCacheAccess(webDBEntities, propertyBBL, RequestTypeId, externalReferenceId, jsonBillParams);
                        }
                        else
                        {   //check if pending request in queue
                            DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetPendingRequest(webDBEntities, propertyBBL, RequestTypeId, jsonBillParams);

                            if (dataRequestLogObj == null) //No Pending Request Create New Request
                            {
                                string requestStr = RequestData.ZillowZEstimate(address);

                                Request requestObj = DAL.Request.Insert(webDBEntities, requestStr, RequestTypeId, null);

                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, RequestTypeId, requestObj.RequestId,
                                                                                               externalReferenceId, jsonBillParams);

                                zPropertyDetails.status = RequestStatus.Pending.ToString();
                                zPropertyDetails.requestId = requestObj.RequestId;
                            }
                            else //Pending request in queue
                            {
                                zPropertyDetails.status = RequestStatus.Pending.ToString();
                                //Send the RequestId for the pending request back
                                zPropertyDetails.requestId = dataRequestLogObj.RequestId;
                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, RequestTypeId,
                                                                                               dataRequestLogObj.RequestId.GetValueOrDefault(), externalReferenceId, jsonBillParams);
                            }
                        }
                        webDBEntitiestransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        zPropertyDetails.status = RequestStatus.Error.ToString();
                        DAL.DataRequestLog.InsertForFailure(propertyBBL, RequestTypeId, externalReferenceId, parameters);
                        Common.Logs.log().Error(string.Format("Exception encountered processing {0} with externalRefId {1}{2}", propertyBBL, externalReferenceId, Common.Utilities.FormatException(e)));
                    }
                }
            }
            return zPropertyDetails;
        }

        /// <summary>
        ///     This method gets the data or current status for a request 
        /// </summary>
        /// <param name="dataRequestLogObj"></param>
        /// <returns></returns>
        public static ZillowPropertyDetails ReRun(DataRequestLog dataRequestLogObj)
        {
            ZillowPropertyDetails zPropertyDetails = new ZillowPropertyDetails();
            zPropertyDetails.BBL = dataRequestLogObj.BBL;
            zPropertyDetails.requestId = dataRequestLogObj.RequestId;
            zPropertyDetails.externalReferenceId = dataRequestLogObj.ExternalReferenceId;
            zPropertyDetails.status = ((RequestStatus)dataRequestLogObj.RequestStatusTypeId).ToString(); ;
            zPropertyDetails.zEstimate = null;
            try
            {
                using (WebDataEntities webDBEntities = new WebDataEntities())
                {
                    if (dataRequestLogObj.RequestStatusTypeId == (int)RequestStatus.Success)
                    {
                        Parameters parameters = JSONToParameters(dataRequestLogObj.RequestParameters);
                        //check if data available
                        WebDataDB.Zillow zillowObj = webDBEntities.Zillows.FirstOrDefault(i => i.BBL == parameters.BBL);

                        if (zillowObj != null && DateTime.UtcNow.Subtract(zillowObj.LastUpdated).Days <= 30)
                            zPropertyDetails.zEstimate = zillowObj.zEstimate;
                        else
                            zPropertyDetails.status = RequestStatus.Error.ToString();
                    }
                }
                return zPropertyDetails;
            }
            catch (Exception e)
            {   Common.Logs.log().Error(string.Format("Exception encountered processing request log for {0} with externalRefId {1}{2}", 
                                        dataRequestLogObj.BBL, dataRequestLogObj.ExternalReferenceId, Common.Utilities.FormatException(e)));
                return null;
            }
        }

        /// <summary>
        ///     This method updates the Zillow table based on the information received from the Request Object
        /// </summary>
        /// <param name="requestObj"></param>
        /// <returns>True if successful else false</returns>
        public static bool UpdateData(Request requestObj)
        {
            using (WebDataEntities webDBEntities = new WebDataEntities())
            {
                using (var webDBEntitiestransaction = webDBEntities.Database.BeginTransaction())
                {
                    try
                    {
                        List<DataRequestLog> logs=null;
                        decimal? zEstimate = null;

                        switch (requestObj.RequestStatusTypeId)
                        {
                            case (int)RequestStatus.Error:
                                logs=DAL.DataRequestLog.SetAsError(webDBEntities, requestObj.RequestId);
                                break;
                            case (int)RequestStatus.Success:
                                {
                                    DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetFirst(webDBEntities, requestObj.RequestId);
                                    if (dataRequestLogObj != null)
                                    {
                                        DexiRobotRequestResponseBuilder.Response.ZillowZEstimate resultObj = null;
                                        try
                                        {   resultObj = (ResponseData.ParseZillowZEstimate(requestObj.ResponseData))[0];
                                            zEstimate = resultObj.Zestimate;
                                        }
                                        catch(Exception e)
                                        {
                                            Common.Logs.log().Error(string.Format("Cannot parse the response data {0}{1}", requestObj.RequestId, Common.Utilities.FormatException(e)));
                                        }

                                        Parameters parameters = JSONToParameters(dataRequestLogObj.RequestParameters);
                                        //check if old data in the DB
                                        WebDataDB.Zillow zillowObj = webDBEntities.Zillows.FirstOrDefault(i => i.BBL == parameters.BBL);
                                        if (zillowObj != null)
                                        {   //Update data with new results
                                            zillowObj.zEstimate = zEstimate;
                                            zillowObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();
                                        }
                                        else
                                        {   // add an entry into cache or DB
                                            zillowObj = new WebDataDB.Zillow();
                                            zillowObj.BBL = parameters.BBL;
                                            zillowObj.zEstimate = zEstimate;
                                            zillowObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();

                                            webDBEntities.Zillows.Add(zillowObj);
                                        }

                                        webDBEntities.SaveChanges();

                                        logs = DAL.DataRequestLog.SetAsSuccess(webDBEntities, requestObj.RequestId); 
                                    }
                                    else
                                        throw (new Exception("Cannot locate Request Log Record(s)"));
                                    break;
                                }
                            default:
                                Common.Logs.log().Warn(String.Format("Update called for a Request Object Id {0} with incorrect Status Id {1}", requestObj.RequestId, requestObj.RequestStatusTypeId));
                                break;
                        }

                        webDBEntitiestransaction.Commit();
                        if (logs != null)
                            MakePortalCallBacks(logs, zEstimate);
                        return true;
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        Common.Logs.log().Error(string.Format("Exception encountered updating request with id {0}{1}", requestObj.RequestId, Common.Utilities.FormatException(e)));
                        return false;
                    }
                }
            }
        }
    }
}