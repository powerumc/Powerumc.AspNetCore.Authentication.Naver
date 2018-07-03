using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Powerumc.AspNetCore.Authentication.Naver
{
    public static class NaverOAuthExtension
    {
        public static AuthenticationBuilder AddNaver(this AuthenticationBuilder builder,
            Action<NaverOptions> configureOptions)
        {
            return builder.AddOAuth<NaverOptions, NaverHandler>("Naver", "Naver", configureOptions);
        }
    }
}