# MES 시스템 문제 해결 가이드

## 🚨 에러 발생 시 진단 방법

### 1단계: Development 모드로 전환 (상세 에러 확인)

#### 방법 1: 환경 변수 설정 (추천)
**Windows PowerShell (관리자 권한):**
```powershell
# 환경 변수 설정
$env:ASPNETCORE_ENVIRONMENT = "Development"

# MESSystem 실행
cd C:\path\to\MESSystem
dotnet run
```

**Windows CMD (관리자 권한):**
```cmd
set ASPNETCORE_ENVIRONMENT=Development
cd C:\path\to\MESSystem
dotnet run
```

#### 방법 2: launchSettings.json 수정
**MESSystem/Properties/launchSettings.json** 파일 확인/수정:
```json
{
  "profiles": {
    "MESSystem": {
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "http://0.0.0.0:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### 2단계: 상세 에러 로그 확인

Development 모드로 실행 후 브라우저에서 페이지를 열면 **상세한 에러 스택 트레이스**가 표시됩니다.

---

## 🔍 일반적인 에러 원인 및 해결 방법

### ❌ 에러 1: 데이터베이스 파일 없음
**증상:**
```
SqliteException: SQLite Error 14: 'unable to open database file'
```

**원인:** MESSystem.db 파일이 없거나 접근 권한 문제

**해결 방법:**
```bash
# 1. MESSystem 폴더로 이동
cd C:\path\to\MESSystem

# 2. 기존 DB 삭제 (있는 경우)
del MESSystem.db
del MESSystem.db-shm
del MESSystem.db-wal

# 3. 애플리케이션 실행 (자동으로 DB 생성됨)
dotnet run

# 또는 IIS에서 실행하는 경우
# IIS 앱풀 계정에 폴더 쓰기 권한 부여
```

**폴더 권한 설정:**
1. MESSystem 폴더 우클릭 → 속성
2. 보안 탭 → 편집
3. IIS_IUSRS 또는 IIS AppPool\MESSystemPool 추가
4. 수정, 읽기 및 실행 권한 부여

---

### ❌ 에러 2: 공유 폴더 접근 권한 없음
**증상:**
```
UnauthorizedAccessException: Access to the path '\\192.168.0.122\Designs\' is denied.
```

**원인:** 공유 폴더에 대한 접근 권한 없음

**해결 방법:**

**옵션 1: 공유 폴더 권한 확인**
```bash
# 공유 폴더 서버 (192.168.0.122)에서
# 1. Designs 폴더 우클릭 → 속성
# 2. 공유 탭 → 고급 공유
# 3. 권한 → Everyone 또는 특정 사용자에게 모든 권한 부여
```

**옵션 2: 임시로 공유 폴더 비활성화** (테스트용)
```json
// appsettings.json 수정
{
  "SharedFolderPath": "",  // 빈 문자열로 변경
  "UploadPath": "wwwroot/uploads/"
}
```

**옵션 3: IIS 앱풀 계정 변경**
1. IIS 관리자 → 애플리케이션 풀 → MESSystemPool 우클릭
2. 고급 설정 → ID 변경
3. 사용자 지정 계정 → 공유 폴더 접근 권한이 있는 도메인 계정

---

### ❌ 에러 3: Entity Framework 마이그레이션 오류
**증상:**
```
InvalidOperationException: No database provider has been configured
```

**원인:** EF Core 설정 누락

**해결 방법:**
```bash
# NuGet 패키지 복원
cd MESSystem
dotnet restore

# 빌드
dotnet build
```

---

### ❌ 에러 4: 세션/캐시 오류
**증상:**
```
InvalidOperationException: Unable to resolve service for type 'Microsoft.Extensions.Caching.Distributed.IDistributedCache'
```

**원인:** 세션 서비스 설정 누락

**해결 방법:**
**Program.cs 확인:**
```csharp
// 반드시 이 순서로 서비스 등록
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 미들웨어 순서 확인
app.UseSession();  // UseAuthorization() 전에 호출
app.UseAuthorization();
```

---

### ❌ 에러 5: Razor Pages 라우팅 오류
**증상:**
```
404 Not Found
```

**원인:** Razor Pages 경로 문제

**해결 방법:**
```csharp
// Program.cs 확인
app.MapRazorPages();  // 반드시 포함
```

---

## 📋 체크리스트: 서버 배포 전 확인사항

### ✅ 1. .NET Runtime 설치 확인
```powershell
# PowerShell에서 실행
dotnet --version
# 출력: 8.0.x 이상이어야 함
```

**설치 필요 시:**
- .NET 8.0 SDK 또는 Hosting Bundle 다운로드
- https://dotnet.microsoft.com/download/dotnet/8.0

### ✅ 2. 데이터베이스 파일 권한
```bash
# MESSystem.db 파일이 있는 폴더
# IIS_IUSRS에게 쓰기 권한 필요
```

### ✅ 3. 공유 폴더 접근 테스트
```powershell
# PowerShell에서 테스트
Test-Path "\\192.168.0.122\Designs\"
# True가 나와야 함
```

### ✅ 4. 방화벽 설정
```powershell
# Windows 방화벽에서 5000번 포트 허용
New-NetFirewallRule -DisplayName "MES System" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow
```

### ✅ 5. appsettings.json 확인
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=MESSystem.db"
  },
  "SharedFolderPath": "\\\\192.168.0.122\\Designs\\",
  "UploadPath": "wwwroot/uploads/"
}
```

