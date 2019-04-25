pushd %~dp0
nuget install OpenCover -Version 4.7.922 -OutputDirectory coverage\tools
nuget install ReportGenerator -Version 4.1.4 -OutputDirectory coverage\tools
dotnet build Source\AirBreather.Common -c Release
FOR /F "tokens=* USEBACKQ" %%D IN (`where dotnet`) DO (
SET DotNetPath=%%D
)
coverage\tools\OpenCover.4.7.922\tools\OpenCover.Console.exe ^
    "-target:%DotNetPath%" ^
    -targetdir:Source\AirBreather.Common ^
    "-targetArgs:test AirBreather.Common.sln -c Release --no-build" ^
    "-filter:+[AirBreather]* +[AirBreather.*]* -[AirBreather.Common.Tests]*" ^
    -output:coverage\raw-coverage-results.xml ^
    -register:user ^
    -oldstyle

dotnet clean Source\AirBreather.Common -c Release

dotnet coverage\tools\ReportGenerator.4.1.4\tools\netcoreapp2.1\ReportGenerator.dll ^
    -reports:coverage\raw-coverage-results.xml ^
    -targetdir:coverage\results

coverage\results\index.htm

popd
