@echo off

set myDir=%~dp0
set configuration=%1
if "%configuration%" == "" set configuration=RELEASE

echo Determining version...
packages\GitVersion.CommandLine.3.6.5\tools\GitVersion.exe /updateAssemblyInfo AssemblyInfo.Version.cs /ensureAssemblyInfo || goto :eof

set TargetFrameworkVersion=v4.5.1
echo Building with msbuild...
msbuild /p:Configuration=%configuration% /p:TargetFrameworkversion=v4.5.1 /verbosity:quiet || goto :eof
call "%myDir%pack" %configuration% || goto :eof
