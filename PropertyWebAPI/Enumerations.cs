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
        /// For requesting waiting to be processed
        /// </summary>
        Pending = 1,

        /// <summary>
        /// For request that end up in error
        /// </summary>
        Error = 2,

        /// <summary>
        /// For requests completed successfully
        /// </summary>
        Success = 3,

        /// <summary>
        /// For Requests send for Processing
        /// </summary>
        SendForProcessing = 4
    }

    /// <summary>
    /// Types of Requests
    /// </summary>
    public enum RequestTypes
    {
        /// <summary>
        /// Request for Water bill
        /// </summary>
        NYCWaterBill = 1,

        /// <summary>
        /// Request for Tax bill
        /// </summary>
        NYCTaxBill = 2,

        /// <summary>
        /// Request for DOB total Penalties and Violations amount due
        /// </summary>
        NYCDOBPenaltiesAndViolations = 3,

        /// <summary>
        /// Request for name of the mortgage servicer
        /// </summary>
        NYCMortgageServicer = 4,

        /// <summary>
        /// Request for zEstimate value
        /// </summary>
        Zillow = 5

    }

}