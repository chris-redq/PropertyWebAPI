using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ACRISDB;
using System.Web.Http.Description;

namespace PropertyWebAPI.Controllers
{
    public class MortgagesDeedsController : ApiController
    {

        //../api/MortgagesDeeds/7326457

        [ResponseType(typeof(vwDocumentsByBBLE))]
        public IHttpActionResult Get(string BBLE)
        {
            
            if (BBLE.Length == 10 || BBLE.Length == 11)
            {
                using (ACRISEntities ACRISE = new ACRISEntities())
                {
                    List<vwDocumentsByBBLE> documentsObj = ACRISE.vwDocumentsByBBLEs
                                                                .Where(i => i.BBLE == BBLE)
                                                                .OrderByDescending(m => m.DocumentDate).ToList<vwDocumentsByBBLE>();
                    if (documentsObj != null)
                        if (documentsObj.Count > 0 )
                            return Ok(documentsObj);
                        else
                            return NotFound();
                }
            }
            return NotFound();
        }
    }
}
