@echo off

:: Build configuration
set TargetFramework=net7.0

:: ----- Build project -----

:: Initialise
pushd "%~dp0"
cd DeepConvert.Tests

:: Clean
if exist bin\Release\%TargetFramework% rd /s /q bin\Release\%TargetFramework% || goto error
if exist obj rd /s /q obj || goto error
dotnet clean -v m -c Release -nologo || goto error

:: Build/publish
dotnet publish -c Release -r win-x64 --self-contained -nologo || goto error

:: ----- Finish -----

:: Exit
powershell write-host -fore Green Build finished.
popd
timeout /t 2 /nobreak >nul
exit /b

:error
pause
