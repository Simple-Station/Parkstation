#!/bin/env sh

# make sure running from root
if [ "$(dirname $0)" != "." ]; then
  cd "$(dirname $0)"
fi
cd ../..

dotnet build -c Debug
