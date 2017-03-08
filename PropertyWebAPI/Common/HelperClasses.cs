using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http.Results;
using ACRISDB;
using NYCDOF;
using eCourtsDB;
using GPADB;
using NYCDOB;
using NYCMADB;
using NYCVNL;
using WebAPISecurityDB;
using log4net;
using AutoMapper;
using System.Text.RegularExpressions;
using System.Web.Http.Controllers;

namespace PropertyWebAPI.Common
{
    public class HelperClasses
    {
    }

    public static class AppInitialization
    {
        public static void Init()
        {

            Logs.Init();
            Logs.log().Info("Property Data Service Started");
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<tfnGetDocumentParties_Result, BAL.DeedParty>();
                cfg.CreateMap<PropertyNotInAssessment, BAL.PropertyLotInformation>();
                cfg.CreateMap<tfnGetGeneralPropertyInformation_Result, BAL.PhysicalPropertyInformation>();
                cfg.CreateMap<vwMotionExpanded, DAL.MotionDetails>();
                cfg.CreateMap<vwCaseExpanded, DAL.CaseDetails>();
                cfg.CreateMap<vwAppearanceExpanded, DAL.AppearanceDetails>();
                cfg.CreateMap<vwAttorneyExpanded, DAL.AttorneyDetails>();
                cfg.CreateMap<tfnGetCaseUpdates_Result, DAL.CaseUpdate>();
                cfg.CreateMap<tfnGetMortgageForeclosureLPsForaProperty_Result, DAL.LPCaseDetails>();
                cfg.CreateMap<tfnGetMortgageForeclosureCasesForaProperty_Result, DAL.CaseBasicInformation>();
                cfg.CreateMap<tfnGetNewMortgageForeclosureCases_Result, DAL.CaseBasicInformationWithBBL>();
                cfg.CreateMap<tfnGetCaseColumnChanges_Result, DAL.CaseDataChange>();
                cfg.CreateMap<tfnGetUnsatisfiedMortgages_Result, BAL.DeedDocument>();
                cfg.CreateMap<vwGeneralLeadInfomation, DAL.LeadSummaryData>();
                cfg.CreateMap<vwGeneralLeadInfomation, BAL.LeadDetailData>();
                cfg.CreateMap<tfnGetDocumentExtractForCase_Result, DAL.CaseDocumentDetails>();
                cfg.CreateMap<vwViolationSummary, DAL.ViolationSummary>();
                cfg.CreateMap<vwComplaintsSummary, DAL.DOBComplaintsSummary>();
                cfg.CreateMap<MultiAgencyViolation, DAL.ECBViolationDetail>();
                cfg.CreateMap<vwTaxLien, DAL.LocalTaxLienDetails>();
                cfg.CreateMap<vwSuggestedSubjectPrice, DAL.SuggestPropertPrices>();
                cfg.CreateMap<vwSalesByMonthByNeighborhood, DAL.SalesDataDetailsByMonth>();
                cfg.CreateMap<SalePriceStatisticsByMonthByNeighborhoodMeanSmoothing, DAL.PriceDetailsByMonth>();
                cfg.CreateMap<PricePerSqFtStatisticsByMonthByNeighborhoodMeanSmoothing, DAL.PricePerSqftDetailsByMonth>();
                cfg.CreateMap<ShowCMASubject_Result, DAL.SubjectDetails>();
                cfg.CreateMap<tfnGetManualCMA_Result, DAL.CMAManualResult>();
                cfg.CreateMap<tfnGetCMA_Result, DAL.CMAResult>();
                cfg.CreateMap<tfnActiveLPsForaProperty_Result, DAL.LPDetail>();
                cfg.CreateMap<WebAPISecurityDB.vwAPIUser, Security.User>();
            });
        }
    }

    public static class HttpResponse
    {
        public static ResponseMessageResult InternalError(HttpRequestMessage Request, string message)
        {
            return new ResponseMessageResult(Request.CreateErrorResponse((HttpStatusCode)500, message));
        }
    }

    public class Context
    {
        private const string HttpContext = "MS_HttpContext";
        private const string RemoteEndpointMessage = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";

        private string userName;
        private string userIPAddress;

        public Context(HttpRequestContext requestContext, HttpRequestMessage request)
        {
            userName = requestContext.Principal.Identity.Name;

            if (request.Properties.ContainsKey(HttpContext))
            {
                dynamic ctx = request.Properties[HttpContext];
                if (ctx != null)
                {
                    userIPAddress=ctx.Request.UserHostAddress;
                }
            }
            else if (request.Properties.ContainsKey(RemoteEndpointMessage))
            {
                dynamic remoteEndpoint = request.Properties[RemoteEndpointMessage];
                if (remoteEndpoint != null)
                {
                    userIPAddress=remoteEndpoint.Address;
                }
            }
        }

        public string getUserName()
        {
            return userName;
        }
    } 

    #region Logs Class
    public static class Logs
    {
        public static void Init()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static ILog log()
        {
            string stackTrace = Environment.StackTrace;
            int count = 0;
            int previousIndex = 0;
            int i = stackTrace.IndexOf("\r\n");
            // The method calling is 4th method on the stack. 
            // Each method is on it's won line separated from other using /r/n characters
            while (i>=0 && count<3)
            {
                previousIndex = i;
                count++;
                i = stackTrace.IndexOf("\r\n",i+2);
            }
            string str = stackTrace.Substring(previousIndex, i - previousIndex);
            str = str.Substring(str.IndexOf(" at ") + 4, str.IndexOf(" in ") - str.IndexOf(" at ") - 4);
            return LogManager.GetLogger(str);
        }

        public static string FormatException(Exception e)
        {
            return "\r\n\r\n" + e.ToString() + "\r\n";
        }
    }
    #endregion

    #region Conversions Class
    public static class Conversions
    {
        public static string FilterNumericOnly(string val)
        {
            string outstr = "";
            if (val == null)
                return null;
            foreach(var p in val)
            {
                if ((p>='0' && p<='9') || p=='.')
                    outstr += p;
            }
            return outstr;
        }

        public static Decimal? ToDecimalorNull(IList<string> lst)
        {
            return ToDecimalorNull(GetSingleValue(lst));
        }

        public static Decimal? ToDecimalorNull(string val)
        {
            if (val == null)
                return null;
            val = FilterNumericOnly(val);
            if (!string.IsNullOrWhiteSpace(val))
                return Convert.ToDecimal(val);
            return null;
        }

        public static int? ToIntorNull(IList<string> lst)
        {
            return ToIntorNull(GetSingleValue(lst));
        }

        public static int? ToIntorNull(string val)
        {
            if (val == null)
                return null;
            val = FilterNumericOnly(val);
            if (!string.IsNullOrWhiteSpace(val))
                return Convert.ToInt32(val);
            return null;
        }

        public static string GetSingleValue(IList<string> lst)
        {
            if (lst == null)
                return null;

            foreach (var p in lst)
            {
                if (!string.IsNullOrWhiteSpace(p))
                    return p.Trim();
            }
            return null;    
        }

        public static string Concat(IList<string> lst)
        {

            if (lst == null)
                return null;

            string outstr = "";

            foreach (var p in lst)
            {
                if (!string.IsNullOrWhiteSpace(p))
                {   if (outstr == "")
                        outstr = p.Trim();
                    else
                        outstr = "~~~" + p.Trim();
                }
            }
            if (outstr == "")
               return null;
            return outstr;
        }

        public static string[] ParseCSV(string instr)
        {
            if (string.IsNullOrEmpty(instr))
                return null;

            string[] strList;
            strList = instr.Split(',');
            for (int i=0; i<strList.Length; i++)
            {
                string cleanStr = FilterNumericOnly(strList[i]);
                if (string.IsNullOrEmpty(cleanStr))
                    strList[i] = null;
                else
                    strList[i] = cleanStr;
            }
            return strList;
        }
    }
    #endregion
}