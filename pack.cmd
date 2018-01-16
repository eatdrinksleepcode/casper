@echo off

set myDir=%~dp0
set configuration=%1
if "%configuration%" == "" set configuration=RELEASE

for /f %%i in ('gitversion.cmd') do set version=%%i

if exist "%myDir%out" rmdir /S /Q "%myDir%out" || goto :eof
mkdir "%myDir%out\packages" || goto :eof
"%myDir%nuget" pack "%myDir%Console\Console.nuspec" -OutputDirectory "%myDir%out\packages" -Properties BINPATH=bin\%configuration%\ -NoPackageAnalysis -Version %version% || goto :eof
