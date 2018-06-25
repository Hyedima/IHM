using DotNetOpenAuth.GoogleOAuth2;
using Microsoft.AspNet.Membership.OpenAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Web.WebPages.OAuth;

namespace IHM.App_Start
{
    public class AuthConfig
    {
        public static void RegisterAuth()
        {
            //OAuthWebSecurity.RegisterFacebookClient("472452203194955", "65697054e289e6cd8e7cb6bc48c6049f");
            GoogleOAuth2Client clientGoog = new GoogleOAuth2Client("823469131819-3ua2ouujlb7ubottign2k3f0km426m1q.apps.googleusercontent.com", "POSyAEpO9RWFtTfF8mNQR9Rx");
            IDictionary<string, string> extraData = new Dictionary<string, string>();
            OpenAuth.AuthenticationClients.Add("google", () => clientGoog, extraData);
            //OpenAuth.AuthenticationClients.AddFacebook("472452203194955", "65697054e289e6cd8e7cb6bc48c6049f");
        }
    }
}