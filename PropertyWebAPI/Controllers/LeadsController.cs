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
        [Route("api/leads")]
        [ResponseType(typeof(List<DAL.LeadSummaryData>))]
        [HttpPost]
        public IHttpActionResult GetLeads([FromBody] DAL.Filters filterdata)
        {
            if (!BAL.Lead.IsValidFilter(filterdata))
                return BadRequest("At least one filter is required");

            try
            {
                var leadList = DAL.Lead.GetPropertyLeads(filterdata).ToList();
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
        public IHttpActionResult SaveScenario([FromBody] DAL.Scenario scenarioObj)
        {
            if (scenarioObj == null || scenarioObj.filterdata==null)
                return BadRequest("No data found or data request is malformed");

            if (!BAL.Lead.IsValidFilter(scenarioObj.filterdata))
                return BadRequest("At least one filter is required");

            if (scenarioObj.username == null || scenarioObj.scenarioname == null || scenarioObj.description == null)
                return BadRequest("userName or scenarioName or description cannot be null");

            try
            {
                return Ok(DAL.Lead.SaveScenario(scenarioObj));
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered while saving scenario Leads{0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
       
    }
}
