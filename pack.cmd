@echo off

set myDir=%~dp0
set configuration=%1
if "%configuration%" == "" set configuration=RELEASE

rmdir /S /Q %myDir%out || goto :eof
mkdir %myDir%out\packages || goto :eof
%myDir%nuget pack %myDir%Console\Console.nuspec -OutputDirectory %myDir%out\packages -Properties "Configuration=%configuration%" || goto :eof
