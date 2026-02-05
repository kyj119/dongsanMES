@echo off
REM MES 시스템 배포 스크립트
REM 관리자 권한으로 실행하세요!

echo ========================================
echo MES 시스템 배포 스크립트
echo ========================================
echo.

REM 관리자 권한 확인
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [오류] 이 스크립트는 관리자 권한이 필요합니다!
    echo        우클릭 > "관리자 권한으로 실행"을 선택하세요.
    pause
    exit /b 1
)

echo [1/6] 환경 확인 중...
echo.

REM .NET 설치 확인
where dotnet >nul 2>&1
if %errorLevel% neq 0 (
    echo [오류] .NET 8.0 Runtime이 설치되어 있지 않습니다!
    echo        https://dotnet.microsoft.com/download/dotnet/8.0 에서 다운로드하세요.
    pause
    exit /b 1
)

echo [✓] .NET 설치 확인 완료
echo.

echo [2/6] 프로젝트 빌드 중...
cd /d "%~dp0"
dotnet publish -c Release -o "C:\inetpub\wwwroot\MESSystem"
if %errorLevel% neq 0 (
    echo [오류] 빌드 실패!
    pause
    exit /b 1
)

echo [✓] 빌드 완료
echo.

echo [3/6] 업로드 폴더 생성 중...
if not exist "C:\inetpub\wwwroot\MESSystem\wwwroot\uploads" (
    mkdir "C:\inetpub\wwwroot\MESSystem\wwwroot\uploads"
)
echo [✓] 업로드 폴더 생성 완료
echo.

echo [4/6] appsettings.json 확인...
echo.
echo ========================================
echo 중요: appsettings.json 파일을 수정하세요!
echo ========================================
echo 파일 위치: C:\inetpub\wwwroot\MESSystem\appsettings.json
echo.
echo 수정 항목:
echo   1. ConnectionStrings의 Password를 실제 sa 비밀번호로 변경
echo   2. SharedFolderPath 확인 (\\192.168.0.122\Designs\)
echo.
echo 수정 후 아무 키나 누르세요...
pause >nul

echo.
echo [5/6] IIS 애플리케이션 풀 생성 중...
%systemroot%\system32\inetsrv\appcmd add apppool /name:MESSystemPool /managedRuntimeVersion:"" /managedPipelineMode:Integrated
if %errorLevel% == 0 (
    echo [✓] 애플리케이션 풀 생성 완료
) else (
    echo [!] 애플리케이션 풀이 이미 존재하거나 생성 실패
)
echo.

echo [6/6] IIS 웹 사이트 생성 중...
%systemroot%\system32\inetsrv\appcmd add site /name:MESSystem /physicalPath:"C:\inetpub\wwwroot\MESSystem" /bindings:http/*:80:
if %errorLevel% == 0 (
    echo [✓] 웹 사이트 생성 완료
) else (
    echo [!] 웹 사이트가 이미 존재하거나 생성 실패
    echo     포트가 사용 중일 수 있습니다. (기본 웹 사이트 중지 필요)
)
echo.

REM 애플리케이션 풀 연결
%systemroot%\system32\inetsrv\appcmd set app "MESSystem/" /applicationPool:MESSystemPool
echo [✓] 애플리케이션 풀 연결 완료
echo.

echo ========================================
echo 배포 완료!
echo ========================================
echo.
echo 다음 단계:
echo   1. SQL Server에서 database_schema.sql 실행
echo      (파일 위치: %~dp0database_schema.sql)
echo.
echo   2. 브라우저에서 접속:
echo      http://localhost
echo.
echo   3. 로그인:
echo      관리자: admin / admin123
echo      현장: field01 / user123
echo.
echo   4. 문제 발생 시:
echo      - IIS 관리자에서 MESSystem 사이트 시작 확인
echo      - appsettings.json의 DB 연결 문자열 확인
echo      - 이벤트 뷰어에서 오류 로그 확인
echo.
pause
