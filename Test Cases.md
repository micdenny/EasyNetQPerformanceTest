# EasyNetQ Performance Test Cases

## Publish

1. Synchronous publish without a bounded queue: `--publish --time 30`
2. Synchronous publish with a bounded queue: `--publish --time 30 --use-queue`
3. Synchronous concurrent publish without a bounded queue using 8 dispatcher: `--publish --time 30 --concurrency 8`
4. Synchronous concurrent publish with a bounded queue using 8 dispatcher: `--publish --time 30 --concurrency 8 --use-queue`
5. Issue [#661](https://github.com/EasyNetQ/EasyNetQ/issues/661): `--publish --count 500 --concurrency 500`

## Subscribe

1. Synchronous subscribe on a pre-filled queue with 1 milion messages: `--subscribe --message-count 1000000`

## RPC

1. Synchronous requests: `--rpc --time 30`
3. Synchronous concurrent requests busing 8 dispatcher: `--rpc --time 30 --concurrency 8`
3. Asynchronous requests awaiting the response one-by-one (no concurrency): `--rpc-async --time 30`
4. Asynchronous concurrent requests awaiting 1 milion responses using 8 dispatcher: `--rpc-async --count 1000000 --concurrency 8`