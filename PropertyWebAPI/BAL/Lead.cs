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

    public class LeadDetailData : vwGeneralLeadInfomation
    {

        public List<BAL.DeedParty> owners;
    }

    public class Lead
    {
        public static LeadDetailData GetPropertyLead(string propertyBBL)
        {
            var leadDetailData = Mapper.Map<LeadDetailData>(DAL.Lead.GetPropertyLead(propertyBBL));

            var deedDetailsObj = BAL.ACRIS.GetLatestDeedDetails(propertyBBL);
            if (deedDetailsObj != null)
                leadDetailData.owners = deedDetailsObj.owners;

            return leadDetailData;
        }
    }
}