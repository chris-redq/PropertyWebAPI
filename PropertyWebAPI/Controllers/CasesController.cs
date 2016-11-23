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

    /// <summary>  
    /// This controller handles all api requests associated with eCourts CCIS Case related data
    /// </summary>  
    [Authorize]
    public class CasesController : ApiController
    {
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
        [ResponseType(typeof(vwCaseExpanded))]
        public IHttpActionResult Get(string countyId, string caseIndexNumber)
        {
            if (countyId.Length == 2 && caseIndexNumber.Length == 11)
            {
                using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
                {
                    vwCaseExpanded caseObj = nycourtsE.vwCaseExpandeds.Find(countyId, caseIndexNumber);
                    if (caseObj != null)
                        return Ok(caseObj);
                }
            }
            return NotFound();
        }

        /// <summary>  
        /// Use this api to get a list of Motions (both Defendent and Plaintiff) for a case
        /// </summary>  
        /// <param name="countyId">
        ///     CCIS County Id associated with the case. 
        /// </param>  
        /// <param name="caseIndexNumber">
        ///    Case Index Number in YYYYXXXXXXX format where YYYY represents the year
        /// </param> 
        /// <returns>Returns a list of Motions for a case</returns>
        [Route("api/cases/{countyId}/{caseIndexNumber}/motions")]
        [ResponseType(typeof(List<vwMotionExpanded>))]
        public IHttpActionResult GetMotions(string countyId, string caseIndexNumber)
        {
            if (countyId.Length == 2 && caseIndexNumber.Length == 11)
            {
                using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
                {
                    List<vwMotionExpanded> motions = nycourtsE.vwMotionExpandeds
                                                        .Where(i => i.CountyId == countyId && i.CaseIndexNumber == caseIndexNumber)
                                                        .OrderByDescending(m => m.SeqNumber).ToList<vwMotionExpanded>();
                    if (motions != null)
                        return Ok(motions);
                }
            }
            return NotFound();
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
        [ResponseType(typeof(List<vwAppearanceExpanded>))]
        public IHttpActionResult GetApperances(string countyId, string caseIndexNumber)
        {
            if (countyId.Length == 2 && caseIndexNumber.Length == 11)
            {
                using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
                {
                    List<vwAppearanceExpanded> appearances = nycourtsE.vwAppearanceExpandeds
                                                                .Where(i => i.CountyId == countyId && i.CaseIndexNumber == caseIndexNumber)
                                                                .OrderBy(m => m.AppearanceDate).ToList<vwAppearanceExpanded>();
                    if (appearances != null)
                        return Ok(appearances);
                }
            }
            return NotFound();
        }

        /// <summary>  
        /// Use this api to get a list of attorneys (both Defendent and Plaintiff) for a case
        /// </summary>  
        /// <param name="countyId">
        ///     CCIS County Id associated with the case. 
        /// </param>  
        /// <param name="caseIndexNumber">
        ///    Case Index Number in YYYYXXXXXXX format where YYYY represents the year
        /// </param> 
        /// <returns>Returns a list of Attorneys for a case</returns>
        [Route("api/cases/{countyId}/{caseIndexNumber}/attorneys")]
        [ResponseType(typeof(List<vwAttorneyExpanded>))]
        public IHttpActionResult GetAttorneys(string countyId, string caseIndexNumber)
        {
            if (countyId.Length == 2 && caseIndexNumber.Length == 11)
            {
                using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
                {
                    List<vwAttorneyExpanded> attorneys = nycourtsE.vwAttorneyExpandeds
                                                            .Where(i => i.CountyId == countyId && i.CaseIndexNumber == caseIndexNumber)
                                                            .OrderByDescending(m => m.SeqNumber).ToList<vwAttorneyExpanded>();
                    if (attorneys != null)
                        return Ok(attorneys);
                }
            }
            return NotFound();
        }

        /// <summary>  
        /// Use this api to get a case's history in a chronoligical manner. This api returns events after April 18, 2016
        /// </summary>  
        /// <param name="countyId">
        ///     CCIS County Id associated with the case. 
        /// </param>  
        /// <param name="caseIndexNumber">
        ///    Case Index Number in YYYYXXXXXXX format where YYYY represents the year
        /// </param> 
        /// <returns>
        ///     Returns a list of changes that happened on case. For example Appearances, Motions, Attorneys, Case Details
        /// </returns>
        [Route("api/cases/{countyId}/{caseIndexNumber}/history")]
        [ResponseType(typeof(List<tfnGetCaseUpdates_Result>))]
        public IHttpActionResult GetCaseHistory(string countyId, string caseIndexNumber)
        {
            if (countyId.Length == 2 && caseIndexNumber.Length == 11)
            {
                using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
                {
                    List<tfnGetCaseUpdates_Result> historyRecords = nycourtsE.tfnGetCaseUpdates(countyId, caseIndexNumber)
                                                                        .OrderBy(m => m.TransactionDateTime)
                                                                        .ThenBy(m => m.DateTimeProcessed).ToList<tfnGetCaseUpdates_Result>();
                    if (historyRecords != null)
                        return Ok(historyRecords);
                }
            }
            return NotFound();
        }

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
        [ResponseType(typeof(List<tfnGetMortgageForeclosureCasesForaProperty_Result>))]
        public IHttpActionResult GetMortgageForeclosureCasesForaProperty(string propertyBBL)
        {
            if (Regex.IsMatch(propertyBBL, "^[1-5][0-9]{9}$"))
            {
                using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
                {
                    List<tfnGetMortgageForeclosureCasesForaProperty_Result> casesList = nycourtsE.tfnGetMortgageForeclosureCasesForaProperty(propertyBBL)
                                                                              .ToList<tfnGetMortgageForeclosureCasesForaProperty_Result>();
                    if (casesList != null)
                    {
                        return Ok(casesList);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }

            return this.BadRequest("Incorrect BBL - Borough Block Lot number");
        }

        /// <summary>  
        ///     Use this api to find all Mortgage Foreclosure related LPs for a property in NYC in the JDLS system and their corresponding cases in the NYS Supreme Court CCIS System. 
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param> 
        /// <param name="effectiveDate">
        ///     AreaAbstractEntities Paramter incase LPs are requested on or after the supplied effectiveDate
        /// </param> 
        /// <returns>
        ///     Returns a list of LPs and their respective cases in eCourts for the given property identified by BBL - Borough Block Lot Number.
        /// </returns>
        [Route("api/cases/mortgageforeclosurelps/{propertyBBL}")]
        [ResponseType(typeof(List<tfnGetMortgageForeclosureLPsForaProperty_Result>))]
        public IHttpActionResult GetMortgageForeclosureLPsForaProperty(string propertyBBL, string effectiveDate="")
        {
            if (Regex.IsMatch(propertyBBL, "^[1-5][0-9]{9}$"))
            {
                DateTime? sDateTime = null;
                DateTime actualDateTime = DateTime.MinValue;

                if (effectiveDate!="" && !DateTime.TryParseExact(effectiveDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out actualDateTime))
                {
                    return this.BadRequest("Incorrect Date Format - use yyyyMMdd format for dates - Eg: 20161030 which Oct 30, 2016");
                }

                if (effectiveDate != "")
                    sDateTime = actualDateTime;

                using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
                {
                    List<tfnGetMortgageForeclosureLPsForaProperty_Result> lpsList = nycourtsE.tfnGetMortgageForeclosureLPsForaProperty(propertyBBL, sDateTime)
                                                                                             .OrderByDescending(m => m.EffectiveDateTime).ToList();
                    if (lpsList != null)
                    {
                        return Ok(lpsList);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }

            return this.BadRequest("Incorrect BBL - Borough Block Lot number");
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
        ///     Returns a list of new forclosure cases registered within the date range
        /// </returns>
        [Route("api/cases/newmortgageforeclosures/{startDate}/{endDate}")]
        [ResponseType(typeof(List<tfnGetNewMortgageForeclosureCases_Result>))]
        public IHttpActionResult GetNewMortgageForeclosureCases(string startDate, string endDate)
        {
            DateTime sDateTime, eDateTime;

            if (!DateTime.TryParseExact(startDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out sDateTime) ||
                !DateTime.TryParseExact(endDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out eDateTime))
            {
                return this.BadRequest("Incorrect Date Format - use yyyyMMdd format for dates - Eg: 20161030 which Oct 30, 2016");
            }

            using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
            {
                List<tfnGetNewMortgageForeclosureCases_Result> casesList = nycourtsE.tfnGetNewMortgageForeclosureCases(sDateTime, eDateTime).ToList();
                if (casesList != null)
                {
                    return Ok(casesList);
                }
                else
                {
                    return NotFound();
                }
            }
            
        }


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
        [ResponseType(typeof(List<tfnGetCaseColumnChanges_Result>))]
        public IHttpActionResult GetCasesColumnValueChanges(string columnName, string startDate, string endDate)
        {
            DateTime sDateTime, eDateTime;

            if (!DateTime.TryParseExact(startDate,"yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out sDateTime) ||
                !DateTime.TryParseExact(endDate,"yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out eDateTime))
            {
                return this.BadRequest("Incorrect Date Format - use yyyyMMdd format for dates - Eg: 20161030 which Oct 30, 2016");
            }

            using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
            {
                List<tfnGetCaseColumnChanges_Result> caseColumnChangesList = nycourtsE.tfnGetCaseColumnChanges("ccis.case", columnName, sDateTime,eDateTime)
                                                                            .ToList<tfnGetCaseColumnChanges_Result>();
                if (caseColumnChangesList != null)
                {
                    return Ok(caseColumnChangesList);
                }
                else
                {
                    return NotFound();
                }
            }
        }
    }
}
