namespace PropertyWebAPI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web.Http;
    using System.Web.Http.Description;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using GPADB;
    using AutoMapper;

    
    public class LeadSummaryData : vwGeneralLeadInfomation
    {
        //Blank classes to mask entity framework details
    }
    /// <summary>  
    /// This controller handles all api requests associated with property leads
    /// </summary>  
    [Authorize]
    public class LeadsController : ApiController
    {
        /// <summary>  
        /// Use this api to get a list of leads satisfying the criteria provided. At least one filter must exist. All filters support
        /// multiple values separated by commas. For string values wildcards are provided. Wildcards is denoted by '*' 
        /// </summary>  
        /// <param name="counties">Optional parameter. Multiple values can be sent. No wildcards.</param>
        /// <param name="buildingclasscodes">Optional parameter. Multiple values can be sent along with wildcards. For example: A*,B1</param>
        /// <param name="ismailingaddressactive">Optional parameter. Valid Values are Y, N or blank. If you send any other value the filter is ignored</param>
        /// <param name="isvacant">Optional parameter. Valid Values are Y, N or blank. If you send any other value the filter is ignored</param>
        /// <param name="zipcodes">Optional parameter. Multiple values can be sent. No wildcards.</param>
        /// <param name="violations">Optional parameter. Minimum one value, max two values can be sent. No wildcards.</param>
        /// <param name="lientypes">Optional parameter. Multiple values can be sent. Valid values are MORTGAGE FORECLOSURES, TAX LIENS and ALL. 
        /// By default if multiple values are provided the filter applies the OR operator between them. Add an additional value OPERATORAND if
        /// the operator of choice is AND instead of OR. Eg: 1) MORTGAGE FORECLOSURES,TAX LIENS 2) ALL,OPERATORAND</param>
        /// <param name="states">Optional parameter. Multiple values (state abbreviations) can be sent. No wildcards.</param>
        /// <param name="cities">Optional parameter. Multiple values can be sent. No wildcards.</param>
        /// <param name="leadgrades">Optional parameter. Multiple values can be sent. No wildcards</param>
        /// <param name="ltv">Optional parameter. Minimum one value, max two values can be sent. No wildcards</param>
        /// <param name="neighborhoods">Optional parameter. Multiple values can be sent along with wildcards.</param>
        /// <param name="equity">Optional parameter. Minimum one value, max two values can be sent. No wildcards.</param>
        /// <param name="isfannie">Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</param>
        /// <param name="isfreddie">Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</param>
        /// <param name="unbuilt">Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</param>
        /// <returns>Returns a list of leads (properties)</returns>
        [Route("api/leads/")]
        [ResponseType(typeof(List<LeadSummaryData>))]
        public IHttpActionResult Get(string zipcodes = null, string neighborhoods = null, string isvacant = null, string leadgrades = null,
                                     string buildingclasscodes = null, string counties = null, string ismailingaddressactive = null,
                                     string lientypes = null, string ltv = null, string equity = null, string violations = null,
                                     string cities = null, string states= null, string isfannie=null, string isfreddie=null, string unbuilt=null)
        {
            if (zipcodes==null && buildingclasscodes==null && counties==null && 
                isvacant==null && violations==null && ismailingaddressactive == null &&
                cities==null && neighborhoods==null && states==null && lientypes==null &&
                leadgrades==null && ltv==null && equity==null && isfannie==null && isfreddie==null &&
                unbuilt==null)
                return this.BadRequest("At least one filter is required");

            try
            {
                using (GPADBEntities gpaE = new GPADBEntities())
                {
                    List<vwGeneralLeadInfomation> leadList = gpaE.GetLeads(zipcodes, buildingclasscodes, counties, isvacant, ismailingaddressactive, violations,
                                                                           cities, neighborhoods, states, lientypes,leadgrades,ltv,equity,isfannie,isfreddie,unbuilt).ToList();
                    if (leadList == null || leadList.Count == 0)
                        return NotFound();
                    return Ok(Mapper.Map<List<vwGeneralLeadInfomation>, List<LeadSummaryData>>(leadList));
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered while retrieving Leads{0}",Common.Utilities.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>  
        /// Use this api to get a lead details
        /// </summary>  
        /// <param name="propertybbl">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>  
        /// <returns>Returns lead details</returns>
        [Route("api/leads/{propertybbl}")]
        [ResponseType(typeof(LeadSummaryData))]
        public IHttpActionResult GetPropertyLead(string propertybbl)
        {
            if (!BAL.BBL.IsValidFormat(propertybbl))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            try
            {
                using (GPADBEntities gpaE = new GPADBEntities())
                {
                    var lead = Mapper.Map<LeadSummaryData>(gpaE.vwGeneralLeadInfomations.Where(x=> x.BBLE==propertybbl).FirstOrDefault());
                    
                    if (lead == null)
                        return NotFound();
                    return Ok(lead);
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered while retrieving Lead for {0}{1}", propertybbl, Common.Utilities.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
    }
}
