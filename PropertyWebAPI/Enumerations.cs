using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PropertyWebAPI
{
    /// <summary>Types of CMAs</summary>
    public enum CMAType
    {
        Regular = 1,

        ShortSale = 2,

        Rehab = 3
            
    }

    /// <summary>Types of CMA Runs</summary>
    public enum CMARunType
    {
        /// <summary>Run automatically on regular basis</summary>
        BatchedRun = 0,

        /// <summary>Automate CMA run at a users request</summary>
        OnDemand = 1,

        /// <summary>CMA computed manually with user input</summary>
        Manual = 2
    }

    /// <summary>Types of Request Status</summary>
    public enum RequestStatus
    {
        /// <summary>For requests completed successfully</summary>
        Success = 0,

        /// <summary>For request that end up in error</summary>
        Error = 1,

        /// <summary>For requesting waiting to be processed</summary>
        Pending = 2,

        /// <summary>For Requests send for Processing</summary>
        SendForProcessing = 3
    }

    /// <summary>Types of Requests</summary>
    public enum RequestTypes
    {
        /// <summary>Request for Tax bill</summary>
        NYCTaxBill = 1001,

        /// <summary>Request for DOB total Penalties and Violations amount due</summary>
        NYCDOBPenaltiesAndViolations = 1002,

        /// <summary>Request for name of the mortgage servicer</summary>
        NYCMortgageServicer = 1003,

        /// <summary>Request for zEstimate value</summary>
        Zillow = 1004,

        /// <summary>Request for Water bill</summary>
        NYCWaterBill = 1007,

        /// <summary>Request for Notice Of Property Value</summary>
        NYCNoticeOfPropertyValue = 1006,

        /// <summary>Request for Mortgage Document Details</summary>
        NYCMortgageDocumentDetails = 1005,

        /// <summary>Request for Freddie Mortgage Check</summary>
        FreddieMortgage = 1008,

        /// <summary>Request for Fannie Mortgage Check</summary>
        FannieMortgage = 1009
    }

}