@echo off
if not exist .\coverage\* mkdir .\coverage
.\packages\OpenCover.4.5.3607-rc27\OpenCover.Console.exe -register:user -filter:"+[AirBreather.Core]* -[AirBreather.Core.Tests]*" "-target:.\packages\xunit.runners.2.0.0-rc1-build2826\tools\xunit.console.exe" "-targetargs:.\AirBreather.Core.Tests\bin\Debug\AirBreather.Core.Tests.dll xunit.xml -noshadow" -output:.\coverage\results.xml
.\packages\ReportGenerator.2.1.0-beta2\ReportGenerator.exe "-reports:.\coverage\results.xml" "-targetdir:.\coverage"
.\coverage\index.htm