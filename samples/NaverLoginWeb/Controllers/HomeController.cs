using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using NaverLoginWeb.Models;

namespace NaverLoginWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult LoginNaver()
        {
            var redirectUrl = Url.Action(nameof(LoginNaverCallback), "Home");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Naver", redirectUrl);

            return Challenge(properties, "Naver");
        }

        [HttpGet]
        public async Task<IActionResult> LoginNaverCallback(string returnUrl = "/")
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return Redirect("/");
            
            // 사용자 정보 설정 
            var user = new ApplicationUser
            {
                UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                EmailConfirmed = false,
                PhoneNumberConfirmed = false
            };

            var findByName = await _userManager.FindByNameAsync(user.UserName);
            if (findByName == null)
            {
                // 외부 인증이 완료되면 사용자 생성
                var createResult =  await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = createResult.Errors.Select(o => o.Description);
                    throw new Exception("CreateAsync failed: " + string.Join("<br/>", errors));
                }
                
                // 사용자가 로그인 생성
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                {
                    var errors = addLoginResult.Errors.Select(o => o.Description);
                    throw new Exception("AddLoginAsync failed: " + string.Join("<br/>", errors));
                }
            }

            // 외부 사용자 인증
            var signInResult =
                await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true);

            if (signInResult.Succeeded)
            {
                return Redirect(returnUrl);
            }

            if (signInResult.IsLockedOut)
            {
                throw new Exception("LockedOut");
            }

            throw new Exception("External login sign in result is failed");
        }
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}