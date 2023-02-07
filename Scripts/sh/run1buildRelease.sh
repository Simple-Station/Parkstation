#!/bin/env sh

# make sure to start from script dir
if [ "$(dirname $0)" != "." ]; then
  cd "$(dirname $0)"
fi

cd ../../

if [ -e sloth.txt ]
then
    dotnet build -c Release
else
    exit
fi
