//-----------------------------------------------------------------------
// <copyright file="WebApiConfig.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using log4net;
    using System.Net;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();
        
            // Configure APIKey Authentication Handler
            GlobalConfiguration.Configuration.MessageHandlers.Add(new APIKeySecurityHandler());

            Common.AppInitialization.Init();

        }
    }
}

/*
        config.Routes.MapHttpRoute(
           name: "CaseDetailsApi",
           routeTemplate: "api/cases/{countyId}/{caseIndexNumber}",
           defaults: new { controller = "Cases" });

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
          name: "CaseColumnValueChangesApi",
          routeTemplate: "api/cases/columnvaluechanges/{columnName}/{startDate}/{endDate}",
           defaults: new { controller = "Cases" });

        config.Routes.MapHttpRoute(
           name: "CasesByBBL",
           routeTemplate: "api/cases/{propertyBBL}",
           defaults: new { controller = "Cases" });

        config.Routes.MapHttpRoute(
           name: "DocumentsByBBLE",
           routeTemplate: "api/mortgagesdeeds/{propertyBBLE}/documents",
           defaults: new { controller = "MortgagesDeeds" });

        config.Routes.MapHttpRoute(
           name: "DeedsByBBLE",
           routeTemplate: "api/mortgagesdeeds/{propertyBBLE}/deeds",
           defaults: new { controller = "MortgagesDeeds" });

        config.Routes.MapHttpRoute(
          name: "ContractsOfSaleByBBLE",
          routeTemplate: "api/mortgagesdeeds/{propertyBBLE}/contractsofsale",
          defaults: new { controller = "MortgagesDeeds" });
        */

/*
config.Routes.MapHttpRoute(
    name: "DefaultApi",
    routeTemplate: "api/{controller}/{countyId}/{caseIndexNumber}",
    defaults: new { countyId = RouteParameter.Optional, caseIndexNumber = RouteParameter.Optional });
*/
