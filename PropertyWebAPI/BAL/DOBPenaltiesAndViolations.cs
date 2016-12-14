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

    #region Local Helper Classes
    /// <summary>
    /// Helper class used to capture DOB Civil Penalties detail and used for serialization into JSON object 
    /// </summary>
    public class DOBPenaltiesAndViolationsSummaryData
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? requestId;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String externalReferenceId;
        public string status;
        public string BBL;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? civilPenaltyAmount;
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
        /// <summary>
        /// Helper class used for serialization and deserialization of parameters necessary to get Tax bill 
        /// </summary>
        [DataContract]
        class DOBPenaltiesAndViolationsParams
        {   [DataMember]
            public string BBL;
        }

        /// <summary>
        ///     This methods converts all parameters required for DOB Civil Penalties into a JSON object
        /// </summary>
        /// <param name="propertyBBL"></param>
        /// <returns>JSON string</returns>
        private static string ParametersToJSON(string propertyBBL)
        {
            DOBPenaltiesAndViolationsParams dParams = new DOBPenaltiesAndViolationsParams();
            dParams.BBL = propertyBBL;
            return JsonConvert.SerializeObject(dParams);
        }

        /// <summary>
        ///     This method converts a JSON back into dCivilPenaltiesParameters Object
        /// </summary>
        /// <param name="jsonParameters"></param>
        /// <returns>dCivilPenaltiesParameters</returns>
        private static DOBPenaltiesAndViolationsParams JSONToParameters(string jsonParameters)
        {
            DOBPenaltiesAndViolationsParams dParams = new DOBPenaltiesAndViolationsParams();
            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonParameters));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(dParams.GetType());
            dParams = (DOBPenaltiesAndViolationsParams)serializer.ReadObject(ms);
            ms.Close();
            return dParams;
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
        public static DOBPenaltiesAndViolationsSummaryData Get(string propertyBBL, string externalReferenceId)
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

                            DAL.DataRequestLog.InsertForCacheAccess(webDBEntities, propertyBBL, (int)RequestTypes.NYCDOBPenaltiesAndViolations , externalReferenceId, parameters);
                        }
                        else
                        {   //check if pending request in queue
                            DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetPendingRequest(webDBEntities, propertyBBL, (int)RequestTypes.NYCDOBPenaltiesAndViolations, parameters);

                            if (dataRequestLogObj == null) //No Pending Request Create New Request
                            {
                                string requestStr = DexiRobotRequestResponseBuilder.Request.RequestData.ECBviolationAndDOBCivilPenalties(propertyBBL);

                                Request requestObj = DAL.Request.Insert(webDBEntities, requestStr, (int)RequestTypes.NYCDOBPenaltiesAndViolations, null);

                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, (int)RequestTypes.NYCDOBPenaltiesAndViolations, requestObj.RequestId,
                                                                                               externalReferenceId, parameters);

                                dPenaltiesAndViolations.status = RequestStatus.Pending.ToString();
                                dPenaltiesAndViolations.requestId = requestObj.RequestId;
                            }
                            else //Pending request in queue
                            {
                                dPenaltiesAndViolations.status = RequestStatus.Pending.ToString();
                                //Send the RequestId for the pending request back
                                dPenaltiesAndViolations.requestId = dataRequestLogObj.RequestId;
                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, (int)RequestTypes.NYCDOBPenaltiesAndViolations,
                                                                                               dataRequestLogObj.RequestId.GetValueOrDefault(), externalReferenceId, parameters);
                            }
                        }
                        webDBEntitiestransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        dPenaltiesAndViolations.status = RequestStatus.Error.ToString();
                        DAL.DataRequestLog.InsertForFailure(propertyBBL, (int)RequestTypes.Zillow, externalReferenceId, parameters);
                        Common.Logs.log().Error(string.Format("Exception encountered processing {0} with externalRefId {1}{2}", 
                                                propertyBBL, externalReferenceId, Common.Utilities.FormatException(e)));
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
                DOBPenaltiesAndViolationsParams taxParams = JSONToParameters(dataRequestLogObj.RequestParameters);

                using (WebDataEntities webDBEntities = new WebDataEntities())
                {
                    if (dataRequestLogObj.RequestStatusTypeId == (int)RequestStatus.Success)
                    {
                        //check if data available
                        WebDataDB.DOBViolation dCivilPenaltiesObj = webDBEntities.DOBViolations.FirstOrDefault(i => i.BBL == taxParams.BBL);

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
                                                       dataRequestLogObj.BBL, dataRequestLogObj.ExternalReferenceId, Common.Utilities.FormatException(e)));
                return null;
            }
        }

        /// <summary>
        ///     This method updates the dCivilPenalties table based on the information received from the Request Object
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
                                        decimal dobTotalPenaltyAmount = 0;
                                        decimal dobTotalViolationAmount = 0;
                                        
                                        foreach (DexiRobotRequestResponseBuilder.Response.ECBViolationAndDOBCivilPenalty row in 
                                                 DexiRobotRequestResponseBuilder.Response.ResponseData.ParseECBviolationAndDOBCivilPenalty(requestObj.ResponseData))
                                        {
                                            dobTotalPenaltyAmount += decimal.Parse(row.DOBCivilPenaltyAmount, System.Globalization.NumberStyles.Any);
                                            dobTotalViolationAmount += decimal.Parse(row.ECBPenaltyDue, System.Globalization.NumberStyles.Any);
                                        }

                                        DOBPenaltiesAndViolationsParams dCivilPenaltiesParams = JSONToParameters(dataRequestLogObj.RequestParameters);
                                        //check if data available
                                        WebDataDB.DOBViolation dobPenaltiesAndViolationsObj = webDBEntities.DOBViolations.FirstOrDefault(i => i.BBL == dCivilPenaltiesParams.BBL);
                                        if (dobPenaltiesAndViolationsObj != null)
                                        {   
                                            dobPenaltiesAndViolationsObj.DOBCivilPenalties = dobTotalPenaltyAmount;
                                            dobPenaltiesAndViolationsObj.ECBViolationAmount = dobTotalViolationAmount;
                                            dobPenaltiesAndViolationsObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();
                                        }
                                        else
                                        {
                                            dobPenaltiesAndViolationsObj = new WebDataDB.DOBViolation();
                                            dobPenaltiesAndViolationsObj.BBL = dCivilPenaltiesParams.BBL;
                                            dobPenaltiesAndViolationsObj.DOBCivilPenalties = dobTotalPenaltyAmount; 
                                            dobPenaltiesAndViolationsObj.ECBViolationAmount = dobTotalViolationAmount;
                                            dobPenaltiesAndViolationsObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();

                                            webDBEntities.DOBViolations.Add(dobPenaltiesAndViolationsObj);
                                        }

                                        webDBEntities.SaveChanges();

                                        DAL.DataRequestLog.SetAsSuccess(webDBEntities, requestObj.RequestId);
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