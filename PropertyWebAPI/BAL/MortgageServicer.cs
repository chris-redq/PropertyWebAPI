//-----------------------------------------------------------------------
// <copyright file="MortgageServicer.cs" company="Redq Technologies, Inc.">
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
    public class MortgageServicerDetails
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long?  requestId;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string externalReferenceId;
        public string status;
        public string BBL;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string servicerName;
    }

    #endregion

    /// <summary>
    ///     This class deals with all the details associated with either returning waterbill details or creating the 
    ///     request for getting is scrapped from the web 
    /// </summary>
    public static class MortgageServicer
    {
        /// <summary>
        /// Helper class used for serialization and deserialization of parameters necessary to get Tax bill 
        /// </summary>
        private class Parameters
        {
            public string BBL;
        }

        /// <summary>
        ///     This methods converts all paramters required for Tax Bills into a JSON object
        /// </summary>
        /// <param name="BBL"></param>
        /// <returns>JSON string</returns>
        private static string ParametersToJSON(string BBL)
        {
            Parameters parameters = new Parameters();
            parameters.BBL = BBL;
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
            DAL.DataRequestLog.InsertForFailure(propertyBBL, (int)RequestTypes.Zillow, externalReferenceId, "Error Code: " + ((HttpStatusCode)httpErrorCode).ToString());
        }

        /// <summary>
        ///     This method deals with all the details associated with either returning the tax bill details or creating the 
        ///     request for getting is scrapped from the web 
        /// </summary>
        /// <param name="propertyBBL"></param>
        /// <param name="externalReferenceId"></param>
        /// <returns></returns>
        public static MortgageServicerDetails Get(string propertyBBL, string externalReferenceId)
        {
            MortgageServicerDetails mServicerDetails = new MortgageServicerDetails();
            mServicerDetails.BBL = propertyBBL;
            mServicerDetails.externalReferenceId = externalReferenceId;
            mServicerDetails.status = RequestStatus.Pending.ToString();

            string parameters = ParametersToJSON(propertyBBL);

            using (WebDataEntities webDBEntities = new WebDataEntities())
            {
                using (var webDBEntitiestransaction = webDBEntities.Database.BeginTransaction())
                {
                    try
                    {
                        string jsonBillParams = ParametersToJSON(propertyBBL);

                        //check if data available
                        WebDataDB.MortgageServicer mortgageServicerObj = webDBEntities.MortgageServicers.FirstOrDefault(i => i.BBL == propertyBBL);

                        // record in database and data is not stale
                        if (mortgageServicerObj != null && DateTime.UtcNow.Subtract(mortgageServicerObj.LastUpdated).Days <= 30)
                        {
                            mServicerDetails.servicerName = mortgageServicerObj.Name;
                            mServicerDetails.status = RequestStatus.Success.ToString();

                            DAL.DataRequestLog.InsertForCacheAccess(webDBEntities, propertyBBL, (int)RequestTypes.NYCMortgageServicer, externalReferenceId, jsonBillParams);
                        }
                        else
                        {   //check if pending request in queue
                            DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetPendingRequest(webDBEntities, propertyBBL, (int)RequestTypes.NYCMortgageServicer, jsonBillParams);

                            if (dataRequestLogObj == null) //No Pending Request Create New Request
                            {
                                string requestStr = DexiRobotRequestResponseBuilder.Request.RequestData.ServicerWebsite(propertyBBL);

                                Request requestObj = DAL.Request.Insert(webDBEntities, requestStr, (int)RequestTypes.Zillow, null);

                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, (int)RequestTypes.NYCMortgageServicer, requestObj.RequestId,
                                                                                               externalReferenceId, jsonBillParams);

                                mServicerDetails.status = RequestStatus.Pending.ToString();
                                mServicerDetails.requestId = requestObj.RequestId;
                            }
                            else //Pending request in queue
                            {
                                mServicerDetails.status = RequestStatus.Pending.ToString();
                                //Send the RequestId for the pending request back
                                mServicerDetails.requestId = dataRequestLogObj.RequestId;
                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, (int)RequestTypes.NYCMortgageServicer,
                                                                                               dataRequestLogObj.RequestId.GetValueOrDefault(), externalReferenceId, jsonBillParams);
                            }
                        }
                        webDBEntitiestransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        mServicerDetails.status = RequestStatus.Error.ToString();
                        DAL.DataRequestLog.InsertForFailure(propertyBBL, (int)RequestTypes.Zillow, externalReferenceId, parameters);
                        Common.Logs.log().Error(string.Format("Exception encountered processing {0} with externalRefId {1}\n{2}\n", propertyBBL, externalReferenceId, e.ToString()));
                    }
                }
            }
            return mServicerDetails;
        }

        /// <summary>
        ///     This method gets the data or current status for a request 
        /// </summary>
        /// <param name="dataRequestLogObj"></param>
        /// <returns></returns>
        public static MortgageServicerDetails ReRun(DataRequestLog dataRequestLogObj)
        {
            MortgageServicerDetails mServicerDetails = new MortgageServicerDetails();
            mServicerDetails.BBL = dataRequestLogObj.BBL;
            mServicerDetails.requestId = dataRequestLogObj.RequestId;
            mServicerDetails.externalReferenceId = dataRequestLogObj.ExternalReferenceId;
            mServicerDetails.status = ((RequestStatus)dataRequestLogObj.RequestStatusTypeId).ToString();
            
            try
            {
                using (WebDataEntities webDBEntities = new WebDataEntities())
                {
                    if (dataRequestLogObj.RequestStatusTypeId == (int)RequestStatus.Success)
                    {
                        Parameters parameters = JSONToParameters(dataRequestLogObj.RequestParameters);
                        //check if data available
                        WebDataDB.MortgageServicer mortgageServicerObj = webDBEntities.MortgageServicers.FirstOrDefault(i => i.BBL == parameters.BBL);

                        if (mortgageServicerObj != null && DateTime.UtcNow.Subtract(mortgageServicerObj.LastUpdated).Days <= 30)
                            mServicerDetails.servicerName = mortgageServicerObj.Name;
                        else
                            mServicerDetails.status = RequestStatus.Error.ToString();
                    }
                }
                return mServicerDetails;
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered processing request log for {0} with externalRefId {1}\n{2}\n", dataRequestLogObj.BBL, dataRequestLogObj.ExternalReferenceId, e.ToString()));
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
                                        DexiRobotRequestResponseBuilder.Response.Servicer resultObj = (DexiRobotRequestResponseBuilder.Response.ResponseData.ParseServicer(requestObj.ResponseData))[0];

                                        Parameters parameters = JSONToParameters(dataRequestLogObj.RequestParameters);
                                        //check if old data in the DB
                                        WebDataDB.MortgageServicer mortgageServicerObj = webDBEntities.MortgageServicers.FirstOrDefault(i => i.BBL == parameters.BBL);
                                        if (mortgageServicerObj != null)
                                        {   //Update data with new results
                                            mortgageServicerObj.Name = resultObj.ServicerName;
                                            mortgageServicerObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();
                                        }
                                        else
                                        {   // add an entry into cache or DB
                                            mortgageServicerObj = new WebDataDB.MortgageServicer();
                                            mortgageServicerObj.BBL = parameters.BBL;
                                            mortgageServicerObj.Name = resultObj.ServicerName;
                                            mortgageServicerObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();

                                            webDBEntities.MortgageServicers.Add(mortgageServicerObj);
                                        }

                                        webDBEntities.SaveChanges();

                                        DAL.DataRequestLog.SetAsSuccess(webDBEntities, requestObj.RequestId);
                                    }
                                    break;
                                }
                            default:
                                Common.Logs.log().Warn(string.Format("Update called for a Request Object Id {0} with incorrect Status Id {1}", requestObj.RequestId, requestObj.RequestStatusTypeId));
                                break;
                        }

                        webDBEntitiestransaction.Commit();
                        return true;
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        Common.Logs.log().Error(string.Format("Exception encountered updating request with id {0}\n{1}\n", requestObj.RequestId, e.ToString()));
                        return false;
                    }
                }
            }
        }
    }
}