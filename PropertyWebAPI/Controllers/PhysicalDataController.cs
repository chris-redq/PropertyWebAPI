//-----------------------------------------------------------------------
// <copyright file="CasesController.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------
namespace PropertyWebAPI.Controllers
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

    #region Local Helper Classes
    /// <summary>  
    ///     Helper class to return app_id and app_key fro GeoClient Api
    /// </summary>  
    class AppTokens
    {
        public string appId;
        public string appKey;
    }

    /// <summary>  
    ///     Simple Address Class
    /// </summary>  
    class GeneralAddress
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
    }

    class PhysicalPropertyInformation : tfnGetGeneralPropertyInformation_Result
    {

    }

    /// <summary>  
    ///     Helper class to return app_id and app_key fro GeoClient Api
    /// </summary>  
    class GeneralPropertyInformation
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public GeneralAddress address;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<BAL.DeedParty> owners;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PhysicalPropertyInformation propertyInformation;
    }

    #endregion

    /// <summary>  
    /// This controller handles all api requests associated with phyical data associated with a property
    /// </summary>  
    [Authorize]
    public class PhysicalDataController : ApiController
    {
        #region Private Helper Methods

        private AppTokens GetConfigurationTokens()
        {
            AppTokens appTokens= new AppTokens();

            appTokens.appId = AppSettings.Get(AppSettings.GEO_CLIENT_APP_ID);
            appTokens.appKey = AppSettings.Get(AppSettings.GEO_CLIENT_APP_KEY);

            return appTokens;
        }

        /// <summary>  
        ///     Method returns address corrections and details based on street number, street address and borough for NYC properties
        /// </summary>  
        private JObject GetAddressDetailsFromGeoClientAPI(string streetNumber, string streetName, string borough)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.cityofnewyork.us/");
            AppTokens appTokens = GetConfigurationTokens();

            HttpResponseMessage response = client.GetAsync("geoclient/v1/address.json?houseNumber=" + streetNumber + "&street=" + streetName + "&borough=" + borough 
                                                            + "&app_id="+appTokens.appId+"&app_key="+appTokens.appKey).Result;
            
            return JObject.Parse(response.Content.ReadAsStringAsync().Result);
        }

        /// <summary>  
        ///     Method returns details based on BBL in NYC
        /// </summary>  
        private JObject GetBBLDetailsFromGeoClientAPI(string propertyBBL)
        {
          
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.cityofnewyork.us/");
            AppTokens appTokens = GetConfigurationTokens();

            string borough = "";
            switch(propertyBBL.Substring(0,1))
            {
                case "1":
                    borough = "Manhattan";
                    break;
                case "2":
                    borough = "Bronx";
                    break;
                case "3":
                    borough = "Brooklyn";
                    break;
                case "4":
                    borough = "Queens";
                    break;
                case "5":
                    borough = "Staten Island";
                    break;
            }

            HttpResponseMessage response = client.GetAsync("geoclient/v1/bbl.json?borough=" + borough + "&block=" + propertyBBL.Substring(1, 5) 
                                                           + "&lot=" + propertyBBL.Substring(6, 4) + "&app_id=" + appTokens.appId + "&app_key=" + appTokens.appKey).Result;

            return JObject.Parse(response.Content.ReadAsStringAsync().Result);
        }

        
        /// <summary>  
        ///     Method returns building information based on the BIN in NYC
        /// </summary>  
        private JObject GetBuildingDetailsFromGeoClientAPI(string buildingIdentificationNumber)
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
        private Boolean CheckIfMessageContainsNotFound(JObject jsonObj, string baseObjectName)
        {
            string msg = (string)jsonObj.SelectToken(baseObjectName + ".message");
            if (msg != null && msg.Contains("NOT FOUND"))
                return true;
            msg = (string)jsonObj.SelectToken(baseObjectName + ".message1");
            if (msg != null && msg.Contains("NOT FOUND"))
                return true;
            msg = (string)jsonObj.SelectToken(baseObjectName + ".message2");
            if (msg != null && msg.Contains("NOT FOUND"))
                return true;
            return false;
        }

        #endregion

        #region GeoClient APIs

        // ../api/physicaldata/nyc/11655/Queens Blvd/Queens/BBL
        /// <summary>  
        ///     Use this method to get Borough Block Lot Number associated with a property in NYC
        /// </summary>  
        /// <param name="streetNumber">
        ///     Street NUmber of teh property without or without hyphens
        /// </param>  
        /// <param name="streetName">
        ///     Name of teh street where property is located 
        /// </param>
        /// <param name="borough">
        ///     Borough in which property is located. Valid values are Manhattan, Bronx, Brooklyn, Queens and Staten Island
        /// </param>
        /// <returns>
        ///     Returns BBL of the Property
        /// </returns>
        [Route("api/physicaldata/nyc/{streetNumber}/{streetName}/{borough}/BBL")]
        [ResponseType(typeof(string))]
        public IHttpActionResult GetNYCBBL(string streetNumber, string streetName, string borough)
        {
            JObject jsonObj = GetAddressDetailsFromGeoClientAPI(streetNumber, streetName, borough);

            string bbl = (string)jsonObj.SelectToken("address.bbl");

            if (bbl == null)
                return NotFound();
            return Ok(bbl);
        }

        // ../api/physicaldata/nyc/11655/Queens Blvd/Queens/
        /// <summary>  
        ///     Use this method to get Borough Block Lot NUmber associated with a property in NYC
        /// </summary>  
        /// <param name="streetNumber">
        ///     Street NUmber of teh property without or without hyphens
        /// </param>  
        /// <param name="streetName">
        ///     Name of teh street where property is located 
        /// </param>
        /// <param name="borough">
        ///     Borough in which property is located. Valid values are Manhattan, Bronx, Brooklyn, Queens and Staten Island
        /// </param>
        /// <returns>
        ///     Returns a Json Object. 
        ///     
        ///     Below is an example of where an exact address match was found (note that the attribute bbl is present but the address was corrected see message attribute)
        ///     {"address":{"assemblyDistrict":"27","bbl":"4022680023","bblBoroughCode":"4","bblTaxBlock":"02268","bblTaxLot":"0023","boardOfElectionsPreferredLgc":"1",
        ///                 "boePreferredStreetName":"QUEENS BOULEVARD","boePreferredstreetCode":"45999001","boroughCode1In":"4","buildingIdentificationNumber":"4052806",
        ///                 "censusBlock2000":"8002","censusBlock2010":"3002","censusTract1990":" 757  ","censusTract2000":" 757  ","censusTract2010":" 75701","cityCouncilDistrict":"29",
        ///                 "civilCourtDistrict":"04","coincidentSegmentCount":"1","communityDistrict":"406","communityDistrictBoroughCode":"4","communityDistrictNumber":"06",
        ///                 "communitySchoolDistrict":"28","condominiumBillingBbl":"0000000000","congressionalDistrict":"06","cooperativeIdNumber":"0000","cornerCode":"NE",
        ///                 "crossStreetNamesFlagIn":"E","dcpCommercialStudyArea":"42004","dcpPreferredLgc":"01","dotStreetLightContractorArea":"4","dynamicBlock":"902",
        ///                 "electionDistrict":"021","fireBattalion":"50","fireCompanyNumber":"151","fireCompanyType":"L","fireDivision":"13","firstBoroughName":"QUEENS",
        ///                 "firstStreetCode":"45999001010","firstStreetNameNormalized":"QUEENS BOULEVARD","fromLionNodeId":"0054662","fromPreferredLgcsFirstSetOf5":"01",
        ///                 "genericId":"0010202","geosupportFunctionCode":"1B","geosupportReturnCode":"00","geosupportReturnCode2":"01","gi5DigitStreetCode1":"59990",
        ///                 "gi5DigitStreetCode2":"16190","giBoroughCode1":"4","giBoroughCode2":"4","giBuildingIdentificationNumber1":"4052806","giBuildingIdentificationNumber2":"4052806",
        ///                 "giDcpPreferredLgc1":"01","giDcpPreferredLgc2":"01","giHighHouseNumber1":"116-65","giHighHouseNumber2":"112-51","giLowHouseNumber1":"116-29","giLowHouseNumber2":"112-51",
        ///                 "giSideOfStreetIndicator1":"L","giSideOfStreetIndicator2":"L","giStreetCode1":"45999001","giStreetCode2":"41619001","giStreetName1":"QUEENS BOULEVARD",
        ///                 "giStreetName2":"78 AVENUE","healthArea":"1920","healthCenterDistrict":"46","highBblOfThisBuildingsCondominiumUnits":"4022680023","highCrossStreetB5SC1":"416190",
        ///                 "highCrossStreetCode1":"41619001","highCrossStreetName1":"78 AVENUE","highHouseNumberOfBlockfaceSortFormat":"100117099AA","houseNumber":"116-55","houseNumberIn":"11655",
        ///                 "houseNumberSortFormat":"100116055AA","hurricaneEvacuationZone":"X","instructionalRegion":"QN","interimAssistanceEligibilityIndicator":"I","internalLabelXCoordinate":"1030749",
        ///                 "internalLabelYCoordinate":"0200078","latitude":40.715959972068973,"latitudeInternalLabel":40.715722393200821,"legacySegmentId":"0089770","lionBoroughCode":"4",
        ///                 "lionBoroughCodeForVanityAddress":"4","lionFaceCode":"3894","lionFaceCodeForVanityAddress":"3894","lionKey":"4389404160","lionKeyForVanityAddress":"4389404160",
        ///                 "lionSequenceNumber":"04160","lionSequenceNumberForVanityAddress":"04160","listOf4Lgcs":"01","longitude":-73.833316667910822,"longitudeInternalLabel":-73.832263927733138,
        ///                 "lowBblOfThisBuildingsCondominiumUnits":"4022680023","lowCrossStreetB5SC1":"416040","lowCrossStreetCode1":"41604001","lowCrossStreetName1":"77 AVENUE",
        ///                 "lowHouseNumberOfBlockfaceSortFormat":"100116001AA","lowHouseNumberOfDefiningAddressRange":"100116029AA","message2":"ADDR NUMBER ALTERED: HYPHEN INSERTED","nta":"QN17",
        ///                 "ntaName":"Forest Hills","numberOfCrossStreetB5SCsHighAddressEnd":"1","numberOfCrossStreetB5SCsLowAddressEnd":"1","numberOfCrossStreetsHighAddressEnd":"1",
        ///                 "numberOfCrossStreetsLowAddressEnd":"1","numberOfEntriesInListOfGeographicIdentifiers":"0002","numberOfExistingStructuresOnLot":"0001","numberOfStreetFrontagesOfLot":"02",
        ///                 "policePatrolBoroughCommand":"6","policePrecinct":"112","reasonCode1a":"2","reasonCode2":"2","returnCode1a":"01","returnCode1e":"00","roadwayType":"1","rpadBuildingClassificationCode":"K2",
        ///                 "rpadSelfCheckCodeForBbl":"3","sanbornBoroughCode":"4","sanbornPageNumber":"090","sanbornVolumeNumber":"19","sanitationCollectionSchedulingSectionAndSubsection":"1E",
        ///                 "sanitationDistrict":"406","sanitationRecyclingCollectionSchedule":"EW","sanitationRegularCollectionSchedule":"WS","sanitationSnowPriorityCode":"P","segmentAzimuth":"314",
        ///                 "segmentIdentifier":"0089770","segmentLengthInFeet":"00525","segmentOrientation":"4","segmentTypeCode":"G","sideOfStreetIndicator":"L","sideOfStreetOfVanityAddress":"L",
        ///                 "splitLowHouseNumber":"100116001AA","stateSenatorialDistrict":"14","streetName1In":"QUEENS BLVD","streetStatus":"2","taxMapNumberSectionAndVolume":"41204","toLionNodeId":"0034479",
        ///                 "toPreferredLgcsFirstSetOf5":"01","trafficDirection":"T","underlyingStreetCode":"45999001","uspsPreferredCityName":"FOREST HILLS","workAreaFormatIndicatorIn":"C",
        ///                 "xCoordinate":"1030457","xCoordinateHighAddressEnd":"1030758","xCoordinateLowAddressEnd":"1030387","xCoordinateOfCenterofCurvature":"0000000","yCoordinate":"0200164",
        ///                 "yCoordinateHighAddressEnd":"0199858","yCoordinateLowAddressEnd":"0200230","yCoordinateOfCenterofCurvature":"0000000","zipCode":"11375"}}
        /// 
        ///     Below is an example of where an exact address match was not found (note attributes message and message2). Also note that there is no
        ///     bbl attribute since the exact address was not located
        ///     {"address":{"assemblyDistrict":"28","boardOfElectionsPreferredLgc":"1","boePreferredStreetName":"QUEENS BOULEVARD","boePreferredstreetCode":"45999001",
        ///                 "boroughCode1In":"4","censusBlock2000":"1001","censusBlock2010":"1002","censusTract1990":" 76902","censusTract2000":" 76902",
        ///                 "censusTract2010":" 76902","cityCouncilDistrict":"29","civilCourtDistrict":"04","coincidentSegmentCount":"1","communityDistrict":"406",
        ///                 "communityDistrictBoroughCode":"4","communityDistrictNumber":"06","communitySchoolDistrict":"28","congressionalDistrict":"06",
        ///                 "crossStreetNamesFlagIn":"E","dcpPreferredLgc":"01","dotStreetLightContractorArea":"4","dynamicBlock":"112","electionDistrict":"020",
        ///                 "fireBattalion":"50","fireCompanyNumber":"305","fireCompanyType":"E","fireDivision":"13","firstBoroughName":"QUEENS","firstStreetCode":
        ///                 "45999001010","firstStreetNameNormalized":"QUEENS BOULEVARD","fromLionNodeId":"0054662","fromPreferredLgcsFirstSetOf5":"01","genericId":"0010202",
        ///                 "geosupportFunctionCode":"1B","geosupportReturnCode":"01","geosupportReturnCode2":"42","healthArea":"1920","healthCenterDistrict":"46",
        ///                 "highCrossStreetB5SC1":"416190","highCrossStreetCode1":"41619001","highCrossStreetName1":"78 AVENUE","highHouseNumberOfBlockfaceSortFormat":
        ///                 "100117098AA","houseNumber":"116-50","houseNumberIn":"11650","houseNumberSortFormat":"100116050AA","hurricaneEvacuationZone":"X",
        ///                 "instructionalRegion":"QN","interimAssistanceEligibilityIndicator":"I","latitude":40.715949019159176,"legacySegmentId":"0089770",
        ///                 "lionBoroughCode":"4","lionBoroughCodeForVanityAddress":"4","lionFaceCode":"3894","lionFaceCodeForVanityAddress":"3894","lionKey":"4389404160",
        ///                 "lionKeyForVanityAddress":"4389404160","lionSequenceNumber":"04160","lionSequenceNumberForVanityAddress":"04160","listOf4Lgcs":"01",
        ///                 "longitude":-73.833334731909687,"lowCrossStreetB5SC1":"416040","lowCrossStreetCode1":"41604001","lowCrossStreetName1":"77 AVENUE",
        ///                 "lowHouseNumberOfBlockfaceSortFormat":"100116000AA","message":"ADDR NUMBER ALTERED: HYPHEN INSERTED","message2":"ADDRESS NUMBER OUT OF RANGE",
        ///                 "nta":"QN17","ntaName":"Forest Hills","numberOfCrossStreetB5SCsHighAddressEnd":"1","numberOfCrossStreetB5SCsLowAddressEnd":"1",
        ///                 "numberOfCrossStreetsHighAddressEnd":"1","numberOfCrossStreetsLowAddressEnd":"1","numberOfStreetCodesAndNamesInList":"02","policePatrolBoroughCommand":"6",
        ///                 "policePrecinct":"112","reasonCode":"2","reasonCode1e":"2","returnCode1a":"42","returnCode1e":"01","roadwayType":"1",
        ///                 "sanitationCollectionSchedulingSectionAndSubsection":"2C","sanitationDistrict":"406","sanitationRecyclingCollectionSchedule":"ETH",
        ///                 "sanitationRegularCollectionSchedule":"MTH","sanitationSnowPriorityCode":"P","segmentAzimuth":"314","segmentIdentifier":"0089770","segmentLengthInFeet":"00525",
        ///                 "segmentOrientation":"4","segmentTypeCode":"G","sideOfStreetIndicator":"R","sideOfStreetOfVanityAddress":"R","splitLowHouseNumber":"100116000AA",
        ///                 "stateSenatorialDistrict":"15","streetCode1":"41604001","streetCode6":"41619001","streetName1":"77 AVENUE","streetName1In":"QUEENS BLVD","streetName6":"78 AVENUE",
        ///                 "streetStatus":"2","toLionNodeId":"0034479","toPreferredLgcsFirstSetOf5":"01","trafficDirection":"T","underlyingStreetCode":"45999001",
        ///                 "uspsPreferredCityName":"FOREST HILLS","workAreaFormatIndicatorIn":"C","xCoordinate":"1030452","xCoordinateHighAddressEnd":"1030758","xCoordinateLowAddressEnd":"1030387",
        ///                 "xCoordinateOfCenterofCurvature":"0000000","yCoordinate":"0200160","yCoordinateHighAddressEnd":"0199858","yCoordinateLowAddressEnd":"0200230",
        ///                 "yCoordinateOfCenterofCurvature":"0000000","zipCode":"11375"}}
        /// </returns>
        [Route("api/physicaldata/nyc/{streetNumber}/{streetName}/{borough}")]
        [ResponseType(typeof(JObject))]
        public IHttpActionResult GetNYCAddressDetails(string streetNumber, string streetName, string borough)
        {
            JObject jsonObj = GetAddressDetailsFromGeoClientAPI(streetNumber, streetName, borough);

            if (jsonObj == null)
                return NotFound();

            if (CheckIfMessageContainsNotFound(jsonObj, "address"))
                return NotFound();

            return Ok(jsonObj);
        }

        // ../api/physicaldata/nyc/4022680023
        /// <summary>  
        ///     Use this method to get property address and physical data associated with a property in NYC
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>   
        /// <returns>
        ///     Returns a Json Object. 
        ///     
        ///     Below is an example of return value
        ///     {"bbl":{"bbl":"2032560199","bblBoroughCode":"2","bblBoroughCodeIn":"2","bblTaxBlock":"03256","bblTaxBlockIn":"03256","bblTaxLot":"0199","bblTaxLotIn":"0199",
        ///             "buildingIdentificationNumber":"2015741","condominiumBillingBbl":"0000000000","cooperativeIdNumber":"0000","firstBoroughName":"BRONX","geosupportFunctionCode":"BL",
        ///             "geosupportReturnCode":"00","gi5DigitStreetCode1":"44020","giBoroughCode1":"2","giBuildingIdentificationNumber1":"2015741","giDcpPreferredLgc1":"01","giHighHouseNumber1":"3015",
        ///             "giLowHouseNumber1":"3015","giSideOfStreetIndicator1":"L","giStreetCode1":"24402001","giStreetName1":"KINGSBRIDGE TERRACE","highBblOfThisBuildingsCondominiumUnits":"2032560199",
        ///             "internalLabelXCoordinate":"1011291","internalLabelYCoordinate":"0258468","latitudeInternalLabel":40.876067820909981,"longitudeInternalLabel":-73.902219672060454,
        ///             "lowBblOfThisBuildingsCondominiumUnits":"2032560199","lowHouseNumberOfDefiningAddressRange":"003015000AA","modeSwitchIn":"X","numberOfEntriesInListOfGeographicIdentifiers":"0001",
        ///             "numberOfExistingStructuresOnLot":"0001","numberOfStreetFrontagesOfLot":"01","returnCode1a":"00","rpadBuildingClassificationCode":"B1","rpadSelfCheckCodeForBbl":"5",
        ///             "sanbornBoroughCode":"2","sanbornPageNumber":"016","sanbornVolumeNumber":"13","taxMapNumberSectionAndVolume":"21201","workAreaFormatIndicatorIn":"C"}}
        /// </returns>
        [Route("api/physicaldata/nyc/{propertyBBL}")]
        [ResponseType(typeof(JObject))]
        public IHttpActionResult GetNYCBBLDetails(string propertyBBL)
        {
            if (!Regex.IsMatch(propertyBBL, "^[1-5][0-9]{9}$"))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            JObject jsonObj = GetBBLDetailsFromGeoClientAPI(propertyBBL);

            if (jsonObj == null)
                return NotFound();

            if (CheckIfMessageContainsNotFound(jsonObj, "bbl"))
                return NotFound();
            return Ok(jsonObj);
        }

        // ../api/physicaldata/nyc/building/
        /// <summary>  
        ///     Use this method to get property address and physical data associated with a property in NYC
        /// </summary>  
        /// <param name="buildingIdentificationNumber">
        ///     BIN or Building Identification Number. 
        /// </param>   
        /// <returns>
        ///     Returns a Json Object. 
        ///     
        ///     Below is an example of return value
        ///     {"bin":{"bbl":"2032560199","bblBoroughCode":"2","bblTaxBlock":"03256","bblTaxLot":"0199","buildingIdentificationNumber":"2015741","buildingIdentificationNumberIn":"2015741",
        ///             "condominiumBillingBbl":"0000000000","cooperativeIdNumber":"0000","firstBoroughName":"BRONX","geosupportFunctionCode":"BN","geosupportReturnCode":"00","gi5DigitStreetCode1":"44020",
        ///             "giBoroughCode1":"2","giBuildingIdentificationNumber1":"2015741","giDcpPreferredLgc1":"01","giHighHouseNumber1":"3015","giLowHouseNumber1":"3015","giSideOfStreetIndicator1":"L",
        ///             "giStreetCode1":"24402001","giStreetName1":"KINGSBRIDGE TERRACE","highBblOfThisBuildingsCondominiumUnits":"2032560199","internalLabelXCoordinate":"1011291",
        ///             "internalLabelYCoordinate":"0258468","latitudeInternalLabel":40.876067820909981,"longitudeInternalLabel":-73.902219672060454,"lowBblOfThisBuildingsCondominiumUnits":"2032560199",
        ///             "lowHouseNumberOfDefiningAddressRange":"003015000AA","modeSwitchIn":"X","numberOfEntriesInListOfGeographicIdentifiers":"0001","numberOfExistingStructuresOnLot":"0001",
        ///             "numberOfStreetFrontagesOfLot":"01","returnCode1a":"00","rpadBuildingClassificationCode":"B1","rpadSelfCheckCodeForBbl":"5","sanbornBoroughCode":"2","sanbornPageNumber":"016",
        ///             "sanbornVolumeNumber":"13","taxMapNumberSectionAndVolume":"21201","workAreaFormatIndicatorIn":"C"}}
        /// </returns>
        [Route("api/physicaldata/nyc/building/{buildingIdentificationNumber}")]
        [ResponseType(typeof(JObject))]
        public IHttpActionResult GetNYCBIN(string buildingIdentificationNumber)
        {
            if (!Regex.IsMatch(buildingIdentificationNumber, "^[1-5][0-9]{6}$"))
                return this.BadRequest("Incorrect BIN - Building Identification Number");

            JObject jsonObj = GetBuildingDetailsFromGeoClientAPI(buildingIdentificationNumber);

            if (jsonObj == null)
                return NotFound();
            
            if (CheckIfMessageContainsNotFound(jsonObj, "bin"))
                return NotFound();
            return Ok(jsonObj);
        }
        #endregion

        #region General Property Information APIs
        // ../api/physicaldata/nyc/4022680023/GeneralInformation
        /// <summary>  
        ///     Use this method to get property address and physical data associated with a property in NYC
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>   
        /// <returns>
        ///     Returns an object of contain cleaned property address and general information about the property
        /// </returns>
        [Route("api/physicaldata/nyc/{propertyBBL}/GeneralInformation")]
        [ResponseType(typeof(GeneralPropertyInformation))]
        public IHttpActionResult GetNYCPropertyDetails(string propertyBBL)
        {
            if (!Regex.IsMatch(propertyBBL, "^[1-5][0-9]{9}$"))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            using (NYCDOFEntities dofDBEntities = new NYCDOFEntities())
            {
                List<tfnGetGeneralPropertyInformation_Result> propertyInfo = dofDBEntities.tfnGetGeneralPropertyInformation(propertyBBL).ToList();

                if (propertyInfo != null && propertyInfo.Count > 0)
                {
                    GeneralAddress address = null;
                    GeneralPropertyInformation propertyDetails = new GeneralPropertyInformation();

                    JObject jsonObj = GetAddressDetailsFromGeoClientAPI(propertyInfo[0].StreetNumber, propertyInfo[0].StreetName, propertyInfo[0].Borough);
                    if (jsonObj != null && !CheckIfMessageContainsNotFound(jsonObj, "address"))
                    {
                        address = new GeneralAddress();
                        address.addressLine1 = propertyInfo[0].StreetNumber + " " + StringUtilities.ToTitleCase(propertyInfo[0].StreetName);
                        if (propertyInfo[0].UnitNumber!=null)
                            address.addressLine2 = "Unit #" + propertyInfo[0].UnitNumber;
                        address.city= StringUtilities.ToTitleCase((string)jsonObj.SelectToken("address.uspsPreferredCityName"));
                        address.state = "NY";
                        address.zip= (string)jsonObj.SelectToken("address.zipCode");
                    }
                    propertyDetails.address = address;
                    propertyDetails.propertyInformation = (PhysicalPropertyInformation)propertyInfo[0];

                    BAL.DeedDetails deedDetailsObj = BAL.ACRIS.GetLatestDeedDetails(propertyBBL);
                    if (deedDetailsObj != null)
                    {
                        propertyDetails.owners = deedDetailsObj.owners;
                    }

                    return Ok(propertyDetails);
                }
                return NotFound();
            }
          
        }

        // ../api/physicaldata/nyc/11655/Queens Blvd/Queens/GeneralInformation
        /// <summary>  
        ///     Use this method to get property address and physical data associated with a property in NYC
        /// </summary>  
        /// <param name="streetNumber">
        ///     Street NUmber of teh property without or without hyphens
        /// </param>  
        /// <param name="streetName">
        ///     Name of teh street where property is located 
        /// </param>
        /// <param name="borough">
        ///     Borough in which property is located. Valid values are Manhattan, Bronx, Brooklyn, Queens and Staten Island
        /// </param>   
        /// <returns>
        ///     Returns an object of contain cleaned property address and general information about the property
        /// </returns>
        [Route("api/physicaldata/nyc/{streetNumber}/{streetName}/{borough}/GeneralInformation")]
        [ResponseType(typeof(GeneralPropertyInformation))]
        public IHttpActionResult GetNYCPropertyDetails(string streetNumber, string streetName, string borough)
        {
            //BBL from address 
            JObject jsonObj = GetAddressDetailsFromGeoClientAPI(streetNumber, streetName, borough);

            if (jsonObj == null)
                return NotFound();

            if (CheckIfMessageContainsNotFound(jsonObj, "address"))
                return NotFound();

            using (NYCDOFEntities dofDBEntities = new NYCDOFEntities())
            {
                //Get General Propert Information from Assessemnt and other sources
                List<tfnGetGeneralPropertyInformation_Result> propertyInfo = dofDBEntities.tfnGetGeneralPropertyInformation((string)jsonObj.SelectToken("address.bbl")).ToList();

                //Create a clean address
                GeneralAddress address = new GeneralAddress();
                address.addressLine1 = propertyInfo[0].StreetNumber + " " + StringUtilities.ToTitleCase(propertyInfo[0].StreetName);
                if (propertyInfo[0].UnitNumber != null)
                    address.addressLine2 = "Unit #" + propertyInfo[0].UnitNumber;
                address.city = StringUtilities.ToTitleCase((string)jsonObj.SelectToken("address.uspsPreferredCityName"));
                address.state = "NY";
                address.zip = (string)jsonObj.SelectToken("address.zipCode");

                GeneralPropertyInformation propertyDetails = new GeneralPropertyInformation();
                propertyDetails.address = address;

                if (propertyInfo != null && propertyInfo.Count > 0)
                   propertyDetails.propertyInformation = (PhysicalPropertyInformation)propertyInfo[0];

                BAL.DeedDetails deedDetailsObj =  BAL.ACRIS.GetLatestDeedDetails((string)jsonObj.SelectToken("address.bbl"));
                if  (deedDetailsObj != null)
                {
                    propertyDetails.owners = deedDetailsObj.owners;
                }

                return Ok(propertyDetails);
            }
        }
        #endregion
    }
}
