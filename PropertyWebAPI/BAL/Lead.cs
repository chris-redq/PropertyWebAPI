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

        public List<BAL.DeedParty> owners;
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

        public static bool IsValidFilter(string zipcodes, string neighborhoods, string isvacant, string leadgrades, string buildingclasscodes, string counties,
                                         string ismailingaddressactive, string lientypes, string ltv, string equity, string violations, string cities, string states,
                                         string hasfannie, string hasfreddie, string unbuilt, string servicer, string landmark, string hasFHA)
        {
            if (zipcodes == null && buildingclasscodes == null && counties == null &&
                isvacant == null && violations == null && ismailingaddressactive == null &&
                cities == null && neighborhoods == null && states == null && lientypes == null &&
                leadgrades == null && ltv == null && equity == null && hasfannie == null && hasfreddie == null &&
                unbuilt == null && servicer == null && landmark == null && hasFHA == null)
                return false;
            return true;
        }
    }
}