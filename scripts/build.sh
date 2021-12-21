#!/bin/bash

set -e

run_build() {
    dotnet build -c Release --no-restore "$1"
}

run_pack() {
    dotnet pack -c Release --no-restore --no-build -o packages "$1"
}

echo "Restoring..."

dotnet restore src/TxCommand/TxCommand.csproj
dotnet restore src/TxCommand.Sql/TxCommand.Sql.csproj
dotnet restore src/TxCommand.Mongo/TxCommand.Mongo.csproj

echo "Building..."

run_build src/TxCommand/TxCommand.csproj
run_build src/TxCommand.Abstractions/TxCommand.Abstractions.csproj

run_build src/TxCommand.Sql/TxCommand.Sql.csproj
run_build src/TxCommand.Sql.Abstractions/TxCommand.Sql.Abstractions.csproj

run_build src/TxCommand.Mongo/TxCommand.Mongo.csproj
run_build src/TxCommand.Mongo.Abstractions/TxCommand.Mongo.Abstractions.csproj

echo "Packing..."

run_pack src/TxCommand/TxCommand.csproj
run_pack src/TxCommand.Abstractions/TxCommand.Abstractions.csproj

run_pack src/TxCommand.Sql/TxCommand.Sql.csproj
run_pack src/TxCommand.Sql.Abstractions/TxCommand.Sql.Abstractions.csproj

run_pack src/TxCommand.Mongo/TxCommand.Mongo.csproj
run_pack src/TxCommand.Mongo.Abstractions/TxCommand.Mongo.Abstractions.csproj