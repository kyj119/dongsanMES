# 🚨 서버 PC 에러 해결 가이드

**현재 상황**: 메인페이지, 대시보드, 카드 목록에서 에러 발생

---

## ⚡ 빠른 해결 방법 (3단계)

### 1단계: Development 모드로 실행 (에러 확인)

**서버 PC에서 PowerShell (관리자 권한) 실행:**

```powershell
# 1. MESSystem 폴더로 이동
cd C:\dongsanMES\MESSystem
# 또는 실제 설치 경로로 이동

# 2. Development 모드 설정
$env:ASPNETCORE_ENVIRONMENT = "Development"

# 3. 실행
dotnet run --urls="http://0.0.0.0:5000"
```

**브라우저에서 접속:**
```
http://localhost:5000
```

이제 **상세한 에러 메시지**가 화면에 표시됩니다!

---

### 2단계: 에러 메시지 확인

에러 메시지를 확인하고 아래 "일반적인 에러별 해결 방법"을 참고하세요.

---

### 3단계: 문제 해결 후 테스트

```powershell
# 다시 실행
dotnet run --urls="http://0.0.0.0:5000"
```

---

## 🔍 일반적인 에러별 해결 방법

### ❌ 에러 1: 데이터베이스 오류

**에러 메시지 예시:**
```
SqliteException: SQLite Error 14: 'unable to open database file'
SqliteException: no such table: Users
```

**해결 방법:**
```powershell
# MESSystem 폴더에서
cd C:\dongsanMES\MESSystem

# 기존 DB 삭제
Remove-Item MESSystem.db -ErrorAction SilentlyContinue
Remove-Item MESSystem.db-shm -ErrorAction SilentlyContinue
Remove-Item MESSystem.db-wal -ErrorAction SilentlyContinue

# 다시 실행 (자동으로 DB 생성됨)
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --urls="http://0.0.0.0:5000"
```

---

### ❌ 에러 2: 공유 폴더 접근 오류

**에러 메시지 예시:**
```
UnauthorizedAccessException: Access to the path '\\192.168.0.122\Designs\' is denied.
IOException: The network path was not found.
```

**빠른 해결 (임시):**
```powershell
# appsettings.json 수정
cd C:\dongsanMES\MESSystem
notepad appsettings.json
```

**다음과 같이 수정:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=MESSystem.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "SharedFolderPath": "",
  "UploadPath": "wwwroot/uploads/"
}
```

**저장 후 다시 실행:**
```powershell
dotnet run --urls="http://0.0.0.0:5000"
```

**영구 해결 방법:**
1. 공유 폴더 서버 (192.168.0.122)에서 Designs 폴더 권한 확인
2. Everyone 또는 서버 PC 계정에 읽기/쓰기 권한 부여
3. appsettings.json에서 SharedFolderPath 다시 설정

---

### ❌ 에러 3: DLL 누락

**에러 메시지 예시:**
```
FileNotFoundException: Could not load file or assembly '...'
```

**해결 방법:**
```powershell
cd C:\dongsanMES\MESSystem

# NuGet 패키지 복원
dotnet restore

# 빌드
dotnet build

# 실행
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --urls="http://0.0.0.0:5000"
```

---

### ❌ 에러 4: 포트 충돌

**에러 메시지 예시:**
```
IOException: Failed to bind to address http://0.0.0.0:5000
```

**해결 방법:**
```powershell
# 포트 5000 사용 중인 프로세스 찾기
netstat -ano | findstr :5000

# 다른 포트로 실행
dotnet run --urls="http://0.0.0.0:5001"
```

---

### ❌ 에러 5: 세션 오류

**에러 메시지 예시:**
```
InvalidOperationException: Unable to resolve service for type 'IDistributedCache'
```

**해결 방법:**
**Program.cs 확인 필요** - GitHub에서 최신 버전 다운로드

```powershell
# Git으로 최신 버전 받기
cd C:\dongsanMES
git pull origin main

