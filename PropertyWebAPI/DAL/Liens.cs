//-----------------------------------------------------------------------
// <copyright file="Violations.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using NYCDOF;
    using AutoMapper;
    using Newtonsoft.Json;

    public class LocalTaxLienDetails: vwTaxLien
    {

    }

    public class LocalLiens
    {
        public LocalTaxLienDetails localTaxLienDetailData;
    }

    public class StateLiens
    {

    }

    public class FederalLiens
    {

    }

    public class OtherLiens
    {
        
    }

    public class AllLiens
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public LocalLiens localLienData;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public StateLiens stateLienData;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public FederalLiens federalLienData;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public OtherLiens otherLienData;
    }

    public class Liens
    {
        public static AllLiens GetAllLiens(string propertyBBL)
        {
            var result = GetNYCLien(propertyBBL);

            if (result == null)
                return null;

            AllLiens lienData = new AllLiens();

            lienData.localLienData = new LocalLiens();

            lienData.localLienData.localTaxLienDetailData = result;

            return lienData;
        }

        public static LocalTaxLienDetails GetNYCLien(string propertyBBL)
        {
            using (NYCDOFEntities nycdofE = new NYCDOFEntities())
            {
                return Mapper.Map<LocalTaxLienDetails>(nycdofE.vwTaxLiens.Where(i=> i.BBL==propertyBBL).FirstOrDefault());
            }
        }
    }
}