@echo off
chcp 65001 >nul
echo ╔════════════════════════════════════════════════════════════╗
echo ║        MES Collector 자동 설치 스크립트                    ║
echo ║        버전: 1.0                                            ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

REM 관리자 권한 확인
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [오류] 관리자 권한이 필요합니다.
    echo 마우스 우클릭 > "관리자 권한으로 실행"을 선택하세요.
    pause
    exit /b 1
)

REM 설치 디렉토리
set INSTALL_DIR=C:\MESCollector
set TOPAZ_DIR=C:\TOPAZ_RIP

echo [1/6] 설치 디렉토리 생성 중...
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
echo ✓ %INSTALL_DIR%

echo.
echo [2/6] .NET 8.0 Runtime 확인 중...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo.
    echo [경고] .NET 8.0 Runtime이 설치되지 않았습니다!
    echo.
    echo 다운로드 링크: https://dotnet.microsoft.com/download/dotnet/8.0
    echo 설치 파일: ASP.NET Core Runtime 8.0.x - Windows x64
    echo.
    echo .NET 설치 후 이 스크립트를 다시 실행하세요.
    pause
    exit /b 1
)
for /f "tokens=*" %%v in ('dotnet --version') do set DOTNET_VERSION=%%v
echo ✓ .NET %DOTNET_VERSION% 설치됨

echo.
echo [3/6] TOPAZ RIP 모니터링 폴더 생성 중...
if not exist "%TOPAZ_DIR%\preview" mkdir "%TOPAZ_DIR%\preview"
if not exist "%TOPAZ_DIR%\printlog" mkdir "%TOPAZ_DIR%\printlog"
if not exist "%TOPAZ_DIR%\job" mkdir "%TOPAZ_DIR%\job"
echo ✓ %TOPAZ_DIR%\preview
echo ✓ %TOPAZ_DIR%\printlog
echo ✓ %TOPAZ_DIR%\job

echo.
echo [4/6] 설정 파일 생성 중...
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
echo ✓ appsettings.json 생성

echo.
echo [5/6] 테스트 스크립트 생성 중...
(
echo @echo off
echo echo 테스트 파일 생성 중...
echo echo.
echo.
echo REM 작업대기 이벤트
echo echo [1/3] 작업대기 이벤트...
echo echo test ^> C:\TOPAZ_RIP\preview\20260205-99-1.bmp.tsc
echo timeout /t 2 /nobreak ^>nul
echo.
echo REM 작업시작 이벤트  
echo echo [2/3] 작업시작 이벤트...
echo ^(
echo echo JobName=테스트작업
echo echo Copies=100
echo echo StartTime=2026-02-05 09:30:00
echo ^) ^> C:\TOPAZ_RIP\printlog\20260205-99-1_093000.log
echo timeout /t 2 /nobreak ^>nul
echo.
echo REM 작업완료 이벤트
echo echo [3/3] 작업완료 이벤트...
echo ^(
echo echo PrintFile=test.ai
echo echo DestSizeX=900.000000
echo echo EndDate=2026-02-05 09:35:00
echo ^) ^> C:\TOPAZ_RIP\job\20260205-99-10002.job
echo.
echo echo 테스트 완료!
echo echo Collector 콘솔에서 이벤트 전송을 확인하세요.
echo pause
) > "%INSTALL_DIR%\test_collector.bat"
echo ✓ test_collector.bat 생성

echo.
echo [6/6] 설치 정보 저장 중...
(
echo MES Collector 설치 정보
echo ========================
echo.
echo 설치 날짜: %DATE% %TIME%
echo 설치 경로: %INSTALL_DIR%
echo .NET 버전: %DOTNET_VERSION%
echo.
echo 모니터링 폴더:
echo   - Preview:  %TOPAZ_DIR%\preview
echo   - PrintLog: %TOPAZ_DIR%\printlog
echo   - Job:      %TOPAZ_DIR%\job
echo.
echo 설정 파일: %INSTALL_DIR%\appsettings.json
echo.
) > "%INSTALL_DIR%\INSTALL_INFO.txt"
echo ✓ INSTALL_INFO.txt 생성

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║                   설치 완료!                                ║
echo ╚════════════════════════════════════════════════════════════╝
echo.
echo 📁 설치 위치: %INSTALL_DIR%
echo 📁 감시 폴더: %TOPAZ_DIR%
echo.
echo ⚠️  중요: 다음 단계를 진행하세요!
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.
echo 1️⃣  설정 파일 수정:
echo    notepad %INSTALL_DIR%\appsettings.json
echo    → ServerUrl을 실제 MES 서버 IP로 변경
echo    → CollectorId를 출력기 식별자로 변경
echo.
echo 2️⃣  MESCollector 파일 복사:
echo    GitHub 또는 샌드박스에서 빌드된 파일을
echo    %INSTALL_DIR% 폴더에 복사
echo.
echo 3️⃣  테스트 실행:
echo    cd %INSTALL_DIR%
echo    dotnet run
echo.
echo 4️⃣  테스트 파일 생성:
echo    (새 CMD 창) %INSTALL_DIR%\test_collector.bat
echo.
echo 5️⃣  Windows Service 등록 (프로덕션):
echo    sc create MESCollector binPath="%INSTALL_DIR%\MESCollector.exe"
echo    sc start MESCollector
echo.
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.
echo 📚 상세 가이드: %INSTALL_DIR%\COLLECTOR_INSTALL_GUIDE.md
echo.
pause
