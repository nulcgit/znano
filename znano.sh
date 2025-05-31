#!/usr/bin/env bash

cd "$(dirname "$0")"
if [ ! -f "./znano" ]; then
    go build -o znano -ldflags="-s -w" .
fi
./znano
