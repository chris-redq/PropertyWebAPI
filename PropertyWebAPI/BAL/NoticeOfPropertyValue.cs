
namespace PropertyWebAPI.BAL
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using WebDataDB;
    using System.Net;
    using System.Runtime.Serialization;
    using RequestResponseBuilder.Response;
    using RequestResponseBuilder.Request;
    using System.Collections.Generic;
    using AutoMapper;


    public class NoticeOfPropertyValueResult : NYCBaseResult
    {
        public WebDataDB.NoticeOfProperyValue noticeOfPropertyValue;
    }

    public class NoticeOfPropertyValueService
    {
        private const int RequestTypeId = (int)RequestTypes.NYCNoticeOfPropertyValue;

        /// <summary>
        /// Helper class used for serialization and deserialization of parameters necessary to retrieve data
        /// </summary>
        [DataContract]
        private class Parameters
        {
            [DataMember]
            public string BBL;
        }

        /// <summary>
        ///     This methods converts all parameters required into a JSON object
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
            DAL.DataRequestLog.InsertForFailure(propertyBBL, RequestTypeId, externalReferenceId, "Error Code: " + ((HttpStatusCode)httpErrorCode).ToString());
        }

        private static WebDataDB.NoticeOfProperyValue CopyData(RequestResponseBuilder.Response.NoticeOfPropertyValue valObj)
        {
            WebDataDB.NoticeOfProperyValue resultObj = Mapper.Map<WebDataDB.NoticeOfProperyValue>(valObj);

            return resultObj;
        }

        /// <summary>
        ///     This method calls back portal for every log record in the list
        /// </summary>
        private static void MakePortalCallBacks(List<DataRequestLog> logs, RequestResponseBuilder.Response.NoticeOfPropertyValue noticeOfPropertyValueObj)
        {
            var resultObj = new BAL.Results();
            resultObj.noticeOfPropertyValueResult = new NoticeOfPropertyValueResult();
            resultObj.noticeOfPropertyValueResult.noticeOfPropertyValue = CopyData(noticeOfPropertyValueObj);

            foreach (var rec in logs)
            {
                resultObj.noticeOfPropertyValueResult.BBL = rec.BBL;
                resultObj.noticeOfPropertyValueResult.requestId = rec.RequestId;
                resultObj.noticeOfPropertyValueResult.status = ((RequestStatus)rec.RequestStatusTypeId).ToString();
                resultObj.noticeOfPropertyValueResult.externalReferenceId = rec.ExternalReferenceId;
                CallingSystem.PostCallBack(resultObj);
            }
        }
        /// <summary>
        ///     This method deals with all the details associated with either returning the Notice Of Property Value details or creating the 
        ///     request for getting the data from the web 
        /// </summary>
        /// <param name="propertyBBL"></param>
        /// <param name="externalReferenceId"></param>
        /// <returns></returns>
        public static NoticeOfPropertyValueResult Get(string propertyBBL, string externalReferenceId)
        {
            NoticeOfPropertyValueResult NPOVResultObj = new NoticeOfPropertyValueResult();
            NPOVResultObj.BBL = propertyBBL;
            NPOVResultObj.externalReferenceId = externalReferenceId;
            NPOVResultObj.status = RequestStatus.Pending.ToString();

            string parameters = ParametersToJSON(propertyBBL);

            using (WebDataEntities webDBEntities = new WebDataEntities())
            {
                using (var webDBEntitiestransaction = webDBEntities.Database.BeginTransaction())
                {
                    try
                    {
                        string jsonBillParams = ParametersToJSON(propertyBBL);

                        //check if data available
                        var noticeOfPropertyValueObj = webDBEntities.NoticeOfProperyValues.FirstOrDefault(i => i.BBL == propertyBBL);

                        // record in database and data is not stale
                        if (noticeOfPropertyValueObj != null && DateTime.UtcNow.Subtract(noticeOfPropertyValueObj.LastUpdated).Days <= 30)
                        {
                            NPOVResultObj.noticeOfPropertyValue = noticeOfPropertyValueObj;
                            NPOVResultObj.status = RequestStatus.Success.ToString();

                            DAL.DataRequestLog.InsertForCacheAccess(webDBEntities, propertyBBL, RequestTypeId, externalReferenceId, jsonBillParams);
                        }
                        else
                        {   //check if pending request in queue
                            DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetPendingRequest(webDBEntities, propertyBBL, RequestTypeId, jsonBillParams);

                            if (dataRequestLogObj == null) //No Pending Request Create New Request
                            {
                                string requestStr = RequestData.ServicerWebsite(propertyBBL);

                                Request requestObj = DAL.Request.Insert(webDBEntities, requestStr, RequestTypeId, null);

                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, RequestTypeId, requestObj.RequestId,
                                                                                               externalReferenceId, jsonBillParams);

                                NPOVResultObj.status = RequestStatus.Pending.ToString();
                                NPOVResultObj.requestId = requestObj.RequestId;
                            }
                            else //Pending request in queue
                            {
                                NPOVResultObj.status = RequestStatus.Pending.ToString();
                                //Send the RequestId for the pending request back
                                NPOVResultObj.requestId = dataRequestLogObj.RequestId;
                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, RequestTypeId,
                                                                                               dataRequestLogObj.RequestId.GetValueOrDefault(), externalReferenceId, jsonBillParams);
                            }
                        }
                        webDBEntitiestransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        NPOVResultObj.status = RequestStatus.Error.ToString();
                        DAL.DataRequestLog.InsertForFailure(propertyBBL, RequestTypeId, externalReferenceId, parameters);
                        Common.Logs.log().Error(string.Format("Exception encountered processing {0} with externalRefId {1}{2}",
                                                propertyBBL, externalReferenceId, Common.Utilities.FormatException(e)));
                    }
                }
            }
            return NPOVResultObj;
        }

        /// <summary>
        ///     This method gets the data or current status for a request 
        /// </summary>
        /// <param name="dataRequestLogObj"></param>
        /// <returns></returns>
        public static NoticeOfPropertyValueResult ReRun(DataRequestLog dataRequestLogObj)
        {
            NoticeOfPropertyValueResult resultObj = new NoticeOfPropertyValueResult();
            resultObj.BBL = dataRequestLogObj.BBL;
            resultObj.requestId = dataRequestLogObj.RequestId;
            resultObj.externalReferenceId = dataRequestLogObj.ExternalReferenceId;
            resultObj.status = ((RequestStatus)dataRequestLogObj.RequestStatusTypeId).ToString();

            try
            {
                using (WebDataEntities webDBEntities = new WebDataEntities())
                {
                    if (dataRequestLogObj.RequestStatusTypeId == (int)RequestStatus.Success)
                    {
                        Parameters parameters = JSONToParameters(dataRequestLogObj.RequestParameters);
                        //check if data available
                        WebDataDB.NoticeOfProperyValue noticeOfPropetyValueObj = webDBEntities.NoticeOfProperyValues.FirstOrDefault(i => i.BBL == parameters.BBL);

                        if (noticeOfPropetyValueObj != null && DateTime.UtcNow.Subtract(noticeOfPropetyValueObj.LastUpdated).Days <= 30)
                            resultObj.noticeOfPropertyValue = noticeOfPropetyValueObj;
                        else
                            resultObj.status = RequestStatus.Error.ToString();
                    }
                }
                return resultObj;
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered processing request log for {0} with externalRefId {1}{2}",
                                                       dataRequestLogObj.BBL, dataRequestLogObj.ExternalReferenceId, Common.Utilities.FormatException(e)));
                return null;
            }
        }

        /// <summary>
        ///     This method updates the Mortgage Servicer table based on the information received from the Request Object
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
                        List<DataRequestLog> logs = null;
                        RequestResponseBuilder.Response.NoticeOfPropertyValue resultObj = null;

                        switch (requestObj.RequestStatusTypeId)
                        {
                            case (int)RequestStatus.Error:
                                logs = DAL.DataRequestLog.SetAsError(webDBEntities, requestObj.RequestId);
                                break;
                            case (int)RequestStatus.Success:
                                {
                                    DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetFirst(webDBEntities, requestObj.RequestId);
                                    if (dataRequestLogObj != null)
                                    {
                                        resultObj = ResponseData.ParseNoticeOfPropertyValue(requestObj.ResponseData);
                                        
                                        Parameters parameters = JSONToParameters(dataRequestLogObj.RequestParameters);
                                        //check if old data in the DB
                                        var noticeOfPropertyValueObj = webDBEntities.NoticeOfProperyValues.FirstOrDefault(i => i.BBL == parameters.BBL);

                                        if (noticeOfPropertyValueObj != null)
                                        {   //Update data with new results
                                            noticeOfPropertyValueObj = CopyData(resultObj);
                                            noticeOfPropertyValueObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();
                                        }
                                        else
                                        {   // add an entry into cache or DB
                                            noticeOfPropertyValueObj = CopyData(resultObj);
                                            noticeOfPropertyValueObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();
                                            webDBEntities.NoticeOfProperyValues.Add(noticeOfPropertyValueObj);
                                        }

                                        webDBEntities.SaveChanges();

                                        logs = DAL.DataRequestLog.SetAsSuccess(webDBEntities, requestObj.RequestId);
                                    }
                                    else
                                        throw (new Exception("Cannot locate Request Log Record(s)"));
                                    break;
                                }
                            default:
                                Common.Logs.log().Warn(string.Format("Update called for a Request Object Id {0} with incorrect Status Id {1}", requestObj.RequestId, requestObj.RequestStatusTypeId));
                                break;
                        }

                        webDBEntitiestransaction.Commit();
                        if (logs != null)
                            MakePortalCallBacks(logs, resultObj);
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