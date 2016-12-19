using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http.Results;
using ACRISDB;
using NYCDOF;
using log4net;
using AutoMapper;
using System.Text.RegularExpressions;

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

    }

    public static class Utilities
    {
        public static string FormatException(Exception e)
        {
            return "\r\n\r\n" + e.ToString() + "\r\n";
        }
    }
}