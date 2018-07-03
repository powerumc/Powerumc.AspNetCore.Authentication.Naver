# Powerumc.AspNetCore.Authentication.Naver

네이버로 OAuth 로그인(네아로)에 필요한 모듈과 샘플 웹 응용 프로그램을 제공합니다.

## 사용방법
Nuget 에서 Naver 로 검색해서 참조를 추가해 주세요.

```
Install-Package Powerumc.AspNetCore.Authentication.Naver -Version 2.1.0
```

```
dotnet add package Powerumc.AspNetCore.Authentication.Naver --version 2.1.0
```

## 제한사항
1. .NET Core = 2.1.0 버전을 지원합니다.
2. 본 예제는 In-Memory 데이터베이스를 사용합니다.
3. 실제 데이터베이스에서 테스트가 완료되었습니다.
4. 이메일 인증을 False로 설정하였습니다.

## 1. Startup.cs 설정

### 네아로 설정

네이버 OAuth(네아로)에서 콜백 URL 을 `/signin-naver` 로 설정하셔야 합니다.

### ConfigureServices 메서드에서 설정

```csharp
// 인코딩 설정
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// 네이버 OAuth 설정
services.AddAuthentication()
    .AddNaver(options =>
    {
        options.ClientId = "네아로 ClientId";
        options.ClientSecret = "네아로 ClientSecret";
    });
```

```csharp
// 인증 정보 저장소
services.AddDbContext<ApplicationDbContext>(options => { options.UseInMemoryDatabase("db"); });
services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
```

### Configure 메서드에서 설정

```csharp
// 인증 사용 설정
app.UseAuthentication();
```

### 참고

본 예제는 인메모리의 데이터베이스를 이용한 예제입니다.  
실제 데이터베이스를 이용할 경우 엔티티 프레임워크 마이그레이션이 필요랍니다.

아래의 명령을 이용하여 엔티티 프레임워크 마이그레이션과 테이블을 생성할 수 있습니다.

```
dotnet ef migrations add Init_identity
dotnet ef database update
```


## 인증 컨트롤러의 액션에서 사용

Controllers 폴더의 HomeController.cs 파일을 참고해 주세요.

```csharp
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
```
