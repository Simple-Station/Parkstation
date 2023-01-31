cd ../../
if [ -e sloth.txt ]
then
    dotnet build -c Debug
else
    exit
fi
pause
