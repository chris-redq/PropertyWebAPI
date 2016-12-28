//-----------------------------------------------------------------------
// <copyright file="BaseResult.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI.BAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Newtonsoft.Json;

    /// <summary>
    /// Use the class to extend any result that needs to be send asynchronously
    /// </summary>
    public abstract class BaseResult
    {
        /// <summary>
        /// This is the unique ID assigned by the system for any unique request made by the API caller. If the user makes multiple requests 
        /// (in a short period or while there is a pending request) for the same data, the system only generates one ID and sends the 
        /// same ID back for the new request.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? requestId;
        /// <summary>
        /// This is the api callers reference number. This reference number can span across multiple calls and can be used to query the data services for status or results
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string externalReferenceId;
        /// <summary>
        /// There are three possible values for Status - Success, Pending and Error. 
        /// Success means the request was successfully process and the data available in the result returned
        /// Pending means the request it yet to be processed
        /// Error means the request resulted in an error and the no data is available.
        /// </summary>
        public string status;
    }

    /// <summary>
    /// Use the class to extend any result that needs to be send asynchronously for NYC Properties 
    /// </summary>
    public abstract class NYCBaseResult: BaseResult
    {
        /// <summary>
        /// Borough Block and Lot (BBL) of the property
        /// </summary>
        public string BBL;
    }
}