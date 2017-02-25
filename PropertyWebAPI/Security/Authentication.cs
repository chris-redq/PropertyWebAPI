
namespace PropertyWebAPI.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using WebAPISecurityDB;
    using System.Security.Cryptography;
    using System.Text;
    using AutoMapper;


    public class User : WebAPISecurityDB.vwAPIUser
    {

    }

    public static class Authentication
    {
        private static string EncodeKey(string key, string salt)
        {
            HashAlgorithm algorithm = new SHA256Managed();

            byte[] plainKeyWithSaltBytes = new byte[key.Length + salt.Length];
            byte[] plainKey = Encoding.ASCII.GetBytes(key);
            byte[] plainSalt = Encoding.ASCII.GetBytes(salt);

            for (int i = 0; i < plainKey.Length; i++)
            {
                plainKeyWithSaltBytes[i] = plainKey[i];
            }
            for (int i = 0; i < plainSalt.Length; i++)
            {
                plainKeyWithSaltBytes[plainKey.Length + i] = plainSalt[i];
            }

            return BitConverter.ToString(algorithm.ComputeHash(plainKeyWithSaltBytes)).Replace("-", "").ToUpper();
        }

        private static bool MatchCredentials(string apiKey, string salt, string encodedKey)
        {
            return (EncodeKey(apiKey, salt) == encodedKey);
        }

        private static string GetKey(HttpRequestMessage request, string key)
        {
            // Check if the key is in the query string if not check the request header for the key
            var queryString = request.RequestUri.ParseQueryString();
            var keyValue = queryString[key];
            if (keyValue == null)
            {
                try
                {
                    keyValue = request.Headers.GetValues(key).FirstOrDefault();
                }
                catch (System.InvalidOperationException e)
                {
                    Common.Logs.log().Error(string.Format("{0} token not found in HTTP Header{1}", key, Common.Logs.FormatException(e)));
                }
            }
            return keyValue;
        }

        public static User AuthenticateOld(HttpRequestMessage request)
        {
            var apiKey = GetKey(request, "apiKey");

            if (apiKey == null)
                return null;

            // Once the api key is found, check if valid api key. 
            try
            {
                using (WebAPISecurityEntities securityDBEntities = new WebAPISecurityEntities())
                {
                    var userObj = securityDBEntities.vwAPIUsers.Where(x => x.APIKey.ToString() == apiKey).FirstOrDefault();
                    if (userObj != null)
                    {
                        return Mapper.Map<User>(userObj);
                    }
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Error in acquiring HttpContext.Current.User{0}", Common.Logs.FormatException(e)));
            }
            return null;
        }

        public static User GetUser()
        {
            var accountId = HttpContext.Current.User.Identity.Name;

            try
            {
                using (WebAPISecurityEntities securityDBEntities = new WebAPISecurityEntities())
                {
                    return Mapper.Map<User>(securityDBEntities.vwAPIUsers.Where(x => x.AccountId == accountId).FirstOrDefault());
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Error in acquiring HttpContext.Current.User{0}", Common.Logs.FormatException(e)));
            }
            return null;
        }

        public static User Authenticate(HttpRequestMessage request)
        {
            var apiKey = GetKey(request, "apiKey");
            var apiAccount = GetKey(request, "apiAccount");
            var apiPassCode = GetKey(request, "apiPassCode");

            if (apiKey == null || apiAccount == null)
                return null;

            // Once the api key is found, check if valid api key. 
            try
            {
                using (WebAPISecurityEntities securityDBEntities = new WebAPISecurityEntities())
                {
                    var userObj = securityDBEntities.vwAPIUsers.Where(x => x.AccountId == apiAccount).FirstOrDefault();
                    if (userObj != null)
                    {
                        if (userObj.APIKey == EncodeKey(apiKey, apiPassCode))
                            return Mapper.Map<User>(userObj);
                    }
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Error in acquiring HttpContext.Current.User{0}", Common.Logs.FormatException(e)));
            }
            return null;
        }
    }
}