# 또는 GitHub에서 다시 클론
cd C:\
git clone https://github.com/kyj119/dongsanMES.git
cd dongsanMES\MESSystem
dotnet restore
dotnet run
```

---

## 📋 완전 재설치 가이드 (모든 문제 해결)

모든 방법이 실패하면 완전히 재설치하세요:

```powershell
# 1. 기존 폴더 백업 (중요!)
cd C:\
Rename-Item dongsanMES dongsanMES_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')

# 2. 최신 버전 클론
git clone https://github.com/kyj119/dongsanMES.git
cd dongsanMES\MESSystem

# 3. 패키지 복원 및 빌드
dotnet restore
dotnet build

# 4. Development 모드로 실행
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --urls="http://0.0.0.0:5000"

# 5. 브라우저에서 테스트
# http://localhost:5000
# 로그인: admin / admin123
```

---

## 🧪 로그인 테스트

정상 작동 확인:

```
http://localhost:5000
```

**테스트 계정:**
- **관리자**: admin / admin123
- **현장**: field01 / user123

**테스트할 페이지:**
1. 로그인 → 성공 확인
2. 대시보드 → 통계 표시 확인
3. 관리 → 분류 관리 → 3개 분류 확인 (태극기, 현수막, 간판)
4. 현장 → 카드 목록 → 빈 목록 확인

---

## 📞 에러 보고하기

위 방법으로 해결되지 않으면 다음 정보를 복사해서 보내주세요:

### 1. 에러 스택 트레이스
Development 모드에서 브라우저에 표시된 **전체 에러 메시지** 복사

### 2. 환경 정보
```powershell
# PowerShell에서 실행
dotnet --info

# 출력 결과 복사
```

### 3. appsettings.json 내용
```powershell
cd C:\dongsanMES\MESSystem
Get-Content appsettings.json
```

### 4. 실행 중인 명령어
```powershell
# 실행한 명령어와 출력 결과 모두 복사
```

---

## 🎯 체크리스트

실행하기 전 확인:

- [ ] .NET 8.0 Runtime 설치됨 (`dotnet --version` → 8.0.x)
- [ ] MESSystem 폴더에 있음 (`cd C:\dongsanMES\MESSystem`)
- [ ] Development 모드 설정됨 (`$env:ASPNETCORE_ENVIRONMENT = "Development"`)
- [ ] 5000번 포트가 비어있음 (`netstat -ano | findstr :5000`)
- [ ] appsettings.json 존재함
- [ ] MESSystem.csproj 존재함

---

## 💡 자주 묻는 질문

### Q: Development 모드로 실행하면 보안 문제 없나요?
A: **테스트/디버깅용으로만 사용**하세요. 에러 해결 후 Production 모드로 전환하세요.

### Q: dotnet 명령어를 찾을 수 없다고 나옵니다
A: .NET 8.0 SDK 또는 Runtime 설치 필요
- 다운로드: https://dotnet.microsoft.com/download/dotnet/8.0

### Q: git 명령어를 찾을 수 없다고 나옵니다
A: Git 설치 필요
- 다운로드: https://git-scm.com/download/win
- 또는 GitHub에서 ZIP으로 다운로드

### Q: 매번 Development 모드로 설정해야 하나요?
A: 환경 변수를 영구 설정할 수 있습니다:
```powershell
# 시스템 환경 변수 설정 (관리자 권한)
[System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development", "Machine")
```

---

## 🚀 에러 해결 후 Production 모드로 전환

에러가 해결되면 Production 모드로 전환:

```powershell
# 1. 환경 변수 제거
Remove-Item Env:\ASPNETCORE_ENVIRONMENT

# 2. Production 모드로 실행
dotnet run --urls="http://0.0.0.0:5000" --environment="Production"
```

---

**도움이 필요하시면 에러 메시지와 함께 문의해주세요!**

**GitHub Issues**: https://github.com/kyj119/dongsanMES/issues
