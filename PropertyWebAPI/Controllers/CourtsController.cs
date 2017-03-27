//-----------------------------------------------------------------------
// <copyright file="CasesController.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

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
    using eCourtsDB;
    using AutoMapper;

    /// <summary>  
    /// This controller handles all api requests associated with eCourts CCIS Case related data
    /// </summary>  
    [Authorize]
    public class CourtsController : ApiController
    {
        #region Firm Methods

        /// <summary>Use this api to get a list of all firms related to a given firm</summary>  
        /// <param name="countyId">CCIS County Id associated with the Law Firm.</param>  
        /// <param name="firmid">ID of the law firm (AttorneyOfRecord) associated with a case</param>  
        /// <param name="partyindicator">Valid Values are - Plaintiff and Defendant. Default is Plaintiff</param>
        /// <param name="withinsamecounty">Default value is false</param>
        /// <returns>Returns a list of all related firms and their case counts in descending order of count</returns>
        [Route("api/courts/lawfirms/{countyId}/{firmid}/relatedlawfirms")]
        [ResponseType(typeof(List<DAL.FirmDetail>))]
        public IHttpActionResult GetRelatedLawFirms(string countyId, string firmid, string partyindicator = "Plaintiff", bool withinsamecounty = false)
        {
            try
            {
                var result = DAL.eCourts.GetRelatedLawFirms(countyId, firmid, partyindicator, withinsamecounty);
                if (result == null || result.Count == 0)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered {0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }


        /// <summary>Use this api to get a list of all Judges that presided over motion decisions associated with cases related to a given firm</summary>  
        /// <param name="countyId">CCIS County Id associated with the Law Firm.</param>  
        /// <param name="firmid">ID of the law firm (AttorneyOfRecord) associated with a case</param>  
        /// <param name="partyindicator">Valid Values are - Plaintiff and Defendant. Default is Plaintiff</param>
        /// <param name="withinsamecounty">Default value is false</param>
        /// <returns>Returns a list of all Judges that presided over motion decisions associated with cases related to a given firm</returns>
        [Route("api/courts/lawfirms/{countyId}/{firmid}/judges")]
        [ResponseType(typeof(List<DAL.JudgeNameCount>))]
        public IHttpActionResult GetJudgesPresidedOverRelatedLawFirmsCases(string countyId, string firmid, string partyindicator = "Plaintiff", bool withinsamecounty=false)
        {
            try
            {
                var result = DAL.eCourts.GetJudgesPresidedOverRelatedLawFirmsCases(countyId, firmid, partyindicator, withinsamecounty);
                if (result == null || result.Count == 0)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered {0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }


        /// <summary>Use this api to get a list of cases associated with a law firm and its related law firms</summary>  
        /// <param name="countyId">CCIS County Id associated with the Law Firm.</param>  
        /// <param name="firmid">ID of the law firm (AttorneyOfRecord) associated with a case</param>  
        /// <param name="partyindicator">Valid Values are - Plaintiff and Defendant. Default is Plaintiff</param>
        /// <param name="withinsamecounty">Default value is false</param>
        /// <param name="judgecountyid">Optional parameter. Set value if cases need filtering based on Attorney Judge Combination</param>
        /// <param name="judgeid">Optional parameter. Set value if cases need filtering based on Attorney Judge Combination</param>
        /// <returns>Returns a list of cases associated with a law firm and its related law firms in descending order of RJI filing date</returns>
        [Route("api/courts/lawfirms/{countyId}/{firmid}/cases")]
        [ResponseType(typeof(List<DAL.FirmCaseDetail>))]
        public IHttpActionResult GetLawFirmCases(string countyId, string firmid, string partyindicator = "Plaintiff", bool withinsamecounty = false, 
                                                 string judgecountyid=null, string judgeid=null)
        {
            if (judgecountyid == null && judgeid != null)
                return BadRequest("judgecountyId is also required when judgeid is present");
            if (judgecountyid != null && judgeid == null)
                return BadRequest("judgeId is also required when judgecountyid is present");

            if (judgecountyid != null)
                withinsamecounty = (countyId == judgecountyid);

            try
            {
                var result = DAL.eCourts.GetLawFirmCases(countyId, firmid, partyindicator, withinsamecounty, judgecountyid, judgeid);
                if (result == null || result.Count == 0)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered {0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>Use this api to get a list of completed cases, cases with a decision on Judgment of Foreclosure and Sale Motion, associated with a law firm and its related law firms</summary>  
        /// <param name="countyId">CCIS County Id associated with the Law Firm.</param>  
        /// <param name="firmid">ID of the law firm (AttorneyOfRecord) associated with a case</param>  
        /// <param name="decision">Valid Values are GRANTED, DENIED, WITHDRAWN and OTHER</param>
        /// <param name="partyindicator">Valid Values are - Plaintiff and Defendant. Default is Plaintiff</param>
        /// <param name="withinsamecounty">Default value is false</param>
        /// <param name="judgecountyid">Optional parameter. Set value if cases need filtering based on Attorney Judge Combination</param>
        /// <param name="judgeid">Optional parameter. Set value if cases need filtering based on Attorney Judge Combination</param>
        /// <returns>Returns a list of completed cases in descending order of RJI filing date</returns>
        [Route("api/courts/lawfirms/{countyId}/{firmid}/{decision}/completedcases")]
        [ResponseType(typeof(List<DAL.FirmCompletedCaseDetail>))]
        public IHttpActionResult GetLawFirmCompletedCases(string countyId, string firmid, string decision, string partyindicator = "Plaintiff", bool withinsamecounty = false,
                                                          string judgecountyid = null, string judgeid = null)
        {
            if (judgecountyid == null && judgeid != null)
                return BadRequest("judgecountyId is also required when judgeid is present");
            if (judgecountyid != null && judgeid == null)
                return BadRequest("judgeId is also required when judgecountyid is present");

            if (judgecountyid != null)
                withinsamecounty = (countyId == judgecountyid);

            try
            {
                var result = DAL.eCourts.GetLawFirmCompletedCases(countyId, firmid, partyindicator, withinsamecounty, judgecountyid, judgeid, decision);
                if (result == null || result.Count == 0)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered {0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        #endregion

        #region Statistics

        #region Firm Statistics
        /// <summary>Use this api to get 5 number summary, instance count and average time taken to a decision, for various ReliefSought 
        /// and decision combinations, for a law firm and its related law firms</summary>  
        /// <param name="countyId">CCIS County Id associated with the Law Firm.</param>  
        /// <param name="firmid">ID of the law firm (AttorneyOfRecord) associated with a case</param>  
        /// <param name="partyindicator">Valid Values are - Plaintiff and Defendant. Default is Plaintiff</param>
        /// <param name="withinsamecounty">Default value is false</param>
        /// <param name="judgecountyid">Optional parameter. Set value if cases need filtering based on Attorney Judge Combination</param>
        /// <param name="judgeid">Optional parameter. Set value if cases need filtering based on Attorney Judge Combination</param>
        /// <returns>Returns 5 number summary plus instance count and Average time taken to decision for various ReliefSought 
        /// and decision combinations for a law firm and its related law firms</returns>
        [Route("api/courts/statistics/lawfirms/{countyId}/{firmid}/DecisionTimeByReliefSoughtAndDecision")]
        [ResponseType(typeof(List<DAL.ReliefSoughtDecision5NumberSummaryPlus>))]
        public IHttpActionResult GetReliefSoughtDecision5NumberSummary(string countyId, string firmid, string partyindicator = "Plaintiff", bool withinsamecounty = false,
                                                                       string judgecountyid = null, string judgeid = null)
        {
            if (judgecountyid == null && judgeid != null)
                return BadRequest("judgecountyId is also required when judgeid is present");
            if (judgecountyid != null && judgeid == null)
                return BadRequest("judgeId is also required when judgecountyid is present");

            if (judgecountyid != null)
                withinsamecounty = (countyId == judgecountyid);

            try
            {
                var result = DAL.eCourts.GetReliefSoughtDecision5NumberSummary(countyId, firmid, partyindicator, withinsamecounty, judgecountyid, judgeid);
                if (result == null || result.Count == 0)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered {0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>Use this api to get 5 number summary, instance count and average time taken to a decision, for Judgment of Foreclosure and Sale
        /// and decision type combinations, for a law firm and its related law firms</summary>  
        /// <param name="countyId">CCIS County Id associated with the Law Firm.</param>  
        /// <param name="firmid">ID of the law firm (AttorneyOfRecord) associated with a case</param>  
        /// <param name="withinsamecounty">Default value is false</param>
        /// <param name="judgecountyid">Optional parameter. Set value if cases need filtering based on Attorney Judge Combination</param>
        /// <param name="judgeid">Optional parameter. Set value if cases need filtering based on Attorney Judge Combination</param>
        /// <returns>Returns 5 number summary plus instance count and Average time taken to decision for various ReliefSought 
        /// and decision combinations for a law firm and its related law firms</returns>
        [Route("api/courts/statistics/lawfirms/{countyId}/{firmid}/CaseCompletionTimeAndDecision")]
        [ResponseType(typeof(List<DAL.CaseCompletionDecision5NumberSummaryPlus>))]
        public IHttpActionResult GetCaseCompletionDecision5NumberSummary(string countyId, string firmid, bool withinsamecounty = false,
                                                                         string judgecountyid = null, string judgeid = null)
        {
            if (judgecountyid == null && judgeid != null)
                return BadRequest("judgecountyId is also required when judgeid is present");
            if (judgecountyid != null && judgeid == null)
                return BadRequest("judgeId is also required when judgecountyid is present");

            if (judgecountyid != null)
                withinsamecounty = (countyId == judgecountyid);

            try
            {
                var result = DAL.eCourts.GetCaseCompletionDecision5NumberSummary(countyId, firmid, withinsamecounty, judgecountyid, judgeid);
                if (result == null || result.Count == 0)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered {0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
        #endregion

        #region Judge Statistics
        /// <summary>Use this api to get a list of cases associated with a Relief Sought for a given judge in a county</summary>  
        /// <param name="countyId">CCIS County Id associated with the Judge.</param>  
        /// <param name="judgeId">ID of the Judge as it appears in the data</param>  
        /// <param name="reliefSought">Relief Sought in a Motion. Use the name as it appears in the data do not modify it</param>
        /// <returns>Returns a list of cases associated with a Relief Sought for a given judge in a county</returns>
        [Route("api/courts/statistics/judges/{countyId}/{judgeId}/{reliefSought}/cases")]
        [ResponseType(typeof(List<vwCaseByJudgeReliefSought>))]
        public IHttpActionResult GetCasesByJudgeReliefSought(string countyId, string judgeId, string reliefSought)
        {
            try
            {
                var result = DAL.eCourts.GetCasesByJudgeReliefSought(countyId, judgeId, reliefSought);
                if (result == null || result.Count == 0)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for CountyId: {0} JudgeId: {1} ReliefSought: {2}{3}", countyId, judgeId, reliefSought, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>Use this api to get a list of decision statistics for various Relief Sought for a given judge.</summary>  
        /// <param name="countyId">CCIS County Id associated with the Judge.</param>    
        /// <param name="judgeId">Id of the Judge as it appears in the data do not modify it</param>  
        /// <returns>Returns a list of decision statistics for various Relief Sought for a given judge.</returns>
        [Route("api/courts/statistics/judges/{countyId}/{judgeId}/DecisionSummaryByReliefSought")]
        [ResponseType(typeof(List<vwMotionSummaryByJudgeReliefSought>))]
        public IHttpActionResult GetMotionSummaryStatisticsByJudgeReliefSought(string countyId, string judgeId)
        {
            try
            {
                var result = DAL.eCourts.GetMotionSummaryStatisticsByJudgeReliefSought(countyId, judgeId);
                if (result == null || result.Count == 0)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for CountyId: {0} JudgeId: {1}{2}", countyId, judgeId, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }

        }

        /// <summary>Use this api to get a 5 Number Summary and Avg on days to decision for a Relief Sought for a Judge in a county.</summary>  
        /// <param name="countyId">CCIS County Id associated with the Judge.</param> 
        /// <param name="judgeId">Id of the Judge as it appears in the data do not modify it</param>  
        /// <param name="reliefSought">Relief Sought in a Motion. Use the name as it appears in the data do not modify it</param>
        /// <returns>Returns 5 Number Summary and Avg on days to decision for a Relief Sought for a Judge in a county.</returns>
        [Route("api/courts/statistics/judges/{countyId}/{judgeId}/{reliefSought}/DecisionTime")]
        [ResponseType(typeof(vwJudgeReliefSought5NumberSummary))]
        public IHttpActionResult GetJudgeReliefSought5PlusNumberSummary(string countyId, string judgeId, string reliefSought)
        {
            try
            {
                var result = DAL.eCourts.GetJudgeReliefSought5PlusNumberSummary(countyId, judgeId, reliefSought);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for CountyId: {0} JudgeId: {1} ReliefSought: {2}{3}", countyId, judgeId, reliefSought, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
        #endregion

        #region Court (Across County) Statistics
        /// <summary>  
        ///     Use this api to get a list of decision statistics for a Relief Sought for all judges in NYC Counties.
        /// </summary>  
        /// <param name="reliefSought">
        ///     Relief Sought in a Motion. Use the name as it appears in the data do not modify it 
        /// </param> 
        /// <returns>
        ///     Returns a list of decision statistics for a Relief Sought for all judges in NYC Counties.
        /// </returns>
        [Route("api/courts/statistics/{reliefSought}/DecisionSummary")]
        [ResponseType(typeof(vwMotionSummaryByReliefSought))]
        public IHttpActionResult GetMotionSummaryStatisticsByReliefSought(string reliefSought)
        {
            try
            {
                var result = DAL.eCourts.GetMotionSummaryStatisticsByReliefSought(reliefSought);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for ReliefSought: {0}{1}", reliefSought, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
        #endregion

        #endregion
    }
}
