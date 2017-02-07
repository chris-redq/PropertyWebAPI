//-----------------------------------------------------------------------
// <copyright file="Lead.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI.DAL
{
    using System.Collections.Generic;
    using System.Linq;
    using GPADB;
    using AutoMapper;


    public class LeadSummaryData : vwGeneralLeadInfomation
    {
        //Blank classes to mask entity framework details
    }

    public class Lead
    {
        public static List<LeadSummaryData> GetPropertyLeads(string zipcodes, string neighborhoods, string isvacant, string leadgrades, string buildingclasscodes, string counties, 
                                                             string ismailingaddressactive, string lientypes, string ltv, string equity, string violations, string cities,
                                                             string states,  string isfannie, string isfreddie, string unbuilt, string servicer, string landmark)
        {
            using (GPADBEntities1 gpaE = new GPADBEntities1())
            {
                return Mapper.Map<List<vwGeneralLeadInfomation>, List<LeadSummaryData>>(gpaE.GetLeads(zipcodes, buildingclasscodes, counties, isvacant, ismailingaddressactive, violations,
                                                                                                     cities, neighborhoods, states, lientypes, leadgrades, ltv, equity, isfannie,
                                                                                                     isfreddie, unbuilt, servicer, landmark).ToList());
            }
        }

        public static LeadSummaryData GetPropertyLead(string propertybbl)
        {
            using (GPADBEntities1 gpaE = new GPADBEntities1())
            {
                return Mapper.Map<LeadSummaryData>(gpaE.vwGeneralLeadInfomations.Where(x => x.BBLE == propertybbl).FirstOrDefault());
            }
        }
    }
}