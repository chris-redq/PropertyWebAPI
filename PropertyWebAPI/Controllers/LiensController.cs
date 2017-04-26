//-----------------------------------------------------------------------
// <copyright file="LiensController.cs" company="Redq Technologies, Inc.">
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
    using AutoMapper;

    public class LiensController : ApiController
    {
        /// <summary>  
        ///     Use this api to get all liens associated with a property
        /// </summary>  
        /// <param name="propertyBBL">
        ///     Borough Block Lot Number. The first character is a number 1-5 followed by 0 padded 5 digit block number followed by 0 padded 4 digit lot number
        /// </param>  
        /// <returns>Returns all lien details</returns>
        [Route("api/liens/{propertyBBL}")]
        [ResponseType(typeof(DAL.AllLiens))]
        public IHttpActionResult GetPropertyLiens(string propertyBBL)
        {
            if (!BAL.BBL.IsValidFormat(propertyBBL))
                return this.BadRequest("Incorrect BBL - Borough Block Lot number");

            try
            {
                var liens = DAL.Liens.GetAllLiens(propertyBBL);

                if (liens == null)
                    return NotFound();
                return Ok(liens);
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Exception encountered while retrieving Lead for {0}{1}", propertyBBL, Common.Logs.FormatException(e)));
                return Common.HttpResponse.InternalError(Request, "Internal Error in processing request");
            }
        }
    }
}
