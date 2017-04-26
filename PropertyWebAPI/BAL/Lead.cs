//-----------------------------------------------------------------------
// <copyright file="Lead.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI.BAL
{
    using System.Collections.Generic;
    using GPADB;
    using AutoMapper;

    public class LeadDetailData : vwGeneralLeadInformation
    {

        public List<DAL.DeedParty> owners;
    }

    public class Lead
    {
        public static LeadDetailData GetPropertyLead(string propertyBBL)
        {
            var leadDetailData = Mapper.Map<LeadDetailData>(DAL.Lead.GetPropertyLead(propertyBBL));
            if (leadDetailData != null)
            {
                var deedDetailsObj = BAL.ACRIS.GetLatestDeedDetails(propertyBBL);
                if (deedDetailsObj != null && deedDetailsObj.owners != null)
                    leadDetailData.owners = deedDetailsObj.owners;
            }
            return leadDetailData;
        }

        public static bool IsValidFilter(DAL.Filters filterdata)
        {
            return IsValidFilter(filterdata.zipcodes, filterdata.neighborhoods, filterdata.isvacant, filterdata.leadgrades, filterdata.buildingclasscodes,
                                 filterdata.counties, filterdata.ismailingaddressactive, filterdata.lientypes, filterdata.ltv, filterdata.equity, 
                                 filterdata.violations, filterdata.cities, filterdata.states, filterdata.hasFannie, filterdata.hasFreddie, filterdata.unbuilt, 
                                 filterdata.servicer, filterdata.landmark, filterdata.hasFHA, filterdata.satisfiedMortgages, filterdata.deedAge, 
                                 filterdata.taxLiensSoldYear, filterdata.taxLiensSoldTotal);
        }

        public static bool IsValidFilter(string zipcodes, string neighborhoods, string isvacant, string leadgrades, string buildingclasscodes, string counties,
                                         string ismailingaddressactive, string lientypes, string ltv, string equity, string violations, string cities, string states,
                                         string hasfannie, string hasfreddie, string unbuilt, string servicer, string landmark, string hasFHA, 
                                         string satisfiedMortgages, string deedAge, string taxlienssoldyear, string taxlienssoldtotal)
        {
            if (zipcodes == null && buildingclasscodes == null && counties == null &&
                isvacant == null && violations == null && ismailingaddressactive == null &&
                cities == null && neighborhoods == null && states == null && lientypes == null &&
                leadgrades == null && ltv == null && equity == null && hasfannie == null && hasfreddie == null &&
                unbuilt == null && servicer == null && landmark == null && hasFHA == null && satisfiedMortgages == null &&
                deedAge == null && taxlienssoldyear == null && taxlienssoldtotal == null)
                return false;
            return true;
        }
    }
}