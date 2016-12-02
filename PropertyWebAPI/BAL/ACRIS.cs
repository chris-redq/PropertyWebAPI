//-----------------------------------------------------------------------
// <copyright file="ACRIS.cs" company="Redq Technologies, Inc.">
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
    using ACRISDB;

    public class DeedParty:tfnGetDocumentParties_Result
    {
        //Blank classes to mask entityframework details
    }

    public class DeedDetails
    {
        public LatestDeedDocument deedDocument;
        public List<DeedParty> owners;
    }

    public class ACRIS
    {
        public static DeedDetails GetLatestDeedDetails(string propertyBBLE)
        {
            using (ACRISEntities acrisDBEntities = new ACRISEntities())
            {
                LatestDeedDocument documentObj = acrisDBEntities.LatestDeedDocuments.FirstOrDefault(i => i.BBLE == propertyBBLE);

                if (documentObj == null)
                    return null;

                DeedDetails deedDetailsObj = new DeedDetails();
                deedDetailsObj.deedDocument = documentObj;
                foreach (tfnGetDocumentParties_Result a in acrisDBEntities.tfnGetDocumentParties(documentObj.DeedUniqueKey, "BUYER").ToList())
                    deedDetailsObj.owners.Add((DeedParty)a);

                return deedDetailsObj;
            }
        }
    }
}