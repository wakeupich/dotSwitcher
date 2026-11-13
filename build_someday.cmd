@echo off
setlocal ENABLEEXTENSIONS
set CONFIG=Release
set "MSBUILD_PATH="
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" (
  for /f "usebackq delims=" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do set "MSBUILD_PATH=%%i"
)
if "%MSBUILD_PATH%"=="" if exist "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" set "MSBUILD_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
if "%MSBUILD_PATH%"=="" (
  echo MSBuild.exe not found.
  exit /b 1
)
echo Using MSBuild: %MSBUILD_PATH%
echo Building dotSwitcher.sln (%CONFIG%)...
"%MSBUILD_PATH%" dotSwitcher.sln /p:Configuration=%CONFIG% /m /v:minimal /nologo
if errorlevel 1 (
  echo Build failed.
  exit /b 1
)
echo.
echo Build finished.
echo Output: bin\%CONFIG%\dotSwitcher.exe
echo Setup:   Setup\bin\%CONFIG%\dotSwitcher-Setup.exe
endlocal