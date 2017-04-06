namespace PropertyWebAPI.Controllers
{
    using System;
    using System.Web.Http;
    using System.Web.Http.Description;
    using NYCDOB;
    using System.Collections.Generic;

    public class PropertyInfoController : ApiController
    {
        /// <summary>Use this api to get a property details</summary>  
        /// <param name="propertybbl">Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number</param>  
        /// <returns>Returns property details</returns>
        [Route("api/propertyinfo/{propertybbl}")]
        [ResponseType(typeof(BAL.PropertyDetailData))]
        public IHttpActionResult GetProperty(string propertybbl)
        {
            if (!BAL.BBL.IsValidFormat(propertybbl))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            try
            {
                var lead = BAL.Property.GetProperty(propertybbl);

                if (lead == null)
                    return NotFound();

                return Ok(lead);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered while retrieving Lead for {0}{1}", propertybbl, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>Use this api to get a list of URLS for certificates of occupancy</summary>  
        /// <param name="propertybbl">Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number</param>  
        /// <returns>Returns a list of URLS for certificates of occupancy for a property</returns>
        [Route("api/propertyinfo/{propertybbl}/certificatesofoccupancy")]
        [ResponseType(typeof(List<CertificateOfOccupancy>))]
        public IHttpActionResult GetCertificatesOfOccupancy(string propertybbl)
        {
            if (!BAL.BBL.IsValidFormat(propertybbl))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            try
            {
                var coList = DAL.Property.GetCertificateOfOccupany(propertybbl);

                if (coList == null || coList.Count==0 )
                    return NotFound();

                return Ok(coList);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered while retrieving Lead for {0}{1}", propertybbl, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
    }
}
