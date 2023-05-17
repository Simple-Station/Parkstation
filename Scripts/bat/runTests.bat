@echo off
cd ../../
dotnet build --configuration Tools --no-restore /p:WarningsAsErrors=nullable /m
dotnet test --configuration Tools --no-build Content.Tests/Content.Tests.csproj -- NUnit.ConsoleOut=0
pause
