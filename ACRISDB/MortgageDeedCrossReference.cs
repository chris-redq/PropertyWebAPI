//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ACRISDB
{
    using System;
    using System.Collections.Generic;
    
    public partial class MortgageDeedCrossReference
    {
        public string UniqueKey { get; set; }
        public string CRFN { get; set; }
        public string DocumentIdReference { get; set; }
        public string ReelYear { get; set; }
        public string ReelBorough { get; set; }
        public string ReelNumber { get; set; }
        public string ReelPage { get; set; }
        public Nullable<System.DateTime> DateLastUpdated { get; set; }
    }
}