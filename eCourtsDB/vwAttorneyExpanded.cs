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
    using System.Collections.Generic;
    
    public partial class vwAttorneyExpanded
    {
        public string CountyId { get; set; }
        public string CaseIndexNumber { get; set; }
        public string SeqNumber { get; set; }
        public string AttorneyOfRecordId { get; set; }
        public string AttorneyOfRecordFirmName { get; set; }
        public string NameAttorneyOfRecord { get; set; }
        public string NotifyAttorneyOfRecord { get; set; }
        public string TrialCounselId { get; set; }
        public string TrialCounselFirmName { get; set; }
        public string TrialCounselName { get; set; }
        public string NotifyTrialCounsel { get; set; }
        public string PartyIndicator { get; set; }
        public string ClientName { get; set; }
        public Nullable<int> NumOfAttorneysRetained { get; set; }
        public Nullable<System.DateTime> DateFirstRetainerStatementFiled { get; set; }
        public Nullable<System.DateTime> DateCurrentRetainerStatementFiled { get; set; }
        public Nullable<System.DateTime> DateAttorneyTerminated { get; set; }
        public Nullable<System.DateTime> DateAttorneyClosingStatementFiled { get; set; }
        public string AttorneyRegistrationNumber { get; set; }
        public string ProseIndicator { get; set; }
    }
}
