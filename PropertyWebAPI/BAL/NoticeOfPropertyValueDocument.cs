
namespace PropertyWebAPI.BAL
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using WebDataDB;
    using System.Net;
    using System.Runtime.Serialization;
    using RequestResponseBuilder;
    using RequestResponseBuilder.RequestObjects;
    using RequestResponseBuilder.ResponseObjects;
    using System.Collections.Generic;
    using AutoMapper;
    using System.Data.Entity;
    using Common;
    
    public class NoticeOfPropertyValueResult : NYCBaseResult
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WebDataDB.NoticeOfProperyValue noticeOfPropertyValue;
    }

    public class NoticeOfPropertyValueDocument
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
        public static void LogFailure(string propertyBBL, string externalReferenceId, string jobId, int httpErrorCode)
        {
            DAL.DataRequestLog.InsertForFailure(propertyBBL, RequestTypeId, externalReferenceId, jobId, "Error Code: " + ((HttpStatusCode)httpErrorCode).ToString());
        }

        private static WebDataDB.NoticeOfProperyValue CopyData(WebDataDB.NoticeOfProperyValue resultObj, RequestResponseBuilder.ResponseObjects.NoticeOfPropertyValue valObj)
        {
            if (resultObj== null)
                resultObj = new WebDataDB.NoticeOfProperyValue();
            
            resultObj.BuildingClass = Conversions.GetSingleValue(valObj.BuildingClass);
            resultObj.PrimaryZoning = Conversions.Concat(valObj.PrimaryZoning);
            resultObj.LotFrontage = Conversions.ToDecimalorNull(valObj.LotFrontage);
            resultObj.LotDepth = Conversions.ToDecimalorNull(valObj.LotDepth);
            resultObj.LotSquareFootage = Conversions.ToDecimalorNull(valObj.LotSquareFootage);
            resultObj.LotShape = Conversions.Concat(valObj.LotShape);
            resultObj.LotType = Conversions.Concat(valObj.LotType);
            resultObj.Proximity = Conversions.Concat(valObj.Proximity);
            resultObj.BuildingFrontage = Conversions.ToDecimalorNull(valObj.BuildingFrontage);
            resultObj.BuildingDepth = Conversions.ToDecimalorNull(valObj.BuildingDepth);
            resultObj.NumberOfBuildings = Conversions.ToIntorNull(valObj.NumberofBuildings);
            resultObj.Style = Conversions.Concat(valObj.Style);
            resultObj.YearBuilt = Conversions.Concat(valObj.YearBuilt);
            resultObj.ExteriorCondition = Conversions.Concat(valObj.ExteriorCondition);
            resultObj.FinishedSquareFootage = Conversions.ToDecimalorNull(valObj.FinishedSquareFootage);
            resultObj.UnfinishedSquareFootage = Conversions.ToDecimalorNull(valObj.UnfinishedSquareFootage);
            resultObj.CommercialUnits = Conversions.ToIntorNull(valObj.CommercialUnits);
            resultObj.CommercialSquareFootage = Conversions.ToDecimalorNull(valObj.CommercialSquareFootage);
            resultObj.ResidentialUnits = Conversions.ToIntorNull(valObj.ResidentialUnits);
            resultObj.GarageType = Conversions.Concat(valObj.GarageType);
            resultObj.GarageSquareFootage = Conversions.ToDecimalorNull(valObj.GarageSquareFootage);
            resultObj.BasementGrade = Conversions.Concat(valObj.BasementGrade);
            resultObj.BasementSquareFootage = Conversions.ToDecimalorNull(valObj.BasementSquareFootage);
            resultObj.BasementType = Conversions.Concat(valObj.BasementType);
            resultObj.ConstructionType = Conversions.Concat(valObj.ConstructionType);
            resultObj.ExteriorWall = Conversions.Concat(valObj.ExteriorWall);
            resultObj.NumberOfStories = Conversions.ToDecimalorNull(valObj.NumberofStories);
            resultObj.StructureType = Conversions.Concat(valObj.StructureType);
            resultObj.Grade = Conversions.Concat(valObj.Grade);
            resultObj.GrossSquareFootage = Conversions.ToDecimalorNull(valObj.GrossSquareFootage);
            resultObj.CommercialOverlay = Conversions.Concat(valObj.CommercialOverlay);
            resultObj.LandFactorPerSqFtinDollars = Conversions.ToDecimalorNull(valObj.LandFactorperSquareFoot);
            resultObj.GrossResidentialSquareFootage = Conversions.ToDecimalorNull(valObj.GrossResidentialSquareFootage);
            resultObj.LandType1 = Conversions.Concat(valObj.LandType1);
            resultObj.LandSize1 = Conversions.ToDecimalorNull(valObj.LandSize1);
            resultObj.NumberOfRooms = Conversions.ToIntorNull(valObj.NumberOfRooms);
            resultObj.HotelClass = Conversions.Concat(valObj.HotelClass);
            resultObj.NumberOfResidentialLots = Conversions.ToIntorNull(valObj.NumberOfResidentialLots);
            resultObj.NumberOfCommercialLots = Conversions.ToIntorNull(valObj.NumberOfCommercialLots);
            resultObj.GrossSquareFootageOfTheSuffix = Conversions.ToDecimalorNull(valObj.GrossSquareFootageOfTheSuffix);
            resultObj.CommercialLots= Conversions.ToIntorNull(valObj.CommercialLots);
            resultObj.LandFactorperSquareFoot = Conversions.ToDecimalorNull(valObj.LandFactorperSquareFoot);
            resultObj.NetSqFtOfLotFromCondoDeclaration = Conversions.ToDecimalorNull(valObj.NetSquareFootageoftheLotfromtheCondoDeclaration);
            resultObj.ResidentialLots= Conversions.ToIntorNull(valObj.ResidentialLots);

            return resultObj;
        }

        /// <summary>
        ///     This method calls back portal for every log record in the list
        /// </summary>
        private static void MakeCallBacks(List<DataRequestLog> logs, NoticeOfProperyValue noticeOfPropertyValueObj)
        {
            var resultObj = new BAL.Results();
            resultObj.noticeOfPropertyValueResult = new NoticeOfPropertyValueResult();
            resultObj.noticeOfPropertyValueResult.noticeOfPropertyValue = noticeOfPropertyValueObj;

            foreach (var rec in logs)
            {
                var cb = CallingSystem.isAnyCallBack(rec.AccountId);
                if (cb == null)
                    continue;

                resultObj.noticeOfPropertyValueResult.BBL = rec.BBL;
                resultObj.noticeOfPropertyValueResult.requestId = rec.RequestId;
                resultObj.noticeOfPropertyValueResult.status = ((RequestStatus)rec.RequestStatusTypeId).ToString();
                resultObj.noticeOfPropertyValueResult.externalReferenceId = rec.ExternalReferenceId;
                CallingSystem.PostCallBack(rec.AccountId, cb, resultObj);
            }
        }
        /// <summary>
        ///     This method deals with all the details associated with either returning the Notice Of Property Value details or creating the 
        ///     request for getting the data from the web 
        /// </summary>
        /// <returns></returns>
        public static NoticeOfPropertyValueResult GetDetails(Common.Context appContext, string propertyBBL, string externalReferenceId)
        {
            return GetDetails(appContext, propertyBBL, externalReferenceId, DAL.Request.MEDIUMPRIORITY, null);
        }

        /// <summary>
        ///     This method deals with all the details associated with either returning the Notice Of Property Value details or creating the 
        ///     request for getting the data from the web 
        /// </summary>
        /// <returns></returns>
        public static NoticeOfPropertyValueResult GetDetails(Common.Context appContext, string propertyBBL, string externalReferenceId, int priority, string jobId)
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

                            DAL.DataRequestLog.InsertForCacheAccess(webDBEntities, propertyBBL, RequestTypeId, externalReferenceId, jobId,  jsonBillParams);
                        }
                        else
                        {   //check if pending request in queue
                            DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetPendingRequest(webDBEntities, propertyBBL, RequestTypeId, jsonBillParams);

                            if (dataRequestLogObj == null) //No Pending Request Create New Request
                            {
                                string requestStr = RequestResponseBuilder.RequestObjects.RequestData.NoticeOfPropertyValue(propertyBBL);

                                Request requestObj = DAL.Request.Insert(webDBEntities, requestStr, RequestTypeId, priority, jobId);

                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, RequestTypeId, requestObj.RequestId,
                                                                                               externalReferenceId, jobId, appContext.getAccountId(), jsonBillParams);

                                NPOVResultObj.status = RequestStatus.Pending.ToString();
                                NPOVResultObj.requestId = requestObj.RequestId;
                            }
                            else //Pending request in queue
                            {
                                //Update Priority if need
                                Request requestObj = DAL.Request.Update(webDBEntities, dataRequestLogObj.RequestId.GetValueOrDefault(), priority, jobId);

                                NPOVResultObj.status = RequestStatus.Pending.ToString();
                                //Send the RequestId for the pending request back
                                NPOVResultObj.requestId = dataRequestLogObj.RequestId;
                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, RequestTypeId,
                                                                                               dataRequestLogObj.RequestId.GetValueOrDefault(), externalReferenceId, 
                                                                                               jobId, appContext.getAccountId(), jsonBillParams);
                            }
                        }
                        webDBEntitiestransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        NPOVResultObj.status = RequestStatus.Error.ToString();
                        DAL.DataRequestLog.InsertForFailure(propertyBBL, RequestTypeId, externalReferenceId, jobId, parameters);
                        Common.Logs.log().Error(string.Format("Exception encountered processing {0} with externalRefId {1}{2}",
                                                propertyBBL, externalReferenceId, Common.Logs.FormatException(e)));
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
                                                       dataRequestLogObj.BBL, dataRequestLogObj.ExternalReferenceId, Common.Logs.FormatException(e)));
                return null;
            }
        }

        /// <summary>
        ///     This method updates the Mortgage Servicer table based on the information received from the Request Object
        /// </summary>
        /// <param name="requestObj"></param>
        /// <param name="appContext"></param>
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
                        NoticeOfProperyValue noticeOfPropertyValueObj = null;

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
                                        var resultObj = ResponseData.ParseNoticeOfPropertyValue(requestObj.ResponseData);
                                        
                                        Parameters parameters = JSONToParameters(dataRequestLogObj.RequestParameters);
                                        //check if old data in the DB
                                        noticeOfPropertyValueObj = webDBEntities.NoticeOfProperyValues.FirstOrDefault(i => i.BBL == parameters.BBL);

                                        if (noticeOfPropertyValueObj != null)
                                        {   //Update data with new results
                                            noticeOfPropertyValueObj = CopyData(noticeOfPropertyValueObj, resultObj);
                                            noticeOfPropertyValueObj.BBL = parameters.BBL;
                                            noticeOfPropertyValueObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();
                                            webDBEntities.Entry(noticeOfPropertyValueObj).State = EntityState.Modified;
                                        }
                                        else
                                        {   // add an entry into cache or DB
                                            noticeOfPropertyValueObj = CopyData(null, resultObj);
                                            noticeOfPropertyValueObj.BBL = parameters.BBL;
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
                            MakeCallBacks(logs, noticeOfPropertyValueObj);
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