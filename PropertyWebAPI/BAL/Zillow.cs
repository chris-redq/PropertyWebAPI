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
    
    #region Local Helper Classes
    /// <summary>
    /// Helper class used to capture Tax bill details and used for serialization into JSON object 
    /// </summary>
    public class ZillowPropertyDetails
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? requestId;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string externalReferenceId;
        public string status;
        public string BBL;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? zEstimate;
    }

    #endregion

    /// <summary>
    ///     This class deals with all the details associated with either returning waterbill details or creating the 
    ///     request for getting is scrapped from the web 
    /// </summary>
    public static class Zillow
    {
        /// <summary>
        /// Helper class used for serialization and deserialization of parameters necessary to get Tax bill 
        /// </summary>
        private class Parameters
        {
            public string BBL;
            public string address;
        }


        /// <summary>
        ///     This methods converts all paramters required for Tax Bills into a JSON object
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
        ///     This method converts a JSON back into TaxBillParameters Object
        /// </summary>
        /// <param name="jsonParameters"></param>
        /// <returns>TaxBillParameters</returns>
        private static Parameters JSONToParameters(string jsonParameters)
        {
            Parameters parameters = new Parameters();
            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonParameters));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(parameters.GetType());
            parameters = (Parameters)serializer.ReadObject(ms);
            ms.Close();
            return parameters;
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
            DAL.DataRequestLog.InsertForFailure(propertyBBL, (int)RequestTypes.Zillow, externalReferenceId, "Error Code: "+((HttpStatusCode)httpErrorCode).ToString());
        }

        /// <summary>
        ///     This method deals with all the details associated with either returning the tax bill details or creating the 
        ///     request for getting is scrapped from the web 
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

                            DAL.DataRequestLog.InsertForCacheAccess(webDBEntities, propertyBBL, (int)RequestTypes.Zillow, externalReferenceId, jsonBillParams);
                        }
                        else
                        {   //check if pending request in queue
                            DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetPendingRequest(webDBEntities, propertyBBL, (int)RequestTypes.Zillow, jsonBillParams);

                            if (dataRequestLogObj == null) //No Pending Request Create New Request
                            {
                                string requestStr = DexiRobotRequestResponseBuilder.Request.RequestData.ZillowZEstimate(address);

                                Request requestObj = DAL.Request.Insert(webDBEntities, requestStr, (int)RequestTypes.Zillow, null);

                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, (int)RequestTypes.Zillow, requestObj.RequestId,
                                                                                               externalReferenceId, jsonBillParams);

                                zPropertyDetails.status = RequestStatus.Pending.ToString();
                                zPropertyDetails.requestId = requestObj.RequestId;
                            }
                            else //Pending request in queue
                            {
                                zPropertyDetails.status = RequestStatus.Pending.ToString();
                                //Send the RequestId for the pending request back
                                zPropertyDetails.requestId = dataRequestLogObj.RequestId;
                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, (int)RequestTypes.Zillow,
                                                                                               dataRequestLogObj.RequestId.GetValueOrDefault(), externalReferenceId, jsonBillParams);
                            }
                        }
                        webDBEntitiestransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        zPropertyDetails.status = RequestStatus.Error.ToString();
                        DAL.DataRequestLog.InsertForFailure(propertyBBL, (int)RequestTypes.Zillow, externalReferenceId, parameters);
                        Common.Logs.log().Error(string.Format("Exception encountered processing {0} with externalRefId {1}: {2}", propertyBBL, externalReferenceId, e.ToString()));
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
            {   Common.Logs.log().Error(string.Format("Exception encountered prcessing resquest log for {0} with externalRefId {1}: {2}", dataRequestLogObj.BBL, dataRequestLogObj.ExternalReferenceId, e.ToString()));
                return null;
            }
        }

        /// <summary>
        ///     This method updates the TaxBill table based on the information received from the Request Object
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
                                        DexiRobotRequestResponseBuilder.Response.ZillowZEstimate resultObj = (DexiRobotRequestResponseBuilder.Response.ResponseData.ParseZillowZEstimate(requestObj.ResponseData))[0];

                                        Parameters parameters = JSONToParameters(dataRequestLogObj.RequestParameters);
                                        //check if old data in the DB
                                        WebDataDB.Zillow zillowObj = webDBEntities.Zillows.FirstOrDefault(i => i.BBL == parameters.BBL);
                                        if (zillowObj != null)
                                        {   //Update data with new results
                                            zillowObj.zEstimate = decimal.Parse(resultObj.Zestimate);
                                            zillowObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();
                                        }
                                        else
                                        {   // add an entry into cache or DB
                                            zillowObj = new WebDataDB.Zillow();
                                            zillowObj.BBL = parameters.BBL;
                                            zillowObj.zEstimate = decimal.Parse(resultObj.Zestimate);
                                            zillowObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();

                                            webDBEntities.Zillows.Add(zillowObj);
                                        }

                                        webDBEntities.SaveChanges();

                                        DAL.DataRequestLog.SetAsSuccess(webDBEntities, requestObj.RequestId);
                                    }
                                    break;
                                }
                            default:
                                Common.Logs.log().Warn(String.Format("Update called for a Request Object Id {0} with incorrect Status Id {2}", requestObj.RequestId, requestObj.RequestStatusTypeId));
                                break;
                        }

                        webDBEntitiestransaction.Commit();
                        return true;
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        Common.Logs.log().Error(string.Format("Exception encountered updating request with id {0}: {2}", requestObj.RequestId, e.ToString()));
                        return false;
                    }
                }
            }
        }
    }
}