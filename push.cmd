@echo off

set myDir=%~dp0
set feed=%1

echo Pushing package to %feed%...
"%myDir%nuget" push "%myDir%out\packages\*.*" %MYGET_API_KEY% -ConfigFile "%myDir%local-nuget.config" -Source %feed% -NonInteractive || exit $?
