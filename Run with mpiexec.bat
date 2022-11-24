@echo off
REM Set "const" variables for debug and release paths
set RELEASE_PATH=bin\Release\net6.0-windows\FractalSharp.exe
set DEBUG_PATH=bin\Debug\net6.0-windows\FractalSharp.exe

for %%i in (%RELEASE_PATH%) do set releaseDate=%%~ti
for %%i in (%DEBUG_PATH%) do set debugDate=%%~ti
if "%releaseDate%"=="%debugDate%" (
	REM Check if no file exist
	if "%releaseDate%" == "" if "%debugDate%" == "" (
		echo No builds found, please generate a build
		timeout 5
		exit
	)
	
	echo Release and Debug builds have the same modified time
	echo Release will be used
	set buildPath=%RELEASE_PATH%
) else (
	REM Check if only one file exist
	if "%releaseDate%" == "%releaseDate%" if "%debugDate%" == "" (
		set buildPath=%RELEASE_PATH%
		echo Only a Release build exist
		echo Release will be used
		goto BuildKnown
	)
	if "%releaseDate%" == "" if "%debugDate%" == "%debugDate%" (
		set buildPath=%DEBUG_PATH%
		echo Only a Debug build exist
		echo Debug will be used
		goto BuildKnown
	)

	REM Get most recent build
	powershell -Command "if ([datetime]::ParseExact('%releaseDate%', 'dd/MM/yyyy HH:mm', $null) -gt [datetime]::ParseExact('%debugDate%', 'dd/MM/yyyy HH:mm', $null)) {exit 1}"
	if errorlevel 1 (
		set buildPath=%RELEASE_PATH%
		echo The Release build is more recent
		echo Release will be used
		goto BuildKnown
	) else (
		set buildPath=%DEBUG_PATH%
		echo The Debug build is more recent
		echo Debug will be used
		goto BuildKnown	
	)
)

:BuildKnown
echo.
set /p nbProcesses=Enter the number of processes : 
echo --------------------------------------------------
mpiexec -n %nbProcesses% %buildPath%