---

## 🔧 빠른 진단 명령어

### Windows PowerShell 진단 스크립트
```powershell
# 진단 스크립트 저장: diagnose.ps1
Write-Host "=== MES System 진단 시작 ===" -ForegroundColor Green

# 1. .NET Runtime 확인
Write-Host "`n1. .NET Runtime 확인..." -ForegroundColor Yellow
dotnet --version

# 2. 데이터베이스 파일 확인
Write-Host "`n2. 데이터베이스 파일 확인..." -ForegroundColor Yellow
Test-Path ".\MESSystem.db"

# 3. 공유 폴더 접근 확인
Write-Host "`n3. 공유 폴더 접근 확인..." -ForegroundColor Yellow
Test-Path "\\192.168.0.122\Designs\"

# 4. 포트 사용 확인
Write-Host "`n4. 포트 5000 사용 확인..." -ForegroundColor Yellow
netstat -an | findstr ":5000"

# 5. 프로세스 확인
Write-Host "`n5. MESSystem 프로세스 확인..." -ForegroundColor Yellow
Get-Process | Where-Object {$_.ProcessName -like "*MESSystem*"}

Write-Host "`n=== 진단 완료 ===" -ForegroundColor Green
```

**실행:**
```powershell
cd C:\path\to\MESSystem
.\diagnose.ps1
```

---

## 📞 에러 보고 방법

에러가 계속 발생하면 다음 정보를 수집해주세요:

### 1. 에러 스택 트레이스
- Development 모드로 실행
- 브라우저에 표시된 전체 에러 메시지 복사

### 2. 로그 파일
```bash
# MESSystem/logs/ 폴더의 최신 로그 확인
```

### 3. 환경 정보
```powershell
# PowerShell에서 실행
dotnet --info
```

### 4. 이벤트 뷰어 확인
1. Windows 키 → "이벤트 뷰어" 검색
2. Windows 로그 → 응용 프로그램
3. ASP.NET Core 관련 에러 확인

---

## 🚀 정상 작동 확인 방법

### 1. 웹 서버 실행 확인
```bash
http://localhost:5000
# 로그인 페이지가 표시되어야 함
```

### 2. 로그인 테스트
- **관리자**: admin / admin123
- **현장**: field01 / user123

### 3. 페이지 접근 테스트
- 메인 페이지: `/`
- 대시보드: `/Dashboard`
- 카드 목록: `/Cards/Index`
- 주문서 목록: `/Admin/Orders/Index`

---

## 📝 자주 발생하는 문제

### Q1: "Request ID: 00-xxxx..." 에러만 표시됨
**A:** Development 모드로 전환 필요 (위의 1단계 참고)

### Q2: 데이터베이스가 계속 리셋됨
**A:** `EnsureCreated()` 대신 Migrations 사용 필요
```bash
dotnet ef migrations add Initial
dotnet ef database update
```

### Q3: IIS에서 500 Internal Server Error
**A:** 
1. web.config 확인
2. IIS 로그 확인: `C:\inetpub\logs\LogFiles`
3. stdout 로그 활성화

### Q4: "Unable to connect to web server 'MESSystem'"
**A:** 
1. 방화벽 확인
2. 포트 충돌 확인: `netstat -ano | findstr :5000`
3. IIS 바인딩 확인

---

## 💡 추가 도움말

더 자세한 도움이 필요하시면:
1. **GitHub Issues**: https://github.com/kyj119/dongsanMES/issues
2. **에러 메시지 전체 복사** 후 문의
3. **환경 정보** (Windows 버전, .NET 버전, IIS 버전) 제공

---

**마지막 업데이트**: 2026-02-05
