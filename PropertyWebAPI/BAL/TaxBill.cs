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
    using Newtonsoft.Json;
    using WebDataDB;
    using System.IO;
    using System.Text;
    using System.Runtime.Serialization.Json;

    #region Local Helper Classes
    /// <summary>
    /// Helper class used to capture Tax bill details and used for serialization into JSON object 
    /// </summary>
    public class TaxBillDetails
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? requestId;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string externalReferenceId;
        public string status;
        public string BBL;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Decimal? billAmount;
    }

    /// <summary>
    /// Helper class used for serialization and deserialization of parameters necessary to get Tax bill 
    /// </summary>
    public class TaxBillParameters
    {
        public string BBL;
    }

    #endregion

    /// <summary>
    ///     This class deals with all the details associated with either returning waterbill details or creating the 
    ///     request for getting is scrapped from the web 
    /// </summary>
    public static class TaxBill
    {
        /// <summary>
        ///     This methods converts all paramters required for Tax Bills into a JSON object
        /// </summary>
        /// <param name="propertyBBL"></param>
        /// <returns>JSON string</returns>
        public static string ParametersToJSON(string propertyBBL)
        {
            TaxBillParameters taxParams = new TaxBillParameters();
            taxParams.BBL = propertyBBL;
            return JsonConvert.SerializeObject(taxParams);
        }

        /// <summary>
        ///     This method converts a JSON back into TaxBillParameters Object
        /// </summary>
        /// <param name="jsonParameters"></param>
        /// <returns>TaxBillParameters</returns>
        public static TaxBillParameters JSONToParameters(string jsonParameters)
        {
            TaxBillParameters taxParams = new TaxBillParameters();
            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonParameters));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(taxParams.GetType());
            taxParams = (TaxBillParameters)serializer.ReadObject(ms);
            ms.Close();
            return taxParams;
        }

        /// <summary>
        ///     This method deals with all the details associated with either returning the tax bill details or creating the 
        ///     request for getting is scrapped from the web 
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

            using (WebDataEntities webDBEntities = new WebDataEntities())
            {
                using (var webDBEntitiestransaction = webDBEntities.Database.BeginTransaction())
                {
                    try
                    {
                        string jsonBillParams = ParametersToJSON(propertyBBL);

                        //check if data available
                        WebDataDB.TaxBill taxBillObj = webDBEntities.TaxBills.FirstOrDefault(i => i.BBL == propertyBBL);

                        // record in database and data is not stale
                        if (taxBillObj != null && DateTime.UtcNow.Subtract(taxBillObj.LastUpdated).Days <= 30)
                        {
                            taxBill.billAmount = taxBillObj.BillAmount;
                            taxBill.status = RequestStatus.Success.ToString();

                            DAL.DataRequestLog.InsertForCacheAccess(webDBEntities, propertyBBL, (int)RequestTypes.TaxBill, externalReferenceId, jsonBillParams);
                        }
                        else
                        {   //check if pending request in queue
                            DataRequestLog dataRequestLogObj = DAL.DataRequestLog.GetPendingRequest(webDBEntities, propertyBBL, (int)RequestTypes.TaxBill, jsonBillParams);

                            if (dataRequestLogObj == null) //No Pending Request Create New Request
                            {
                                string requestStr = propertyBBL; // we need a helper class to convert propertyBBL into a correct format so that the webscrapping service can read

                                Request requestObj = DAL.Request.Insert(webDBEntities, requestStr, (int)RequestTypes.TaxBill, null);

                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, (int)RequestTypes.TaxBill, requestObj.RequestId, 
                                                                                               externalReferenceId, jsonBillParams);

                                taxBill.status = RequestStatus.Pending.ToString();
                                taxBill.requestId = requestObj.RequestId;
                            }
                            else //Pending request in queue
                            {
                                taxBill.status = RequestStatus.Pending.ToString();
                                //Send the RequestId for the pending request back
                                taxBill.requestId = dataRequestLogObj.RequestId;
                                dataRequestLogObj = DAL.DataRequestLog.InsertForWebDataRequest(webDBEntities, propertyBBL, (int)RequestTypes.TaxBill, 
                                                                                               dataRequestLogObj.RequestId.GetValueOrDefault(), externalReferenceId, jsonBillParams);
                            }
                        }
                        webDBEntitiestransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        webDBEntitiestransaction.Rollback();
                        taxBill.status = RequestStatus.Error.ToString();
                        //log externalReferenceId error from exception
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
            taxBill.status = ((RequestStatus)dataRequestLogObj.RequestStatusTypeId).ToString(); ;
            taxBill.billAmount = null;

            TaxBillParameters taxParams = JSONToParameters(dataRequestLogObj.RequestParameters);

            using (WebDataEntities webDBEntities = new WebDataEntities())
            {
                if (dataRequestLogObj.RequestStatusTypeId == (int)RequestStatus.Success)
                {
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

        /// <summary>
        ///     This method updates the TaxBill table based on the information received from the Request Object
        /// </summary>
        /// <param name="requestObj"></param>
        /// <returns></returns>
        public static void UpdateData(Request requestObj)
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
                                    if (dataRequestLogObj == null)
                                        return;
                                    TaxBillParameters taxBillParams = JSONToParameters(dataRequestLogObj.RequestParameters);

                                    //check if data available
                                    WebDataDB.TaxBill taxBillObj = webDBEntities.TaxBills.FirstOrDefault(i => i.BBL == taxBillParams.BBL);
                                    if (taxBillObj != null)
                                    {
                                        taxBillObj.BillAmount = 100; //parse from Request requestObj.ResponseData with helper class
                                        taxBillObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();
                                    }
                                    else
                                    {
                                        taxBillObj = new WebDataDB.TaxBill();
                                        taxBillObj.BBL = taxBillParams.BBL;
                                        taxBillObj.BillAmount = 100; //parse from Request requestObj.ResponseData with helper class
                                        taxBillObj.LastUpdated = requestObj.DateTimeEnded.GetValueOrDefault();

                                        webDBEntities.TaxBills.Add(taxBillObj);
                                    }

                                    webDBEntities.SaveChanges();

                                    DAL.DataRequestLog.SetAsError(webDBEntities, requestObj.RequestId);
                                    break;
                                }
                            default:
                                break;
                        }
                       
                        webDBEntitiestransaction.Commit();
                    }
                    catch(Exception e )
                    {
                        webDBEntitiestransaction.Rollback();
                        //Log something 
                    }
                }
            }
        }
    }
}