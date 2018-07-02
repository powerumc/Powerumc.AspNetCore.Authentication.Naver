using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Powerumc.AspNetCore.Authentication.Naver
{
    public class NaverHandler : OAuthHandler<NaverOptions>
    {
        public NaverHandler(IOptionsMonitor<NaverOptions> options, ILoggerFactory logger, UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }
        
        private void AddQueryString<T>(IDictionary<string, string> queryStrings, AuthenticationProperties properties, string name, Func<T, string> formatter, T defaultValue)
        {
            string str = null;
            T parameter = properties.GetParameter<T>(name);
            if (parameter != null)
            {
                str = formatter(parameter);
            }
            else if (!properties.Items.TryGetValue(name, out str))
            {
                str = formatter(defaultValue);
            }
            properties.Items.Remove(name);
            if (str != null)
            {
                queryStrings[name] = str;
            }
        }

        private void AddQueryString(IDictionary<string, string> queryStrings, AuthenticationProperties properties, string name, string defaultValue = null)
        {
            this.AddQueryString<string>(queryStrings, properties, name, (string x) => x, defaultValue);
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity,
            AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            var str = QueryHelpers.AddQueryString(base.Options.UserInformationEndpoint, "access_token", tokens.AccessToken);
            

            var async = await base.Backchannel.GetAsync(str, base.Context.RequestAborted);
            if (!async.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"An error occurred when retrieving Naver user information ({async.StatusCode}). Please check if the authentication information is correct and the corresponding Naver Graph API is enabled.");
            }

            var jObject = JObject.Parse(await async.Content.ReadAsStringAsync());
            
            
            
            var oAuthCreatingTicketContext = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties,
                base.Context, base.Scheme, base.Options, base.Backchannel, tokens, jObject);
            oAuthCreatingTicketContext.RunClaimActions();

            await base.Events.CreatingTicket(oAuthCreatingTicketContext);

            var authenticationTicket = new AuthenticationTicket(oAuthCreatingTicketContext.Principal,
                oAuthCreatingTicketContext.Properties, base.Scheme.Name);

            return authenticationTicket;
        }
    }
}