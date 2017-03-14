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

    public class LeadSummaryData : vwGeneralLeadInfomation
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
        public static List<LeadSummaryData> GetPropertyLeads(string zipcodes, string neighborhoods, string isvacant, string leadgrades, string buildingclasscodes, string counties, 
                                                             string ismailingaddressactive, string lientypes, string ltv, string equity, string violations, string cities,
                                                             string states,  string isfannie, string isfreddie, string unbuilt, string servicer, string landmark)
        {
            using (GPADBEntities1 gpaE = new GPADBEntities1())
            {
                return Mapper.Map<List<vwGeneralLeadInfomation>, List<LeadSummaryData>>(gpaE.GetLeads(zipcodes, buildingclasscodes, counties, isvacant, ismailingaddressactive, violations,
                                                                                                     cities, neighborhoods, states, lientypes, leadgrades, ltv, equity, isfannie,
                                                                                                     isfreddie, unbuilt, servicer, landmark).ToList());
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


        public static long SaveScenario(string userName, string scenarioName, string description, string zipcodes, string neighborhoods, string isvacant, string leadgrades, 
                                        string buildingclasscodes, string counties, string ismailingaddressactive, string lientypes, string ltv, string equity, string violations, 
                                        string cities, string states, string isfannie, string isfreddie, string unbuilt, string servicer, string landmark)
        {
            try
            {
                using (GPADBEntities1 gpaE = new GPADBEntities1())
                {
                    var scenarioObj = new GPADB.SavedScenario();
                    scenarioObj.BuildingClassCodes = buildingclasscodes;
                    scenarioObj.Cities = cities;
                    scenarioObj.Counties = counties;
                    scenarioObj.DateTimeSaved = DateTime.UtcNow;
                    scenarioObj.Description = description;
                    scenarioObj.IsFannie = isfannie;
                    scenarioObj.IsFreddie = isfreddie;
                    scenarioObj.Landmark = landmark;
                    scenarioObj.LeadGrades = leadgrades;
                    scenarioObj.LienTypes = lientypes;
                    scenarioObj.LTV = ltv;
                    scenarioObj.MailingAddressActive = ismailingaddressactive;
                    scenarioObj.Neighborhoods = neighborhoods;
                    scenarioObj.ScenarioName = scenarioName;
                    scenarioObj.Servicer = servicer;
                    scenarioObj.States = states;
                    scenarioObj.UnbuiltArea = unbuilt;
                    scenarioObj.UserName = userName;
                    scenarioObj.Vacant = isvacant;
                    scenarioObj.Violations = violations;
                    scenarioObj.ZipCode = zipcodes;
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
                Common.Logs.log().Error(string.Format("Exception encountered saving scenario {0} for {1}{2}", scenarioName, userName, Common.Logs.FormatException(e)));
                return -1;
            }
        }

        public static LeadSummaryData GetPropertyLead(string propertybbl)
        {
            using (GPADBEntities1 gpaE = new GPADBEntities1())
            {
                return Mapper.Map<LeadSummaryData>(gpaE.vwGeneralLeadInfomations.Where(x => x.BBLE == propertybbl).FirstOrDefault());
            }
        }
    }
}