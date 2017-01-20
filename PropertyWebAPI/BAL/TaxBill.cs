//-----------------------------------------------------------------------
// <copyright file="TaxBill.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI.BAL
{
    using System;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using Newtonsoft.Json;
    using WebDataDB;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using DexiRobotRequestResponseBuilder.Request;
    using DexiRobotRequestResponseBuilder.Response;



    #region Local Helper Classes
    /// <summary>
    /// Helper class used to capture Tax bill details and used for serialization into JSON object 
    /// </summary>
    public class TaxBillDetails: NYCBaseResult
    {   
        /// <summary>
        /// Amount owed in taxes
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Decimal? billAmount;
    }
    #endregion

    /// <summary>
    ///     This class deals with all the details associated with either returning tax bill details or creating the 
    ///     request for getting is scrapped from the web 
    /// </summary>
    public static class TaxBill
    {
        private const int RequestTypeId = (int)RequestTypes.NYCTaxBill;

        /// <summary>
        /// Helper class used for serialization and deserialization of parameters necessary to get Tax bill 
        /// </summary>
        [DataContract]
        private class Parameters
        {   [DataMember]
            public string BBL;
        }

        /// <summary>
        ///     This methods converts all parameters required for Tax Bills into a JSON object
        /// </summary>
        /// <param name="propertyBBL"></param>
        /// <returns>JSON string</returns>
        private static string ParametersToJSON(string propertyBBL)
        {
            Parameters taxParams = new Parameters();
            taxParams.BBL = propertyBBL;
            return JsonConvert.SerializeObject(taxParams);
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
        /// <param name="billAmount">Bill Amount</param>
        /// <param name="logs">List or Request Log Records</param>
        private static void MakePortalCallBacks(List<DataRequestLog> logs, Decimal? billAmount)
        {
            var resultObj = new BAL.Results();
            resultObj.taxBill = new TaxBillDetails();
            resultObj.taxBill.billAmount = billAmount;

            foreach (var rec in logs)
            {
                resultObj.taxBill.BBL = rec.BBL;
                resultObj.taxBill.requestId = rec.RequestId;
                resultObj.taxBill.status = ((RequestStatus)rec.RequestStatusTypeId).ToString();
                resultObj.taxBill.externalReferenceId = rec.ExternalReferenceId;
                Portal.PostCallBack(resultObj);
            }
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
        /// <summary>
        ///     This method deals with all the details associated with either returning the tax bill details or creating the 
        ///     request for getting it scrapped from the web 
        /// </summary>
        /// <param name="propertyBBL"></param>
        /// <param name="externalReferenceId"></param>
        /// <returns></returns>
        public static TaxBillDetails Get(string propertyBBL, string externalReferenceId)
        {
            TaxBillDetails taxBill = new TaxBillDetails();
            taxBill.BBL = propertyBBL;
            taxBill.externalReferenceId = externalReferenceId;
            taxBill.status = RequestStatus.Pending.ToString();
            taxBill.billAmount = null;

            string parameters = ParametersToJSON(propertyBBL);

            using (WebDataEntities webDBEntities = new WebDataEntities())
            {
                using (var webDBEntitiestransaction = webDBEntities.Database.BeginTransaction())
                {
                    try
                    {
                        //check if data available
                        WebDataDB.TaxBill taxBillObj = webDBEntities.TaxBills.FirstOrDefault(i => i.BBL == propertyBBL);

                        // record in database and data is not stale
                        if (taxBillObj != null && DateTime.UtcNow.Subtract(taxBillObj.LastUpdated).Days <= 30)
                        {
                            taxBill.billAmount = taxBillObj.BillAmount;
                            taxBill.status = RequestStatus.Success.ToString();

                            DAL.DataRequestLog.InsertForCacheAccess(webDBEntities, propertyBBL, RequestTypeId, externalReferenceId, parameters);
                        }
                        else
                        {   //check if pending request in queue
                            DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetPendingRequest(webDBEntities, propertyBBL, RequestTypeId, parameters);

                            if (dataRequestLogObj == null) //No Pending Request Create New Request
                            {
                                string requestStr = RequestData.PropertyTaxesNYC(propertyBBL);

                                Request requestObj = DAL.Request.Insert(webDBEntities, requestStr, RequestTypeId, null);

                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, RequestTypeId, requestObj.RequestId, 
                                                                                               externalReferenceId, parameters);

                                taxBill.status = RequestStatus.Pending.ToString();
                                taxBill.requestId = requestObj.RequestId;
                            }
                            else //Pending request in queue
                            {
                                taxBill.status = RequestStatus.Pending.ToString();
                                //Send the RequestId for the pending request back
                                taxBill.requestId = dataRequestLogObj.RequestId;
                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, RequestTypeId, 
                                                                                               dataRequestLogObj.RequestId.GetValueOrDefault(), externalReferenceId, parameters);
                            }
                        }
                        webDBEntitiestransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        taxBill.status = RequestStatus.Error.ToString();
                        DAL.DataRequestLog.InsertForFailure(propertyBBL, RequestTypeId, externalReferenceId, parameters);
                        Common.Logs.log().Error(string.Format("Exception encountered processing {0} with externalRefId {1}{2}", propertyBBL, externalReferenceId, Common.Utilities.FormatException(e)));
                    }
                }
            }
            return taxBill;
        }

        /// <summary>
        ///     This method gets the data or current status for a request 
        /// </summary>
        /// <param name="dataRequestLogObj"></param>
        /// <returns></returns>
        public static TaxBillDetails ReRun(DataRequestLog dataRequestLogObj)
        {
            TaxBillDetails taxBill = new TaxBillDetails();
            taxBill.BBL = dataRequestLogObj.BBL;
            taxBill.requestId = dataRequestLogObj.RequestId;
            taxBill.externalReferenceId = dataRequestLogObj.ExternalReferenceId;
            taxBill.status = ((RequestStatus)dataRequestLogObj.RequestStatusTypeId).ToString(); 
            taxBill.billAmount = null;

            try
            {
                using (WebDataEntities webDBEntities = new WebDataEntities())
                {
                    if (dataRequestLogObj.RequestStatusTypeId == (int)RequestStatus.Success)
                    {
                        Parameters taxParams = JSONToParameters(dataRequestLogObj.RequestParameters);
                        //check if data available
                        WebDataDB.TaxBill taxBillObj = webDBEntities.TaxBills.FirstOrDefault(i => i.BBL == taxParams.BBL);

                        if (taxBillObj != null && DateTime.UtcNow.Subtract(taxBillObj.LastUpdated).Days <= 30)
                            taxBill.billAmount = taxBillObj.BillAmount;
                        else
                            taxBill.status = RequestStatus.Error.ToString();
                    }
                }
                return taxBill;
            }
            catch(Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered precessing request log for {0} with externalRefId {1}{2}", 
                                                       dataRequestLogObj.BBL, dataRequestLogObj.ExternalReferenceId, Common.Utilities.FormatException(e)));
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
                        List<DataRequestLog> logs=null;
                        Decimal? billAmount = null;

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
                                        var resultObj = ResponseData.ParsePropertyTaxesNYC(requestObj.ResponseData)[0];
                                        billAmount = resultObj.TotalDueAmountToPay.GetValueOrDefault();

                                        Parameters taxBillParams = JSONToParameters(dataRequestLogObj.RequestParameters);
                                        //check if old data in the DB
                                        WebDataDB.TaxBill taxBillObj = webDBEntities.TaxBills.FirstOrDefault(i => i.BBL == taxBillParams.BBL);
                                        if (taxBillObj != null)
                                        {   //Update data with new results
                                            taxBillObj.BillAmount = resultObj.TotalDueAmountToPay.GetValueOrDefault();
                                            taxBillObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();
                                        }
                                        else
                                        {   // add an entry into cache or DB
                                            taxBillObj = new WebDataDB.TaxBill();
                                            taxBillObj.BBL = taxBillParams.BBL;
                                            taxBillObj.BillAmount = resultObj.TotalDueAmountToPay.GetValueOrDefault();
                                            taxBillObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();

                                            webDBEntities.TaxBills.Add(taxBillObj);
                                        }

                                        webDBEntities.SaveChanges();
                                        logs = DAL.DataRequestLog.SetAsSuccess(webDBEntities, requestObj.RequestId);
                                    }
                                    else
                                        throw (new Exception("Cannot locate Request Log Record(s)"));

                                    break;
                                }
                            default:
                                Common.Logs.log().Warn(String.Format("Update called for a Request Object Id {0} with incorrect Status Id {2}", requestObj.RequestId, requestObj.RequestStatusTypeId));
                                break;
                        }
                        webDBEntitiestransaction.Commit();
                        if (logs != null)
                            MakePortalCallBacks(logs,billAmount);
                        return true;
                    }
                    catch(Exception e )
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