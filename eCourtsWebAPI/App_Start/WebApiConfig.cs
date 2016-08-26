using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace eCourtsWebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "CaseMotionsApi",
                routeTemplate: "api/cases/{countyId}/{caseIndexNumber}/motions",
                defaults: new { controller = "Cases" });

            config.Routes.MapHttpRoute(
                name: "CaseAppearancessApi",
                routeTemplate: "api/cases/{countyId}/{caseIndexNumber}/appearances",
                defaults: new { controller = "Cases" });

            config.Routes.MapHttpRoute(
               name: "CaseAttorneysApi",
               routeTemplate: "api/cases/{countyId}/{caseIndexNumber}/attorneys",
               defaults: new { controller = "Cases" });

            config.Routes.MapHttpRoute(
               name: "CaseHistoryApi",
               routeTemplate: "api/cases/{countyId}/{caseIndexNumber}/history",
               defaults: new { controller = "Cases" });

            config.Routes.MapHttpRoute(
               name: "CasesByBBL",
               routeTemplate: "api/cases/{BBL}",
               defaults: new { controller = "Cases" });

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{countyId}/{caseIndexNumber}",
                defaults: new { countyId = RouteParameter.Optional, caseIndexNumber = RouteParameter.Optional });
           
        }
    }
}
