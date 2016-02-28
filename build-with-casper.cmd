@echo off

set myDir=%~dp0

rmdir /S /Q %myDir%out || goto :eof
set TargetFrameworkVersion=v4.5.1
call %myDir%casper %* || goto :eof
