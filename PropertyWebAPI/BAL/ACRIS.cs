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

    public class DeedParty:tfnGetDocumentParties_Result
    {
        //
    }

    public class DocumentParty : tfnGetDocumentParties_Result
    {
        //
    }

    public class DeedDocument : tfnGetUnsatisfiedMortgages_Result
    {
        //Blank classes to mask entity framework details
    }

    public class PropertyLotInformation : PropertyNotInAssessment
    {
        //Blank classes to mask entity framework details
    }

    public class DeedDetails
    {
        public LatestDeedDocument deedDocument;
        public List<DeedParty> owners;
    }

    public class DocumentDetail: tfnGetDocuments_Result
    {
        
    }

    public class MortgageRelatedDocumentDetail
    {
        public string UniqueKey { get; set; }
        public string DocumentType { get; set; }
        public string DocumentTypeDescription { get; set; }
        public Nullable<System.DateTime> DocumentDate { get; set; }
        public Nullable<System.DateTime> DateRecorded { get; set; }
        public string URL { get; set; }
        public List<DocumentParty> Parties;
    }

    public class MortgageDocumentDetail
    {
        public string BBLE { get; set; }
        public string UniqueKey { get; set; }
        public string CRFN { get; set; }
        public string PropertyType { get; set; }
        public string DocumentType { get; set; }
        public string DocumentTypeDescription { get; set; }
        public string DocumentClassCodeDescription { get; set; }
        public Nullable<System.DateTime> DocumentDate { get; set; }
        public Nullable<decimal> DocumentAmount { get; set; }
        public Nullable<decimal> PercentageOfTransaction { get; set; }
        public Nullable<System.DateTime> DateRecorded { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }
        public string RecordedBorough { get; set; }
        public Nullable<System.DateTime> DateLastUpdated { get; set; }
        public string ReelYear { get; set; }
        public string ReelNumber { get; set; }
        public string ReelPage { get; set; }
        public string URL { get; set; }
        public bool IsPaid { get; set; }
        public List<DocumentParty> Parties;
        public List<MortgageRelatedDocumentDetail> RelatedDocuments;

    }

    

    public class ACRIS
    {
        public static List<DocumentDetail> GetDocuments(string propertyBBL)
        {
            using (ACRISEntities acrisDBEntities = new ACRISEntities())
            {
                return Mapper.Map<List<tfnGetDocuments_Result>, List <DocumentDetail>>( acrisDBEntities.tfnGetDocuments(propertyBBL,null)
                                                                                                       .OrderByDescending(m => (m.DocumentDate != null) ? m.DocumentDate : m.DateRecorded)
                                                                                                       .ToList());
            }
        }

        public static List<DocumentDetail> GetDeeds(string propertyBBL)
        {
           using (ACRISEntities acrisDBEntities = new ACRISEntities())
            {
                return Mapper.Map<List<tfnGetDocuments_Result>, List<DocumentDetail>>(acrisDBEntities.tfnGetDocuments(propertyBBL, null)
                                                                                                     .Where(i => i.DocumentType == "DEED" || i.DocumentType == "DEEDO")
                                                                                                     .OrderByDescending(m => (m.DocumentDate!=null)? m.DocumentDate : m.DateRecorded)
                                                                                                     .ToList());
            }
        }

        public static List<MortgageDocumentDetail> GetMortgageChain(string propertyBBL)
        {
            using (ACRISEntities acrisDBEntities = new ACRISEntities())
            {
                var resultList = acrisDBEntities.tfnGetMortgageChain(propertyBBL).OrderByDescending(m => (m.DocumentDate != null) ? m.DocumentDate : m.DateRecorded)
                                                                                 .ThenByDescending(m => (m.RelatedDocumentDate != null) ? m.RelatedDocumentDate : m.RelatedDocumentRecordDate)
                                                                                 .ToList();
                if (resultList == null || resultList.Count == 0)
                    return null;

                List<MortgageDocumentDetail> mortgageChain = new List<MortgageDocumentDetail>();
                string documentKey = "";
                MortgageDocumentDetail mortgage = null;
                foreach (var rec in resultList)
                {
                    if (documentKey!=rec.UniqueKey)
                    {
                        documentKey = rec.UniqueKey;

                        mortgage = Mapper.Map<MortgageDocumentDetail>(rec);
                        mortgage.IsPaid = false;
                        mortgage.Parties = Mapper.Map<List<tfnGetDocumentParties_Result>, List<DocumentParty>>(acrisDBEntities.tfnGetDocumentParties(rec.UniqueKey, null).ToList());
                        mortgageChain.Add(mortgage);
                    }
                    if (rec.RelatedDocumentUniqueKey!=null)
                    {
                        if (mortgage.RelatedDocuments==null)
                            mortgage.RelatedDocuments = new List<MortgageRelatedDocumentDetail>();
                        if (rec.RelatedDocumentType == "SAT")
                            mortgage.IsPaid = true;
                        MortgageRelatedDocumentDetail rDoc = new MortgageRelatedDocumentDetail();
                        rDoc.UniqueKey = rec.RelatedDocumentUniqueKey;
                        rDoc.DocumentType = rec.RelatedDocumentType;
                        rDoc.DocumentTypeDescription = rec.RelatedDocumentTypeDescription;
                        rDoc.DocumentDate = rec.RelatedDocumentDate;
                        rDoc.DateRecorded = rec.RelatedDocumentRecordDate;
                        rDoc.URL = rec.RelatedDocumentURL;
                        rDoc.Parties = Mapper.Map<List<tfnGetDocumentParties_Result>, List<DocumentParty>>(acrisDBEntities.tfnGetDocumentParties(rec.RelatedDocumentUniqueKey, null).ToList());
                        mortgage.RelatedDocuments.Add(rDoc);
                    }
                }
                return mortgageChain;
            }
        }

        public static DeedDetails GetLatestDeedDetails(string propertyBBLE)
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
                    DeedDetails deedDetailsObj = new DeedDetails();
                    deedDetailsObj.deedDocument = documentObj;
                    foreach (tfnGetDocumentParties_Result a in acrisDBEntities.tfnGetDocumentParties(documentObj.DeedUniqueKey, "BUYER").ToList())
                    {
                        if (deedDetailsObj.owners == null)
                            deedDetailsObj.owners = new List<DeedParty>();
                        deedDetailsObj.owners.Add(Mapper.Map<DeedParty>(a));
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

        public static PropertyLotInformation GetLotInformation(string propertyBBLE)
        {
            try
            {   using (ACRISEntities acrisDBEntities = new ACRISEntities())
                {
                    PropertyNotInAssessment propertyLotInformationObj = acrisDBEntities.PropertyNotInAssessments.FirstOrDefault(i => i.BBL == propertyBBLE);

                    if (propertyLotInformationObj == null)
                    {   Common.Logs.log().Error(string.Format("Error finding record for BBLE {0} in ACRIS", propertyBBLE));
                        return null;
                    }
                    return Mapper.Map<PropertyLotInformation>(propertyLotInformationObj);
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Error reading AreaAbstract{0}", Common.Logs.FormatException(e)));
                return null;
            }
        }
    }
}