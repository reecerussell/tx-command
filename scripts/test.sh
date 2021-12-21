#!/bin/bash

set -e

run_test() {
    echo "Testing $1..."
    dotnet test -c Release -p:CollectCoverage=true -p:CoverletOutputFormat=cobertura -p:CoverletOutput=../Coverage/$2/ "$1"
}

cd tests

echo "Running tests..."

run_test TxCommand.Net5.Tests/TxCommand.Net5.Tests.csproj TxCommand.Net5.Tests
run_test TxCommand.NetCore3_1.Tests/TxCommand.NetCore3_1.Tests.csproj TxCommand.NetCore3_1.Tests

echo "Running SQL tests..."

echo "Starting Docker environment..."
docker-compose -f docker-compose.sql.yaml up -d & sleep 20

echo "Running tests..."
run_test TxCommand.Sql.Net5.Tests/TxCommand.Sql.Net5.Tests.csproj TxCommand.Sql.Net5.Tests
run_test TxCommand.Sql.NetCore3_1.Tests/TxCommand.Sql.NetCore3_1.Tests.csproj TxCommand.Sql.NetCore3_1.Tests

echo "Cleaning up Docker environment..."
docker-compose -f docker-compose.sql.yaml down

echo "Running Mongo tests..."

echo "Starting Docker environment..."
docker-compose -f docker-compose.mongo.yaml up -d && sleep 35

echo "Running tests..."
run_test TxCommand.Mongo.Net5.Tests/TxCommand.Mongo.Net5.Tests.csproj TxCommand.Mongo.Net5.Tests
run_test TxCommand.Mongo.NetCore3_1.Tests/TxCommand.Mongo.NetCore3_1.Tests.csproj TxCommand.Mongo.NetCore3_1.Tests

echo "Cleaning up Docker environment..."
docker-compose -f docker-compose.mongo.yaml down

echo "Finished!"