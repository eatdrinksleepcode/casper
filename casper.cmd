@echo off

set task=%1
if "%task%" == "" set task=Build

set myDir=%~dp0
"%myDir%.msbuild\Casper.Console\tools\casper.exe" build.casper %task% || goto :eof
