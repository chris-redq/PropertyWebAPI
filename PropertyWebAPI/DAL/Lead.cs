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
    using System.Data.Entity;

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
        /// <summary>Optional parameter. Multiple values can be sent. Valid values are MORTGAGE FORECLOSURES, TAX LIENS, TAX LIEN LPS, TAX LIEN LIST and ALL. 
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
        /// <summary>Optional parameter. Always in months. Minimum one value, max two values can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string deedAge { get; set; }
        /// <summary>Optional parameter. Multiple values can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string taxLiensSoldYear { get; set; }
        /// <summary>Optional parameter. Minimum one value, max two values can be sent. No wildcards.</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string taxLiensSoldTotal { get; set; }
        /// <summary>Optional parameter. Minimum one value, max two values can be sent. No wildcards. -1 looks for deceased owner(s). 
        /// In case there are multiple owners, all owners must be deceased for the lead to qualify for this criteria. 
        /// A single positive number looks for owner(s) with age >= than the number provided. When two positive numbers are provided the owner(s) 
        /// age must be between the range provided. In case of multiple owners the minimum of the age is considered for query purposes </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ownerLivingOrAge { get; set; }
        /// <summary>Optional parameter. Minimum one value, max two values can be sent. No wildcards. -1 means no unsatisfied mortgages ie.  
        /// no open mortgages. A single positive number looks for mortgage(s) older than the number provided (mortgage age is in Months). When two 
        /// positive numbers are provided the mortgage(s) age must be between the range provided. In case of multiple mortgages the minimum of 
        /// the age is considered for query purposes </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string mortgageExistingOrAge { get; set; }
    }

    public class Query
    {
        public string   userName;
        public Filters  queryFilters;
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
        /// <summary>List of selected properties (List of BBLs)</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> selectPropertyList;
    }

    public class List
    {
        /// <summary>Name of the user saving the scenario</summary>
        public string username { get; set; }
        /// <summary>Name of the scenario</summary>
        public string listName { get; set; }
        /// <summary>Description of the scenario</summary>
        public string description { get; set; }
        /// <summary>List of selected properties (List of BBLs)</summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> selectPropertyList;
    }

    public class LeadSummaryData : vwGeneralLeadInformation
    {
        //Blank classes to mask entity framework details
    }


    public class LeadInput
    {
        /// <summary>BBL of the Lead</summary>
        public string propertybbl;
        /// <summary>Type of lead 1 - Miscellaneous</summary>
        public int leadtype;
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

    public class SavedListData
    {
        /// <summary>Unique List ID</summary>
        public long ListId { get; set; }
        /// <summary>List Name provided by the User</summary>
        public string ListName { get; set; }
        /// <summary>UTC date and time when list was saved</summary>
        public System.DateTime DateTimeSaved { get; set; }
        /// <summary>Optional - Description of the list. This is provided by the front-end when a save scenario request is made</summary>
        public string Description { get; set; }
    }

    public class Lead
    {
        public static List<LeadSummaryData> GetPropertyLeads(Filters filterdata, string userName)
        {
            return GetPropertyLeads(filterdata.zipcodes, filterdata.neighborhoods, filterdata.isvacant, filterdata.leadgrades, filterdata.buildingclasscodes,
                                    filterdata.counties, filterdata.ismailingaddressactive, filterdata.lientypes, filterdata.ltv, filterdata.equity, filterdata.violations,
                                    filterdata.cities, filterdata.states, filterdata.hasFannie, filterdata.hasFreddie, filterdata.unbuilt, filterdata.servicer,
                                    filterdata.landmark, filterdata.hasFHA, filterdata.deedAge, filterdata.taxLiensSoldYear, filterdata.taxLiensSoldTotal, 
                                    filterdata.ownerLivingOrAge, filterdata.mortgageExistingOrAge, userName);
        }

        public static List<LeadSummaryData> GetPropertyLeads(string zipcodes, string neighborhoods, string isvacant, string leadgrades, string buildingclasscodes, string counties, 
                                                             string ismailingaddressactive, string lientypes, string ltv, string equity, string violations, string cities,
                                                             string states,  string hasfannie, string hasfreddie, string unbuilt, string servicer, string landmark, string hasFHA,
                                                             string deedAge, string taxLiensSoldYear, string taxLiensSoldTotal, string ownerLivingOrAge, string mortgageExistingOrAge,
                                                             string userName)
        {
            using (GPADBEntities1 gpaE = new GPADBEntities1())
            {
                return Mapper.Map<List<vwGeneralLeadInformation>, List<LeadSummaryData>>(gpaE.GetLeads(zipcodes, buildingclasscodes, counties, isvacant, ismailingaddressactive, violations,
                                                                                                       cities, neighborhoods, states, lientypes, leadgrades, ltv, equity, hasfannie,
                                                                                                       hasfreddie, unbuilt, servicer, landmark, hasFHA, deedAge, taxLiensSoldYear, 
                                                                                                       taxLiensSoldTotal, ownerLivingOrAge, mortgageExistingOrAge, userName).ToList());
            }
        }

        public static LeadSummaryData GetPropertyLead(string propertybbl)
        {
            using (GPADBEntities1 gpaE = new GPADBEntities1())
            {
                return Mapper.Map<LeadSummaryData>(gpaE.vwGeneralLeadInformations.Where(x => x.BBLE == propertybbl).FirstOrDefault());
            }
        }

        public static bool AddLeads(List<LeadInput> leadsList, string externalReferenceId)
        {
            foreach (var lead in leadsList)
            {
                if (!BAL.BBL.IsValidFormat(lead.propertybbl))
                {
                    Common.Logs.log().Warn(string.Format("Incorrect BBL {0} in Bulk Lead Add with external reference {1}", lead.propertybbl, externalReferenceId));
                    continue;
                }

                if (!BAL.BBL.IsValid(lead.propertybbl))
                {
                    Common.Logs.log().Warn(string.Format("Incorrect BBL {0} in Bulk Lead Add with external reference {1}", lead.propertybbl, externalReferenceId));
                    continue;
                }

                try
                {
                    using (GPADBEntities1 gpaE = new GPADBEntities1())
                    {
                        var leadObj = gpaE.Leads.Find(lead.propertybbl);
                        if (leadObj != null)
                            return true;
                        leadObj = new GPADB.Lead();
                        leadObj.BBL = lead.propertybbl;
                        leadObj.Source = 1;
                        leadObj.TaxLienList = 0;
                        leadObj.TaxLienLP = 0;
                        leadObj.TaxLienSold = 0;
                        leadObj.MortgageForeclosureLienLP = 0;
                        leadObj.LeadType = lead.leadtype;
                        leadObj.LastUpdateDate = DateTime.UtcNow;
                        gpaE.Leads.Add(leadObj);
                    }
                }
                catch (DbEntityValidationException dbEx)
                {
                    Guid g;
                    g = Guid.NewGuid();
                    Common.Logs.log().Error(string.Format("Error ReferenceID: {0}", g));
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Common.Logs.log().Error(string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage));
                        }
                    }
                    throw new Common.AppException(string.Format("Internal Server Error ReferenceID: {0}", g));
                }
                catch (Exception e)
                {
                    Guid g;
                    g = Guid.NewGuid();

                    Common.Logs.log().Error(string.Format("Exception encountered adding lead BBL:{0} ReferenceID:{1}{2}", lead.propertybbl,
                                                           g, Common.Logs.FormatException(e)));
                    throw new Common.AppException(string.Format("Internal Server Error ReferenceID: {0}", g));
                }
            }
            return true;
        }

        #region SavedScenarios

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
                    scenarioObj.DeedAge = inScenarioObj.filterdata.deedAge;
                    scenarioObj.TaxLiensSoldYear = inScenarioObj.filterdata.taxLiensSoldYear;
                    scenarioObj.TaxLiensSoldTotal = inScenarioObj.filterdata.taxLiensSoldTotal;
                    scenarioObj.OwnerLivingOrAge = inScenarioObj.filterdata.ownerLivingOrAge;
                    scenarioObj.MortgageExistingOrAge = inScenarioObj.filterdata.mortgageExistingOrAge;

                    gpaE.SavedScenarios.Add(scenarioObj);
                    gpaE.SaveChanges();

                    if (inScenarioObj.selectPropertyList != null)
                    {
                        foreach (var bbl in inScenarioObj.selectPropertyList)
                        {
                            var selectPropertyObj = new GPADB.SelectScenarioProperty();
                            selectPropertyObj.ScenarioId = scenarioObj.ScenarioId;
                            selectPropertyObj.BBL = bbl;

                            gpaE.SelectScenarioProperties.Add(selectPropertyObj);
                        }
                        gpaE.SaveChanges();
                    }

                    return scenarioObj.ScenarioId;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                Guid g;
                g = Guid.NewGuid();

                Common.Logs.log().Error(string.Format("Error ReferenceID: {0}", g));

                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Common.Logs.log().Error(string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage));
                    }
                }

                throw new Common.AppException(string.Format("Internal Server Error ReferenceID: {0}", g));
            }
            catch (Exception e)
            {
                Guid g;
                g = Guid.NewGuid();

                Common.Logs.log().Error(string.Format("Exception encountered saving scenario {0} for {1} ReferenceId:{2}{3}", inScenarioObj.scenarioname,
                                                       inScenarioObj.username, g, Common.Logs.FormatException(e)));

                throw new Common.AppException(string.Format("Internal Server Error ReferenceID: {0}", g));
            }
        }

        public static bool UpdateScenario(long scenarioId, List<string> selectProperties, string scenarioName)
        {
            GPADB.SavedScenario scenarioObj = null;

            try
            {
                using (GPADBEntities1 gpaE = new GPADBEntities1())
                {
                    scenarioObj = gpaE.SavedScenarios.Find(scenarioId);

                    if (scenarioObj == null)
                        return false;

                    
                    scenarioObj.ScenarioName = scenarioName;
                    gpaE.Entry(scenarioObj).State = EntityState.Modified;
                    gpaE.SaveChanges();

                    if (selectProperties != null)
                    {
                        gpaE.SelectScenarioProperties.RemoveRange(gpaE.SelectScenarioProperties.Where(x => x.ScenarioId == scenarioId));
                        gpaE.SaveChanges();

                        foreach (var bbl in selectProperties)
                        {
                            var selectPropertyObj = new GPADB.SelectScenarioProperty();
                            selectPropertyObj.ScenarioId = scenarioObj.ScenarioId;
                            selectPropertyObj.BBL = bbl;

                            gpaE.SelectScenarioProperties.Add(selectPropertyObj);
                        }
                        gpaE.SaveChanges();
                    }
                    return true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                Guid g;
                g = Guid.NewGuid();

                Common.Logs.log().Error(string.Format("Error ReferenceID: {0}", g));

                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Common.Logs.log().Error(string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage));
                    }
                }

                throw new Common.AppException(string.Format("Internal Server Error ReferenceID: {0}", g));
            }
            catch (Exception e)
            {
                Guid g;
                g = Guid.NewGuid();

                Common.Logs.log().Error(string.Format("Exception encountered saving select properties for scenariodId{1} ReferenceID:{2}{3}", scenarioId, g, Common.Logs.FormatException(e)));

                throw new Common.AppException(string.Format("Internal Server Error ReferenceID: {0}", g));
            }
        } 
        #endregion


        #region SavedLists

        public static List<SavedListData> GetLists(string userName)
        {
            try
            {
                using (GPADBEntities1 gpaE = new GPADBEntities1())
                {
                    List<SavedListData> resultList = null;

                    foreach (var rec in gpaE.SavedLists.Where(m => m.UserName == userName).OrderByDescending(m => m.DateTimeSaved).ToList())
                    {
                        if (resultList == null)
                            resultList = new List<SavedListData>();
                        var result = new SavedListData();
                        result.ListName = rec.ListName;
                        result.ListId = rec.ListId;
                        result.DateTimeSaved = rec.DateTimeSaved;
                        result.Description = rec.Description;

                        resultList.Add(result);
                    }
                    return resultList;
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered getting saved lists for user: {0}{1}", userName, Common.Logs.FormatException(e)));
                return null;
            }
        }

        public static long SaveList(List inListObj)
        {
            try
            {
                using (GPADBEntities1 gpaE = new GPADBEntities1())
                {
                    var listObj = new GPADB.SavedList();

                    listObj.DateTimeSaved = DateTime.UtcNow;
                    listObj.Description = inListObj.description;
                    listObj.ListName = inListObj.listName;
                    listObj.UserName = inListObj.username;

                    gpaE.SavedLists.Add(listObj);
                    gpaE.SaveChanges();

                    if (inListObj.selectPropertyList != null)
                    {
                        foreach (var bbl in inListObj.selectPropertyList)
                        {
                            if (!BAL.BBL.IsValidFormat(bbl))
                            {
                                Common.Logs.log().Warn(string.Format("Incorrect BBL {0} in SaveList", bbl));
                                continue;
                            }

                            if (!BAL.BBL.IsValid(bbl))
                            {
                                Common.Logs.log().Warn(string.Format("Incorrect BBL {0} in SaveList", bbl));
                                continue;
                            }

                            var selectPropertyObj = new GPADB.SelectListProperty();
                            selectPropertyObj.ListId = listObj.ListId;
                            selectPropertyObj.BBL = bbl;

                            gpaE.SelectListProperties.Add(selectPropertyObj);
                        }
                        gpaE.SaveChanges();
                    }

                    return listObj.ListId;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                Guid g;
                g = Guid.NewGuid();
                Common.Logs.log().Error(string.Format("Error ReferenceID: {0}", g));
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Common.Logs.log().Error(string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage));
                    }
                }
                throw new Common.AppException(string.Format("Internal Server Error ReferenceID: {0}", g));
            }
            catch (Exception e)
            {
                Guid g;
                g = Guid.NewGuid();

                Common.Logs.log().Error(string.Format("Exception encountered saving list {0} for {1} ReferenceID:{1}{2}", inListObj.listName,
                                                       inListObj.username, g, Common.Logs.FormatException(e)));
                throw new Common.AppException(string.Format("Internal Server Error ReferenceID: {0}", g));
            }
        }

        public static bool SaveList(long listId, List inListObj)
        {
            GPADB.SavedList listObj = null;

            try
            {
                using (GPADBEntities1 gpaE = new GPADBEntities1())
                {
                    listObj = gpaE.SavedLists.Find(listId);

                    if (listObj == null)
                        return false;

                    if (!string.IsNullOrEmpty(inListObj.username))
                        listObj.UserName = inListObj.username;

                    if (!string.IsNullOrEmpty(inListObj.listName))
                        listObj.ListName = inListObj.listName;

                    if (!string.IsNullOrEmpty(inListObj.description))
                        listObj.Description = inListObj.description;

                    if (!string.IsNullOrEmpty(inListObj.listName) || 
                        !string.IsNullOrEmpty(inListObj.description) ||
                        !string.IsNullOrEmpty(inListObj.username))
                    {
                        gpaE.Entry(listObj).State = EntityState.Modified;
                        gpaE.SaveChanges();
                    }

                    if (inListObj.selectPropertyList != null)
                    {
                        gpaE.SelectListProperties.RemoveRange(gpaE.SelectListProperties.Where(x => x.ListId == listId));
                        gpaE.SaveChanges();

                        foreach (var bbl in inListObj.selectPropertyList)
                        {
                            if (!BAL.BBL.IsValidFormat(bbl))
                            {
                                Common.Logs.log().Warn(string.Format("Incorrect BBL {0} in SaveList", bbl));
                                continue;
                            }

                            if (!BAL.BBL.IsValid(bbl))
                            {
                                Common.Logs.log().Warn(string.Format("Incorrect BBL {0} in SaveList", bbl));
                                continue;
                            }

                            var selectPropertyObj = new GPADB.SelectListProperty();
                            selectPropertyObj.ListId = listObj.ListId;
                            selectPropertyObj.BBL = bbl;

                            gpaE.SelectListProperties.Add(selectPropertyObj);
                        }
                        gpaE.SaveChanges();
                    }
                    return true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                Guid g;
                g = Guid.NewGuid();
                Common.Logs.log().Error(string.Format("Error ReferenceID: {0}", g));
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Common.Logs.log().Error(string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage));
                    }
                }
                throw new Common.AppException(string.Format("Internal Server Error ReferenceID: {0}", g));
            }
            catch (Exception e)
            {
                Guid g;
                g = Guid.NewGuid();

                Common.Logs.log().Error(string.Format("Exception encountered saving list ID {0} ReferenceID:{1}{2}", listId,
                                                       g, Common.Logs.FormatException(e)));
                throw new Common.AppException(string.Format("Internal Server Error ReferenceID: {0}", g));
            }
        }


        public static bool DeletePropertiesFromList(long listId, List<string> bblList)
        {
            GPADB.SavedList listObj = null;

            try
            {
                using (GPADBEntities1 gpaE = new GPADBEntities1())
                {
                    listObj = gpaE.SavedLists.Find(listId);

                    if (listObj == null)
                        return false;
                    foreach (var propertyBBL in bblList)
                    {
                        var resObj = gpaE.SelectListProperties.Where(x => x.ListId == listId && x.BBL == propertyBBL).First();
                        if (resObj != null)
                            gpaE.Entry(resObj).State = EntityState.Deleted;
                    }
                    gpaE.SaveChanges();
                    return true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                Guid g;
                g = Guid.NewGuid();
                Common.Logs.log().Error(string.Format("Error ReferenceID: {0}", g));
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Common.Logs.log().Error(string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage));
                    }
                }
                throw new Common.AppException(string.Format("Internal Server Error ReferenceID: {0}", g));
            }
            catch (Exception e)
            {
                Guid g;
                g = Guid.NewGuid();

                Common.Logs.log().Error(string.Format("Exception encountered saving list ID {0} ReferenceID:{1}{2}", listId,
                                                       g, Common.Logs.FormatException(e)));
                throw new Common.AppException(string.Format("Internal Server Error ReferenceID: {0}", g));
            }
        }

        public static bool AddPropertiesToList(long listId, List<string> bblList)
        {
            GPADB.SavedList listObj = null;

            try
            {
                using (GPADBEntities1 gpaE = new GPADBEntities1())
                {
                    listObj = gpaE.SavedLists.Find(listId);

                    if (listObj == null)
                        return false;
                    foreach (var propertyBBL in bblList)
                    {
                        var resObj = gpaE.SelectListProperties.Where(x => x.ListId == listId && x.BBL == propertyBBL).First();
                        if (resObj == null)
                        {
                            if (!BAL.BBL.IsValidFormat(propertyBBL))
                            {
                                Common.Logs.log().Warn(string.Format("Incorrect BBL {0} in SaveList", propertyBBL));
                                continue;
                            }

                            if (!BAL.BBL.IsValid(propertyBBL))
                            {
                                Common.Logs.log().Warn(string.Format("Incorrect BBL {0} in SaveList", propertyBBL));
                                continue;
                            }

                            var selectPropertyObj = new GPADB.SelectListProperty();
                            selectPropertyObj.ListId = listObj.ListId;
                            selectPropertyObj.BBL = propertyBBL;

                            gpaE.SelectListProperties.Add(selectPropertyObj);
                        }
                    }
                    gpaE.SaveChanges();
                    return true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                Guid g;
                g = Guid.NewGuid();
                Common.Logs.log().Error(string.Format("Error ReferenceID: {0}", g));
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Common.Logs.log().Error(string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage));
                    }
                }
                throw new Common.AppException(string.Format("Internal Server Error ReferenceID: {0}", g));
            }
            catch (Exception e)
            {
                Guid g;
                g = Guid.NewGuid();

                Common.Logs.log().Error(string.Format("Exception encountered saving list ID {0} ReferenceID:{1}{2}", listId,
                                                       g, Common.Logs.FormatException(e)));
                throw new Common.AppException(string.Format("Internal Server Error ReferenceID: {0}", g));
            }
        }

        #endregion
    }
}