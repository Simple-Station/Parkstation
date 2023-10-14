cd ..\..\
dotnet restore
dotnet build --configuration DebugOpt --no-restore /p:WarningsAsErrors=nullable /m
mkdir Scripts\logs
del Scripts\logs\Content.YAMLLinter.log
dotnet run --project Content.YAMLLinter/Content.YAMLLinter.csproj --no-build -- NUnit.ConsoleOut=0 > Scripts\logs\Content.YAMLLinter.log
pause
