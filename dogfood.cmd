@echo off

set myDir=%~dp0
set localNugetConfig=%myDir%.local-nuget.config
copy %myDir%local-nuget.config %localNugetConfig% || goto :eof

call %myDir%build DEBUG || goto :eof
if exist %myDir%.msbuild rmdir /S /Q %myDir%.msbuild || goto :eof
mkdir %myDir%.msbuild || goto :eof
echo Installing casper...
%myDir%nuget sources Remove -Name "Casper Dev" -ConfigFile %localNugetConfig%
%myDir%nuget sources Add -Name "Casper Dev" -Source %myDir%\out\packages -ConfigFile %localNugetConfig% || goto :eof
REM Adding any source to a NuGet config also adds the default source, which we don't want for this operation
%myDir%nuget sources Disable -Name "https://www.nuget.org/api/v2/" -ConfigFile %localNugetConfig% || goto :eof
%myDir%nuget install Casper.Console -OutputDirectory %myDir%.msbuild -pre -Verbosity quiet -Source "Casper Dev" -ConfigFile %localNugetConfig% || goto :eof
del %localNugetConfig%
msbuild /t:Clean || goto :eof
msbuild /t:Clean /p:Configuration=Release || goto :eof
call %myDir%build-with-casper %* || goto :eof
