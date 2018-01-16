@echo off

set myDir=%~dp0

%mydir%packages\GitVersion.CommandLine.3.6.5\tools\GitVersion.exe /updateAssemblyInfo AssemblyInfo.Version.cs /ensureAssemblyInfo /output json /showVariable NuGetVersionV2 || goto :eof
