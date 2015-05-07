@echo off
cls
".nuget\NuGet.exe" "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"
".nuget\NuGet.exe" "Install" "FAKE.IIS" "-OutputDirectory" "packages" "-ExcludeVersion"

call npm install

SET TARGET="Default"

IF NOT [%1]==[] (set TARGET="%1")

"packages\FAKE\tools\Fake.exe" build.fsx "target=%TARGET%"