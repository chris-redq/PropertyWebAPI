namespace PropertyWebAPI.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using WebDataDB;

    /// <summary>
    /// </summary>
    public static class RequestType
    {
        /// <summary>
        /// </summary>
        public static int GetDaysToRefresh(int requestTypeId)
        {
            using (WebDataEntities webDBEntities = new WebDataEntities())
            {
                try
                {
                    var requestTypeObj = webDBEntities.RequestTypes.Where(i => i.RequestTypeId==requestTypeId).FirstOrDefault();
                    if (requestTypeObj != null && requestTypeObj.DaysToRefresh != null)
                        return requestTypeObj.DaysToRefresh.GetValueOrDefault();

                    return 60;
                }
                catch (Exception e)
                {
                    Common.Logs.log().Error(string.Format("Exception encountered getting DaysToRefresh for RequestTypeId:{0}", requestTypeId, Common.Logs.FormatException(e)));
                    return 60;
                }
            }
        }
    }
}
