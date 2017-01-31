//-----------------------------------------------------------------------
// <copyright file="Violations.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------


namespace PropertyWebAPI.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using NYCDOB;
    using NYCVNL;
    using AutoMapper;

    public class ViolationSummary: vwViolationSummary
    {

    }

    public class DOBComplaintsSummary : vwComplaintsSummary
    {

    }

    public class ECBViolationDetail : MultiAgencyViolation
    {

    }

    public class Violations
    {
        public static List<Violation> GetDOBViolations(string propertyBBL, string active)
        {
            using (NYCDOBEntities nycdobE = new NYCDOBEntities())
            {
                if (active.ToUpper()=="IGNORE")
                    return nycdobE.Violations
                                  .Where(i => i.BBL == propertyBBL)
                                  .OrderByDescending(m => m.IssueDate).ToList();
                else
                    return nycdobE.Violations
                                  .Where(i => i.BBL == propertyBBL &&i. IsClosed==(active.ToUpper()=="Y"?false:true))
                                  .OrderByDescending(m => m.IssueDate).ToList();
            }
        }

        public static ViolationSummary GetDOBViolationsSummary(string propertyBBL)
        {
            using (NYCDOBEntities nycdobE = new NYCDOBEntities())
            {
                return Mapper.Map<ViolationSummary>(nycdobE.vwViolationSummaries.Where(i => i.BBL == propertyBBL).FirstOrDefault());
            }
        }

        public static List<Complaint> GetDOBCompliants(string propertyBBL, string active)
        {
            using (NYCDOBEntities nycdobE = new NYCDOBEntities())
            {
                if (active.ToUpper() == "IGNORE")
                    return nycdobE.Complaints
                          .Where(i => i.BBL == propertyBBL)
                          .OrderByDescending(m => m.DateEntered).ToList();
                else
                    return nycdobE.Complaints
                              .Where(i => i.BBL == propertyBBL && i.Status == (active == "Y" ? "ACTIVE" : "CLOSED"))
                              .OrderByDescending(m => m.DateEntered).ToList();
            }
        }

        public static DOBComplaintsSummary GetDOBComplaintsSummary(string propertyBBL)
        {
            using (NYCDOBEntities nycdobE = new NYCDOBEntities())
            {
                return Mapper.Map<DOBComplaintsSummary>(nycdobE.vwComplaintsSummaries.Where(i => i.BBL == propertyBBL).FirstOrDefault());
            }
        }

        public static List<ECBViolationDetail> GetECBViolations(string propertyBBL, string active)
        {
            using (NYCVNLEntities nycvnlE = new NYCVNLEntities())
            {
                if (active.ToUpper() == "IGNORE")
                    return Mapper.Map<List<MultiAgencyViolation>, List<ECBViolationDetail>>(nycvnlE.MultiAgencyViolations
                                                                                                   .Where(i => i.BBL == propertyBBL)
                                                                                                   .OrderByDescending(m => m.ViolationDate).ToList());
                else
                {   if (active=="Y")
                        return Mapper.Map<List<MultiAgencyViolation>, List<ECBViolationDetail>>(nycvnlE.MultiAgencyViolations
                                                                                                       .Where(i => i.BBL == propertyBBL && 
                                                                                                                   i.HearingStatus!="WRITTEN OFF" && 
                                                                                                                   i.HearingStatus != "PAID IN FULL")
                                                                                                       .OrderByDescending(m => m.ViolationDate).ToList());
                    else
                        return Mapper.Map<List<MultiAgencyViolation>, List<ECBViolationDetail>>(nycvnlE.MultiAgencyViolations
                                                                                                           .Where(i => i.BBL == propertyBBL &&
                                                                                                                       (i.HearingStatus == "WRITTEN OFF" ||
                                                                                                                        i.HearingStatus == "PAID IN FULL"))
                                                                                                           .OrderByDescending(m => m.ViolationDate).ToList());
                }
            }
        }

        public static ECBViolationsSummary GetECBViolationsSummary(string propertyBBL)
        {
            using (NYCVNLEntities nycvnlE = new NYCVNLEntities())
            {
                return nycvnlE.ECBViolationsSummaries.Find(propertyBBL);
            }
        }

        public static List<ECBViolationsSummaryByAgency> GetECBViolationsSummaryByAgency(string propertyBBL)
        {
            using (NYCVNLEntities nycvnlE = new NYCVNLEntities())
            {
                return nycvnlE.ECBViolationsSummaryByAgencies.Where(m => m.BBL == propertyBBL).ToList();
            }
        }
    }
}