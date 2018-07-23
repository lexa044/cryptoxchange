#!/bin/bash

BUILDIR=${1:-../../build}

echo "Building into $BUILDIR"

# publish
mkdir -p $BUILDIR
dotnet publish -c Release --framework netcoreapp2.0 -o $BUILDIR