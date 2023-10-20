cd ..\..\
dotnet restore
dotnet build --configuration DebugOpt --no-restore /p:WarningsAsErrors=nullable /m
mkdir Scripts\logs
del Scripts\logs\Content.Tests.log
dotnet test --no-build --configuration DebugOpt Content.Tests/Content.Tests.csproj -- NUnit.ConsoleOut=0 > Scripts\logs\Content.Tests.log
del Scripts\logs\Content.IntegrationTests.log
dotnet test --no-build --configuration DebugOpt Content.IntegrationTests/Content.IntegrationTests.csproj -- NUnit.ConsoleOut=0 NUnit.MapWarningTo=Failed > Scripts\logs\Content.IntegrationTests.log
pause
