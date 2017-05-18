
namespace PropertyWebAPI.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using ACRISDB;
    using AutoMapper;

    public class DeedParty : tfnGetDocumentParties_Result
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

    public class DocumentDetail : tfnGetDocuments_Result
    {
        public List<DocumentParty> Parties;
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
                var resList = Mapper.Map<List<tfnGetDocuments_Result>, List<DocumentDetail>>(acrisDBEntities.tfnGetDocuments(propertyBBL, null)
                                                                                                            .OrderByDescending(m => (m.DocumentDate != null) ? m.DocumentDate : m.DateRecorded)
                                                                                                            .ToList());
                foreach (var doc in resList)
                {
                    foreach (tfnGetDocumentParties_Result a in acrisDBEntities.tfnGetDocumentParties(doc.UniqueKey, null).OrderBy(x => x.PartyTypeCode).ToList())
                    {
                        if (doc.Parties == null)
                            doc.Parties = new List<DocumentParty>();
                        doc.Parties.Add(Mapper.Map<DocumentParty>(a));
                    }
                }
                return resList; 
            }
        }

        public static List<DocumentDetail> GetDeeds(string propertyBBL)
        {
            using (ACRISEntities acrisDBEntities = new ACRISEntities())
            {
                var resList = Mapper.Map<List<tfnGetDocuments_Result>, List<DocumentDetail>>(acrisDBEntities.tfnGetDocuments(propertyBBL, null)
                                                                                                            .Where(i => i.DocumentType == "DEED" || i.DocumentType == "DEEDO")
                                                                                                            .OrderByDescending(m => (m.DocumentDate != null) ? m.DocumentDate : m.DateRecorded)
                                                                                                            .ToList());
                foreach (var doc in resList)
                {
                    foreach (tfnGetDocumentParties_Result a in acrisDBEntities.tfnGetDocumentParties(doc.UniqueKey, null).OrderBy(x=> x.PartyTypeCode).ToList())
                    {
                        if (doc.Parties == null)
                            doc.Parties = new List<DocumentParty>();
                        doc.Parties.Add(Mapper.Map<DocumentParty>(a));
                    }
                }
                return resList;
            }
        }

        public static PropertyLotInformation GetLotInformation(string propertyBBLE)
        {
            try
            {
                using (ACRISEntities acrisDBEntities = new ACRISEntities())
                {
                    PropertyNotInAssessment propertyLotInformationObj = acrisDBEntities.PropertyNotInAssessments.FirstOrDefault(i => i.BBL == propertyBBLE);

                    if (propertyLotInformationObj == null)
                    {
                        Common.Logs.log().Error(string.Format("Error finding record for BBLE {0} in ACRIS", propertyBBLE));
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