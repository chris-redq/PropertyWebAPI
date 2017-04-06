//-----------------------------------------------------------------------
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------
namespace PropertyWebAPI.BAL
{
    using System.Collections.Generic;
    using GPADB;
    using AutoMapper;

    public class PropertyDetailData : vwGeneralPropertyInformation
    {
        public List<BAL.DeedParty> owners;
    }

    public class Property
    {
        public static PropertyDetailData GetProperty(string propertyBBL)
        {
            var propertyDetailData = Mapper.Map<PropertyDetailData>(DAL.Property.GetProperty(propertyBBL));
            if (propertyDetailData != null)
            {
                var deedDetailsObj = BAL.ACRIS.GetLatestDeedDetails(propertyBBL);
                if (deedDetailsObj != null && deedDetailsObj.owners != null)
                    propertyDetailData.owners = deedDetailsObj.owners;
            }
            return propertyDetailData;
        }
    }
}