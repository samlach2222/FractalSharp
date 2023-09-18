@echo off

REM -----------------------------
REM TO EXECUTE IN ADMIN MODE ONLY
REM -----------------------------
if not "%1"=="am_admin" (powershell start -verb runas '%0' am_admin & exit /b)
cd /d %~dp0


ECHO check if git is installed
git --version >nul 2>&1
IF errorlevel 1 (
    ECHO -------------------------------------------------------
    ECHO GIT IS NOT INSTALLED. PLEASE INSTALL GIT AND TRY AGAIN.
    ECHO -------------------------------------------------------
    pause
    exit
) ELSE (
    ECHO -----------------
    ECHO GIT IS INSTALLED.
    ECHO -----------------
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
ECHO ---------------
ECHO INSTALL MPIEXEC
ECHO ---------------
"MPI WINDOWS INSTALLER.exe" -unattend

ECHO --------------------------------------------------
ECHO ALL IS INSTALLED, YOU CAN NOW LAUNCH VISUAL STUDIO
ECHO --------------------------------------------------
