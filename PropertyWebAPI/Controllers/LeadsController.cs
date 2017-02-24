namespace PropertyWebAPI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using GPADB;
    using System.Web.Http.Description;
    using AutoMapper;



    /// <summary>  
    /// This controller handles all api requests associated with property leads
    /// </summary>  
    [Authorize]
    public class LeadsController : ApiController
    {

        /// <summary>  
        /// Use this api to get a lead details
        /// </summary>  
        /// <param name="propertybbl">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>  
        /// <returns>Returns lead details</returns>
        [Route("api/leads/{propertybbl}")]
        [ResponseType(typeof(BAL.LeadDetailData))]
        public IHttpActionResult GetPropertyLead(string propertybbl)
        {
            if (!BAL.BBL.IsValidFormat(propertybbl))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            try
            {
                var lead = BAL.Lead.GetPropertyLead(propertybbl);
                    
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

        /// <summary>  
        /// Use this api to get a list of leads satisfying the criteria provided. At least one filter must exist. All filters support
        /// multiple values separated by commas. For string values wildcards are provided. Wildcard is denoted by '*' 
        /// </summary>  
        /// <param name="counties">Optional parameter. Multiple values can be sent. No wildcards.</param>
        /// <param name="buildingclasscodes">Optional parameter. Multiple values can be sent along with wildcards. For example: A*,B1</param>
        /// <param name="ismailingaddressactive">Optional parameter. Valid Values are Y, N or blank. If you send any other value the filter is ignored</param>
        /// <param name="isvacant">Optional parameter. Valid Values are Y, N or blank. If you send any other value the filter is ignored</param>
        /// <param name="zipcodes">Optional parameter. Multiple values can be sent. No wildcards.</param>
        /// <param name="violations">Optional parameter. Minimum one value, max two values can be sent. No wildcards.</param>
        /// <param name="lientypes">Optional parameter. Multiple values can be sent. Valid values are MORTGAGE FORECLOSURES, TAX LIENS, TAX LIEN LPS and ALL. 
        /// By default if multiple values are provided the filter applies the OR operator between them. Add an additional value OPERATORAND if
        /// the operator of choice is AND instead of OR. Eg: 1) MORTGAGE FORECLOSURES,TAX LIENS 2) ALL,OPERATORAND</param>
        /// <param name="states">Optional parameter. Multiple values (state abbreviations) can be sent. No wildcards.</param>
        /// <param name="cities">Optional parameter. Multiple values can be sent. No wildcards.</param>
        /// <param name="leadgrades">Optional parameter. Multiple values can be sent. Valid values are A, B, C and D. No wildcards</param>
        /// <param name="ltv">Optional parameter. Minimum one value, max two values can be sent. No wildcards</param>
        /// <param name="neighborhoods">Optional parameter. Multiple values can be sent along with wildcards.</param>
        /// <param name="equity">Optional parameter. Minimum one value, max two values can be sent. No wildcards.</param>
        /// <param name="isfannie">Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</param>
        /// <param name="isfreddie">Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</param>
        /// <param name="unbuilt">Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</param>
        /// <param name="servicer">Optional parameter. Multiple values can be sent along with wildcards.</param>
        /// <param name="landmark">Optional parameter. Multiple values can be sent along. Valid values are Y, N, NULL and NOT NULL.</param>
        /// <returns>Returns a list of leads (properties)</returns>
        [Route("api/leads/")]
        [ResponseType(typeof(List<DAL.LeadSummaryData>))]
        public IHttpActionResult GetLeads(string zipcodes = null, string neighborhoods = null, string isvacant = null, string leadgrades = null,
                                          string buildingclasscodes = null, string counties = null, string ismailingaddressactive = null,
                                          string lientypes = null, string ltv = null, string equity = null, string violations = null,
                                          string cities = null, string states = null, string isfannie = null, string isfreddie = null, 
                                          string unbuilt = null, string servicer=null, string landmark = null)
        {
            if (zipcodes == null && buildingclasscodes == null && counties == null &&
                isvacant == null && violations == null && ismailingaddressactive == null &&
                cities == null && neighborhoods == null && states == null && lientypes == null &&
                leadgrades == null && ltv == null && equity == null && isfannie == null && isfreddie == null &&
                unbuilt == null && servicer==null && landmark==null)
                return this.BadRequest("At least one filter is required");

            try
            {
                var leadList = DAL.Lead.GetPropertyLeads(zipcodes, neighborhoods, isvacant, leadgrades, buildingclasscodes, counties, ismailingaddressactive,
                                                         lientypes, ltv, equity, violations, cities, states, isfannie, isfreddie, unbuilt, servicer, landmark).ToList();
                if (leadList == null || leadList.Count == 0)
                        return NotFound();
                    return Ok(leadList);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered while retrieving Leads{0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }


        /*
        [Route("api/leads/Old")]
        [ResponseType(typeof(List<DAL.LeadSummaryData>))]
        public IHttpActionResult GetOld(string zipcodes = null, string neighborhoods = null, string isvacant = null, string leadgrades = null,
                                     string buildingclasscodes = null, string counties = null, string ismailingaddressactive = null,
                                     string lientypes = null, string ltv = null, string equity = null, string violations = null,
                                     string cities = null, string states= null, string isfannie=null, string isfreddie=null, string unbuilt=null, string servicer=null)
        {
            if (zipcodes==null && buildingclasscodes==null && counties==null && 
                isvacant==null && violations==null && ismailingaddressactive == null &&
                cities==null && neighborhoods==null && states==null && lientypes==null &&
                leadgrades==null && ltv==null && equity==null && isfannie==null && isfreddie==null &&
                unbuilt==null && servicer==null)
                return this.BadRequest("At least one filter is required");

            try
            {
                using (GPADBEntities1 gpaE = new GPADBEntities1())
                {
                    var leadList = Mapper.Map<List<vwGeneralLeadInfomation>, List<DAL.LeadSummaryData>>(gpaE.GetLeads(zipcodes, buildingclasscodes, counties, isvacant, ismailingaddressactive, violations,
                                                                                                         cities, neighborhoods, states, lientypes, leadgrades, ltv, equity, isfannie,
                                                                                                         isfreddie, unbuilt, servicer).ToList());
                    if (leadList == null || leadList.Count == 0)
                        return NotFound();
                    return Ok(leadList);
                }
                
               // var leadList = DAL.Lead.GetPropertyLeads(zipcodes, buildingclasscodes, counties, isvacant, ismailingaddressactive, violations, cities, neighborhoods, states, 
               //                                          lientypes,leadgrades,ltv,equity,isfannie,isfreddie,unbuilt).ToList();
                
                
                
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered while retrieving Leads{0}",Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
*/
    }
}
