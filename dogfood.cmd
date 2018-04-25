@echo off

set myDir=%~dp0

call "%myDir%bootstrap" || goto :eof

"%myDir%.casper\old\Casper.Console\tools\casper.exe" dogfood.casper Dogfood || goto :eof
