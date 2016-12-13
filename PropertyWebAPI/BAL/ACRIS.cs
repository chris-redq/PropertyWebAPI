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
        //Blank classes to mask entity framework details
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
            try
            {
                using (ACRISEntities acrisDBEntities = new ACRISEntities())
                {
                    LatestDeedDocument documentObj = acrisDBEntities.LatestDeedDocuments.FirstOrDefault(i => i.BBLE == propertyBBLE);

                    if (documentObj == null)
                    {   Common.Logs.log().Error(string.Format("Error finding record for BBLE {0} in LatestDeedDocument", propertyBBLE));
                        return null;
                    }
                    DeedDetails deedDetailsObj = new DeedDetails();
                    deedDetailsObj.deedDocument = documentObj;
                    foreach (tfnGetDocumentParties_Result a in acrisDBEntities.tfnGetDocumentParties(documentObj.DeedUniqueKey, "BUYER").ToList())
                    {
                        if (deedDetailsObj.owners == null)
                            deedDetailsObj.owners = new List<DeedParty>();
                        deedDetailsObj.owners.Add(Mapper.Map<DeedParty>(a));
                    }

                    return deedDetailsObj;
                }
            }
            catch(Exception e)
            {
                Common.Logs.log().Error(string.Format("Error reading AreaAbstract\n{0}\n", e.ToString()));
                return null;
            }
        }

        public static PropertyLotInformation GetLotInformation(string propertyBBLE)
        {
            try
            {   using (ACRISEntities acrisDBEntities = new ACRISEntities())
                {
                    PropertyNotInAssessment propertyLotInformationObj = acrisDBEntities.PropertyNotInAssessments.FirstOrDefault(i => i.BBL == propertyBBLE);

                    if (propertyLotInformationObj == null)
                    {   Common.Logs.log().Error(string.Format("Error finding record for BBLE {0} in ACRIS", propertyBBLE));
                        return null;
                    }
                    return Mapper.Map<PropertyLotInformation>(propertyLotInformationObj);
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Error reading AreaAbstract\n{0}\n", e.ToString()));
                return null;
            }
        }
        }
    }
}