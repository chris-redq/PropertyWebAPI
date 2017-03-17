//-----------------------------------------------------------------------
// <copyright file="Results.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI.BAL
{
    using Newtonsoft.Json;

    public class Results
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BAL.WaterBillDetails waterBill;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BAL.TaxBillDetails taxBill;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BAL.MortgageServicerDetails mortgageServicer;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BAL.DOBPenaltiesAndViolationsSummaryData dobPenaltiesAndViolationsSummary;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BAL.ZillowPropertyDetails zillowProperty;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BAL.NoticeOfPropertyValueResult noticeOfPropertyValueResult;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BAL.MortgageDocumentResult mortgageDocumentResult;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BAL.FreddieMortgageDetails freddieMacResult;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal FannieMortgageDetails fannieMaeResult;
    }
}