# FractalSharp

[![Project version](https://img.shields.io/badge/version-1.0-purple.svg)](https://img.shields.io/badge "Project version")

<p align="center">
  <img src="Project Repport/FractalSharp logo.svg" width="40%">
</p>

### [CLICK HERE FOR THE 🇫🇷 FRENCH VERSION 🇫🇷](README_FR.md)

## Menu
- [Introduction](#introduction)
- [Performance](#performance)
- [Compilation and startup](#compilation-and-startup)

## Introduction
### What is FractalSharp?
The FractalSharp distributed system project consists of implementing Mandelbrot's suite
suite using a distributed system. It can be run in several ways
- On 1 single process
- On a multitude of processes
- On several interconnected machines

Once the image of the Mandelbrot fractal has been calculated, it is displayed on screen and the user can
draw a rectangle (keeping the same ratio as the base image) to recalculate the Mandelbrot
Mandelbrot sequence, generate the zoomed image and display it.

### Choice of technologies
The distributed system part of FractalSharp is implemented using MPI (Message Passing Interface).
In addition, FractalSharp uses two different programming languages:
- **FractalSharp** and **FractalSharpMPI** are two programs written in C# for Windows.
MPI, we used MPI.NET from Microsoft. FractalSharp is designed and adapted for
Windows, but we need to run our program on Linux computers in order to use the
use the cluster made available to us. However, MPI.NET is very complex to compile on Linux
and dotnet is not available on the computers provided. So there are :
- **FractalPlusPlus** and **FractalPlusPlusMPI** are two programs written in C++ and are mainly designed
for Linux, to run on the cluster provided.

### Breaking down the problem
The problem can easily be broken down into two crucial points:
- The image generated by calculating the Mandelbrot sequence must be created by **a separate program**.
(The ...MPI versions of the programs), because you can't restart an MPI calculation from a program
just finished its MPI calculation (MPI processes finish the main program and can't be
cannot be recreated).
- The image display and the part of the MPI program that requests the Mandelbrot sequence calculation
program must both be in **a different Thread**, as waiting for mouse clicks to draw the
rectangle on the image would block the MPI calculation.

## Performance
### Performance comparison
The aim of our program is to propose a performance analysis of the addition of MPI
in the calculation of a fractal based on the Mandelbrot suite. We have 3 different test conditions:
- C# (Windows)
- C++ (Windows)
- C++ (Linux)

Based on these 3 conditions, we will run 6 different tests:
- Unzoomed 640x630 image
- 640x360 zoomed image
- Unzoomed 1280x720 image
- Zoomed 1280x720 image
- Unzoomed 4K (3840x2160) image
- 4K (3840x2160) zoomed image
- Very high quality image (16000x9000) unzoomed

On these first 5 tests, we took the following numbers of processes:
- C# - from 1 to 8
- C++ Windows - from 1 to 8
- C++ Linux - from 1 to 8 then 12, 16, 32 and 64

In the last image, only the C++ Linux tests have been performed.
<p align="center">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/b4a40d54-3d50-45fc-b00e-9c67207f7b94" width="40%" margin-left="1%">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/c3d68672-cffd-4d7a-86f9-4d39bf151866" width="40%" margin-right="1%">
</p>

<p align="center">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/3c909da1-c57c-4953-9add-638b7e8a3e84" width="75%">
</p>

In this first test, we have an unzoomed 640x630 image. Performance on Windows
will only be tested up to 8 processes, as our current configuration doesn't allow us to test
on more.

The performance of C# and C++ Linux gets worse as the number of processes increases.
image is very small, the time required to move data is greater than the time saved by
by parallelizing calculations. The C++ Windows version delivers excellent performance.

<p align="center">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/94c40791-028c-4be7-8ca2-6fd0b36a6e4d" width="75%">
</p>

In this second test, we have a 640x630 zoomed image. On a small image, zoomed-in performance
performance is more or less the same as on a zoomed-out image.

<p align="center">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/72424914-db25-4875-9703-d16f70b6d383" width="75%">
</p>

In this third test, we have an unzoomed **1280x720** image. On this medium-sized image
image, we're starting to see significant differences. The performance of the C# program on
Windows degrades very quickly. However, on the same operating system, C++ achieves slightly better
slightly better performance by increasing the number of MPI processes. However, the performance
does not justify the use of MPI and 8 different processes. The other big difference is on
Linux with C++. Up to now, performance has been fairly constant up to 16 processes, but deteriorated enormously beyond that.
degraded dramatically beyond that. On this image, performance, like on Windows, is better up to 16
processes, degrading beyond that, but much less so. This suggests that performance beyond
performance beyond 16 processes would be very good on large images.

<p align="center">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/2443d467-a760-4343-a1a9-dcb4e27dfc53" width="75%">
</p>

In this fourth test, we have a zoomed-in **1280x720** image. There is no significant
between the zoomed and unzoomed images.

<p align="center">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/25d9c12e-fe5f-4f04-9439-6fc9834b817e" width="75%">
</p>

In this fifth test, we have an unzoomed **4K** image. From now on, we won't be focusing on C# performance
C# performance, but C++ performance is becoming increasingly interesting.
interesting.

Times in seconds are starting to get bigger and bigger, and Windows and Linux performance is getting closer and closer.
closer together. The previous hypothesis is beginning to be confirmed.
with few MPI processes, and become much better the more processes are added. There are now
very little difference beyond 16 processes.

<p align="center">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/97e94492-aca1-49cf-acfe-715c4f6685ed" width="75%">
</p>

In this sixth test, we have a zoomed-in **4K** image. There's little change compared to the unzoomed image.
the unzoomed image, but the difference in performance between C++ Windows and Linux disappears
a little more.

<p align="center">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/3bc0f840-cd52-499a-a205-2a366846212b" width="75%">
</p>

In this seventh and final test, we have a zoomed-in **16000x9000** image. On this image, we
to demonstrate our hypothesis. We have therefore only collected C++ Linux data.
On this very large image, we can see a clear difference between using MPI and not using MPI.
not. We also observe very good performance when using 64 processes (the more processes you use, the better the performance).
processes, the better the performance).


## Compilation and startup
### Compilation procedure
To launch the program, we'll first compile it. Since FractalSharp
and FractalPlusPlus are two different programs, we will detail the compilation steps for each.
1. Go to the Source Code folder in the archive, or (optionally) clone from git [https://github.com/samlach2222/FractalSharp.git](https://github.com/samlach2222/FractalSharp.git)

**FractalSharp (Windows)**
1. Launch this batch file to install MPI and SDL : `REQUIREMENTS/Install_SDL.bat`
2. Install Visual Studio, then launch the project with the `FractalSharp.sln` file.
3. Right-click on the solution, then `Generate solution`.
4. Go to the `.\FractalSharpbin\[Release|Debug]\net6.0-windows\` folder.

**FractalPlusPlus (Windows)**
1. Launch this batch file to install MPI and SDL : `REQUIREMENTS/Install_SDL.bat`
2. Install Visual Studio, then launch the project with the `FractalPlus.sln` file.
3. Right-click on the solution, then `Generate solution`.
4. Go to the `.\x64\[Release|Debug]\` folder.

**FractalPlusPlus (Linux)**
1. Run the installation program with the command `./build_linux.sh`.
2. Go to the `.\build_linux\` folder

### Startup + Test data
We can now launch the program. On each version, there are two ways to launch the program
program. The first is the classic one, i.e. launch the GUI program (with the graphical
display). Here, you can zoom in by drawing a rectangle. The second way to launch the program
is to use only the MPI part, in which case the program is launched with command-line
command line arguments.

**On Windows:**

GUI → Run **FractalSharp.exe** or **FractalPlusGUI.exe** (depending on the programming language
programming language.

MPI → Run the **FractalSharpMPI.exe** or **FractalPlusMPI.exe** program (depending on the desired programming language) as follows
programming language) as follows:

`mpiexec -n [NombreProcessMPI] [FractalSharpMPI.exe | FractalPlusMPI.exe] [TailleX] [TailleY] [minComplexX] [maxComplexX]
[minComplexY] [maxComplexY]`

**On Linux:**

GUI → Run the **./FractalPlusPlusGUI** program.

MPI → Run the **./FractalPlusPlusMPI** program as follows 

`mpiexec -hostfile [FilenameHost] -n [NumberMPIProcess] ./FractalPlusPlusMPI [SizeX] [SizeY] [minComplexX] [maxComplexX]
[minComplexY] [maxComplexY]`

**Test data:**

For the GUI version, there isn't really any test data. This version is mainly used to check that it's working properly.
operation. You are free to zoom in as you wish. However, at some point (between the 3rd
and 4th zoom), the program displays only black. This is because we've reached the maximum number of decimal places
for our calculation algorithm.

For the MPI version, the recommended test data are as follows:

- Calculation of an unzoomed **FullHD** image: `SizeX = 1920 SizeY = 1080 minComplexX = -2 maxComplexX = 2
minComplexY = -1.125 maxComplexY = 1.125`
- Calculation of a **FullHD zoomed image**: `SizeX = 1920 SizeY = 1080 minComplexX = -1.828 maxComplexX = -1.64
minComplexY = -0.057 maxComplexY = 0.049`
- Calculation of an unzoomed **16000*9000 image**: `SizeX = 16000 SizeY = 9000 minComplexX = -2 maxComplexX = 2
minComplexY = -1.125 maxComplexY = 1.125`
