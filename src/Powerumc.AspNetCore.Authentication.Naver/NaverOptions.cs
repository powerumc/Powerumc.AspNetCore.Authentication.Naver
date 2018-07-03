using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace Powerumc.AspNetCore.Authentication.Naver
{
    public class NaverOptions : OAuthOptions
    {
        public NaverOptions()
        {
            base.CallbackPath = new PathString("/signin-naver");
            base.AuthorizationEndpoint = "https://nid.naver.com/oauth2.0/authorize";
            base.TokenEndpoint = "https://nid.naver.com/oauth2.0/token?grant_type=authorization_code";
            base.UserInformationEndpoint = "https://openapi.naver.com/v1/nid/me";
            
            base.ClaimActions.MapJsonSubKey(ClaimTypes.NameIdentifier, "response", "id");
            base.ClaimActions.MapJsonSubKey(ClaimTypes.Name, "response", "name");
            base.ClaimActions.MapJsonSubKey(ClaimTypes.Email, "response", "email");
            base.ClaimActions.MapJsonSubKey(ClaimTypes.Gender, "response", "gender");
            base.ClaimActions.MapJsonSubKey("urn:naver:avatar", "response", "profile_image");
            base.ClaimActions.MapJsonSubKey("urn:naver:age", "response", "age");
            base.ClaimActions.MapJsonSubKey("urn:naver:nickname", "response", "nickname");
            base.ClaimActions.MapJsonSubKey("urn:naver:birthday", "response", "birthday");
        }
    }
}