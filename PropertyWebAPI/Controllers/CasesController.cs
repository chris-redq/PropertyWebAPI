﻿//-----------------------------------------------------------------------
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
    public class CasesController : ApiController
    {
        #region Get Data By CaseIndexNumber
        /// <summary>  
        ///     Use this method when calling parameters are CountyId and CaseIndexNumber and return type is a List
        /// </summary>
        private IHttpActionResult TemplateCaseIndexNumberQueriesList<T>(string countyId, string caseIndexNumber, Func<string, string, List<T>> asFunc)
        {
            if (DAL.eCourts.IsValidCountyId(countyId))
                return BadRequest("Incorrect CountyId (only 2 digits long)");

            if (DAL.eCourts.IsValidCaseIndexNumber(caseIndexNumber))
                return BadRequest("Incorrect Case Index Number (only 11 digits long YYYY9999999)");

            try
            {
                List<T> outList = asFunc(countyId, caseIndexNumber);
                if (outList == null || outList.Count == 0)
                    return NotFound();
                return Ok(outList);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for CountyId: {0} CaseIndexNumber: {1}{2}", countyId, caseIndexNumber, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>  
        ///     Use this method when calling parameters are CountyId and CaseIndexNumber and return type is an object
        /// </summary>
        private IHttpActionResult TemplateCaseIndexNumberQueries<T>(string countyId, string caseIndexNumber, Func<string, string, T> asFunc)
        {
            if (DAL.eCourts.IsValidCountyId(countyId))
                return BadRequest("Incorrect CountyId (only 2 digits long)");

            if (DAL.eCourts.IsValidCaseIndexNumber(caseIndexNumber))
                return BadRequest("Incorrect Case Index Number (only 11 digits long YYYY9999999)");

            try
            {
                T outObj = asFunc(countyId, caseIndexNumber);
                if (outObj == null)
                    return NotFound();
                return Ok(outObj);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for CountyId: {0} CaseIndexNumber: {1}{2}", countyId, caseIndexNumber, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>  
        /// Use this api to get a detailed information on a case
        /// </summary>  
        /// <param name="countyId">
        ///     CCIS County Id associated with the case. 
        /// </param>  
        /// <param name="caseIndexNumber">
        ///    Case Index Number in YYYYXXXXXXX format where YYYY represents the year
        /// </param> 
        /// <returns>Returns detailed information on a case</returns>
        [Route("api/cases/{countyId}/{caseIndexNumber}")]
        [ResponseType(typeof(DAL.CaseDetails))]
        public IHttpActionResult Get(string countyId, string caseIndexNumber)
        {
            return TemplateCaseIndexNumberQueries<vwCaseExpanded>(countyId, caseIndexNumber, DAL.eCourts.GetCaseDetails);
        }

        /// <summary>  
        /// Use this api to get a list of Motions (both Defendant and Plaintiff) for a case
        /// </summary>  
        /// <param name="countyId">
        ///     CCIS County Id associated with the case. 
        /// </param>  
        /// <param name="caseIndexNumber">
        ///    Case Index Number in YYYYXXXXXXX format where YYYY represents the year
        /// </param> 
        /// <returns>Returns a list of Motions for a case</returns>
        [Route("api/cases/{countyId}/{caseIndexNumber}/motions")]
        [ResponseType(typeof(List<DAL.MotionDetails>))]
        public IHttpActionResult GetMotions(string countyId, string caseIndexNumber)
        {
            return TemplateCaseIndexNumberQueriesList<DAL.MotionDetails>(countyId, caseIndexNumber, DAL.eCourts.GetAllMotionsForACase);
        }
      
        /// <summary>  
        /// Use this api to get a list of Appearances for a case
        /// </summary>  
        /// <param name="countyId">
        ///     CCIS County Id associated with the case. 
        /// </param>  
        /// <param name="caseIndexNumber">
        ///    Case Index Number in YYYYXXXXXXX format where YYYY represents the year
        /// </param> 
        /// <returns>Returns a list of Appearances for a case</returns>
        [Route("api/cases/{countyId}/{caseIndexNumber}/appearances")]
        [ResponseType(typeof(List<DAL.AppearanceDetails>))]
        public IHttpActionResult GetApperances(string countyId, string caseIndexNumber)
        {
            return TemplateCaseIndexNumberQueriesList<DAL.AppearanceDetails>(countyId, caseIndexNumber, DAL.eCourts.GetAllAppearancesForACase);
        }

        /// <summary>  
        /// Use this api to get a list of attorneys (both Defendant and Plaintiff) for a case
        /// </summary>  
        /// <param name="countyId">
        ///     CCIS County Id associated with the case. 
        /// </param>  
        /// <param name="caseIndexNumber">
        ///    Case Index Number in YYYYXXXXXXX format where YYYY represents the year
        /// </param> 
        /// <returns>Returns a list of Attorneys for a case</returns>
        [Route("api/cases/{countyId}/{caseIndexNumber}/attorneys")]
        [ResponseType(typeof(List<DAL.AttorneyDetails>))]
        public IHttpActionResult GetAttorneys(string countyId, string caseIndexNumber)
        {
            return TemplateCaseIndexNumberQueriesList<DAL.AttorneyDetails>(countyId, caseIndexNumber, DAL.eCourts.GetAllAttorneysForACase);
        }

        /// <summary>  
        /// Use this api to get a case's history in a chronological manner. This api returns events after April 18, 2016
        /// </summary>  
        /// <param name="countyId">
        ///     CCIS County Id associated with the case. 
        /// </param>  
        /// <param name="caseIndexNumber">
        ///    Case Index Number in YYYY9999999 format where YYYY represents the year
        /// </param> 
        /// <returns>
        ///     Returns a list of changes that happened on case. For example Appearances, Motions, Attorneys, Case Details
        /// </returns>
        [Route("api/cases/{countyId}/{caseIndexNumber}/history")]
        [ResponseType(typeof(List<DAL.CaseUpdate>))]
        public IHttpActionResult GetCaseHistory(string countyId, string caseIndexNumber)
        {
            return TemplateCaseIndexNumberQueriesList<DAL.CaseUpdate>(countyId, caseIndexNumber, DAL.eCourts.GetRecordedCaseHistory);
        }

        /// <summary>  
        /// Use this api to get a list of minutes associated (documents filed with court) with a case. Currently, we have data only for
        /// New York and Kings County
        /// </summary>  
        /// <param name="countyId">
        ///     CCIS County Id associated with the case. 
        /// </param>  
        /// <param name="caseIndexNumber">
        ///    Case Index Number in YYYYXXXXXXX format where YYYY represents the year
        /// </param> 
        /// <returns>Returns a list of minutes for a case</returns>
        [Route("api/cases/{countyId}/{caseIndexNumber}/minutes")]
        [ResponseType(typeof(List<DAL.CaseDocumentDetails>))]
        public IHttpActionResult GetCaseMinutes(string countyId, string caseIndexNumber)
        {
            return TemplateCaseIndexNumberQueriesList<DAL.CaseDocumentDetails>(countyId, caseIndexNumber, DAL.eCourts.GetAllMinutesForACase);
        }
        #endregion

        #region Get Data Based on BBL

        /// <summary>  
        ///     Use this api to find NYS Supreme Court Mortgage Foreclosure Cases and their respective status for a property in NYC. 
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>  
        /// <returns>
        ///     Returns a list of cases and their status in eCourts for the given property identified by BBL - Borough Block Lot Number.
        /// </returns>
        [Route("api/cases/mortgageforeclosures/{propertyBBL}")]
        [ResponseType(typeof(List<DAL.CaseBasicInformation>))]
        public IHttpActionResult GetMortgageForeclosureCasesForaProperty(string propertyBBL)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            try
            {
                using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
                {
                    List<tfnGetMortgageForeclosureCasesForaProperty_Result> casesList = nycourtsE.tfnGetMortgageForeclosureCasesForaProperty(propertyBBL).ToList();
                    if (casesList == null || casesList.Count==0)
                        return NotFound();
                    return Ok(Mapper.Map<List<tfnGetMortgageForeclosureCasesForaProperty_Result>, List<DAL.CaseBasicInformation>>(casesList));
                }
            }
            catch(Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL {0}{1}", propertyBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }

        }

        /// <summary>  
        ///     Use this api to find all active LPs associated with in a property in JDLS System. 
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>  
        /// <param name="effectiveAsOfDate">This is an optional parameter in yyyyMMdd format. Use this parameter as a cutoff date to select LPs for consideration. Default Value is 01/01/2007</param>
        /// <returns>
        ///     Returns a list of all active LPs associated with in a property in JDLS System.
        /// </returns>
        [Route("api/cases/activelps/{propertyBBL}")]
        [ResponseType(typeof(List<DAL.LPDetail>))]
        public IHttpActionResult GetAllActiveLPsForaProperty(string propertyBBL, string effectiveAsOfDate="20070101")
        {
            if (!BAL.BBL.IsValidFormat(propertyBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            DateTime? sDateTime = null;
            DateTime actualDateTime = DateTime.MinValue;

            if (effectiveAsOfDate != "" && !DateTime.TryParseExact(effectiveAsOfDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out actualDateTime))
            {
                return this.BadRequest("Incorrect Date Format - use yyyyMMdd format for dates - Eg: 20161030 which Oct 30, 2016");
            }

            if (effectiveAsOfDate != "")
                sDateTime = actualDateTime;

            try
            {
                using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
                {
                    var resultList = DAL.eCourts.GetAllActiveLPsForAProperty(propertyBBL, sDateTime);
                    if (resultList == null || resultList.Count == 0)
                        return NotFound();
                    return Ok(resultList);
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL {0} with effectiveDate{1}{2}", propertyBBL, effectiveAsOfDate, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        /// <summary>  
        ///     Use this api to find all Mortgage Foreclosure related LPs for a property in NYC in the JDLS system and their corresponding cases in the NYS Supreme Court CCIS System. 
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param> 
        /// <param name="effectiveDate">
        ///     Optional parameter in case LPs are requested on or after the supplied effectiveDate
        /// </param> 
        /// <returns>
        ///     Returns a list of LPs and their respective cases in eCourts for the given property identified by BBL - Borough Block Lot Number.
        /// </returns>
        [Route("api/cases/mortgageforeclosurelps/{propertyBBL}")]
        [ResponseType(typeof(List<DAL.LPCaseDetails>))]
        public IHttpActionResult GetMortgageForeclosureLPsForaProperty(string propertyBBL, string effectiveDate="")
        {
            if (!BAL.BBL.IsValidFormat(propertyBBL))
                return BadRequest("Incorrect BBL - Borough Block Lot number");

            DateTime? sDateTime = null;
            DateTime actualDateTime = DateTime.MinValue;

            if (effectiveDate!="" && !DateTime.TryParseExact(effectiveDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out actualDateTime))
            {
                return this.BadRequest("Incorrect Date Format - use yyyyMMdd format for dates - Eg: 20161030 which Oct 30, 2016");
            }

            if (effectiveDate != "")
                sDateTime = actualDateTime;

            try
            {
                using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
                {
                    List<tfnGetMortgageForeclosureLPsForaProperty_Result> lpsList = nycourtsE.tfnGetMortgageForeclosureLPsForaProperty(propertyBBL, sDateTime)
                                                                                             .OrderByDescending(m => m.EffectiveDateTime).ToList();
                    if (lpsList == null || lpsList.Count == 0)
                        return NotFound();

                    return Ok(Mapper.Map<List<tfnGetMortgageForeclosureLPsForaProperty_Result>, List<DAL.LPCaseDetails>>(lpsList));
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for BBL {0}{1}", propertyBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
        
        /// <summary>  
        ///     Use this api to get all new mortgage foreclosure cases added for a select set of counties within a data range. The select set currently is 5 Boroughs and LI counties 
        /// </summary>  
        /// <param name="startDate">
        ///     Start date of the date range in yyyyMMdd format eg: 20160120
        /// </param> 
        /// <param name="endDate">
        ///     End date of the date range in yyyyMMdd format eg: 20161030
        /// </param> 
        /// <returns>
        ///     Returns a list of new foreclosure cases registered within the date range
        /// </returns>
        [Route("api/cases/newmortgageforeclosures/{startDate}/{endDate}")]
        [ResponseType(typeof(List<DAL.CaseBasicInformationWithBBL>))]
        public IHttpActionResult GetNewMortgageForeclosureCases(string startDate, string endDate)
        {
            DateTime sDateTime, eDateTime;

            if (!DateTime.TryParseExact(startDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out sDateTime) ||
                !DateTime.TryParseExact(endDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out eDateTime))
            {
                return this.BadRequest("Incorrect Date Format - use yyyyMMdd format for dates - eg: 20161030 which Oct 30, 2016");
            }

            try
            {
                using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
                {
                    List<tfnGetNewMortgageForeclosureCases_Result> casesList = nycourtsE.tfnGetNewMortgageForeclosureCases(sDateTime, eDateTime).ToList();
                    if (casesList == null || casesList.Count==0)
                        return NotFound();
                    return Ok(Mapper.Map<List<tfnGetNewMortgageForeclosureCases_Result>, List<DAL.CaseBasicInformationWithBBL>>(casesList));
                    
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for Start Date: {0} End Date:{1}{2}", startDate, endDate, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        #endregion

        #region Audit data
        /// <summary>  
        ///     Use this api to find any changes in a specific column on the CCIS Case table for a given date range. 
        ///     For example use this API if you want to check which cases had their CaseStatus change for a given date range
        /// </summary>  
        /// <param name="columnName">
        ///     Column name in the ccis.case table for which changes are being requested. For example CaseStatus
        /// </param>  
        /// <param name="startDate">
        ///    Start date of the date range in yyyyMMdd format eg: 20160120
        /// </param> 
        /// <param name="endDate">
        ///    End date of the date range in yyyyMMdd format eg: 20161030
        /// </param> 
        /// <returns>
        ///     Returns a list of rows (cases) with old column value and new value
        /// </returns>
        [Route("api/cases/columnvaluechanges/{columnName}/{startDate}/{endDate}")]
        [ResponseType(typeof(List<DAL.CaseDataChange>))]
        public IHttpActionResult GetCasesColumnValueChanges(string columnName, string startDate, string endDate)
        {
            DateTime sDateTime, eDateTime;

            if (!DateTime.TryParseExact(startDate,"yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out sDateTime) ||
                !DateTime.TryParseExact(endDate,"yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out eDateTime))
            {
                return this.BadRequest("Incorrect Date Format - use yyyyMMdd format for dates - eg: 20161030 which Oct 30, 2016");
            }

            try
            {
                using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
                {
                    List<tfnGetCaseColumnChanges_Result> caseColumnChangesList = nycourtsE.tfnGetCaseColumnChanges("ccis.case", columnName, sDateTime, eDateTime).ToList();
                    if (caseColumnChangesList == null || caseColumnChangesList.Count==0)
                        return NotFound();
                    return Ok(Mapper.Map<List<tfnGetCaseColumnChanges_Result>, List<DAL.CaseDataChange>>(caseColumnChangesList));
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered for Start Date: {0} End Date:{1}{2}", startDate, endDate, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
        #endregion

        #region Statistics

        /// <summary>  
        ///     Use this api to get a list of cases associated with a Relief Sought for a given judge in a county
        /// </summary>  
        /// <param name="countyId">
        ///     CCIS County Id associated with the Judge. 
        /// </param>  
        /// <param name="judgeId">
        ///     ID of the Judge as it appears in the data
        /// </param>  
        /// <param name="reliefSought">
        ///     Relief Sought in a Motion. Use the name as it appears in the data do not modify it 
        /// </param>
        /// <returns>
        ///     Returns a list of cases associated with a Relief Sought for a given judge in a county
        /// </returns>
        [Route("api/cases/statistics/{countyId}/{judgeId}/{reliefSought}/CasesForJudgeWithReliefSought")]
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


        /// <summary>  
        ///     Use this api to get a list of decision statistics for a Relief Sought for all judges in NYC Counties.
        /// </summary>  
        /// <param name="reliefSought">
        ///     Relief Sought in a Motion. Use the name as it appears in the data do not modify it 
        /// </param> 
        /// <returns>
        ///     Returns a list of decision statistics for a Relief Sought for all judges in NYC Counties.
        /// </returns>
        [Route("api/cases/statistics/{reliefSought}/DecisionSummaryByReliefSought")]
        [ResponseType(typeof(vwMotionSummaryByReliefSought))]
        public IHttpActionResult  GetMotionSummaryStatisticsByReliefSought(string reliefSought)
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

        /// <summary>  
        ///     Use this api to get a list of decision statistics for various Relief Sought for a given judge.
        /// </summary>  
        /// <param name="countyId">
        ///     CCIS County Id associated with the Judge. 
        /// </param>    
        /// <param name="judgeId">
        ///     Id of the Judge as it appears in the data do not modify it 
        /// </param>  
        /// <returns>
        ///     Returns a list of decision statistics for various Relief Sought for a given judge.
        /// </returns>
        [Route("api/cases/statistics/{countyId}/{judgeId}/DecisionSummaryForJudgeByReleifSought")]
        [ResponseType(typeof(List<vwMotionSummaryByJudgeReliefSought>))]
        public IHttpActionResult  GetMotionSummaryStatisticsByJudgeReliefSought(string countyId, string judgeId)
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

        /// <summary>  
        ///     Use this api to get a 5 Number Summary and Avg on days to decision for a Relief Sought for a Judge in a county.
        /// </summary>  
        /// <param name="countyId">
        ///     CCIS County Id associated with the Judge. 
        /// </param> 
        /// <param name="judgeId">
        ///     Id of the Judge as it appears in the data do not modify it 
        /// </param>  
        /// <param name="reliefSought">
        ///     Relief Sought in a Motion. Use the name as it appears in the data do not modify it 
        /// </param>
        /// <returns>
        ///     Returns 5 Number Summary and Avg on days to decision for a Relief Sought for a Judge in a county.
        /// </returns>
        [Route("api/cases/statistics/{countyId}/{judgeId}/{reliefSought}/DecisionTimeByReleifSought")]
        [ResponseType(typeof(vwJudgeReliefSought5NumberSummary))]
        public IHttpActionResult  GetJudgeReliefSought5PlusNumberSummary(string countyId, string judgeId, string reliefSought)
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
    }
}
