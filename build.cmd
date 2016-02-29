@echo off

set myDir=%~dp0
set configuration=%1
if "%configuration%" == "" set configuration=RELEASE

set TargetFrameworkVersion=v4.5.1
echo Building with msbuild...
msbuild /p:Configuration=%configuration% /p:TargetFrameworkversion=v4.5.1 /verbosity:quiet || goto :eof
call %myDir%pack %configuration% || goto :eof
