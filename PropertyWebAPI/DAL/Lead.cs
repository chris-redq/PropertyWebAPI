//-----------------------------------------------------------------------
// <copyright file="Lead.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI.DAL
{
    using System.Collections.Generic;
    using System.Linq;
    using GPADB;
    using AutoMapper;
    using System;
    using System.Data.Entity.Validation;
    using Newtonsoft.Json;

    public class Filters
    {
        /// <summary>Optional parameter. Multiple values can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string zipcodes { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent along with wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string neighborhoods { get; set; }
        /// <summary>Optional parameter. Valid Values are Y, N or blank. If you send any other value the filter is ignored.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string isvacant { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent. Valid values are A, B, C and D. No wildcards</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string leadgrades { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent along with wildcards. For example: A*,B1</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string buildingclasscodes { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string counties { get; set; }
        /// <summary>Optional parameter. Valid Values are Y, N or blank. If you send any other value the filter is ignored</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ismailingaddressactive { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent. Valid values are MORTGAGE FORECLOSURES, TAX LIENS, TAX LIEN LPS and ALL. 
        /// By default if multiple values are provided the filter applies the OR operator between them. Add an additional value OPERATORAND if
        /// the operator of choice is AND instead of OR. Eg: 1) MORTGAGE FORECLOSURES,TAX LIENS 2) ALL,OPERATORAND</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string lientypes { get; set; }
        /// <summary>Optional parameter. Minimum one value, max two values can be sent. No wildcards</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ltv { get; set; }
        /// <summary>Optional parameter. Minimum one value, max two values can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string equity { get; set; }
        /// <summary>Optional parameter. Minimum one value, max two values can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string violations { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string cities { get; set; }
        /// <summary>Optional parameter. Multiple values (state abbreviations) can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string states { get; set; }
        /// <summary>Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string hasFannie { get; set; }
        /// <summary>Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string hasFreddie { get; set; }
        /// <summary>Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string unbuilt { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent along with wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string servicer { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent along. Valid values are Y, N, NULL and NOT NULL.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string landmark { get; set; }
        /// <summary>Optional parameter. Valid Values are Y, N. Any other value the filter is ignored</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string hasFHA { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent along. Valid values are Y, N, NULL and NOT NULL.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string satisfiedMortgages { get; set; }
        /// <summary>Optional parameter. Always in months. Minimum one value, max two values can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string deedAge { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string taxLiensSoldYear { get; set; }
        /// <summary>Optional parameter. Minimum one value, max two values can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string taxLiensSoldTotal { get; set; }
    }

    public class Scenario
    {
        /// <summary>Name of the user saving the scenario</summary>
        public string username { get; set; }
        /// <summary>Name of the scenario</summary>
        public string scenarioname { get; set; }
        /// <summary>Description of the scenario</summary>
        public string description { get; set; }
        /// <summary>Filters for the Lead List</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Filters filterdata;
    }

    public class LeadSummaryData : vwGeneralLeadInformation
    {
        //Blank classes to mask entity framework details
    }
  

    public class ScenarioListData
    {
        /// <summary>Unique Scenario ID</summary>
        public long ScenarioId { get; set; }
        /// <summary>Scenario Name provided by the User</summary>
        public string ScenarioName { get; set; }
        /// <summary>UTC date and time when scenario was saved</summary>
        public System.DateTime DateTimeSaved { get; set; }
        /// <summary>Description of the filters associated with the scenario. This is provided by the front-end when a save scenario request is made</summary>
        public string Description { get; set; }
    }

    public class Lead
    {
        public static List<LeadSummaryData> GetPropertyLeads(Filters filterdata)
        {
            return GetPropertyLeads(filterdata.zipcodes, filterdata.neighborhoods, filterdata.isvacant, filterdata.leadgrades, filterdata.buildingclasscodes,
                                    filterdata.counties, filterdata.ismailingaddressactive, filterdata.lientypes, filterdata.ltv, filterdata.equity, filterdata.violations,
                                    filterdata.cities, filterdata.states, filterdata.hasFannie, filterdata.hasFreddie, filterdata.unbuilt, filterdata.servicer,
                                    filterdata.landmark, filterdata.hasFHA, filterdata.satisfiedMortgages, filterdata.deedAge, filterdata.taxLiensSoldYear,
                                    filterdata.taxLiensSoldTotal);
        }

        public static List<LeadSummaryData> GetPropertyLeads(string zipcodes, string neighborhoods, string isvacant, string leadgrades, string buildingclasscodes, string counties, 
                                                             string ismailingaddressactive, string lientypes, string ltv, string equity, string violations, string cities,
                                                             string states,  string hasfannie, string hasfreddie, string unbuilt, string servicer, string landmark, string hasFHA,
                                                             string satisfiedMortgages, string deedAge, string taxLiensSoldYear, string taxLiensSoldTotal)
        {
            using (GPADBEntities1 gpaE = new GPADBEntities1())
            {
                return Mapper.Map<List<vwGeneralLeadInformation>, List<LeadSummaryData>>(gpaE.GetLeads(zipcodes, buildingclasscodes, counties, isvacant, ismailingaddressactive, violations,
                                                                                                       cities, neighborhoods, states, lientypes, leadgrades, ltv, equity, hasfannie,
                                                                                                       hasfreddie, unbuilt, servicer, landmark, hasFHA, satisfiedMortgages, deedAge, 
                                                                                                       taxLiensSoldYear, taxLiensSoldTotal).ToList());
            }
        }

        public static List<ScenarioListData> GetScenarios(string userName)
        {
            try
            {
                using (GPADBEntities1 gpaE = new GPADBEntities1())
                {
                    List<ScenarioListData> resultList = null;
                    
                    foreach (var rec in gpaE.SavedScenarios.Where(m => m.UserName == userName).OrderByDescending(m => m.DateTimeSaved).ToList())
                    {
                        if (resultList == null)
                            resultList = new List<ScenarioListData>();
                        var result = new ScenarioListData();
                        result.ScenarioName = rec.ScenarioName;
                        result.ScenarioId = rec.ScenarioId;
                        result.DateTimeSaved = rec.DateTimeSaved;
                        result.Description = rec.Description;

                        resultList.Add(result);
                    }
                    return resultList;
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered getting scenarios for user: {0}{1}", userName, Common.Logs.FormatException(e)));
                return null;
            }
        }


        public static long SaveScenario(Scenario inScenarioObj)
        {
            try
            {
                using (GPADBEntities1 gpaE = new GPADBEntities1())
                {
                    var scenarioObj = new GPADB.SavedScenario();

                    scenarioObj.BuildingClassCodes = inScenarioObj.filterdata.buildingclasscodes;
                    scenarioObj.Cities = inScenarioObj.filterdata.cities;
                    scenarioObj.Counties = inScenarioObj.filterdata.counties;
                    scenarioObj.DateTimeSaved = DateTime.UtcNow;
                    scenarioObj.Description = inScenarioObj.description;
                    scenarioObj.HasFannie = inScenarioObj.filterdata.hasFannie;
                    scenarioObj.HasFreddie = inScenarioObj.filterdata.hasFreddie;
                    scenarioObj.HasFHA = inScenarioObj.filterdata.hasFHA;
                    scenarioObj.Landmark = inScenarioObj.filterdata.landmark;
                    scenarioObj.LeadGrades = inScenarioObj.filterdata.leadgrades;
                    scenarioObj.LienTypes = inScenarioObj.filterdata.lientypes;
                    scenarioObj.LTV = inScenarioObj.filterdata.ltv;
                    scenarioObj.MailingAddressActive = inScenarioObj.filterdata.ismailingaddressactive;
                    scenarioObj.Neighborhoods = inScenarioObj.filterdata.neighborhoods;
                    scenarioObj.ScenarioName = inScenarioObj.scenarioname;
                    scenarioObj.Servicer = inScenarioObj.filterdata.servicer;
                    scenarioObj.States = inScenarioObj.filterdata.states;
                    scenarioObj.UnbuiltArea = inScenarioObj.filterdata.unbuilt;
                    scenarioObj.UserName = inScenarioObj.username;
                    scenarioObj.Vacant = inScenarioObj.filterdata.isvacant;
                    scenarioObj.Violations = inScenarioObj.filterdata.violations;
                    scenarioObj.ZipCode = inScenarioObj.filterdata.zipcodes;
                    scenarioObj.SatisfiedMortgages = inScenarioObj.filterdata.satisfiedMortgages;
                    scenarioObj.DeedAge = inScenarioObj.filterdata.deedAge;
                    scenarioObj.TaxLiensSoldYear = inScenarioObj.filterdata.taxLiensSoldYear;
                    scenarioObj.TaxLiensSoldTotal = inScenarioObj.filterdata.taxLiensSoldTotal;

                    gpaE.SavedScenarios.Add(scenarioObj);
                    gpaE.SaveChanges();
                    return scenarioObj.ScenarioId;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Common.Logs.log().Error(string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage));
                    }
                }
                return -1;
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered saving scenario {0} for {1}{2}", inScenarioObj.scenarioname,
                                                       inScenarioObj.username, Common.Logs.FormatException(e)));
                return -1;
            }
        }

        public static LeadSummaryData GetPropertyLead(string propertybbl)
        {
            using (GPADBEntities1 gpaE = new GPADBEntities1())
            {
                return Mapper.Map<LeadSummaryData>(gpaE.vwGeneralLeadInformations.Where(x => x.BBLE == propertybbl).FirstOrDefault());
            }
        }
    }
}