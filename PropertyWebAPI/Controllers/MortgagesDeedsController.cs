//-----------------------------------------------------------------------
// <copyright file="MortgagesDeedsController.cs" company="Redq Technologies, Inc.">
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
    using System.Web.Http;
    using System.Web.Http.Description;
    using System.Text.RegularExpressions;
    using ACRISDB;
    using AutoMapper;


    /// <summary>  
    ///     This controller handles all ACRIS related requests for Mortgage and Deed related documents
    /// </summary>
    [Authorize]
    public class MortgagesDeedsController : ApiController
    {

        // ../api/MortgagesDeeds/3001670091/documents
        /// <summary>  
        ///     Use this method to retrieve all Mortgage and Deed related documents for a property in NYC from the ACRIS system, in a reverse chronological order by recording date (document date is sometimes NULL). 
        /// </summary>  
        /// <param name="propertyBBLE">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, followed by 0 padded 5 digit block number, 
        ///     followed by 0 padded 4 digit lot number and finally ending with optional alpha character indicating the easement associated with the property</param>  
        /// <returns>
        ///     Returns a list of all Mortgage and Deed related documents filed with the ACRIS system for a given property identified by a BBLE - Borough Block Lot and Easement Number.
        /// </returns>
        [Route("api/mortgagesdeeds/{propertyBBLE}/documents")]
        [ResponseType(typeof(List<DAL.DocumentDetail>))]
        public IHttpActionResult Get(string propertyBBLE)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBLE))
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");

            try
            {
                var documentsObj = DAL.ACRIS.GetDocuments(propertyBBLE);
                                                                            
                 if (documentsObj == null || documentsObj.Count <= 0)
                     return NotFound();
                 return Ok(documentsObj);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Error reading AreaAbstract DB{0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        // ../api/MortgagesDeeds/3001670091/mortgagechain
        /// <summary>  
        ///     Use this method to retrieve mortgage chain for a property in NYC. 
        /// </summary>  
        /// <param name="propertyBBLE">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, followed by 0 padded 5 digit block number, 
        ///     followed by 0 padded 4 digit lot number and finally ending with optional alpha character indicating the easement associated with the property</param>  
        /// <returns>
        ///     Returns mortgage chain for a property in NYC.
        /// </returns>
        [Route("api/mortgagesdeeds/{propertyBBLE}/mortgagechain")]
        [ResponseType(typeof(List<DAL.MortgageDocumentDetail>))]
        public IHttpActionResult GetMortgageChain(string propertyBBLE)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBLE))
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");

            try
            {
                var documentsObj = BAL.ACRIS.GetMortgageChain(propertyBBLE);

                if (documentsObj == null || documentsObj.Count <= 0)
                    return NotFound();
                return Ok(documentsObj);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Error reading AreaAbstract DB{0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }




        // ../api/MortgagesDeeds/3001670091/deeds
        /// <summary>  
        ///     Use this method to retrieve all deeds associated with a property in NYC from the ACRIS system, in a reverse chronological order.
        ///     This does not include any Deed change documents
        /// </summary>  
        /// <param name="propertyBBLE">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, followed by 0 padded 5 digit block number, 
        ///     followed by 0 padded 4 digit lot number and finally ending with optional alpha character indicating the easement associated with the property</param>  
        /// <returns>
        ///     Returns a list of all deeds filed with the ACRIS system for a given property identified by a BBLE - Borough Block Lot and Easement Number.
        /// </returns>
        [Route("api/mortgagesdeeds/{propertyBBLE}/deeds")]
        [ResponseType(typeof(List<DAL.DocumentDetail>))]
        public IHttpActionResult GetAllDeeds(string propertyBBLE)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBLE))
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");

            try
            {
                var documentsObj = DAL.ACRIS.GetDeeds(propertyBBLE);

                if (documentsObj == null || documentsObj.Count <= 0)
                    return NotFound();
                return Ok(documentsObj);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Error reading AreaAbstract DB{0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        // ../api/MortgagesDeeds/3001670091/mortgageservicer
        /// <summary>  
        ///     Use this method to retrieve the current mortgage servicer for the property
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, followed by 0 padded 5 digit block number, 
        ///     followed by 0 padded 4 digit lot number and finally ending with optional alpha character indicating the easement associated with the property
        /// </param>  
        /// <param name="externalReferenceId"></param>
        /// <returns>
        ///     Returns the name of the Mortgage Servicer
        /// </returns>
        [Route("api/mortgagesdeeds/{propertyBBL}/mortgageservicer")]
        [ResponseType(typeof(BAL.MortgageServicerDetails))]
        public IHttpActionResult GetMortgageServicer(string propertyBBL, string externalReferenceId="")
        {
            if (!BAL.BBL.IsValidFormat(propertyBBL))
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");

            BAL.MortgageServicerDetails mortgageServicerObj = BAL.MortgageServicer.Get(propertyBBL, externalReferenceId);
            if (mortgageServicerObj == null)
                return NotFound();
            return Ok(mortgageServicerObj);
        }

        // ../api/MortgagesDeeds/3001670091/latestdeed
        /// <summary>  
        ///     Use this method to retrieve details about the latest deed for a property
        /// </summary>  
        /// <param name="propertyBBLE">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, followed by 0 padded 5 digit block number, 
        ///     followed by 0 padded 4 digit lot number and finally ending with optional alpha character indicating the easement associated with the property</param>  
        /// <returns>
        ///     Returns deed details and owners filed with the ACRIS system for a given property identified by a BBLE - Borough Block Lot and Easement Number.
        /// </returns>
        [Route("api/mortgagesdeeds/{propertyBBLE}/latestdeed")]
        [ResponseType(typeof(DAL.DeedDetails))]
        public IHttpActionResult GetLatestDeedDetails(string propertyBBLE)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBLE))
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");
            
            DAL.DeedDetails deedDetailsObj = BAL.ACRIS.GetLatestDeedDetails(propertyBBLE);
            if (deedDetailsObj==null)
                return NotFound();

            return Ok(deedDetailsObj);
        }

        // ../api/MortgagesDeeds/3001670091/unsatisfiedmortgages
        /// <summary>  
        ///     Use this method to retrieve all unsatisfied mortgages for a property
        /// </summary>  
        /// <param name="propertyBBLE">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, followed by 0 padded 5 digit block number, 
        ///     followed by 0 padded 4 digit lot number and finally ending with optional alpha character indicating the easement associated with the property</param>  
        /// <returns>
        ///     Returns unsatisfied mortgages in the ACRIS system for a given property identified by a BBLE - Borough Block Lot and Easement Number.
        /// </returns>
        [Route("api/mortgagesdeeds/{propertyBBLE}/unsatisfiedMortgages")]
        [ResponseType(typeof(List<DAL.DeedDocument>))]
        public IHttpActionResult GetUnsatisfiedMortgages(string propertyBBLE)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBLE))
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");

            try
            {
                using (ACRISEntities acrisDBEntities = new ACRISEntities())
                {
                    List<tfnGetUnsatisfiedMortgages_Result> mortgagesList = acrisDBEntities.tfnGetUnsatisfiedMortgages(propertyBBLE).ToList();

                    if (mortgagesList == null || mortgagesList.Count <= 0)
                        return NotFound();

                    return Ok(Mapper.Map<List<tfnGetUnsatisfiedMortgages_Result>,List<DAL.DeedDocument>>(mortgagesList));
                }
            }
            catch(Exception e)
            {
                Common.Logs.log().Error(string.Format("Error reading AreaAbstract DB{0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        // ../api/MortgagesDeeds/3001670091/contractsofsale
        /// <summary>  
        ///     Use this method to retrieve all Contract of Sales as well as Memorandum of Contracts associated with a property in NYC from the ACRIS system, 
        ///     in a reverse chronological order.
        /// </summary>  
        /// <param name="propertyBBLE">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, followed by 0 padded 5 digit block number, 
        ///     followed by 0 padded 4 digit lot number and finally ending with optional alpha character indicating the easement associated with the property</param>  
        /// <returns>
        ///     Returns a list of all Contract of Sales as well as Memorandum of Contracts filed with the ACRIS system for a given property identified by a 
        ///     BBLE - Borough Block Lot and Easement Number.
        /// </returns>
        [Route("api/mortgagesdeeds/{propertyBBLE}/contractsofsale")]
        [ResponseType(typeof(vwDocumentsByBBLE))]
        public IHttpActionResult GetAllContractsOfSaleAndMemorandumsOfContract(string propertyBBLE)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBLE))
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");

            try
            {
                using (ACRISEntities acrisDBEntities = new ACRISEntities())
                {
                    List<vwDocumentsByBBLE> documentsObj = acrisDBEntities.vwDocumentsByBBLEs
                                                                          .Where(i => i.BBLE == propertyBBLE && (i.DocumentType == "CNTR" || i.DocumentType == "MCON"))
                                                                          .OrderByDescending(m => m.DocumentDate).ToList<vwDocumentsByBBLE>();
                    if (documentsObj == null || documentsObj.Count <= 0)
                        return NotFound();
                    return Ok(documentsObj);
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Error reading AreaAbstract DB{0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        // ./api/MortgagesDeeds/3001670091/MortgageDocumentDetails
        /// <summary>  
        ///     Use this api to get mortgage document details for all unsatisfied mortgages associated with the property
        /// </summary>  
        /// <param name="propertyBBLE">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, followed by 0 padded 5 digit block number, 
        ///     followed by 0 padded 4 digit lot number and finally ending with optional alpha character indicating the easement associated with the property
        /// </param>  
        /// <param name="externalReferenceId">
        ///     The user of the API can provide their own reference number for a request. This reference number is sent back along with results to the caller when their request is furnished later asynchronously.
        /// </param>
        /// <returns>
        ///     Returns mortgage document details for all unsatisfied mortgages associated with the property
        /// </returns>
        [Route("api/mortgagesdeeds/{propertyBBLE}/UnsatisfiedMortgageDocumentDetails")]
        [ResponseType(typeof(BAL.MortgageDocumentResult))]
        public IHttpActionResult GetUnsatisfiedMortgageDocumentDetails(string propertyBBLE, string externalReferenceId = null)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBLE))
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");

            if (!BAL.BBL.IsValid(propertyBBLE))
                return BadRequest("BBLE - Borough Block Lot & Easement number, not found");

            try
            {
                var resultList = BAL.MortgageDocument.GetDetailsAllUnstaisfiedMortgages(propertyBBLE, externalReferenceId);

                if (resultList == null || resultList.Count <= 0)
                    return NotFound();

                return Ok(resultList);
            }
            catch(Exception e)
            {
                Common.Logs.log().Error(string.Format("Error reading AreaAbstract DB{0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        // ./api/MortgagesDeeds/3001670091/CheckFreddieMacMortgage
        /// <summary>  
        ///     Use this api to check if borrower associated with a property has a Freddie Mac mortgage
        /// </summary>  
        /// <param name="propertyBBLE">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, followed by 0 padded 5 digit block number, 
        ///     followed by 0 padded 4 digit lot number and finally ending with optional alpha character indicating the easement associated with the property
        /// </param>  
        /// <param name="externalReferenceId">
        ///     The user of the API can provide their own reference number for a request. This reference number is sent back along with results to the caller when their request is furnished later asynchronously.
        /// </param>
        /// <returns>
        ///     Returns true if a borrower associated with a property has a Freddie Mac mortgage 
        /// </returns>
        [Route("api/mortgagesdeeds/{propertyBBLE}/CheckFreddieMacMortgage")]
        [ResponseType(typeof(BAL.FreddieMortgageDetails))]
        [HttpPost]
        public IHttpActionResult CheckForFreddieMacMortgage(string propertyBBLE, string externalReferenceId = null)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBLE))
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");

            if (!BAL.BBL.IsValid(propertyBBLE))
                return BadRequest("BBLE - Borough Block Lot & Easement number, not found");

            try
            {
                BAL.Freddie.Parameters inParameters = new BAL.Freddie.Parameters();
                var resultObj = BAL.Freddie.Get(inParameters, externalReferenceId);

                if (resultObj == null)
                    return NotFound();

                return Ok(resultObj);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Error checking Freddie Mac Mortgage", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }

        // ./api/MortgagesDeeds/3001670091/CheckFannieMaeMortgage
        /// <summary>  
        ///     Use this api to check if borrower associated with a property has a Fannie Mae mortgage
        /// </summary>  
        /// <param name="propertyBBLE">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, followed by 0 padded 5 digit block number, 
        ///     followed by 0 padded 4 digit lot number and finally ending with optional alpha character indicating the easement associated with the property
        /// </param>  
        /// <param name="externalReferenceId">
        ///     The user of the API can provide their own reference number for a request. This reference number is sent back along with results to the caller when their request is furnished later asynchronously.
        /// </param>
        /// <returns>
        ///     Returns true if a borrower associated with a property has a Fannie Mae mortgage 
        /// </returns>
        [Route("api/mortgagesdeeds/{propertyBBLE}/CheckFannieMaeMortgage")]
        [ResponseType(typeof(BAL.FannieMortgageDetails))]
        [HttpPost]
        public IHttpActionResult CheckForFannieMaeMortgage(string propertyBBLE, string externalReferenceId = null)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBLE))
                return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");

            if (!BAL.BBL.IsValid(propertyBBLE))
                return BadRequest("BBLE - Borough Block Lot & Easement number, not found");

            try
            {
                BAL.Fannie.Parameters inParameters = new BAL.Fannie.Parameters();
                var resultObj = BAL.Fannie.Get(inParameters, externalReferenceId);

                if (resultObj == null)
                    return NotFound();

                return Ok(resultObj);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Error checking Fannie Mae Mortgage{0}", Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
    }
}
