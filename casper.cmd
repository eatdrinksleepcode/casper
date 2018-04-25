@echo off

set task=%*
if "%task%" == "" set task=Build
set myDir=%~dp0

call "%myDir%bootstrap" || goto :eof

"%myDir%.casper\old\Casper.Console\tools\casper.exe" build.casper %task% || goto :eof
