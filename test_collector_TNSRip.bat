@echo off
chcp 65001 >nul
echo Testing Collector...
echo.

REM Preview event
echo [1/3] Creating preview file...
echo test > C:\TNSRip-X11\Preview\20260206-99-1.bmp.tsc
timeout /t 2 /nobreak >nul

REM PrintLog event
echo [2/3] Creating printlog file...
(
echo JobName=TestJob
echo Copies=100
echo StartTime=2026-02-06 09:30:00
) > C:\TNSRip-X11\Temp\20260206-99-1_093000.log
timeout /t 2 /nobreak >nul

REM Job event
echo [3/3] Creating job file...
(
echo PrintFile=test.ai
echo DestSizeX=900.000000
echo EndDate=2026-02-06 09:35:00
) > C:\TNSRip-X11\Job\20260206-99-10002.job

echo.
echo Test complete!
echo Check Collector console for event transmission.
echo.
pause
