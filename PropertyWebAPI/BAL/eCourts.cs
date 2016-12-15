//-----------------------------------------------------------------------
// <copyright file="eCourts.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI.BAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using eCourtsDB;
    using AutoMapper;


    /// <summary>
    ///     All business level abstractions for both JDLS and CCIs systems from eCourts are defined in this class
    /// </summary>
    public class eCourts
    {
        public static bool IsValidCountyId(string countyId)
        {   uint sInt;
            return string.IsNullOrEmpty(countyId) && countyId.Length==2 && uint.TryParse(countyId, out sInt);
        }

        public static bool IsValidCaseIndexNumber(string caseIndexNumber)
        {
            ulong sLong;
            return string.IsNullOrEmpty(caseIndexNumber) && caseIndexNumber.Length == 11 && ulong.TryParse(caseIndexNumber, out sLong);
        }

        public static vwCaseExpanded GetCaseDetails(string countyId, string caseIndexNumber)
        {
            using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
            {
                return nycourtsE.vwCaseExpandeds.Find(countyId, caseIndexNumber);
            }
        }

        public static List<vwMotionExpanded> GetAllMotionsForACase(string countyId, string caseIndexNumber)
        {
            using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
            {
                return nycourtsE.vwMotionExpandeds
                                .Where(i => i.CountyId == countyId && i.CaseIndexNumber == caseIndexNumber)
                                .OrderByDescending(m => m.SeqNumber).ToList();
            }
        }

        public static List<vwAppearanceExpanded> GetAllAppearancesForACase(string countyId, string caseIndexNumber)
        {
            using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
            {
                return nycourtsE.vwAppearanceExpandeds
                                .Where(i => i.CountyId == countyId && i.CaseIndexNumber == caseIndexNumber)
                                .OrderBy(m => m.AppearanceDate).ToList();
            }
        }

        public static List<vwAttorneyExpanded> GetAllAttorneysForACase(string countyId, string caseIndexNumber)
        {
            using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
            {
                return nycourtsE.vwAttorneyExpandeds.Where(i => i.CountyId == countyId && i.CaseIndexNumber == caseIndexNumber)
                                                    .OrderByDescending(m => m.SeqNumber).ToList();
            }
        }

        public static List<tfnGetCaseUpdates_Result> GetRecordedCaseHistory(string countyId, string caseIndexNumber)
        {
            using (NYCOURTSEntities nycourtsE = new NYCOURTSEntities())
            {
                return nycourtsE.tfnGetCaseUpdates(countyId, caseIndexNumber)
                                .OrderBy(m => m.TransactionDateTime)
                                .ThenBy(m => m.DateTimeProcessed).ToList();
            }
        }
    }

}