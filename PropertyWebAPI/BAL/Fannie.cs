//-----------------------------------------------------------------------
// <copyright file="Fannie.cs" company="Redq Technologies, Inc.">
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
    using System.Net;
    using System.Runtime.Serialization;
    using RequestResponseBuilder.ResponseObjects;
    using RequestResponseBuilder.RequestObjects;
    using System.Collections.Generic;
    using System.Data.Entity.Validation;
    using System.Data.Entity;

    #region Local Helper Classes
    /// <summary>
    /// Helper class used to capture Mortgage Servicer details and used for serialization into JSON object 
    /// </summary>
    public class FannieMortgageDetails : NYCBaseResult
    {
        /// <summary>True if there is a Fannie Mae Mortgage</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? isFannieMortgage;
    }

    #endregion

    /// <summary>
    ///     This class deals with all the details associated with either returning Mortgage Servicer details or creating the 
    ///     request to get data scrapped from the web 
    /// </summary>
    public static class Fannie
    {
        private const int RequestTypeId = (int)RequestTypes.FannieMortgage;

        /// <summary>
        /// Helper class used for serialization and deserialization of parameters necessary to get Mortgage Servicer data
        /// </summary>
        [DataContract]
        public class Parameters
        {
            public string BBL;
            public string firstName;
            public string lastName;
            public string address;
            public string unitNumber;
            public string city;
            public string state;
            public string zipCode;
            public string sSNLast4;
        }

        /// <summary>
        ///     This methods converts all parameters required for Mortgage Servicer into a JSON object
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>JSON string</returns>
        private static string ParametersToJSON(Parameters parameters)
        {
            return JsonConvert.SerializeObject(parameters);
        }

        /// <summary>
        ///     This method converts a JSON back into Mortgage Servicer Parameters Object
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
        /// <param name="jobId"></param>
        /// <param name="httpErrorCode"></param>
        /// <returns></returns>
        public static void LogFailure(string propertyBBL, string externalReferenceId, string jobId, int httpErrorCode)
        {
            DAL.DataRequestLog.InsertForFailure(propertyBBL, RequestTypeId, externalReferenceId, jobId, "Error Code: " + ((HttpStatusCode)httpErrorCode).ToString());
        }


        /// <summary>
        ///     This method calls back portal for every log record in the list
        /// </summary>
        private static void MakeCallBacks(Common.Context appContext, List<DataRequestLog> logs, bool? isFannieMaeMortgage)
        {
            if (!CallingSystem.isAnyCallBack(appContext))
                return;

            var resultObj = new BAL.Results();
            resultObj.fannieMaeResult = new FannieMortgageDetails();
            resultObj.fannieMaeResult.isFannieMortgage = isFannieMaeMortgage;

            foreach (var rec in logs)
            {
                resultObj.mortgageServicer.BBL = rec.BBL;
                resultObj.mortgageServicer.requestId = rec.RequestId;
                resultObj.mortgageServicer.status = ((RequestStatus)rec.RequestStatusTypeId).ToString();
                resultObj.mortgageServicer.externalReferenceId = rec.ExternalReferenceId;
                CallingSystem.PostCallBack(appContext, resultObj);
            }
        }

        /// <summary>
        ///     This method deals with all the details associated with either returning the Mortgage Servicer details or creating the 
        ///     request for getting it scrapped from the web 
        /// </summary>
        public static FannieMortgageDetails Get(Parameters parameters, string externalReferenceId)
        {
            return Get(parameters, externalReferenceId, DAL.Request.MEDIUMPRIORITY, null);
        }

        /// <summary>
        ///     This method deals with all the details associated with either returning the Mortgage Servicer details or creating the 
        ///     request for getting it scrapped from the web 
        /// </summary>
        public static FannieMortgageDetails Get(Parameters inParameters, string externalReferenceId, int priority, string jobId)
        {
            FannieMortgageDetails mDetails = new FannieMortgageDetails();
            mDetails.BBL = inParameters.BBL;
            mDetails.externalReferenceId = externalReferenceId;
            mDetails.status = RequestStatus.Pending.ToString();

            string parameters = ParametersToJSON(inParameters);

            using (WebDataEntities webDBEntities = new WebDataEntities())
            {
                using (var webDBEntitiestransaction = webDBEntities.Database.BeginTransaction())
                {
                    try
                    {
                        //check if data available
                        WebDataDB.FannieMortgage mortgageObj = webDBEntities.FannieMortgages.FirstOrDefault(i => i.BBL == inParameters.BBL);

                        // record in database and data is not stale
                        if (mortgageObj != null && DateTime.UtcNow.Subtract(mortgageObj.LastUpdated).Days <= 30)
                        {
                            mDetails.isFannieMortgage = mortgageObj.IsFannie;
                            mDetails.status = RequestStatus.Success.ToString();

                            DAL.DataRequestLog.InsertForCacheAccess(webDBEntities, inParameters.BBL, RequestTypeId, externalReferenceId, jobId, parameters);
                        }
                        else
                        {   //check if pending request in queue
                            DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetPendingRequest(webDBEntities, inParameters.BBL, RequestTypeId, parameters);

                            if (dataRequestLogObj == null) //No Pending Request Create New Request
                            {
                                string requestStr = "";//RequestData.XXXX(inParameters);

                                Request requestObj = DAL.Request.Insert(webDBEntities, requestStr, RequestTypeId, priority, jobId);

                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, inParameters.BBL, RequestTypeId, requestObj.RequestId,
                                                                                               externalReferenceId, jobId, parameters);

                                mDetails.status = RequestStatus.Pending.ToString();
                                mDetails.requestId = requestObj.RequestId;
                            }
                            else //Pending request in queue
                            {
                                mDetails.status = RequestStatus.Pending.ToString();
                                //Send the RequestId for the pending request back
                                mDetails.requestId = dataRequestLogObj.RequestId;
                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, inParameters.BBL, RequestTypeId,
                                                                                               dataRequestLogObj.RequestId.GetValueOrDefault(), externalReferenceId, jobId, parameters);
                            }
                        }
                        webDBEntitiestransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        mDetails.status = RequestStatus.Error.ToString();
                        DAL.DataRequestLog.InsertForFailure(inParameters.BBL, RequestTypeId, externalReferenceId, jobId, parameters);
                        Common.Logs.log().Error(string.Format("Exception encountered processing {0} with externalRefId {1}{2}",
                                                inParameters.BBL, externalReferenceId, Common.Logs.FormatException(e)));
                    }
                }
            }
            return mDetails;
        }

        /// <summary>
        ///     This method gets the data or current status for a request 
        /// </summary>
        /// <param name="dataRequestLogObj"></param>
        /// <returns></returns>
        public static FannieMortgageDetails ReRun(DataRequestLog dataRequestLogObj)
        {
            FannieMortgageDetails mDetails = new FannieMortgageDetails();
            mDetails.BBL = dataRequestLogObj.BBL;
            mDetails.requestId = dataRequestLogObj.RequestId;
            mDetails.externalReferenceId = dataRequestLogObj.ExternalReferenceId;
            mDetails.status = ((RequestStatus)dataRequestLogObj.RequestStatusTypeId).ToString();

            try
            {
                using (WebDataEntities webDBEntities = new WebDataEntities())
                {
                    if (dataRequestLogObj.RequestStatusTypeId == (int)RequestStatus.Success)
                    {
                        Parameters parameters = JSONToParameters(dataRequestLogObj.RequestParameters);
                        //check if data available
                        WebDataDB.FannieMortgage mortgageObj = webDBEntities.FannieMortgages.FirstOrDefault(i => i.BBL == parameters.BBL);

                        if (mortgageObj != null && DateTime.UtcNow.Subtract(mortgageObj.LastUpdated).Days <= 30)
                            mDetails.isFannieMortgage = mortgageObj.IsFannie;
                        else
                            mDetails.status = RequestStatus.Error.ToString();
                    }
                }
                return mDetails;
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered processing request log for {0} with externalRefId {1}{2}",
                                                       dataRequestLogObj.BBL, dataRequestLogObj.ExternalReferenceId, Common.Logs.FormatException(e)));
                return null;
            }
        }

        /// <summary>
        ///     This method updates the Mortgage Servicer table based on the information received from the Request Object
        /// </summary>
        /// <param name="appContext"></param>
        /// <param name="requestObj"></param>
        /// <returns>True if successful else false</returns>
        public static bool UpdateData(Common.Context appContext, Request requestObj)
        {
            using (WebDataEntities webDBEntities = new WebDataEntities())
            {
                using (var webDBEntitiestransaction = webDBEntities.Database.BeginTransaction())
                {
                    try
                    {
                        List<DataRequestLog> logs = null;
                        bool? isFannie = null;

                        switch (requestObj.RequestStatusTypeId)
                        {
                            case (int)RequestStatus.Error:
                                logs = DAL.DataRequestLog.SetAsError(webDBEntities, requestObj.RequestId);
                                break;
                            case (int)RequestStatus.Success:
                                {
                                    DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetFirst(webDBEntities, requestObj.RequestId);
                                    if (dataRequestLogObj == null)
                                        throw (new Exception("Cannot locate Request Log Record(s)"));

                                    //var resultObj =  // need to modify
                                    isFannie = false; // resultObj.ServicerName;

                                    if (isFannie == null)
                                        logs = DAL.DataRequestLog.SetAsError(webDBEntities, requestObj.RequestId);
                                    else
                                    {
                                        Parameters parameters = JSONToParameters(dataRequestLogObj.RequestParameters);
                                        //check if old data in the DB
                                        WebDataDB.FannieMortgage mortgageObj = webDBEntities.FannieMortgages.FirstOrDefault(i => i.BBL == parameters.BBL);
                                        if (mortgageObj != null)
                                        {   //Update data with new results
                                            mortgageObj.IsFannie = isFannie.GetValueOrDefault();
                                            mortgageObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();
                                            webDBEntities.Entry(mortgageObj).State = EntityState.Modified;
                                        }
                                        else
                                        {   // add an entry into cache or DB
                                            mortgageObj = new WebDataDB.FannieMortgage();
                                            mortgageObj.BBL = parameters.BBL;
                                            mortgageObj.IsFannie = isFannie.GetValueOrDefault();
                                            mortgageObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();

                                            webDBEntities.FannieMortgages.Add(mortgageObj);
                                        }

                                        webDBEntities.SaveChanges();

                                        logs = DAL.DataRequestLog.SetAsSuccess(webDBEntities, requestObj.RequestId);
                                    }
                                    break;
                                }
                            default:
                                Common.Logs.log().Warn(string.Format("Update called for a Request Object Id {0} with incorrect Status Id {1}", requestObj.RequestId, requestObj.RequestStatusTypeId));
                                break;
                        }

                        webDBEntitiestransaction.Commit();
                        if (logs != null)
                            MakeCallBacks(appContext, logs, isFannie);
                        return true;
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                Common.Logs.log().Error(string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage));
                            }
                        }
                        return false;
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