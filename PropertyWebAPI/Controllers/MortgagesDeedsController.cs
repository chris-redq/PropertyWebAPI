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

    public class DeedDetails
    {
        public LatestDeedDocument DeedDocument;
        public List<tfnGetDocumentParties_Result> Owners;
    }

    /// <summary>  
    ///     This controller handles all ACRIS related requests for Mortgage and Deed related documents
    /// </summary>
    [Authorize]
    public class MortgagesDeedsController : ApiController
    {

        // ../api/MortgagesDeeds/3001670091
        /// <summary>  
        ///     Use this method to retrieve all Mortgage and Deed related documents for a property in NYC from the ACRIS system, in a reverse chronological order by recording date (document date is sometimes NULL). 
        /// </summary>  
        /// <param name="propertyBBLE">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, followed by 0 padded 5 digit block number, 
        ///     followed by 0 padded 4 digit lot number and finally ending with optional alpha character indicating the easement associated with the property</param>  
        /// <returns>
        ///     Returns a list of all Mortagage and Deed related documents filed with the ACRIS system for a given property identified by a BBLE - Borough Block Lot and Easement Number.
        /// </returns>
        [Route("api/mortgagesdeeds/{propertyBBLE}/documents")]
        [ResponseType(typeof(vwDocumentsByBBLE))]
        public IHttpActionResult Get(string propertyBBLE)
        {
            
            if (Regex.IsMatch(propertyBBLE, "^[1-5][0-9]{9}[A-Z]??$"))
            {
                using (ACRISEntities acrisDBEntities = new ACRISEntities())
                {
                    List<vwDocumentsByBBLE> documentsObj = acrisDBEntities.vwDocumentsByBBLEs
                                                                          .Where(i => i.BBLE == propertyBBLE)
                                                                          .OrderByDescending(m => m.DateRecorded).ToList();
                    if (documentsObj != null)
                    {
                        if (documentsObj.Count > 0)
                        {
                            return Ok(documentsObj);
                        }

                        return NotFound();
                    }
                }
            }

            return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");
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
        [ResponseType(typeof(vwDocumentsByBBLE))]
        public IHttpActionResult GetAllDeeds(string propertyBBLE)
        {

            if (Regex.IsMatch(propertyBBLE, "^[1-5][0-9]{9}[A-Z]??$"))
            {
                using (ACRISEntities acrisDBEntities = new ACRISEntities())
                {
                    List<vwDocumentsByBBLE> documentsObj = acrisDBEntities.vwDocumentsByBBLEs
                                                                          .Where(i => i.BBLE == propertyBBLE && (i.DocumentType=="DEED" || i.DocumentType=="DEEDO"))
                                                                          .OrderByDescending(m => m.DocumentDate).ToList<vwDocumentsByBBLE>();
                    if (documentsObj != null)
                    {
                        if (documentsObj.Count > 0)
                        {
                            return Ok(documentsObj);
                        }

                        return NotFound();
                    }
                }
            }

            return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");
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
        [ResponseType(typeof(DeedDetails))]
        public IHttpActionResult GetLatestDeedDetails(string propertyBBLE)
        {

            if (Regex.IsMatch(propertyBBLE, "^[1-5][0-9]{9}[A-Z]??$"))
            {
                using (ACRISEntities acrisDBEntities = new ACRISEntities())
                {
                    LatestDeedDocument documentObj =  acrisDBEntities.LatestDeedDocuments.FirstOrDefault(i => i.BBLE == propertyBBLE);

                    if (documentObj == null)
                        return NotFound();

                    DeedDetails deedDetailsObj = new DeedDetails();
                    deedDetailsObj.DeedDocument = documentObj;
                    deedDetailsObj.Owners = acrisDBEntities.tfnGetDocumentParties(documentObj.DeedUniqueKey, "BUYER").ToList();

                    return Ok(deedDetailsObj);
                }
            }

            return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");
        }

        // ../api/MortgagesDeeds/3001670091/unsatisfiedmortgages
        /// <summary>  
        ///     Use this method to retrieve all unsatsified mortgages for a property
        /// </summary>  
        /// <param name="propertyBBLE">
        ///     Borough Block Lot and Easement Number. The first character is a number between 1-5 indicating the borough associated with the property, followed by 0 padded 5 digit block number, 
        ///     followed by 0 padded 4 digit lot number and finally ending with optional alpha character indicating the easement associated with the property</param>  
        /// <returns>
        ///     Returns unsatisfied mortgages in the ACRIS system for a given property identified by a BBLE - Borough Block Lot and Easement Number.
        /// </returns>
        [Route("api/mortgagesdeeds/{propertyBBLE}/unsatisfiedMortgages")]
        [ResponseType(typeof(tfnGetUnsatisfiedMortgages_Result))]
        public IHttpActionResult GetUnsatisfiedMortgages(string propertyBBLE)
        {

            if (Regex.IsMatch(propertyBBLE, "^[1-5][0-9]{9}[A-Z]??$"))
            {
                using (ACRISEntities acrisDBEntities = new ACRISEntities())
                {
                    List<tfnGetUnsatisfiedMortgages_Result> mortgagesList = acrisDBEntities.tfnGetUnsatisfiedMortgages(propertyBBLE).ToList();

                    if (mortgagesList != null)
                    {
                        if (mortgagesList.Count > 0)
                        {
                            return Ok(mortgagesList);
                        }

                        return NotFound();
                    }
                }
            }

            return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");
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
        ///     Returns a list of all all Contract of Sales as well as Memorandum of Contracts filed with the ACRIS system for a given property identified by a 
        ///     BBLE - Borough Block Lot and Easement Number.
        /// </returns>
        [Route("api/mortgagesdeeds/{propertyBBLE}/contractsofsale")]
        [ResponseType(typeof(vwDocumentsByBBLE))]
        public IHttpActionResult GetAllContractsOfSaleAndMemorandumsOfContract(string propertyBBLE)
        {

            if (Regex.IsMatch(propertyBBLE, "^[1-5][0-9]{9}[A-Z]??$"))
            {
                using (ACRISEntities acrisDBEntities = new ACRISEntities())
                {
                    List<vwDocumentsByBBLE> documentsObj = acrisDBEntities.vwDocumentsByBBLEs
                                                                          .Where(i => i.BBLE == propertyBBLE && (i.DocumentType == "CNTR" || i.DocumentType == "MCON"))
                                                                          .OrderByDescending(m => m.DocumentDate).ToList<vwDocumentsByBBLE>();
                    if (documentsObj != null)
                    {
                        if (documentsObj.Count > 0)
                        {
                            return Ok(documentsObj);
                        }

                        return NotFound();
                    }
                }
            }

            return BadRequest("Incorrect BBLE - Borough Block Lot & Easement number");
        }
    }
}

