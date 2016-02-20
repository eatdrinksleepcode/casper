@echo off

set task=%1
if "%task%" == "" set task=Build

set myDir=%~dp0
for /f %%i in ('dir /A:D /B /S .msbuild\Casper.Console.*') do set casperPath=%%i
echo Running casper...
%casperPath%\tools\casper.exe build.casper %task% || goto :eof
