@echo off

cd /d %~dp0

ECHO ---------------
ECHO INSTALL MPIEXEC
ECHO ---------------
echo You will need administrator privileges for installation
REM MPI is needed for installing MPI packages with vcpkg
"MPI WINDOWS INSTALLER.exe" -unattend

git --version >nul 2>&1
IF errorlevel 1 (
    ECHO -------------------------------------------------------
    ECHO GIT IS NOT INSTALLED. PLEASE INSTALL GIT AND TRY AGAIN.
    ECHO -------------------------------------------------------
    pause
    exit
)

ECHO ------------------
ECHO INSTALL VCPKG BASE
ECHO ------------------
git clone https://github.com/Microsoft/vcpkg.git
cd vcpkg
cmd /c bootstrap-vcpkg.bat -disableMetrics
ECHO ----------------------
ECHO INSTALL VCPKG PACKAGES
ECHO ----------------------
vcpkg install sdl1:x64-windows
vcpkg install sdl1:x86-windows
vcpkg install mpi:x64-windows
vcpkg install mpi:x86-windows
vcpkg integrate install

ECHO --------------------------------------------------
ECHO ALL IS INSTALLED, YOU CAN NOW LAUNCH VISUAL STUDIO
ECHO --------------------------------------------------
timeout 8