@echo off

set task=%*
if "%task%" == "" set task=Build

set myDir=%~dp0
"%myDir%.casper\new\Casper.Console\tools\casper.exe" build.casper %task% || goto :eof
