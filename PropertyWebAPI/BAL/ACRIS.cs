//-----------------------------------------------------------------------
// <copyright file="ACRIS.cs" company="Redq Technologies, Inc.">
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
    using ACRISDB;
    using AutoMapper;
   

    public class ACRIS
    {
        public static List<DAL.MortgageDocumentDetail> GetMortgageChain(string propertyBBL)
        {
            using (ACRISEntities acrisDBEntities = new ACRISEntities())
            {
                var resultList = acrisDBEntities.tfnGetMortgageChain(propertyBBL).OrderByDescending(m => (m.DocumentDate != null) ? m.DocumentDate : m.DateRecorded)
                                                                                 .ThenByDescending(m => (m.RelatedDocumentDate != null) ? m.RelatedDocumentDate : m.RelatedDocumentRecordDate)
                                                                                 .ToList();
                if (resultList == null || resultList.Count == 0)
                    return null;

                List<DAL.MortgageDocumentDetail> mortgageChain = new List<DAL.MortgageDocumentDetail>();
                string documentKey = "";
                DAL.MortgageDocumentDetail mortgage = null;

                foreach (var rec in resultList)
                {
                    if (documentKey!=rec.UniqueKey)
                    {
                        documentKey = rec.UniqueKey;

                        mortgage = Mapper.Map<DAL.MortgageDocumentDetail>(rec);
                        mortgage.IsPaid = false;
                        mortgage.Parties = Mapper.Map<List<tfnGetDocumentParties_Result>, List<DAL.DocumentParty>>(acrisDBEntities.tfnGetDocumentParties(rec.UniqueKey, null).ToList());
                        mortgageChain.Add(mortgage);
                    }
                    if (rec.RelatedDocumentUniqueKey!=null)
                    {
                        if (mortgage.RelatedDocuments==null)
                            mortgage.RelatedDocuments = new List<DAL.MortgageRelatedDocumentDetail>();
                        if (rec.RelatedDocumentType == "SAT")
                            mortgage.IsPaid = true;
                        DAL.MortgageRelatedDocumentDetail rDoc = new DAL.MortgageRelatedDocumentDetail();
                        rDoc.UniqueKey = rec.RelatedDocumentUniqueKey;
                        rDoc.DocumentType = rec.RelatedDocumentType;
                        rDoc.DocumentTypeDescription = rec.RelatedDocumentTypeDescription;
                        rDoc.DocumentDate = rec.RelatedDocumentDate;
                        rDoc.DateRecorded = rec.RelatedDocumentRecordDate;
                        rDoc.URL = rec.RelatedDocumentURL;
                        rDoc.Parties = Mapper.Map<List<tfnGetDocumentParties_Result>, List<DAL.DocumentParty>>(acrisDBEntities.tfnGetDocumentParties(rec.RelatedDocumentUniqueKey, null).ToList());
                        mortgage.RelatedDocuments.Add(rDoc);
                    }
                }
                return mortgageChain;
            }
        }

        public static DAL.DeedDetails GetLatestDeedDetails(string propertyBBLE)
        {
            try
            {
                using (ACRISEntities acrisDBEntities = new ACRISEntities())
                {
                    LatestDeedDocument documentObj = acrisDBEntities.LatestDeedDocuments.FirstOrDefault(i => i.BBLE == propertyBBLE);

                    if (documentObj == null)
                    {   Common.Logs.log().Error(string.Format("Error finding record for BBLE {0} in LatestDeedDocument", propertyBBLE));
                        return null;
                    }
                    DAL.DeedDetails deedDetailsObj = new DAL.DeedDetails();
                    deedDetailsObj.deedDocument = documentObj;
                    foreach (tfnGetDocumentParties_Result a in acrisDBEntities.tfnGetDocumentParties(documentObj.DeedUniqueKey, "BUYER").ToList())
                    {
                        if (deedDetailsObj.owners == null)
                            deedDetailsObj.owners = new List<DAL.DeedParty>();
                        deedDetailsObj.owners.Add(Mapper.Map<DAL.DeedParty>(a));
                    }
                    return deedDetailsObj;
                }
            }
            catch(Exception e)
            {
                Common.Logs.log().Error(string.Format("Error reading AreaAbstract{0}", Common.Logs.FormatException(e)));
                return null;
            }
        }
     
    }
}