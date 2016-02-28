@echo off

set myDir=%~dp0

call %myDir%build DEBUG || goto :eof
if exist %myDir%.msbuild rmdir /S /Q %myDir%.msbuild || goto :eof
mkdir %myDir%.msbuild || goto :eof
echo Installing casper...
%myDir%nuget install Casper.Console -OutputDirectory %myDir%.msbuild -pre -Verbosity quiet -Source "Casper Dev" || goto :eof
msbuild /t:Clean || goto :eof
msbuild /t:Clean /p:Configuration=Release || goto :eof
call %myDir%build-with-casper %* || goto :eof
