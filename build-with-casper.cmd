@echo off

set myDir=%~dp0

rmdir /S /Q %myDir%out || goto :eof
call %myDir%casper %* || goto :eof
