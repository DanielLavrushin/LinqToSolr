@echo off

SETLOCAL

::::::::::::::::::::::::::::::::::::::::::::::::: CONSTANT :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: source folder
SET srcFolder=LinqToSolr\bin\
 
:: dll file name
SET "dllName=LinqToSolr.dll"

:: Nuspec Name
SET "nuspecName=LinqToSolr.nuspec"

:: Nuget Name (left part before version)
SET "nugetPackageName=LinqToSolr"


::::::::::::::::::::::::::::::::::::::::::::::::: CONSTANT :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


CHOICE /C RD /M "Select What to build: R - Release, D - Debug"
IF ERRORLEVEL 1 SET buildType=Release
IF ERRORLEVEL 2 SET buildType=Debug

SET srcFolder=%srcFolder%%buildType%\net452\

:: GET THE CURRENT FOLDER
SET currentDirecotyPath="%cd%"


:: GET THE PATH WITHOUT BLD FOLDER
FOR %%F in (%currentDirecotyPath%) DO SET myPath="%%~dpF"

SET pathWhereSerch=%myPath%%srcFolder%

::ECHO %PATHWHERESERCH%
SET pathWhereSerch=%pathWhereSerch:"=%
ECHO %pathWhereSerch%

:: FIND THE FILE IN THE FOULD SEARCH
FOR /r "%pathWhereSerch%" %%a in (*) do if "%%~nxa"=="%dllName%" SET pathFile=%%~dpnxa




:: RELACE IN THE PATH \ WITH \\
SET pathFile=%pathFile:\=\\%
::echo %pathFile%

:: GET THE VERSION FROM DLL 
FOR /F "delims== tokens=2" %%x IN ('WMIC DATAFILE WHERE "name='%pathFile%'" get  Version /format:Textvaluelist')  DO (SET NugetFileVersion=%%x)

:: CREATE NUGET PAKAGE
echo IPRestriction Version  %NugetFileVersion%  will be create

nuget pack %nuspecName% -Version %NugetFileVersion% -properties Configuration=%buildType%

PAUSE
