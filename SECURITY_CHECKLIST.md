# 🔒 MES 시스템 보안 체크리스트

**작성일**: 2026-02-05  
**프로젝트**: MES System  
**목적**: 프로덕션 배포 전 보안 점검

---

## 📋 긴급 수정 항목 (배포 전 필수)

### 🔴 1. 비밀번호 보안 (최우선)

**현재 상태**: ❌ 평문 저장
```csharp
// 현재 코드 (취약!)
Password = "admin123"  // 평문 저장
```

**필수 수정**:
```bash
# 1. BCrypt 패키지 설치
cd MESSystem
dotnet add package BCrypt.Net-Next --version 4.0.3
```

```csharp
// 2. Models/User.cs - PasswordHash 메서드 추가
using BCrypt.Net;

public class User
{
    // 기존 Password 속성 유지 (DB 호환)
    public string Password { get; set; } = string.Empty;
    
    // 헬퍼 메서드
    public void SetPassword(string plainPassword)
    {
        Password = BCrypt.HashPassword(plainPassword);
    }
    
    public bool VerifyPassword(string plainPassword)
    {
        return BCrypt.Verify(plainPassword, Password);
    }
}
```

```csharp
// 3. Program.cs - 초기 데이터 수정
var admin = new User { Username = "admin", FullName = "관리자" };
admin.SetPassword("admin123");

var designer = new User { Username = "designer", FullName = "디자이너" };
designer.SetPassword("designer123");
```

```csharp
// 4. Pages/Account/Login.cshtml.cs - 로그인 로직 수정
public async Task<IActionResult> OnPostAsync()
{
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Username == Username);
    
    if (user == null || !user.VerifyPassword(Password))
    {
        ModelState.AddModelError(string.Empty, "아이디 또는 비밀번호가 올바르지 않습니다.");
        return Page();
    }
    
    // 로그인 성공
    // ...
}
```

**완료 체크**: [ ] 비밀번호 해싱 적용 완료

---

### 🔴 2. CSRF 보호 (필수)

**현재 상태**: ❌ CSRF 토큰 없음

**필수 수정**:
```csharp
// 1. Program.cs - AntiForgery 서비스 추가
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});
```

```cshtml
<!-- 2. 모든 POST 폼에 토큰 추가 -->
<form method="post">
    @Html.AntiForgeryToken()
    
    <!-- 폼 필드 -->
</form>
```

**적용 대상**:
- [ ] Account/Login.cshtml
- [ ] Admin/Orders/Create.cshtml
- [ ] Admin/Orders/Edit.cshtml
- [ ] Admin/Products/Create.cshtml
- [ ] Admin/Products/Edit.cshtml
- [ ] Admin/Categories/Create.cshtml
- [ ] Admin/Categories/Edit.cshtml
- [ ] Admin/Clients/Create.cshtml
- [ ] Admin/Clients/Edit.cshtml

**완료 체크**: [ ] 모든 POST 폼에 CSRF 토큰 추가 완료

---

### 🟡 3. 파일 업로드 보안

**현재 상태**: ⚠️ 검증 부족

**필수 수정**:
```csharp
// Services/FileUploadService.cs 개선

public class FileUploadService
{
    private readonly IConfiguration _config;
    private readonly ILogger<FileUploadService> _logger;
    
    // 허용된 확장자
    private static readonly string[] AllowedExtensions = 
        { ".ai", ".eps", ".pdf", ".jpg", ".png", ".zip" };
    
    // 최대 파일 크기 (100MB)
    private const long MaxFileSize = 100 * 1024 * 1024;
    
    public async Task<(bool Success, string? FilePath, string? Error)> 
        UploadFileAsync(IFormFile file, string orderNumber)
    {
        // 1. NULL 체크
        if (file == null || file.Length == 0)
        {
            return (false, null, "파일이 선택되지 않았습니다.");
        }
        
        // 2. 크기 체크
        if (file.Length > MaxFileSize)
        {
            return (false, null, $"파일 크기는 {MaxFileSize / 1024 / 1024}MB를 초과할 수 없습니다.");
        }
        
        // 3. 확장자 체크
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return (false, null, 
                $"허용되지 않는 파일 형식입니다. 허용: {string.Join(", ", AllowedExtensions)}");
        }
        
        // 4. 파일명 검증 (경로 탐색 공격 방지)
        var fileName = Path.GetFileName(file.FileName);
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
        {
            return (false, null, "올바르지 않은 파일명입니다.");
        }
        
        // 5. 안전한 파일명 생성
        var safeFileName = $"{orderNumber}_{Guid.NewGuid()}{extension}";
        
        try
        {
            // 파일 저장 로직
            // ...
            
            return (true, filePath, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "파일 업로드 실패: {FileName}", fileName);
            return (false, null, "파일 업로드 중 오류가 발생했습니다.");
        }
    }
}
```

