@echo off

set VERBOSE=0
if "%1"=="--verbose" set VERBOSE=1
set CURRENT=%~dp0

pushd %CURRENT%

set EnqPerfTestExe=%CURRENT%EasyNetQPerformanceTest\bin\Release\EnqPerfTest.exe

echo ----------------------PUBLISH----------------------
echo 1. Synchronous publish without a bounded queue
echo %EnqPerfTestExe% --publish --time 30
%EnqPerfTestExe% --publish --time 30

echo ----------------------PUBLISH----------------------
echo 2. Synchronous publish with a bounded queue
echo %EnqPerfTestExe% --publish --time 30 --use-queue
%EnqPerfTestExe% --publish --time 30 --use-queue

echo ----------------------PUBLISH----------------------
echo 3. Synchronous concurrent publish without a bounded queue using 8 dispatcher
echo %EnqPerfTestExe% --publish --time 30 --concurrency 8
%EnqPerfTestExe% --publish --time 30 --concurrency 8

echo ----------------------PUBLISH----------------------
echo 4. Synchronous concurrent publish with a bounded queue using 8 dispatcher
echo %EnqPerfTestExe% --publish --time 30 --concurrency 8 --use-queue
%EnqPerfTestExe% --publish --time 30 --concurrency 8 --use-queue

echo ----------------------PUBLISH----------------------
echo 5. Issue #661 https://github.com/EasyNetQ/EasyNetQ/issues/661
echo %EnqPerfTestExe% --publish --count 50 --concurrency 50
%EnqPerfTestExe% --publish --count 50 --concurrency 50

echo ---------------------SUBSCRIBE---------------------
echo Synchronous subscribe on a pre-filled queue with 1 milion messages
echo %EnqPerfTestExe% --subscribe --message-count 1000000
%EnqPerfTestExe% --subscribe --message-count 1000000

echo ------------------------RPC------------------------
echo Synchronous requests
echo %EnqPerfTestExe% --rpc --time 30
%EnqPerfTestExe% --rpc --time 30

echo ------------------------RPC------------------------
echo Synchronous concurrent requests busing 8 dispatcher
echo %EnqPerfTestExe% --rpc --time 30 --concurrency 8
%EnqPerfTestExe% --rpc --time 30 --concurrency 8

echo ------------------------RPC------------------------
echo Asynchronous requests awaiting the response one-by-one (no concurrency)
echo %EnqPerfTestExe% --rpc-async --time 30
%EnqPerfTestExe% --rpc-async --time 30

echo ------------------------RPC------------------------
echo Asynchronous concurrent requests awaiting 1 milion responses using 8 dispatcher
echo %EnqPerfTestExe% --rpc-async --count 1000000 --concurrency 8
%EnqPerfTestExe% --rpc-async --count 1000000 --concurrency 8

if "%VERBOSE%"=="1" pause