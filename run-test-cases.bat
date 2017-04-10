@echo off

set VERBOSE=0
if "%1"=="--verbose" set VERBOSE=1
set CURRENT=%~dp0

pushd %CURRENT%

if "%VERBOSE%"=="1" echo test-cases.bat %1
test-cases.bat %1 > test-cases-results.log

if "%VERBOSE%"=="1" pause