**완료 체크**: [ ] 파일 업로드 검증 로직 추가 완료

---

## 📋 중요 개선 항목 (Week 2)

### 🟡 4. API 인증

**현재 상태**: ❌ API 인증 없음

**권장 수정**:
```csharp
// 1. appsettings.json에 API Key 추가
{
  "CollectorApiKey": "YOUR_STRONG_API_KEY_HERE_MIN_32_CHARS"
}
```

```csharp
// 2. API Key 검증 미들웨어
public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
{
    private const string ApiKeyHeaderName = "X-API-KEY";
    
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, 
        ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(
            ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        var config = context.HttpContext.RequestServices
            .GetRequiredService<IConfiguration>();
        var apiKey = config.GetValue<string>("CollectorApiKey");
        
        if (!apiKey.Equals(extractedApiKey))
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        await next();
    }
}
```

```csharp
// 3. Program.cs - API 엔드포인트에 적용
app.MapPost("/api/events", [ApiKeyAuth] async (...) => { });
```

```csharp
// 4. MESCollector - API Key 추가
// appsettings.json
{
  "Collector": {
    "ServerUrl": "http://192.168.0.100",
    "ApiKey": "YOUR_STRONG_API_KEY_HERE_MIN_32_CHARS"
  }
}
```

```csharp
// 5. ApiService.cs - 헤더 추가
_httpClient.DefaultRequestHeaders.Add("X-API-KEY", _settings.ApiKey);
```

**완료 체크**: [ ] API 인증 구현 완료

---

### 🟡 5. HTTPS 강제

**현재 상태**: ⚠️ HTTPS 리디렉션만 있음

**권장 설정**:
```csharp
// Program.cs
if (!app.Environment.IsDevelopment())
{
    // HSTS 활성화 (1년)
    app.UseHsts();
}

// HTTPS 리디렉션 (모든 환경)
app.UseHttpsRedirection();
```

**IIS 배포 시**:
```xml
<!-- web.config -->
<rewrite>
  <rules>
    <rule name="HTTPS Redirect" stopProcessing="true">
      <match url="(.*)" />
      <conditions>
        <add input="{HTTPS}" pattern="off" ignoreCase="true" />
      </conditions>
      <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" 
              redirectType="Permanent" />
    </rule>
  </rules>
</rewrite>
```

**완료 체크**: [ ] HTTPS 인증서 설치 및 강제 적용

---

### 🟡 6. 권한 체크 통일

**현재 상태**: ⚠️ 페이지마다 중복 코드

**권장 수정**:
```csharp
// 1. 새 파일: Filters/AdminOnlyAttribute.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MESSystem.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminOnlyAttribute : Attribute, IPageFilter
{
    public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }
    
    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var role = session.GetString("Role");
        
        if (role != "관리자")
        {
            context.Result = new RedirectToPageResult("/Account/Login");
        }
    }
    
    public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }
}
```

```csharp
// 2. Program.cs - 필터 등록
builder.Services.AddRazorPages()
    .AddMvcOptions(options =>
    {
        options.Filters.Add<AdminOnlyAttribute>();
    });
```

```csharp
// 3. PageModel에 적용
[AdminOnly]
public class OrdersModel : PageModel
{
    // 권한 체크 코드 제거 가능
}
```

**완료 체크**: [ ] 권한 체크 Attribute 구현 완료

---

## 📋 장기 개선 항목 (Week 3-4)

### 🟢 7. 로깅 보안

**권장 설정**:
```csharp
// appsettings.json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "Logs/mes-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

**민감 정보 마스킹**:
```csharp
_logger.LogInformation("사용자 로그인: {Username}", username);
// ❌ _logger.LogInformation("로그인: {Username}/{Password}", username, password);
```

**완료 체크**: [ ] Serilog 적용 및 민감 정보 마스킹

---

### 🟢 8. SQL Injection 방어

**현재 상태**: ✅ Entity Framework Core 사용 (안전)

**주의사항**:
```csharp
// ✅ 안전 (Parameterized Query)
.Where(u => u.Username == username)

// ❌ 절대 금지! (Raw SQL)
_context.Database.ExecuteSqlRaw($"SELECT * FROM Users WHERE Username = '{username}'")

