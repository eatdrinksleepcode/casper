@echo off

set myDir=%~dp0
set localNugetConfig=%myDir%local-nuget.config
set /p casperVersion=<=.casper.version

echo Installing old Casper (%casperVersion%)...
set oldPackageDir=%myDir%.casper\old
if exist "%oldPackageDir%" rmdir /S /Q "%oldPackageDir%" || goto :eof
mkdir "%oldPackageDir%" || goto :eof
"%myDir%nuget" install Casper.Console -Version %casperVersion% -OutputDirectory "%oldPackageDir%" -pre -Verbosity quiet -Source "MyGet" -ConfigFile "%localNugetConfig%" -ExcludeVersion || goto :eof
