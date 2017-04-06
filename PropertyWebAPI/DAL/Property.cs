//-----------------------------------------------------------------------
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------
namespace PropertyWebAPI.DAL
{
    using System.Collections.Generic;
    using System.Linq;
    using GPADB;
    using NYCDOB;
    using AutoMapper;

    public class PropertySummaryData : vwGeneralPropertyInformation
    {
        //Blank classes to mask entity framework details
    }

    public class Property
    {
        public static List<CertificateOfOccupancy> GetCertificateOfOccupany(string propertybbl)
        {
            using (NYCDOBEntities dobE = new NYCDOBEntities())
            {
                return dobE.CertificateOfOccupancies.Where(x => x.BBL == propertybbl).ToList();
            }
        }

        public static PropertySummaryData GetProperty(string propertybbl)
        {
            using (GPADBEntities1 gpaE = new GPADBEntities1())
            {
                return Mapper.Map<PropertySummaryData>(gpaE.vwGeneralPropertyInformations.Where(x => x.BBLE == propertybbl).FirstOrDefault());
            }
        }
    }
}