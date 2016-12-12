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
    using AutoMapper;

    public class DeedParty:tfnGetDocumentParties_Result
    {
        //
    }

    public class PropertyLotInformation : PropertyNotInAssessment
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
                {   if (deedDetailsObj.owners == null)
                        deedDetailsObj.owners = new List<DeedParty>();
                    deedDetailsObj.owners.Add(Mapper.Map<DeedParty>(a));
                }

                return deedDetailsObj;
            }
        }

        public static PropertyLotInformation GetLotInformation(string propertyBBLE)
        {
            using (ACRISEntities acrisDBEntities = new ACRISEntities())
            {
                PropertyNotInAssessment propertyLotInformationObj = acrisDBEntities.PropertyNotInAssessments.FirstOrDefault(i => i.BBL == propertyBBLE);

                if (propertyLotInformationObj == null)
                    return null;
                return Mapper.Map<PropertyLotInformation>(propertyLotInformationObj); 
            }
        }
    }
}