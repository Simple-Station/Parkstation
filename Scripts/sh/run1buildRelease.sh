#!/bin/env sh

# make sure to start from script dir
if [ "$(dirname $0)" != "." ]; then
  cd "$(dirname $0)"
fi

# make sure running from root
cd ../..

dotnet build -c Release
