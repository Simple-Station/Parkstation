cd ../../
if [ -e sloth.txt ]
then
    dotnet build -c Release
else
    exit
fi
pause
