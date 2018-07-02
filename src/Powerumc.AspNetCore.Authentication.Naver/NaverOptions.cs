using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace Powerumc.AspNetCore.Authentication.Naver
{
    public class NaverOptions : OAuthOptions
    {
        /// <summary>
        /// 클라이언트 Id
        /// </summary>
        public string ClientId { get; set; }
        
        /// <summary>
        /// 클라이언트 Secret
        /// </summary>
        public string ClientSecret { get; set; }

        public NaverOptions()
        {
            base.CallbackPath = new PathString("/signin-naver");
            base.AuthorizationEndpoint = "https://nid.naver.com/oauth2.0/authorize";
            base.TokenEndpoint = "https://nid.naver.com/oauth2.0/token?grant_type=authorization_code";
            base.UserInformationEndpoint = "https://openapi.naver.com/v1/nid/me";
        }
    }
}