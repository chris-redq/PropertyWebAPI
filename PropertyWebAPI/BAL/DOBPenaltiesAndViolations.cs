//-----------------------------------------------------------------------
// <copyright file="DOBPenaltiesAndViolations.cs" company="Redq Technologies, Inc.">
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
    using System.Net;
    using System.Text;
    using System.Runtime.Serialization.Json;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using RequestResponseBuilder;
    using RequestResponseBuilder.ResponseObjects;

    #region Local Helper Classes
    /// <summary>
    /// Helper class used to capture DOB Civil Penalties detail and used for serialization into JSON object 
    /// </summary>
    public class DOBPenaltiesAndViolationsSummaryData: NYCBaseResult
    {
        /// <summary>
        /// Sum total of all Civil Penalties due
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? civilPenaltyAmount;
        /// <summary>
        /// Sum total of all ECB Violations due
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? violationAmount;
    }
       
    #endregion

    /// <summary>
    ///     This class deals with all the details associated with either returning DOB Violations and Penalties details or creating the 
    ///     request to get data scrapped from the web 
    /// </summary>
    public static class DOBPenaltiesAndViolationsSummary
    {
        private const int RequestTypeId = (int)RequestTypes.NYCDOBPenaltiesAndViolations;

        /// <summary>
        /// Helper class used for serialization and deserialization of parameters necessary to get DOB Penalties and ECB Violations data 
        /// </summary>
        [DataContract]
        class Parameters
        {   [DataMember]
            public string BBL;
        }

        /// <summary>
        ///     This methods converts all parameters required for DOB Civil Penalties and ECB Violations into a JSON object
        /// </summary>
        /// <param name="propertyBBL"></param>
        /// <returns>JSON string</returns>
        private static string ParametersToJSON(string propertyBBL)
        {
            var dParams = new Parameters();
            dParams.BBL = propertyBBL;
            return JsonConvert.SerializeObject(dParams);
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
        ///     This method calls back portal for every log record in the list
        /// </summary>
        private static void MakeCallBacks(List<DataRequestLog> logs, decimal? penaltyAmount, decimal? violationAmount)
        {
            var resultObj = new BAL.Results();
            resultObj.dobPenaltiesAndViolationsSummary = new DOBPenaltiesAndViolationsSummaryData();
            resultObj.dobPenaltiesAndViolationsSummary.civilPenaltyAmount = penaltyAmount;
            resultObj.dobPenaltiesAndViolationsSummary.violationAmount = violationAmount;

            foreach (var rec in logs)
            {
                var cb = CallingSystem.isAnyCallBack(rec.AccountId);
                if (cb==null)
                    continue;

                resultObj.dobPenaltiesAndViolationsSummary.BBL = rec.BBL;
                resultObj.dobPenaltiesAndViolationsSummary.requestId = rec.RequestId;
                resultObj.dobPenaltiesAndViolationsSummary.status = ((RequestStatus)rec.RequestStatusTypeId).ToString();
                resultObj.dobPenaltiesAndViolationsSummary.externalReferenceId = rec.ExternalReferenceId;
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
        ///     This method deals with all the details associated with either returning the DOB Penalties and Violations details or creating the 
        ///     request for getting it scrapped from the web 
        /// </summary>
        public static DOBPenaltiesAndViolationsSummaryData Get(Common.Context appContext, string propertyBBL, string externalReferenceId)
        {
            return Get(appContext, propertyBBL, externalReferenceId, DAL.Request.MEDIUMPRIORITY, null);
        }
        
        /// <summary>
        ///     This method deals with all the details associated with either returning the DOB Penalties and Violations details or creating the 
        ///     request for getting it scrapped from the web 
        /// </summary>
        public static DOBPenaltiesAndViolationsSummaryData Get(Common.Context appContext, string propertyBBL, string externalReferenceId, int priority, string jobId)
        {
            DOBPenaltiesAndViolationsSummaryData dPenaltiesAndViolations = new DOBPenaltiesAndViolationsSummaryData();
            dPenaltiesAndViolations.BBL = propertyBBL;
            dPenaltiesAndViolations.externalReferenceId = externalReferenceId;
            dPenaltiesAndViolations.status = RequestStatus.Pending.ToString();
            dPenaltiesAndViolations.civilPenaltyAmount = null;
            dPenaltiesAndViolations.violationAmount = null;

            string parameters = ParametersToJSON(propertyBBL);

            using (WebDataEntities webDBEntities = new WebDataEntities())
            {
                using (var webDBEntitiestransaction = webDBEntities.Database.BeginTransaction())
                {
                    try
                    {
                        //check if data available
                        WebDataDB.DOBViolation dobViolationObj = webDBEntities.DOBViolations.FirstOrDefault(i => i.BBL == propertyBBL);

                        // record in database and data is not stale
                        if (dobViolationObj != null && DateTime.UtcNow.Subtract(dobViolationObj.LastUpdated).Days <= 15)
                        {
                            dPenaltiesAndViolations.civilPenaltyAmount = dobViolationObj.DOBCivilPenalties;
                            dPenaltiesAndViolations.violationAmount = dobViolationObj.ECBViolationAmount;
                            dPenaltiesAndViolations.status = RequestStatus.Success.ToString();

                            DAL.DataRequestLog.InsertForCacheAccess(webDBEntities, propertyBBL, RequestTypeId , externalReferenceId, jobId, parameters);
                        }
                        else
                        {   //check if pending request in queue
                            DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetPendingRequest(webDBEntities, propertyBBL, RequestTypeId, parameters);

                            if (dataRequestLogObj == null) //No Pending Request Create New Request
                            {
                                string requestStr = RequestResponseBuilder.RequestObjects.RequestData.ECBviolationAndDOBCivilPenalties(propertyBBL);

                                Request requestObj = DAL.Request.Insert(webDBEntities, requestStr, RequestTypeId, priority, jobId);

                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, RequestTypeId, requestObj.RequestId,
                                                                                               externalReferenceId, jobId, appContext.getAccountId(), parameters);

                                dPenaltiesAndViolations.status = RequestStatus.Pending.ToString();
                                dPenaltiesAndViolations.requestId = requestObj.RequestId;
                            }
                            else //Pending request in queue
                            {
                                dPenaltiesAndViolations.status = RequestStatus.Pending.ToString();
                                //Send the RequestId for the pending request back
                                dPenaltiesAndViolations.requestId = dataRequestLogObj.RequestId;
                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, RequestTypeId,
                                                                                               dataRequestLogObj.RequestId.GetValueOrDefault(), externalReferenceId, 
                                                                                               jobId, appContext.getAccountId(), parameters);
                            }
                        }
                        webDBEntitiestransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        dPenaltiesAndViolations.status = RequestStatus.Error.ToString();
                        DAL.DataRequestLog.InsertForFailure(propertyBBL, RequestTypeId, externalReferenceId, jobId, parameters);
                        Common.Logs.log().Error(string.Format("Exception encountered processing {0} with externalRefId {1}{2}", 
                                                propertyBBL, externalReferenceId, Common.Logs.FormatException(e)));
                    }
                }
            }
            return dPenaltiesAndViolations;
        }

        /// <summary>
        ///     This method gets the data or current status for a request 
        /// </summary>
        /// <param name="dataRequestLogObj"></param>
        /// <returns></returns>
        public static DOBPenaltiesAndViolationsSummaryData ReRun(DataRequestLog dataRequestLogObj)
        {
            DOBPenaltiesAndViolationsSummaryData dPenaltiesAndViolations = new DOBPenaltiesAndViolationsSummaryData();
            dPenaltiesAndViolations.BBL = dataRequestLogObj.BBL;
            dPenaltiesAndViolations.requestId = dataRequestLogObj.RequestId;
            dPenaltiesAndViolations.externalReferenceId = dataRequestLogObj.ExternalReferenceId;
            dPenaltiesAndViolations.status = ((RequestStatus)dataRequestLogObj.RequestStatusTypeId).ToString(); ;
            dPenaltiesAndViolations.civilPenaltyAmount = null;
            dPenaltiesAndViolations.violationAmount = null;

            try
            {
                Parameters parameters = JSONToParameters(dataRequestLogObj.RequestParameters);

                using (WebDataEntities webDBEntities = new WebDataEntities())
                {
                    if (dataRequestLogObj.RequestStatusTypeId == (int)RequestStatus.Success)
                    {
                        //check if data available
                        WebDataDB.DOBViolation dCivilPenaltiesObj = webDBEntities.DOBViolations.FirstOrDefault(i => i.BBL == parameters.BBL);

                        if (dCivilPenaltiesObj != null && DateTime.UtcNow.Subtract(dCivilPenaltiesObj.LastUpdated).Days <= 30)
                        {
                            dPenaltiesAndViolations.civilPenaltyAmount = dCivilPenaltiesObj.DOBCivilPenalties;
                            dPenaltiesAndViolations.violationAmount = dCivilPenaltiesObj.ECBViolationAmount;
                        }
                        else
                            dPenaltiesAndViolations.status = RequestStatus.Error.ToString();
                    }
                }
                return dPenaltiesAndViolations;
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered processing request log for {0} with externalRefId {1}{2}", 
                                                       dataRequestLogObj.BBL, dataRequestLogObj.ExternalReferenceId, Common.Logs.FormatException(e)));
                return null;
            }
        }

        /// <summary>
        ///     This method updates the dCivilPenalties table based on the information received from the Request Object
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
                        decimal? penaltyAmount = null, violationAmount = null;

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
                                        decimal dobTotalPenaltyAmount = 0;
                                        decimal dobTotalViolationAmount = 0;

                                        foreach (var row in ResponseData.ParseECBviolationAndDOBCivilPenalty(requestObj.ResponseData).DOBCivilPenalties)
                                            dobTotalPenaltyAmount += row.DOBCivilPenaltyAmount.GetValueOrDefault();

                                        foreach (var row in ResponseData.ParseECBviolationAndDOBCivilPenalty(requestObj.ResponseData).ECBViolations)
                                            dobTotalViolationAmount += row.ECBPenaltyDue.GetValueOrDefault();

                                        penaltyAmount = dobTotalPenaltyAmount;
                                        violationAmount = dobTotalViolationAmount;

                                        Parameters parameters = JSONToParameters(dataRequestLogObj.RequestParameters);
                                        //check if data available
                                        WebDataDB.DOBViolation dobPenaltiesAndViolationsObj = webDBEntities.DOBViolations.FirstOrDefault(i => i.BBL == parameters.BBL);
                                        if (dobPenaltiesAndViolationsObj != null)
                                        {   
                                            dobPenaltiesAndViolationsObj.DOBCivilPenalties = dobTotalPenaltyAmount;
                                            dobPenaltiesAndViolationsObj.ECBViolationAmount = dobTotalViolationAmount;
                                            dobPenaltiesAndViolationsObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();
                                        }
                                        else
                                        {
                                            dobPenaltiesAndViolationsObj = new WebDataDB.DOBViolation();
                                            dobPenaltiesAndViolationsObj.BBL = parameters.BBL;
                                            dobPenaltiesAndViolationsObj.DOBCivilPenalties = dobTotalPenaltyAmount; 
                                            dobPenaltiesAndViolationsObj.ECBViolationAmount = dobTotalViolationAmount;
                                            dobPenaltiesAndViolationsObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();

                                            webDBEntities.DOBViolations.Add(dobPenaltiesAndViolationsObj);
                                        }

                                        webDBEntities.SaveChanges();

                                        logs=DAL.DataRequestLog.SetAsSuccess(webDBEntities, requestObj.RequestId);
                                    }
                                    else
                                        throw (new Exception("Cannot locate Request Log Record(s)"));
                                    break;
                                }
                            default:
                                Common.Logs.log().Warn(string.Format("Update called for a Request Object Id {0} with incorrect Status Id {1}",requestObj.RequestId,requestObj.RequestStatusTypeId));
                                break;
                        }

                        webDBEntitiestransaction.Commit();
                        if (logs != null)
                            MakeCallBacks(logs, penaltyAmount, violationAmount);
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