@echo off

cd /d %~dp0

ECHO -----------------------
ECHO INSTALL MPI EXECUTABLES
ECHO -----------------------
ECHO Last version of executables (and SDK) can be downloaded at https://learn.microsoft.com/en-us/message-passing-interface/microsoft-mpi-release-notes
ECHO.
ECHO You will need administrator privileges for installation
"MPI WINDOWS INSTALLER.exe" -unattend

ECHO ----------------------------------------
ECHO EXTRACT NUGET PACKAGES (SDL AND MPI SDK)
ECHO ----------------------------------------
powershell -Command "Expand-Archive .\packages.zip -Force -DestinationPath '..\FractalPlusPlus'"

ECHO --------------------------------------------
ECHO YOU CAN NOW BUILD WITH VISUAL STUDIO AND RUN
ECHO --------------------------------------------
timeout 8