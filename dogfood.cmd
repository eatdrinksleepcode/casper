@echo off

set myDir=%~dp0
set localNugetConfig=%myDir%.local-nuget.config
copy "%myDir%local-nuget.config" "%localNugetConfig%" || goto :eof

call "%myDir%build" DEBUG || goto :eof
echo Installing old Casper...
set oldPackageDir=%myDir%.casper\old
if exist "%oldPackageDir%" rmdir /S /Q "%oldPackageDir%" || goto :eof
mkdir "%oldPackageDir%" || goto :eof
"%myDir%nuget" install Casper.Console -Version 0.1.0-ci0146 -OutputDirectory "%oldPackageDir%" -pre -Verbosity quiet -Source "MyGet" -ConfigFile "%localNugetConfig%" -ExcludeVersion || goto :eof

set oldCasper=%oldPackageDir%\Casper.Console\tools\casper.exe
echo Building new Casper with old Casper (from "%oldCasper%")...
"%oldCasper%" build.casper %task% || goto :eof

echo Installing new casper...
set newPackageDir=%myDir%.casper\new
if exist "%newPackageDir%" rmdir /S /Q "%newPackageDir%" || goto :eof
mkdir "%newPackageDir%" || goto :eof
"%myDir%nuget" install Casper.Console -OutputDirectory "%newPackageDir%" -pre -Verbosity quiet -Source "Casper Dev" -ConfigFile "%localNugetConfig%" -ExcludeVersion || goto :eof

echo Cleaning outputs...
msbuild /t:Clean /verbosity:quiet /nologo || goto :eof
msbuild /t:Clean /p:Configuration=Release /verbosity:quiet /nologo || goto :eof

echo Building with new Casper...
if exist "%myDir%out" rmdir /S /Q "%myDir%out" || goto :eof
call "%myDir%casper" %* || goto :eof
