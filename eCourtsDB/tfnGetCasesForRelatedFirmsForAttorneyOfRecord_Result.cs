//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace eCourtsDB
{
    using System;
    
    public partial class tfnGetCasesForRelatedFirmsForAttorneyOfRecord_Result
    {
        public string CountyId { get; set; }
        public string CaseIndexNumber { get; set; }
        public string CaseIndexNumberExternal { get; set; }
        public string SeqNumber { get; set; }
        public string CourtType { get; set; }
        public string CaseStatus { get; set; }
        public string PostDispositionIndicator { get; set; }
        public Nullable<int> NumberOfComments { get; set; }
        public string PlaintiffName { get; set; }
        public string DefendantName { get; set; }
        public string MunicipalityInvolvement { get; set; }
        public string CaseComplexity { get; set; }
        public Nullable<System.DateTime> RJIDispositionDeadlineDate { get; set; }
        public Nullable<System.DateTime> RJIPreNOIDeadlineDate { get; set; }
        public Nullable<System.DateTime> PostNOIDispositionDeadlineDate { get; set; }
        public Nullable<System.DateTime> DateRJIFiled { get; set; }
        public Nullable<System.DateTime> DateNOIFiled { get; set; }
        public Nullable<System.DateTime> TimeStampNOIEntered { get; set; }
        public Nullable<System.DateTime> DateIssueJoined { get; set; }
        public string ActionOrCaseTypeId { get; set; }
        public string ActionOrCaseType { get; set; }
        public string ActionOrCaseTypeDescription { get; set; }
        public string DamagesSought { get; set; }
        public string GeneralPreference { get; set; }
        public string SpecialPreference { get; set; }
        public Nullable<System.DateTime> CaseDispositionDate { get; set; }
        public string RJITypeCode { get; set; }
        public string TypeOfRJI { get; set; }
        public Nullable<System.DateTime> DateOfRJI { get; set; }
        public string RJICourtPartId { get; set; }
        public string RJICourtPart { get; set; }
        public string CommentForOtherRJIType { get; set; }
        public string IASCategoryId { get; set; }
        public string IASCategory { get; set; }
        public string IASJudgeId { get; set; }
        public string IASJudgeName { get; set; }
        public Nullable<System.DateTime> IASAssignmentDate { get; set; }
        public Nullable<System.DateTime> TimeStampLastIASInformationEntered { get; set; }
        public string PostDispositionActionJudgeId { get; set; }
        public string PostDispositionActionJudgeName { get; set; }
        public string PROSEIndicator { get; set; }
        public Nullable<System.DateTime> DateRJIEntered { get; set; }
        public string PrivateCaseIndicator { get; set; }
        public string IsEfiled { get; set; }
        public string Comment { get; set; }
    }
}