// ✅ Raw SQL 사용 시 파라미터 사용
_context.Database.ExecuteSqlRaw(
    "SELECT * FROM Users WHERE Username = {0}", username)
```

**완료 체크**: [ ] Raw SQL 사용 여부 점검

---

### 🟢 9. 세션 보안

**권장 설정**:
```csharp
// Program.cs
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;           // ✅ 이미 적용
    options.Cookie.IsEssential = true;        // ✅ 이미 적용
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ⚠️ 추가 필요
    options.Cookie.SameSite = SameSiteMode.Strict;           // ⚠️ 추가 필요
});
```

**완료 체크**: [ ] 세션 쿠키 보안 강화

---

### 🟢 10. 데이터베이스 연결 문자열

**현재 상태**: ⚠️ appsettings.json에 평문 저장

**프로덕션 권장**:
```bash
# 1. 환경 변수 사용
export ConnectionStrings__DefaultConnection="Server=...;Password=..."

# 2. 또는 User Secrets (개발)
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Password=..."
```

```csharp
// 3. Azure Key Vault (클라우드 배포 시)
builder.Configuration.AddAzureKeyVault(...);
```

**완료 체크**: [ ] 프로덕션 DB 연결 문자열 보안 설정

---

## 📋 배포 전 최종 체크리스트

### 필수 항목 (배포 불가능)
- [ ] 비밀번호 BCrypt 해싱 적용
- [ ] CSRF 토큰 모든 폼에 추가
- [ ] 파일 업로드 검증 강화
- [ ] appsettings.json에서 개발용 설정 제거
- [ ] 데이터베이스 연결 문자열 환경 변수로 이동

### 중요 항목 (권장)
- [ ] API Key 인증 구현
- [ ] HTTPS 인증서 설치
- [ ] 권한 체크 Attribute 적용
- [ ] 에러 페이지 커스터마이징
- [ ] 로깅 시스템 점검

### 선택 항목 (개선)
- [ ] 2FA (이중 인증) 고려
- [ ] 감사 로그 강화
- [ ] 세션 쿠키 보안 강화
- [ ] Rate Limiting 적용
- [ ] 정기 보안 감사 계획

---

## 🔧 빠른 적용 스크립트

### 비밀번호 해싱 일괄 적용
```bash
#!/bin/bash
# apply_bcrypt.sh

cd MESSystem

# 1. 패키지 설치
dotnet add package BCrypt.Net-Next --version 4.0.3

# 2. User 모델 수정 (수동 필요)
echo "User.cs에 SetPassword/VerifyPassword 메서드 추가 필요"

# 3. 빌드 테스트
dotnet build

echo "✅ BCrypt 패키지 설치 완료"
echo "⚠️  User.cs, Login.cshtml.cs 수정 필요"
```

### CSRF 토큰 일괄 적용
```bash
#!/bin/bash
# apply_csrf.sh

cd MESSystem/Pages

# 모든 POST 폼에 CSRF 토큰 추가 (수동 확인 필요)
find . -name "*.cshtml" -type f -exec grep -l 'method="post"' {} \;

echo "⚠️  위 파일들에 @Html.AntiForgeryToken() 추가 필요"
```

---

## 📞 보안 문의 및 지원

### 보안 문제 발견 시
1. 즉시 서버 중지
2. 로그 확인 (`Logs/` 디렉토리)
3. 데이터베이스 백업
4. 패치 적용 후 재시작

### 참고 자료
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://docs.microsoft.com/aspnet/core/security/)
- [BCrypt.Net-Next](https://github.com/BcryptNet/bcrypt.net)

---

## ⚠️ 중요 경고

### 절대 하지 말아야 할 것
1. ❌ 비밀번호를 평문으로 저장
2. ❌ SQL 쿼리에 사용자 입력을 직접 삽입
3. ❌ 민감한 정보를 로그에 기록
4. ❌ 프로덕션에서 Debug 모드 실행
5. ❌ appsettings.json을 Git에 커밋 (실제 비밀번호 포함 시)

### 반드시 해야 할 것
1. ✅ 모든 사용자 입력 검증
2. ✅ HTTPS 사용
3. ✅ 정기적인 보안 업데이트
4. ✅ 에러 메시지에 민감 정보 제외
5. ✅ 최소 권한 원칙 적용

---

**보안은 한 번의 설정이 아닌 지속적인 관리입니다.**

**마지막 업데이트**: 2026-02-05  
**다음 보안 점검**: Day 3 완료 후
