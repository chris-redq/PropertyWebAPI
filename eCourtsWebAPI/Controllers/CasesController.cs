﻿using eCourtsDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace eCourtsWebAPI.Controllers
{
    public class CasesController : ApiController
    {
        NYCOURTSEntities nycourtsE = new NYCOURTSEntities();

        //../api/Cases/7326457
         
        [ResponseType(typeof(vwCaseExpanded))]
        public IHttpActionResult Get(string countyId, string caseIndexNumber)
        {
            //nycourtsE
            if (countyId.Length == 2 && caseIndexNumber.Length == 11)
            {
                vwCaseExpanded caseObj= nycourtsE.vwCaseExpandeds.Find(countyId, caseIndexNumber);
                if (caseObj != null)
                    return Ok(caseObj);
            }
            return NotFound();
        }

        [Route("api/cases/{countyId}/{caseIndexNumber}/motions")]
        [ResponseType(typeof(List<vwMotionExpanded>))]
        public IHttpActionResult GetMotions(string countyId, string caseIndexNumber)
        {
            //nycourtsE
            if (countyId.Length == 2 && caseIndexNumber.Length == 11)
            {
                List<vwMotionExpanded> motions = nycourtsE.vwMotionExpandeds.Where(i => i.CountyId == countyId && i.CaseIndexNumber == caseIndexNumber).OrderByDescending(m=> m.SeqNumber).ToList<vwMotionExpanded>();
                if (motions != null)
                    return Ok(motions);
            }
            return NotFound();
        }


        [Route("api/cases/{countyId}/{caseIndexNumber}/appearances")]
        [ResponseType(typeof(List<vwAppearanceExpanded>))]
        public IHttpActionResult GetApperances(string countyId, string caseIndexNumber)
        {
            //nycourtsE
            if (countyId.Length == 2 && caseIndexNumber.Length == 11)
            {
                List<vwAppearanceExpanded> appearances = nycourtsE.vwAppearanceExpandeds.Where(i => i.CountyId == countyId && i.CaseIndexNumber == caseIndexNumber).OrderBy(m=> m.AppearanceDate).ToList<vwAppearanceExpanded>();
                if (appearances != null)
                    return Ok(appearances);
            }
            return NotFound();
        }

        [Route("api/cases/{countyId}/{caseIndexNumber}/attorneys")]
        [ResponseType(typeof(List<vwAttorneyExpanded>))]
        public IHttpActionResult GetAttorneys(string countyId, string caseIndexNumber)
        {
            //nycourtsE
            if (countyId.Length == 2 && caseIndexNumber.Length == 11)
            {
                List<vwAttorneyExpanded> attorneys = nycourtsE.vwAttorneyExpandeds.Where(i => i.CountyId == countyId && i.CaseIndexNumber == caseIndexNumber).OrderByDescending(m => m.SeqNumber).ToList<vwAttorneyExpanded>();
                if (attorneys != null)
                    return Ok(attorneys);
            }
            return NotFound();
        }

        [Route("api/cases/{countyId}/{caseIndexNumber}/history")]
        [ResponseType(typeof(List<tfnGetCaseUpdates_Result>))]
        public IHttpActionResult GetCaseHistory(string countyId, string caseIndexNumber)
        {
            //nycourtsE
            if (countyId.Length == 2 && caseIndexNumber.Length == 11)
            {
                List<tfnGetCaseUpdates_Result> historyRecords = nycourtsE.tfnGetCaseUpdates(countyId, caseIndexNumber)
                                                                            .OrderBy(m => m.TransactionDateTime)
                                                                            .ThenBy(m => m.DateTimeProcessed).ToList<tfnGetCaseUpdates_Result>();
                if (historyRecords != null)
                    return Ok(historyRecords);
            }
            return NotFound();
        }
    }
}
