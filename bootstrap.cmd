@echo off

set myDir=%~dp0
set localNugetConfig=%myDir%local-nuget.config

echo Installing old Casper...
set oldPackageDir=%myDir%.casper\old
if exist "%oldPackageDir%" rmdir /S /Q "%oldPackageDir%" || goto :eof
mkdir "%oldPackageDir%" || goto :eof
"%myDir%nuget" install Casper.Console -Version 0.1.0-ci0166 -OutputDirectory "%oldPackageDir%" -pre -Verbosity quiet -Source "MyGet" -ConfigFile "%localNugetConfig%" -ExcludeVersion || goto :eof
