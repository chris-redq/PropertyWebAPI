//-----------------------------------------------------------------------
// <copyright file="NYCPhysicalPropertyData.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------
namespace PropertyWebAPI.BAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web.Http;
    using System.Web.Http.Description;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NYCDOF;
    using AutoMapper;
    using RequestResponseBuilder;

    #region Local Helper Classes
    /// <summary>  
    ///     Simple Address Class
    /// </summary>  
    public class GeneralAddress
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string addressLine1;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string addressLine2;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string city;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string state;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string zip;

        private static bool IsEmpty(string str)
        {
            if ((str == null) || (str.Length == 0))
                return true;
            return false;
        }

        public override string ToString()
        {
            string address = (addressLine1 == null ? "" : addressLine1);
            address = address + (address.Length > 0 && !IsEmpty(addressLine2) ? ", " : "") + (addressLine2 == null ? "" : addressLine2);
            address = address + (address.Length > 0 && !IsEmpty(city) ? ", " : "") + (city == null ? "" : city);
            address = address + (address.Length > 0 && !IsEmpty(state) ? ", " : "") + (state == null ? "" : state);
            address = address + (address.Length > 0 && !IsEmpty(zip) ? " " : "") + (zip == null ? "" : zip);
            return address;
        }
    }

    public class PhysicalPropertyInformation : tfnGetGeneralPropertyInformation_Result
    {
        //blank class used for renaming purposes
    }

    /// <summary>  
    ///     Helper class to return app_id and app_key fro GeoClient Api
    /// </summary>  
    public class GeneralPropertyInformation
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public GeneralAddress address;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<BAL.DeedParty> owners;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PhysicalPropertyInformation propertyInformation;
        public string errors;
    }
    #endregion

    public class NYCPhysicalPropertyData
    {
        /// <summary>  
        ///     Helper class to return app_id and app_key fro GeoClient Api
        /// </summary>  
        private class AppTokens
        {
            public string appId;
            public string appKey;
        }

        private static AppTokens GetConfigurationTokens()
        {
            AppTokens appTokens = new AppTokens();

            appTokens.appId = AppSettings.Get(AppSettings.GEO_CLIENT_APP_ID);
            appTokens.appKey = AppSettings.Get(AppSettings.GEO_CLIENT_APP_KEY);

            return appTokens;
        }

        /// <summary>  
        ///     Method returns address corrections and details based on street number, street address and borough for NYC properties
        /// </summary>  
        public static JObject GetAddressDetailsFromGeoClientAPI(string streetNumber, string streetName, string borough)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.cityofnewyork.us/");
            AppTokens appTokens = GetConfigurationTokens();

            try
            {

                HttpResponseMessage response = client.GetAsync("geoclient/v1/address.json?houseNumber=" + streetNumber + "&street=" + streetName + "&borough=" + borough
                                                            + "&app_id=" + appTokens.appId + "&app_key=" + appTokens.appKey).Result;

                return JObject.Parse(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Geoclient API call failed{0}", Common.Logs.FormatException(e)));
                return null;
            }
        }

        /// <summary>  
        ///     Method returns details based on BBL in NYC
        /// </summary>  
        public static JObject GetBBLDetailsFromGeoClientAPI(string propertyBBL)
        {

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.cityofnewyork.us/");
            AppTokens appTokens = GetConfigurationTokens();

            string borough = BAL.BBL.GetBoroughName(propertyBBL);

            HttpResponseMessage response = client.GetAsync("geoclient/v1/bbl.json?borough=" + borough + "&block=" + propertyBBL.Substring(1, 5)
                                                           + "&lot=" + propertyBBL.Substring(6, 4) + "&app_id=" + appTokens.appId + "&app_key=" + appTokens.appKey).Result;

            return JObject.Parse(response.Content.ReadAsStringAsync().Result);
        }
        
        /// <summary>  
        ///     Method returns building information based on the BIN in NYC
        /// </summary>  
        public static JObject GetBuildingDetailsFromGeoClientAPI(string buildingIdentificationNumber)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.cityofnewyork.us/");
            AppTokens appTokens = GetConfigurationTokens();

            HttpResponseMessage response = client.GetAsync("geoclient/v1/bin.json?bin=" + buildingIdentificationNumber
                                                            + "&app_id=" + appTokens.appId + "&app_key=" + appTokens.appKey).Result;

            return JObject.Parse(response.Content.ReadAsStringAsync().Result);
        }

        /// <summary>  
        ///     Method returns true if any of the messages in JSON contain NOT FOUND
        /// </summary>  
        public static bool CheckIfMessageContainsNotFound(JObject jsonObj, string baseObjectName)
        {
            string msg = (string)jsonObj.SelectToken(baseObjectName + ".message");
            if (msg != null && (msg.Contains("NOT FOUND") || msg.Contains("OUT OF RANGE")))
                return true;
            msg = (string)jsonObj.SelectToken(baseObjectName + ".message1");
            if (msg != null && (msg.Contains("NOT FOUND") || msg.Contains("OUT OF RANGE")))
                return true;
            msg = (string)jsonObj.SelectToken(baseObjectName + ".message2");
            if (msg != null && (msg.Contains("NOT FOUND") || msg.Contains("OUT OF RANGE")))
                return true;
            return false;
        }

        /// <summary>
        ///     Returns Physical Data about a property identified by BBL
        /// </summary>
        /// <param name="propertyBBL"></param>
        /// <param name="addresscleanup"></param>
        /// <returns></returns>
        public static GeneralPropertyInformation Get(string propertyBBL, bool addresscleanup)
        {
            try
            {
                using (NYCDOFEntities dofDBEntities = new NYCDOFEntities())
                {
                    //Find BBL in Assessment Table
                    List<tfnGetGeneralPropertyInformation_Result> propertyInfo = dofDBEntities.tfnGetGeneralPropertyInformation(propertyBBL).ToList();
                    if (propertyInfo == null || propertyInfo.Count <= 0)
                    {
                        //BBL not in Assessment Table check ACRIS
                        BAL.PropertyLotInformation lotObj = BAL.ACRIS.GetLotInformation(propertyBBL);
                        if (lotObj != null)
                        {
                            propertyInfo.Add(new tfnGetGeneralPropertyInformation_Result());
                            propertyInfo[0].BBLE = propertyBBL;
                            propertyInfo[0].Borough = BAL.BBL.GetBoroughName(propertyBBL);
                            propertyInfo[0].Block = BAL.BBL.GetBlock(propertyBBL);
                            propertyInfo[0].Lot = BAL.BBL.GetLot(propertyBBL);

                            propertyInfo[0].StreetName = lotObj.StreetName;
                            propertyInfo[0].StreetNumber = lotObj.StreetNumber;
                            propertyInfo[0].UnitNumber = lotObj.UnitNumber;
                        }
                    }

                    if (propertyInfo == null || propertyInfo.Count <= 0)
                        return null;    //BBL not found 

                    GeneralAddress address = null;
                    GeneralPropertyInformation propertyDetails = new GeneralPropertyInformation();

                    //Clean address using GeoClinet API
                    JObject jsonObj = null;
                    if (addresscleanup)
                        jsonObj = GetAddressDetailsFromGeoClientAPI(propertyInfo[0].StreetNumber, propertyInfo[0].StreetName, propertyInfo[0].Borough);

                    if (jsonObj != null && !CheckIfMessageContainsNotFound(jsonObj, "address"))
                    {
                        address = new GeneralAddress();
                        address.addressLine1 = propertyInfo[0].StreetNumber + " " + StringUtilities.ToTitleCase(propertyInfo[0].StreetName);
                        if (propertyInfo[0].UnitNumber != null)
                            address.addressLine2 = "Unit #" + propertyInfo[0].UnitNumber;
                        address.city = StringUtilities.ToTitleCase((string)jsonObj.SelectToken("address.uspsPreferredCityName"));
                        address.state = "NY";
                        address.zip = (string)jsonObj.SelectToken("address.zipCode");
                    }
                    else
                    {   // Use the address from Assessment or ACRIS
                        address = new GeneralAddress();
                        address.addressLine1 = propertyInfo[0].StreetNumber + " " + StringUtilities.ToTitleCase(propertyInfo[0].StreetName);
                        if (propertyInfo[0].UnitNumber != null)
                            address.addressLine2 = "Unit #" + propertyInfo[0].UnitNumber;
                        address.city = StringUtilities.ToTitleCase(BAL.BBL.GetBoroughName(propertyBBL));
                        address.state = "NY";
                        if (propertyInfo[0].ZipCode != null)
                            address.zip = propertyInfo[0].ZipCode;
                    }
                    propertyDetails.address = address;
                    propertyDetails.propertyInformation = Mapper.Map<PhysicalPropertyInformation>(propertyInfo[0]);

                    BAL.DeedDetails deedDetailsObj = BAL.ACRIS.GetLatestDeedDetails(propertyBBL);
                    if (deedDetailsObj != null)
                        propertyDetails.owners = deedDetailsObj.owners;

                    return propertyDetails;
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Error reading data from NYCDOF{0}", Common.Logs.FormatException(e)));
                return null;
            }
        }

        public static GeneralPropertyInformation Get(string streetNumber, string streetName, string borough)
        {
            //BBL from address 
            JObject jsonObj = GetAddressDetailsFromGeoClientAPI(streetNumber, streetName, borough);

            if (jsonObj == null)
                return null;

            if (CheckIfMessageContainsNotFound(jsonObj, "address"))
                return null;

            GeneralPropertyInformation propertyDetails = new GeneralPropertyInformation();
            try
            {
                using (NYCDOFEntities dofDBEntities = new NYCDOFEntities())
                {
                    //Get General Property Information from Assessment and other sources
                    List<tfnGetGeneralPropertyInformation_Result> propertyInfo = dofDBEntities.tfnGetGeneralPropertyInformation((string)jsonObj.SelectToken("address.bbl")).ToList();

                    if (propertyInfo != null && propertyInfo.Count > 0)
                    {
                        propertyDetails.propertyInformation = Mapper.Map<PhysicalPropertyInformation>(propertyInfo[0]);
                        //Create a clean address
                        GeneralAddress address = new GeneralAddress();
                        address.addressLine1 = propertyInfo[0].StreetNumber + " " + StringUtilities.ToTitleCase(propertyInfo[0].StreetName);
                        if (propertyInfo[0].UnitNumber != null)
                            address.addressLine2 = "Unit #" + propertyInfo[0].UnitNumber;
                        address.city = StringUtilities.ToTitleCase((string)jsonObj.SelectToken("address.uspsPreferredCityName"));
                        address.state = "NY";
                        address.zip = (string)jsonObj.SelectToken("address.zipCode");

                        propertyDetails.address = address;
                    }
                    else
                        Common.Logs.log().Error(string.Format("Error finding record for BBLE {0} in Assessment", (string)jsonObj.SelectToken("address.bbl")));
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Error reading data from NYCDOF{0}", Common.Logs.FormatException(e)));
                return null;
            }

            if (BAL.BBL.GetLot((string)jsonObj.SelectToken("address.bbl")) <= 6999)
            {
                BAL.DeedDetails deedDetailsObj = BAL.ACRIS.GetLatestDeedDetails((string)jsonObj.SelectToken("address.bbl"));
                if (deedDetailsObj != null)
                {
                    propertyDetails.owners = deedDetailsObj.owners;
                }
            }
            else
                propertyDetails.errors = string.Format("Cannot get ownership information for Lot number {0}", BAL.BBL.GetLot((string)jsonObj.SelectToken("address.bbl")));

            return propertyDetails;
        }
    }
}
    