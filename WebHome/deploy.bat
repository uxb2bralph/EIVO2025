@echo off
setlocal enabledelayedexpansion

:: ============================================================
:: deploy.bat - Build & deploy WebHome to an IIS application path
::
:: Usage:   deploy.bat <destination_iis_app_path>
:: Example: deploy.bat "C:\inetpub\wwwroot\EIVO2025"
::
:: Steps:
::   1. dotnet publish (Release)
::   2. Stop the matching IIS app pool (release file locks)
::   3. Robocopy /MIR the publish output to the destination
::   4. Restart the IIS app pool
:: ============================================================

if "%~1"=="" (
    echo Usage: deploy.bat ^<destination_iis_app_path^>
    echo.
    echo Builds WebHome (Release) and deploys to the given IIS application path.
    echo The IIS app pool is stopped during copy to avoid file-lock issues,
    echo then restarted after the copy completes.
    echo.
    echo Example: deploy.bat "C:\inetpub\wwwroot\EIVO2025"
    exit /b 1
)

set "DEST=%~1"
set "SCRIPT_DIR=%~dp0"
set "PUBLISH_DIR=%SCRIPT_DIR%bin\publish"
set "APPCMD=C:\Windows\System32\inetsrv\appcmd.exe"

echo ============================================
echo  WebHome Deploy
echo  Destination : %DEST%
echo ============================================
echo.

:: --- [1/4] Build ---
echo [1/4] dotnet publish -c Release ...
dotnet publish "%SCRIPT_DIR%WebHome.csproj" -c Release -o "%PUBLISH_DIR%"
if !errorlevel! neq 0 (
    echo.
    echo [ERROR] Build failed. Aborting.
    exit /b 1
)
echo.

:: --- [2/4] Stop IIS app pool ---
echo [2/4] Stopping IIS app pool ...
set "APPPOOL="
for /f "usebackq delims=" %%i in (`powershell -NoProfile -ExecutionPolicy Bypass -Command "$d='%DEST%'; $a=Get-WebApplication -EA SilentlyContinue | Where-Object {$_.PhysicalPath -eq $d}; if(-not $a){$s=Get-WebSite -EA SilentlyContinue | Where-Object {$_.PhysicalPath -eq $d} | Select-Object -First 1; if($s){$a=$s.ApplicationPool}}; if($a){$a}"`) do set "APPPOOL=%%i"

if defined APPPOOL (
    echo       Stopping app pool: !APPPOOL!
    "!APPCMD!" stop apppool /apppool.name:"!APPPOOL!" >nul 2>&1
    timeout /t 2 /nobreak >nul
) else (
    echo       No matching IIS app pool found. Continuing without stop.
)
echo.

:: --- [3/4] Copy files ---
echo [3/4] Copying publish output to destination ...
robocopy "%PUBLISH_DIR%" "%DEST%" /MIR /R:3 /W:2 /NP /NDL /NFL /NJH /NJS /XD App_Data logs
if !errorlevel! geq 8 (
    echo.
    echo [ERROR] Robocopy failed (exit code: !errorlevel!).
    if defined APPPOOL "!APPCMD!" start apppool /apppool.name:"!APPPOOL!" >nul 2>&1
    exit /b 1
)
echo.

:: --- [4/4] Restart IIS app pool ---
echo [4/4] Restarting IIS app pool ...
if defined APPPOOL (
    "!APPCMD!" start apppool /apppool.name:"!APPPOOL!" >nul 2>&1
    echo       App pool started: !APPPOOL!
) else (
    echo       No app pool to restart.
)
echo.
echo ============================================
echo  Deploy complete!
echo ============================================
endlocal
