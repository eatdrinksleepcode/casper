@echo off

set myDir=%~dp0
set configuration=%1
if "%configuration%" == "" set configuration=RELEASE

msbuild /p:Configuration=%configuration% || goto :eof
call %myDir%pack %configuration% || goto :eof
