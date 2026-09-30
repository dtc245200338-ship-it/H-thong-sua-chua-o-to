@echo off
cd /d "%~dp0"
dotnet restore --configfile NuGet.Config
if errorlevel 1 goto end
dotnet run --no-launch-profile --urls http://localhost:5080
:end
pause
