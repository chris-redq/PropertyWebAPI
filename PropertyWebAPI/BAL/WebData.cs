
namespace PropertyWebAPI.BAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;

    public class WebData
    {

        public static void BulkRetrieve(Common.Context appContext, List<string> bblList, int requestTypeId = 0, string externalReferenceId = null)
        { 
            string jobId = "SCH-" + DateTime.UtcNow.ToString() + " UTC";

            if (bblList == null || bblList.Count <= 0)
                return;
            
            foreach (var bbl in bblList)
            {
                if (!BAL.BBL.IsValidFormat(bbl))
                {                
                    Common.Logs.log().Warn(string.Format("Incorrect BBL {0} in Bulk Retrieve with Job Id {1}", bbl, jobId));
                    continue;
                }

                if (!BAL.BBL.IsValid(bbl))
                {
                    Common.Logs.log().Warn(string.Format("Incorrect BBL {0} in Bulk Retrieve with Job Id {1}", bbl, jobId));
                    continue;
                }

                switch (requestTypeId)
                {
                    case (int)RequestTypes.NYCTaxBill:
                        BAL.TaxBill.Get(appContext, bbl, externalReferenceId, DAL.Request.BULKDATAPRIORITY, jobId);
                        break;
                    case (int)RequestTypes.NYCWaterBill:
                        BAL.WaterBill.Get(appContext, bbl, externalReferenceId, DAL.Request.BULKDATAPRIORITY, jobId);
                        break;
                    case (int)RequestTypes.NYCMortgageServicer:
                        BAL.MortgageServicer.Get(appContext, bbl, externalReferenceId, DAL.Request.BULKDATAPRIORITY, jobId);
                        break;
                    case (int)RequestTypes.Zillow:
                        {
                            var propInfo = BAL.NYCPhysicalPropertyData.Get(bbl, true);
                            if (propInfo != null)
                                BAL.Zillow.LogFailure(bbl, externalReferenceId, jobId, (int)HttpStatusCode.BadRequest);
                            else
                                BAL.Zillow.Get(appContext, bbl, propInfo.address.ToString(), externalReferenceId, DAL.Request.BULKDATAPRIORITY, jobId);
                            break;
                        }
                    case (int)RequestTypes.NYCNoticeOfPropertyValue:
                        BAL.NoticeOfPropertyValueDocument.GetDetails(appContext, bbl, externalReferenceId, DAL.Request.BULKDATAPRIORITY, jobId);
                        break;
                    case (int)RequestTypes.NYCMortgageDocumentDetails:
                        BAL.MortgageDocument.GetDetailsAllUnstaisfiedMortgages(appContext, bbl, externalReferenceId, DAL.Request.BULKDATAPRIORITY, jobId);
                        break;
                    case 0:
                        {
                            BAL.TaxBill.Get(appContext, bbl, externalReferenceId, DAL.Request.BULKDATAPRIORITY, jobId);
                            BAL.WaterBill.Get(appContext, bbl, externalReferenceId, DAL.Request.BULKDATAPRIORITY, jobId);
                            BAL.MortgageServicer.Get(appContext, bbl, externalReferenceId, DAL.Request.BULKDATAPRIORITY, jobId);
                            var propInfo = BAL.NYCPhysicalPropertyData.Get(bbl, true);
                            if (propInfo != null)
                                BAL.Zillow.LogFailure(bbl, externalReferenceId, jobId, (int)HttpStatusCode.BadRequest);
                            else
                                BAL.Zillow.Get(appContext, bbl, propInfo.address.ToString(), externalReferenceId, DAL.Request.BULKDATAPRIORITY, jobId);
                            BAL.NoticeOfPropertyValueDocument.GetDetails(appContext, bbl, externalReferenceId, DAL.Request.BULKDATAPRIORITY, jobId);
                            BAL.MortgageDocument.GetDetailsAllUnstaisfiedMortgages(appContext, bbl, externalReferenceId, DAL.Request.BULKDATAPRIORITY, jobId);
                            break;
                        }
                    default:
                        String msg = String.Format("Cannot process request - Invalid Request Type: {0}", requestTypeId);
                        Common.Logs.log().Warn(msg);
                        return;
                }
            }
        }
    }
}