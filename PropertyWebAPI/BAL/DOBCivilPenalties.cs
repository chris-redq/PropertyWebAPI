//-----------------------------------------------------------------------
// <copyright file="DOBCivilPenalties.cs" company="Redq Technologies, Inc.">
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

    #region Local Helper Classes
    /// <summary>
    /// Helper class used to capture DOB Civil Penalties detail and used for serialization into JSON object 
    /// </summary>
    public class DOBCivilPenaltiesDetail
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? requestId;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String externalReferenceId;
        public string status;
        public string BBL;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? penaltyAmount;
    }
       
    #endregion

    /// <summary>
    ///     This class deals with all the details associated with either returning waterbill details or creating the 
    ///     request for getting is scrapped from the web 
    /// </summary>
    public static class DOBCivilPenalties
    {
        /// <summary>
        /// Helper class used for serialization and deserialization of parameters necessary to get Tax bill 
        /// </summary>
        class DOBCivilPenaltiesParams
        {
            public string BBL;
        }

        /// <summary>
        ///     This methods converts all paramters required for DOB Civil Penalties into a JSON object
        /// </summary>
        /// <param name="propertyBBL"></param>
        /// <returns>JSON string</returns>
        private static string ParametersToJSON(string propertyBBL)
        {
            DOBCivilPenaltiesParams dParams = new DOBCivilPenaltiesParams();
            dParams.BBL = propertyBBL;
            return JsonConvert.SerializeObject(dParams);
        }

        /// <summary>
        ///     This method converts a JSON back into dCivilPenaltiesParameters Object
        /// </summary>
        /// <param name="jsonParameters"></param>
        /// <returns>dCivilPenaltiesParameters</returns>
        private static DOBCivilPenaltiesParams JSONToParameters(string jsonParameters)
        {
            DOBCivilPenaltiesParams dParams = new DOBCivilPenaltiesParams();
            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonParameters));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(dParams.GetType());
            dParams = (DOBCivilPenaltiesParams)serializer.ReadObject(ms);
            ms.Close();
            return dParams;
        }

        /// <summary>
        ///     This method deals with all the details associated with either returning the tax bill details or creating the 
        ///     request for getting is scrapped from the web 
        /// </summary>
        /// <param name="propertyBBL"></param>
        /// <param name="externalReferenceId"></param>
        /// <returns></returns>
        public static DOBCivilPenaltiesDetail Get(string propertyBBL, string externalReferenceId)
        {
            DOBCivilPenaltiesDetail dCivilPenalties = new DOBCivilPenaltiesDetail();
            dCivilPenalties.BBL = propertyBBL;
            dCivilPenalties.externalReferenceId = externalReferenceId;
            dCivilPenalties.status = RequestStatus.Pending.ToString();
            dCivilPenalties.penaltyAmount = null;

            using (WebDataEntities webDBEntities = new WebDataEntities())
            {
                using (var webDBEntitiestransaction = webDBEntities.Database.BeginTransaction())
                {
                    try
                    {
                        string jsonParams = ParametersToJSON(propertyBBL);

                        //check if data available
                        WebDataDB.DOBViolation dobViolationObj = webDBEntities.DOBViolations.FirstOrDefault(i => i.BBL == propertyBBL);

                        // record in database and data is not stale
                        if (dobViolationObj != null && DateTime.UtcNow.Subtract(dobViolationObj.LastUpdated).Days <= 15)
                        {
                            dCivilPenalties.penaltyAmount = dobViolationObj.DOBCivilPenalties;
                            dCivilPenalties.status = RequestStatus.Success.ToString();

                            DAL.DataRequestLog.InsertForCacheAccess(webDBEntities, propertyBBL, (int)RequestTypes.DOBCivilPenalties, externalReferenceId, jsonParams);
                        }
                        else
                        {   //check if pending request in queue
                            DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetPendingRequest(webDBEntities, propertyBBL, (int)RequestTypes.DOBCivilPenalties, jsonParams);

                            if (dataRequestLogObj == null) //No Pending Request Create New Request
                            {
                                string requestStr = propertyBBL; // we need a helper class to convert propertyBBL into a correct format so that the webscrapping service can read

                                Request requestObj = DAL.Request.Insert(webDBEntities, requestStr, (int)RequestTypes.DOBCivilPenalties, null);

                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, (int)RequestTypes.DOBCivilPenalties, requestObj.RequestId,
                                                                                               externalReferenceId, jsonParams);

                                dCivilPenalties.status = RequestStatus.Pending.ToString();
                                dCivilPenalties.requestId = requestObj.RequestId;
                            }
                            else //Pending request in queue
                            {
                                dCivilPenalties.status = RequestStatus.Pending.ToString();
                                //Send the RequestId for the pending request back
                                dCivilPenalties.requestId = dataRequestLogObj.RequestId;
                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, (int)RequestTypes.DOBCivilPenalties,
                                                                                               dataRequestLogObj.RequestId.GetValueOrDefault(), externalReferenceId, jsonParams);
                            }
                        }
                        webDBEntitiestransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        dCivilPenalties.status = RequestStatus.Error.ToString();
                        //log externalReferenceId error from exception
                    }
                }
            }
            return dCivilPenalties;
        }

        /// <summary>
        ///     This method gets the data or current status for a request 
        /// </summary>
        /// <param name="dataRequestLogObj"></param>
        /// <returns></returns>
        public static DOBCivilPenaltiesDetail ReRun(DataRequestLog dataRequestLogObj)
        {
            DOBCivilPenaltiesDetail dCivilPenalties = new DOBCivilPenaltiesDetail();
            dCivilPenalties.BBL = dataRequestLogObj.BBL;
            dCivilPenalties.requestId = dataRequestLogObj.RequestId;
            dCivilPenalties.externalReferenceId = dataRequestLogObj.ExternalReferenceId;
            dCivilPenalties.status = ((RequestStatus)dataRequestLogObj.RequestStatusTypeId).ToString(); ;
            dCivilPenalties.penaltyAmount= null;

            DOBCivilPenaltiesParams taxParams = JSONToParameters(dataRequestLogObj.RequestParameters);

            using (WebDataEntities webDBEntities = new WebDataEntities())
            {
                if (dataRequestLogObj.RequestStatusTypeId == (int)RequestStatus.Success)
                {
                    //check if data available
                    WebDataDB.DOBViolation dCivilPenaltiesObj = webDBEntities.DOBViolations.FirstOrDefault(i => i.BBL == taxParams.BBL);

                    if (dCivilPenaltiesObj != null && DateTime.UtcNow.Subtract(dCivilPenaltiesObj.LastUpdated).Days <= 30)
                        dCivilPenalties.penaltyAmount = dCivilPenaltiesObj.DOBCivilPenalties;
                    else
                        dCivilPenalties.status = RequestStatus.Error.ToString();
                }
            }
            return dCivilPenalties;
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
                                        DOBCivilPenaltiesParams dCivilPenaltiesParams = JSONToParameters(dataRequestLogObj.RequestParameters);

                                        //check if data available
                                        WebDataDB.DOBViolation dCivilPenaltiesObj = webDBEntities.DOBViolations.FirstOrDefault(i => i.BBL == dCivilPenaltiesParams.BBL);
                                        if (dCivilPenaltiesObj != null)
                                        {
                                            dCivilPenaltiesObj.DOBCivilPenalties = 100; //parse from Request requestObj.ResponseData with helper class
                                            dCivilPenaltiesObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();
                                        }
                                        else
                                        {
                                            dCivilPenaltiesObj = new WebDataDB.DOBViolation();
                                            dCivilPenaltiesObj.BBL = dCivilPenaltiesParams.BBL;
                                            dCivilPenaltiesObj.DOBCivilPenalties = 100; //parse from Request requestObj.ResponseData with helper class                                           dCivilPenaltiesObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();

                                            webDBEntities.DOBViolations.Add(dCivilPenaltiesObj);
                                        }

                                        webDBEntities.SaveChanges();

                                        DAL.DataRequestLog.SetAsSuccess(webDBEntities, requestObj.RequestId);
                                    }
                                    break;
                                }
                            default:
                                break;
                        }

                        webDBEntitiestransaction.Commit();
                        return true;
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        //Log something
                        return false;
                    }
                }
            }
        }
    }
}