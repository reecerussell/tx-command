#!/bin/bash

set -e

run_build() {
    dotnet build -c Release --no-restore "$1"
}

run_pack() {
    dotnet pack -c Release --no-restore --no-build -o packages "$1"
}

echo "Restoring..."

dotnet restore TxCommand/TxCommand.csproj
dotnet restore Sql/TxCommand.Sql/TxCommand.Sql.csproj

echo "Building..."

run_build TxCommand/TxCommand.csproj
run_build TxCommand.Abstractions/TxCommand.Abstractions.csproj

run_build Sql/TxCommand.Sql/TxCommand.Sql.csproj
run_build Sql/TxCommand.Sql.Abstractions/TxCommand.Sql.Abstractions.csproj

echo "Packing..."

run_pack TxCommand/TxCommand.csproj
run_pack TxCommand.Abstractions/TxCommand.Abstractions.csproj

run_pack Sql/TxCommand.Sql/TxCommand.Sql.csproj
run_pack Sql/TxCommand.Sql.Abstractions/TxCommand.Sql.Abstractions.csproj