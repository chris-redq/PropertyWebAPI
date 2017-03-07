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
        ///     This method deals with all the details associated with either returning the water bill details or creating the 
        ///     request for getting is scrapped from the web 
        /// </summary>
        public static WaterBillDetails Get(string propertyBBL, string externalReferenceId)
        {
            return Get(propertyBBL, externalReferenceId, DAL.Request.MEDIUMPRIORITY, null);
        }

        /// <summary>
        ///     This method deals with all the details associated with either returning the water bill details or creating the 
        ///     request for getting is scrapped from the web 
        /// </summary>
        public static WaterBillDetails Get(string propertyBBL, string externalReferenceId, int priority, string jobId)
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
                    try
                    {
                        string jsonBillParams = ParametersToJSON(propertyBBL);

                        //check if data available
                        WebDataDB.WaterBill waterBillObj = webDBEntities.WaterBills.FirstOrDefault(i => i.BBL == propertyBBL);

                        // record in database and data is not stale
                        if (waterBillObj != null && DateTime.UtcNow.Subtract(waterBillObj.LastUpdated).Days <= 30)
                        {
                            waterBill.billAmount = waterBillObj.BillAmount;
                            waterBill.status = RequestStatus.Success.ToString();

                            DAL.DataRequestLog.InsertForCacheAccess(webDBEntities, propertyBBL, (int)RequestTypes.NYCWaterBill, externalReferenceId, jobId, jsonBillParams);
                        }
                        else
                        {
                            //check if pending request in queue
                            DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetPendingRequest(webDBEntities, propertyBBL, (int)RequestTypes.NYCWaterBill, jsonBillParams);

                            if (dataRequestLogObj == null) //No Pending Request Create New Request
                            {
                                string requestStr = propertyBBL; // we need a helper class to convert propertyBBL into a correct format so that the web scrapping service can read

                                Request requestObj = DAL.Request.Insert(webDBEntities, requestStr, (int)RequestTypes.NYCWaterBill, priority, jobId);

                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, (int)RequestTypes.NYCWaterBill, requestObj.RequestId,
                                                                                               externalReferenceId, jobId, jsonBillParams);

                                waterBill.status = RequestStatus.Pending.ToString();
                                waterBill.requestId = requestObj.RequestId;
                            }
                            else //Pending request in queue
                            {
                                waterBill.status = RequestStatus.Pending.ToString();
                                //Send the RequestId for the pending request back
                                waterBill.requestId = dataRequestLogObj.RequestId;
                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, (int)RequestTypes.NYCWaterBill,
                                                                                               dataRequestLogObj.RequestId.GetValueOrDefault(), externalReferenceId, jobId, jsonBillParams);
                            }
                        }
                        webDBEntitiestransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        waterBill.status = RequestStatus.Error.ToString();
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

            Parameters waterParams = JSONToParameters(dataRequestLogObj.RequestParameters);

            using (WebDataEntities webDBEntities = new WebDataEntities())
            {
                if (dataRequestLogObj.RequestStatusTypeId == (int)RequestStatus.Success)
                {
                    //check if data available
                    WebDataDB.WaterBill waterBillObj = webDBEntities.WaterBills.FirstOrDefault(i => i.BBL == waterParams.BBL);

                    if (waterBillObj != null && DateTime.UtcNow.Subtract(waterBillObj.LastUpdated).Days <= 30)
                        waterBill.billAmount = waterBillObj.BillAmount;
                    else
                        waterBill.status = RequestStatus.Error.ToString();
                }
            }
            return waterBill;
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
                                        Parameters waterBillParams = JSONToParameters(dataRequestLogObj.RequestParameters);

                                        //check if data available
                                        WebDataDB.WaterBill waterBillObj = webDBEntities.WaterBills.FirstOrDefault(i => i.BBL == waterBillParams.BBL);
                                        if (waterBillObj != null)
                                        {
                                            waterBillObj.BillAmount = 100; //parse from Request requestObj.ResponseData with helper class
                                            waterBillObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();
                                        }
                                        else
                                        {
                                            waterBillObj = new WebDataDB.WaterBill();
                                            waterBillObj.BBL = waterBillParams.BBL;
                                            waterBillObj.BillAmount = 100; //parse from Request requestObj.ResponseData with helper class
                                            waterBillObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();

                                            webDBEntities.WaterBills.Add(waterBillObj);
                                        }

                                        webDBEntities.SaveChanges();

                                        DAL.DataRequestLog.SetAsSuccess(webDBEntities, requestObj.RequestId);
                                    }
                                    break;
                                }
                            default:
                                Common.Logs.log().Warn(String.Format("Update called for a Request Id {0} with incorrect Status Id {2}", requestObj.RequestId, requestObj.RequestStatusTypeId));
                                break;
                        }

                        webDBEntitiestransaction.Commit();
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