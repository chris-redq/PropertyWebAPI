namespace PropertyWebAPI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using GPADB;
    using System.Web.Http.Description;
    using AutoMapper;
    using Newtonsoft.Json;

    public class Scenario
    {
        /// <summary>Name of the user saving the scenario</summary>
        public string username { get; set; }
        /// <summary>Name of the scenario</summary>
        public string scenarioname { get; set; }
        /// <summary>Description of the scenario</summary>
        public string description { get; set; }
        /// <summary>Filters for the Lead List</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Filters filterdata;
    }

     public class Filters
    {
        /// <summary>Optional parameter. Multiple values can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string zipcodes { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent along with wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string neighborhoods { get; set; }
        /// <summary>Optional parameter. Valid Values are Y, N or blank. If you send any other value the filter is ignored.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string isvacant { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent. Valid values are A, B, C and D. No wildcards</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string leadgrades { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent along with wildcards. For example: A*,B1</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string buildingclasscodes { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string counties { get; set; }
        /// <summary>Optional parameter. Valid Values are Y, N or blank. If you send any other value the filter is ignored</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ismailingaddressactive { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent. Valid values are MORTGAGE FORECLOSURES, TAX LIENS, TAX LIEN LPS and ALL. 
        /// By default if multiple values are provided the filter applies the OR operator between them. Add an additional value OPERATORAND if
        /// the operator of choice is AND instead of OR. Eg: 1) MORTGAGE FORECLOSURES,TAX LIENS 2) ALL,OPERATORAND</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string lientypes { get; set; }
        /// <summary>Optional parameter. Minimum one value, max two values can be sent. No wildcards</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ltv { get; set; }
        /// <summary>Optional parameter. Minimum one value, max two values can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string equity { get; set; }
        /// <summary>Optional parameter. Minimum one value, max two values can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string violations { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string cities { get; set; }
        /// <summary>Optional parameter. Multiple values (state abbreviations) can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string states { get; set; }
        /// <summary>Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string hasFannie { get; set; }
        /// <summary>Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string hasFreddie { get; set; }
        /// <summary>Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string unbuilt { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent along with wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string servicer { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent along. Valid values are Y, N, NULL and NOT NULL.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string landmark { get; set; }
        public string hasFHA { get; set; }
    }

    /// <summary>  
    /// This controller handles all api requests associated with property leads
    /// </summary>  
    [Authorize]
    public class LeadsController : ApiController
    {

       

        /// <summary>Use this api to get a lead details</summary>  
        /// <param name="propertybbl">Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number</param>  
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
        /// <param name="hasfannie">Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</param>
        /// <param name="hasfreddie">Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</param>
        /// <param name="unbuilt">Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</param>
        /// <param name="servicer">Optional parameter. Multiple values can be sent along with wildcards.</param>
        /// <param name="landmark">Optional parameter. Multiple values can be sent along. Valid values are Y, N, NULL and NOT NULL.</param>
        /// <param name="hasFHA">Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</param>
        /// <returns>Returns a list of leads (properties)</returns>
        [Route("api/leads/")]
        [ResponseType(typeof(List<DAL.LeadSummaryData>))]
        public IHttpActionResult GetLeads(string zipcodes = null, string neighborhoods = null, string isvacant = null, string leadgrades = null,
                                          string buildingclasscodes = null, string counties = null, string ismailingaddressactive = null,
                                          string lientypes = null, string ltv = null, string equity = null, string violations = null,
                                          string cities = null, string states = null, string hasfannie = null, string hasfreddie = null, 
                                          string unbuilt = null, string servicer=null, string landmark = null, string hasFHA = null)
        {
            if (!BAL.Lead.IsValidFilter(zipcodes, neighborhoods, isvacant, leadgrades, buildingclasscodes, counties, ismailingaddressactive, 
                                       lientypes, ltv, equity, violations, cities, states, hasfannie, hasfreddie, unbuilt, servicer, landmark, hasFHA))
                return BadRequest("At least one filter is required");

            try
            {
                var leadList = DAL.Lead.GetPropertyLeads(zipcodes, neighborhoods, isvacant, leadgrades, buildingclasscodes, counties, ismailingaddressactive,
                                                         lientypes, ltv, equity, violations, cities, states, hasfannie, hasfreddie, unbuilt, servicer, landmark,
                                                         hasFHA).ToList();
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

        /// <summary>  
        /// Use this api to get a list of all scenarios saved for a user
        /// </summary>  
        /// <param name="username">User name associated with scenarios</param>
        /// <returns>Returns a list of filtered leads (properties)</returns>
        [Route("api/leads/{username}/scenarios")]
        [ResponseType(typeof(List<DAL.ScenarioListData>))]
        public IHttpActionResult GetScenarios(string username)
        {
            try
            {
                var scenarioList = DAL.Lead.GetScenarios(username).ToList();
                if (scenarioList == null || scenarioList.Count == 0)
                    return NotFound();
                return Ok(scenarioList);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered while retrieving scenarios{0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>  
        /// Use this api to get a list of leads satisfying the filters provided. At least one filter must exist. All filters support
        /// multiple values separated by commas. For string values wildcards are provided. Wildcard is denoted by '*' 
        /// </summary>  
        /// <param name="filterdata">Filters for the Lead List</param>
        /// <returns>Returns a list of filtered leads (properties)</returns>
        [Route("api/leads/filtered")]
        [ResponseType(typeof(List<DAL.LeadSummaryData>))]
        [HttpPost]
        public IHttpActionResult GetFilteredLeads([FromBody] Filters filterdata)
        {
            if (!BAL.Lead.IsValidFilter(filterdata.zipcodes, filterdata.neighborhoods, filterdata.isvacant, filterdata.leadgrades, filterdata.buildingclasscodes,
                                        filterdata.counties, filterdata.ismailingaddressactive, filterdata.lientypes, filterdata.ltv, filterdata.equity, filterdata.violations,
                                        filterdata.cities, filterdata.states, filterdata.hasFannie, filterdata.hasFreddie, filterdata.unbuilt, filterdata.servicer, 
                                        filterdata.landmark, filterdata.hasFHA))
                return BadRequest("At least one filter is required");

            try
            {
                var leadList = DAL.Lead.GetPropertyLeads(filterdata.zipcodes, filterdata.neighborhoods, filterdata.isvacant, filterdata.leadgrades, filterdata.buildingclasscodes,
                                                         filterdata.counties, filterdata.ismailingaddressactive, filterdata.lientypes, filterdata.ltv, filterdata.equity, filterdata.violations,
                                                         filterdata.cities, filterdata.states, filterdata.hasFannie, filterdata.hasFreddie, filterdata.unbuilt, filterdata.servicer, 
                                                         filterdata.landmark, filterdata.hasFHA).ToList();
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

        /// <summary>  
        /// Use this api to save a scenario. At least one filter must exist. All filters support
        /// multiple values separated by commas. For string values wildcards are provided. Wildcard is denoted by '*'.
        /// For more details refer to the documentation for each filter.
        /// </summary>  
        /// <param name="scenarioObj"></param>
        [Route("api/leads/scenarios")]
        [ResponseType(typeof(long))]
        [HttpPost]
        public IHttpActionResult SaveScenario([FromBody] Scenario scenarioObj)
        {
            if (scenarioObj == null || scenarioObj.filterdata==null)
                return BadRequest("No data found or data request is malformed");

            if (!BAL.Lead.IsValidFilter(scenarioObj.filterdata.zipcodes, scenarioObj.filterdata.neighborhoods, scenarioObj.filterdata.isvacant, scenarioObj.filterdata.leadgrades, scenarioObj.filterdata.buildingclasscodes, 
                                        scenarioObj.filterdata.counties, scenarioObj.filterdata.ismailingaddressactive, scenarioObj.filterdata.lientypes, scenarioObj.filterdata.ltv, scenarioObj.filterdata.equity, 
                                        scenarioObj.filterdata.violations, scenarioObj.filterdata.cities, scenarioObj.filterdata.states, scenarioObj.filterdata.hasFannie, scenarioObj.filterdata.hasFreddie, scenarioObj.filterdata.unbuilt, 
                                        scenarioObj.filterdata.servicer, scenarioObj.filterdata.landmark, scenarioObj.filterdata.hasFHA))
                return BadRequest("At least one filter is required");

            if (scenarioObj.username == null || scenarioObj.scenarioname == null || scenarioObj.description == null)
                return BadRequest("userName or scenarioName or description cannot be null");

            try
            {
                
                return Ok(DAL.Lead.SaveScenario(scenarioObj.username, scenarioObj.scenarioname, scenarioObj.description, scenarioObj.filterdata.zipcodes, scenarioObj.filterdata.neighborhoods,
                                                scenarioObj.filterdata.isvacant, scenarioObj.filterdata.leadgrades, scenarioObj.filterdata.buildingclasscodes, scenarioObj.filterdata.counties, scenarioObj.filterdata.ismailingaddressactive,
                                                scenarioObj.filterdata.lientypes, scenarioObj.filterdata.ltv, scenarioObj.filterdata.equity, scenarioObj.filterdata.violations, scenarioObj.filterdata.cities, scenarioObj.filterdata.states,
                                                scenarioObj.filterdata.hasFannie, scenarioObj.filterdata.hasFreddie, scenarioObj.filterdata.unbuilt, scenarioObj.filterdata.servicer, scenarioObj.filterdata.landmark,
                                                scenarioObj.filterdata.hasFHA));
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered while saving scenario Leads{0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
       
    }
}
