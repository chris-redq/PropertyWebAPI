

namespace PropertyWebAPI.Security
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Controllers;

    public class Roles
    {
        public const string DATAREQUEST = "datarequest";
        public const string CALLBACK = "callback";
        public const string SCHEDULING = "scheduling";
    }

    public class APIAuthorize: AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var userObj = Authentication.GetUser();
            if (userObj == null)
            {
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            List<string> actionRoles = null;
            if (Roles != null)
                actionRoles = Roles.Split(',').Select(x => x.ToLower().Trim()).ToList();

            if (actionRoles.Count == 0) // no role based authorization, user is authorized
                return;

            List<string> userRoles = null;
            if (userObj.Roles != null)
                userRoles = userObj.Roles.Split(',').Select(x => x.ToLower().Trim()).ToList();

            if (userRoles == null || userRoles.Count==0) // No roles assigned to user, hence not authorized
            {
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            foreach (var actionRole in actionRoles)
            {
                foreach (var userRole in userRoles)
                {
                    if (actionRole == userRole)
                    {
                        return;
                    }
                }
            }
            actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            //base.OnAuthorization(actionContext);
        }
    }
}