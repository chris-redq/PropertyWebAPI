using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PropertyWebAPI
{
    /// <summary>
    /// Types of Request Status 
    /// </summary>
    public enum RequestStatus
    {
        /// <summary>
        /// For requests completed successfully
        /// </summary>
        Success = 0,

        /// <summary>
        /// For request that end up in error
        /// </summary>
        Error = 1,

        /// <summary>
        /// For requesting waiting to be processed
        /// </summary>
        Pending = 2,

        /// <summary>
        /// For Requests send for Processing
        /// </summary>
        SendForProcessing = 3
    }

    /// <summary>
    /// Types of Requests
    /// </summary>
    public enum RequestTypes
    {
        /// <summary>
        /// Request for Tax bill
        /// </summary>
        NYCTaxBill = 1001,

        /// <summary>
        /// Request for DOB total Penalties and Violations amount due
        /// </summary>
        NYCDOBPenaltiesAndViolations = 1002,

        /// <summary>
        /// Request for name of the mortgage servicer
        /// </summary>
        NYCMortgageServicer = 1003,

        /// <summary>
        /// Request for zEstimate value
        /// </summary>
        Zillow = 1004,

        /// <summary>
        /// Request for Water bill
        /// </summary>
        NYCWaterBill = 1005

    }

}