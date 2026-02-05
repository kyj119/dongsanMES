# 🚀 MES Collector 출력기 PC 설치 가이드

**작성일**: 2026-02-05  
**대상**: Windows 출력기 PC에 Collector 설치 및 테스트

---

## 📋 목차
1. [사전 준비](#사전-준비)
2. [빠른 설치 (5분)](#빠른-설치-5분)
3. [상세 설치 (처음 사용자)](#상세-설치-처음-사용자)
4. [테스트 방법](#테스트-방법)
5. [문제 해결](#문제-해결)

---

## 1. 사전 준비

### 필요한 것
- ✅ Windows 출력기 PC (Windows 10/11 또는 Windows Server)
- ✅ .NET 8.0 Runtime (무료, 자동 설치)
- ✅ MES 서버 IP 주소 (예: `192.168.0.100`)
- ✅ 관리자 권한

### 확인 사항
```
출력기 PC:
- IP: 192.168.0.XXX (사내 네트워크)
- TOPAZ RIP 폴더: C:\TOPAZ_RIP\
- 네트워크: MES 서버와 통신 가능

MES 서버:
- IP: 192.168.0.100 (예시)
- 포트: 5000
- 상태: 실행 중
```

---

## 2. 빠른 설치 (5분)

### 📦 방법 1: 배포 패키지 사용 (권장)

#### 단계 1: 파일 준비
```
1. 샌드박스에서 빌드된 파일 다운로드:
   /home/user/webapp/MESCollector/

2. 출력기 PC로 복사:
   → C:\MESCollector\
```

#### 단계 2: 설치 스크립트 실행
```batch
@echo off
echo ========================================
echo MES Collector 자동 설치 스크립트
echo ========================================

REM 1. 설치 디렉토리 확인
set INSTALL_DIR=C:\MESCollector
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"

REM 2. .NET 8.0 Runtime 확인
echo [단계 1/5] .NET 8.0 Runtime 확인 중...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo .NET 8.0 Runtime이 설치되지 않았습니다.
    echo 다운로드: https://dotnet.microsoft.com/download/dotnet/8.0
    echo "ASP.NET Core Runtime 8.0.x - Windows Hosting Bundle" 설치 필요
    pause
    exit /b 1
)
echo ✓ .NET 8.0 Runtime 설치 확인

REM 3. TOPAZ RIP 폴더 생성 (테스트용)
echo [단계 2/5] 모니터링 폴더 생성 중...
set TOPAZ_DIR=C:\TOPAZ_RIP
if not exist "%TOPAZ_DIR%\preview" mkdir "%TOPAZ_DIR%\preview"
if not exist "%TOPAZ_DIR%\printlog" mkdir "%TOPAZ_DIR%\printlog"
if not exist "%TOPAZ_DIR%\job" mkdir "%TOPAZ_DIR%\job"
echo ✓ 폴더 생성 완료

REM 4. 설정 파일 생성
echo [단계 3/5] 설정 파일 생성 중...
(
echo {
echo   "Collector": {
echo     "ServerUrl": "http://192.168.0.100:5000",
echo     "CollectorId": "COLLECTOR-MACHINE-01",
echo     "WatchPaths": {
echo       "Preview": "C:\\TOPAZ_RIP\\preview",
echo       "PrintLog": "C:\\TOPAZ_RIP\\printlog",
echo       "Job": "C:\\TOPAZ_RIP\\job"
echo     },
echo     "RetryCount": 3,
echo     "RetryDelaySeconds": 5
echo   },
echo   "Serilog": {
echo     "MinimumLevel": "Information",
echo     "WriteTo": [
echo       {
echo         "Name": "File",
echo         "Args": {
echo           "path": "logs/collector-.log",
echo           "rollingInterval": "Day",
echo           "retainedFileCountLimit": 30
echo         }
echo       },
echo       {
echo         "Name": "Console"
echo       }
echo     ]
echo   }
echo }
) > "%INSTALL_DIR%\appsettings.json"
echo ✓ 설정 파일 생성 완료

REM 5. 안내
echo.
echo [단계 4/5] 설정 확인
echo ========================================
echo 설치 위치: %INSTALL_DIR%
echo 감시 폴더: %TOPAZ_DIR%
echo 설정 파일: %INSTALL_DIR%\appsettings.json
echo ========================================
echo.
echo ⚠️ 중요: appsettings.json 파일을 열어서
echo          ServerUrl을 실제 MES 서버 IP로 수정하세요!
echo.
echo [단계 5/5] 실행 방법
echo ----------------------------------------
echo 테스트 실행:
echo   cd C:\MESCollector
echo   dotnet MESCollector.dll
echo.
echo Windows Service 등록:
echo   sc create MESCollector binPath="C:\MESCollector\MESCollector.exe"
echo   sc start MESCollector
echo ----------------------------------------
echo.
echo 설치 완료!
pause
