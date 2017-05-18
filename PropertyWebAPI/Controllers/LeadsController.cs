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

        /// <summary>  
        /// Use this api to get a list of leads satisfying the filters provided. At least one filter must exist. All filters support
        /// multiple values separated by commas. For string values wildcards are provided. Wildcard is denoted by '*' 
        /// </summary>  
        /// <param name="query">Filters for the Lead List</param>
        /// <returns>Returns a list of filtered leads (properties)</returns>
        [Route("api/leads")]
        [ResponseType(typeof(List<DAL.LeadSummaryData>))]
        [HttpPost]
        public IHttpActionResult GetLeads([FromBody] DAL.Query query)
        {
            if (query == null || !BAL.Lead.IsValidFilter(query.queryFilters))
                return BadRequest("At least one filter is required");

            try
            {
                var leadList = DAL.Lead.GetPropertyLeads(query.queryFilters, query.userName).ToList();
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
        /// <returns>A list of scenarios names and relevant data</returns>
        [Route("api/leads/{username}/scenarios")]
        [ResponseType(typeof(List<DAL.ScenarioListData>))]
        public IHttpActionResult GetScenarios(string username)
        {
            var scenarioList = DAL.Lead.GetScenarios(username).ToList();
            if (scenarioList == null || scenarioList.Count == 0)
                return NotFound();
            return Ok(scenarioList);
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

            return Ok(DAL.Lead.SaveScenario(scenarioObj));
        }

        /// <summary>  
        /// Use this api to save a new list and select list of properties. 
        /// </summary>  
        /// <param name="listObj"></param>
        [Route("api/leads/lists")]
        [ResponseType(typeof(long))]
        [HttpPost]
        public IHttpActionResult SaveList([FromBody] DAL.List listObj)
        {
            if (listObj == null)
                return BadRequest("No data found or data request is malformed");

            if (listObj.username == null || listObj.listName == null)
                return BadRequest("userName or listName cannot be null");

            return Ok(DAL.Lead.SaveList(listObj));
        }

        /// <summary>  
        /// Use this api to update an existing list's attributes and/or select list of properties.
        /// </summary>  
        /// <param name="listid"></param>
        /// <param name="listObj">All attributes are optional, but at least one attribute must exist to make the call meaningful</param>
        [Route("api/leads/lists/{listid}")]
        [ResponseType(typeof(bool))]
        [HttpPut]
        public IHttpActionResult UpdateList(long listid, [FromBody] DAL.List listObj)
        {
            if (listObj == null)
                return BadRequest("No data found or data request is malformed");

            if (!DAL.Lead.SaveList(listid, listObj))
                return NotFound();
            return Ok(true);
        }

        /// <summary>  
        /// Use this api to save additional properties to an existing list. 
        /// </summary>  
        /// <param name="listid"></param>
        /// <param name="bbllist">List of properties to be added to the list</param>
        [Route("api/leads/lists/{listid}/properties")]
        [ResponseType(typeof(bool))]
        [HttpPost]
        public IHttpActionResult AddPropertyToList(long listid, [FromBody]List<string> bbllist)
        {
            if (bbllist == null || bbllist.Count <= 0)
                return BadRequest("No Properties found or data request is malformed");
            if (!DAL.Lead.AddPropertiesToList(listid, bbllist))
                return NotFound();
            return Ok(true);
        }

        /// <summary>  
        /// Use this api to remove a set of properties from an existing list. 
        /// </summary>  
        /// <param name="listid"></param>
        /// <param name="bbllist">List of properties to be removed from the list</param>
        [Route("api/leads/lists/{listid}/properties")]
        [ResponseType(typeof(bool))]
        [HttpDelete]
        [DefaultException]
        public IHttpActionResult RemovePropertyFromList(long listid, [FromBody]List<string> bbllist)
        {
            if (bbllist == null || bbllist.Count <= 0)
                return BadRequest("No Properties found or data request is malformed");

            if (!DAL.Lead.DeletePropertiesFromList(listid, bbllist))
                return NotFound();
            return Ok(true);
        }

        /// <summary>  
        /// Use this api to get a list of all user lists saved
        /// </summary>  
        /// <param name="username">User name associated with lists</param>
        /// <returns>A list of list names and relevant data</returns>
        [Route("api/leads/{username}/lists")]
        [ResponseType(typeof(List<DAL.SavedListData>))]
        public IHttpActionResult GetLists(string username)
        {
            var userLists = DAL.Lead.GetLists(username).ToList();
            if (userLists == null || userLists.Count == 0)
                return NotFound();
            return Ok(userLists);
        }

        /// <summary>  
        /// Use this api to add new leads. 
        /// </summary>  
        /// <param name="leadlist">List of properties and their lead types to be added to the GPA system</param>
        /// <param name="externalReferenceId">External Reference Id used by the calling systems to track callbacks</param>
        [Route("api/leads/bulkload/{externalReferenceId}")]
        [ResponseType(typeof(long))]
        [HttpPost]
        public IHttpActionResult AddLeads([FromBody] List<DAL.LeadInput> leadlist, string externalReferenceId=null)
        {
            if ((leadlist == null) || (leadlist.Count==0))
                return BadRequest("No data found or data request is malformed");

            Common.Context appContext = new Common.Context(RequestContext, Request);
            if (string.IsNullOrEmpty(externalReferenceId))
                externalReferenceId = "EREF-" + DateTime.UtcNow.ToString() + " UTC";

            DAL.Lead.AddLeads(leadlist, externalReferenceId);
 
            List<string> bblList = new List<string>();
            foreach (var lead in leadlist)
                bblList.Add(lead.propertybbl);
            BAL.WebData.BulkRetrieve(appContext, bblList, 0, externalReferenceId);

            return Ok(true);
        }
    }
}
