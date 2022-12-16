#include <iostream>
#include <SDL/SDL.h>
#include <SDL/SDL_thread.h>
#undef main // Needed to overwrite the overwritten main method by SDL

#if defined(WIN32) || defined(_WIN32) || defined(__WIN32) && !defined(__CYGWIN__)
#include <windows.h>
#include <tchar.h>
#else
#include <sys/types.h>
#include <sys/wait.h>
#include <spawn.h>
#endif
#include <string>
#include <filesystem>
#include <thread>

void AskUserNbProcessMpi();
void CalculateMandelbrot(double, double, double, double);
void InitializeForm(int, int);
int WindowLoop();
void SetMandelbrotImage();
int getScreenWidth();
int getScreenHeight();

/// <summary>
/// Form where the mandelbrot image is displayed
/// </summary>
SDL_Surface* form;

/// <summary>
/// Width of the main screen in pixel
/// </summary>
int screenWidth = getScreenWidth();

/// <summary>
/// Height of the main screen in pixel
/// </summary>
int screenHeight = getScreenHeight();

/// <summary>
/// Ratio size of the image.
/// Here 80% of the screen size.
/// </summary>
constexpr double ratioImage = 0.8;

/// <summary>
/// Width in pixel of the image
/// </summary>
int pixelWidth = (int)(screenWidth * ratioImage);

/// <summary>
/// Height in pixel of the image
/// </summary>
int pixelHeight = (int)(screenHeight * ratioImage);

bool rectangleFinished = true;

/// <summary>
/// X coord of P1 in the axe
/// </summary>
double P1XinAxe = -2.0;

/// <summary>
/// Y coord of P1 in the axe
/// </summary>
double P1YinAxe = P1XinAxe * pixelHeight / pixelWidth;

/// <summary>
/// X coord of P2 in the axe
/// </summary>
double P2XinAxe = 2.0;

/// <summary>
/// Y coord of P2 in the axe
/// </summary>
double P2YinAxe = P2XinAxe * pixelHeight / pixelWidth;

/// <summary>
/// The number of process we use in MPI
/// 1 is the default value and it's to use the program without MPI
/// </summary>
int nbProcessMpi = 1;

/// <summary>
/// Main method of the program
/// </summary>
/// <returns>exit code</returns>
int main()
{
	AskUserNbProcessMpi();
	InitializeForm(pixelWidth, pixelHeight);
	CalculateMandelbrot(0, 0, pixelWidth, pixelHeight); // Calculate a Mandelbrot image before entering the SDL window loop
	WindowLoop();
	return 0;
}

/// <summary>
/// Ask the user the number of process to use
/// 1 is without MPI
/// </summary>
void AskUserNbProcessMpi() {
	do
	{
		std::cout << "Type the number of MPI processes you want to use (1 is without MPI) : ";
		std::cin >> nbProcessMpi;

		if (nbProcessMpi < 1)
		{
			std::cout << "\nYou must type a number greater than 0" << std::endl;
		}
	} while (nbProcessMpi < 1);
}

/// <summary>
/// This method initialize the form where the user is able to see and zoom in the Mandelbrot image
/// </summary>
/// <param name="pixelWidth">the width of the Mandelbrot image in pixels. It's also the width of the form's content</param>
/// <param name="pixelHeight">the height of the Mandelbrot image in pixels. It's also the height of the form's content</param>
void InitializeForm(int pixelWidth, int pixelHeight) {
	
	if (SDL_Init(SDL_INIT_VIDEO) < 0) {
		throw std::runtime_error(std::string("Unable to init SDL: ") + SDL_GetError());
	}
	
	// make sure SDL cleans up before exit
	atexit(SDL_Quit);
	// create a new window
	form = SDL_SetVideoMode(pixelWidth, pixelHeight, 0, SDL_HWSURFACE | SDL_DOUBLEBUF);
	if (!form) {
		throw std::runtime_error("Unable to set " + std::to_string(pixelWidth) + "x" + std::to_string(pixelHeight) + " video: " + SDL_GetError());
	}
	
	// Change form name and icon
	SDL_WM_SetCaption("FractalPlusPlus", "FractalSharp logo.ico");
}

/// <summary>
/// Call the MPI program to calculate the Mandelbrot with all the parameters
/// </summary>
/// <param name="P1x">Optional parameter which is the x coordinate of the first point after selecting an area to zoom in</param>
/// <param name="P1y">Optional parameter which is the y coordinate of the first point after selecting an area to zoom in</param>
/// <param name="P2x">Optional parameter which is the x coordinate of the second point after selecting an area to zoom in</param>
/// <param name="P2y">Optional parameter which is the y coordinate of the second point after selecting an area to zoom in</param>
void CalculateMandelbrot(double P1x = 0, double P1y = 0, double P2x = 0, double P2y = 0) {
	// Calculate the previous absolute range of the image
	double rangeX = abs(P2XinAxe - P1XinAxe);
	double rangeY = abs(P2YinAxe - P1YinAxe);

	// Calculate the new range
	double localP1XinAxe = P1x / pixelWidth * rangeX - rangeX / 2;
	double localP1YinAxe = P1y / pixelHeight * rangeY - rangeY / 2;
	double localP2XinAxe = P2x / pixelWidth * rangeX - rangeX / 2;
	double localP2YinAxe = P2y / pixelHeight * rangeY - rangeY / 2;

	// Reorder the points in case the user selected the area from bottom to top and/or from right to left
	if (localP1XinAxe < localP2XinAxe)
	{
		P1XinAxe = localP1XinAxe;
		P2XinAxe = localP2XinAxe;
	}
	else
	{
		P1XinAxe = localP2XinAxe;
		P2XinAxe = localP1XinAxe;
	}
	if (localP1YinAxe < localP2YinAxe)
	{
		P1YinAxe = localP1YinAxe;
		P2YinAxe = localP2YinAxe;
	}
	else
	{
		P1YinAxe = localP2YinAxe;
		P2YinAxe = localP1YinAxe;
	}

	// Display the new range
	std::cout << "----------------------------------------------" << std::endl;
	std::cout << "Range of the Mandelbrot set :" << std::endl;
	std::cout << "rangeX = " << P2XinAxe - P1XinAxe << ", rangeY = " << P2YinAxe - P1YinAxe << std::endl;
	std::cout << "--------------------------------------------------" << std::endl;

	// Execute the MPI program to generate the Mandelbrot image

#if defined(WIN32) || defined(_WIN32) || defined(__WIN32) && !defined(__CYGWIN__)
	constexpr char FPPExeName[] = "FractalPlusPlusMPI.exe";
#else
	constexpr char FPPExeName[] = "./FractalPlusPlusMPI";  // In linux we need to prepend "./" when it's in the current directory
#endif
	constexpr char MPIExeName[] = "mpiexec";
	std::string commandeString;

	if (nbProcessMpi == 1)
	{
		// Store the command in the commandeString variable
		commandeString = std::string(FPPExeName) + ' ' + std::to_string(pixelWidth) + ' ' + std::to_string(pixelHeight) + ' ' + std::to_string(P1XinAxe) + ' ' + std::to_string(P2XinAxe) + ' ' + std::to_string(P1YinAxe) + ' ' + std::to_string(P2YinAxe);
	}
	else
	{
		// Store the command in the commandeString variable
		commandeString = std::string(MPIExeName) + " -n " + std::to_string(nbProcessMpi) + ' ' + std::string(FPPExeName) + ' ' + std::to_string(pixelWidth) + ' ' + std::to_string(pixelHeight) + ' ' + std::to_string(P1XinAxe) + ' ' + std::to_string(P2XinAxe) + ' ' + std::to_string(P1YinAxe) + ' ' + std::to_string(P2YinAxe);
	}

	std::cout.flush();  // Flush the terminal buffer before calling the system method to avoid mixing the output of the two programs
	system(commandeString.c_str());

	SetMandelbrotImage();
}

/// <summary>
/// Looping method of the SDL window to draw the rectangle when the user is selecting an area to zoom in
/// and to calculate the Mandelbrot image when the user has finished selecting an area.
/// </summary>
/// <returns>exit code</returns>
int WindowLoop() {
	// user mouse events
	int P1x = -1;
	int P1y = -1;
	int P2x = -1;
	int P2y = -1;
	SDL_Event event;
	bool running = true;

	while (running) {
		// wait for mouse click event
		while (SDL_PollEvent(&event)) {
			switch (event.type) {
				case SDL_MOUSEBUTTONDOWN:
					if (!rectangleFinished) {
						P1x = event.button.x;
						P1y = event.button.y;
						std::cout << "\n\n\n\n" << std::endl;  // Separate the Mandelbrot image generations
						std::cout << "--------------------------------------------------" << std::endl;
						std::cout << "P1 points at (" + std::to_string(P1x) + ", " + std::to_string(P1y) + ")" << std::endl;
					}
					break;
				case SDL_MOUSEBUTTONUP:
					if (!rectangleFinished) {
						if (event.button.x < 0 || event.button.x > pixelWidth || event.button.y < 0 || event.button.y > pixelHeight) {
							rectangleFinished = false;
						}
						else
						{
							std::cout << "P2 points at (" + std::to_string(P2x) + ", " + std::to_string(P2y) + ")" << std::endl;

							rectangleFinished = true;

							CalculateMandelbrot(P1x, P1y, P2x, P2y); // Generate the Mandelbrot image with the selected area

							P1x = -1;
							P1y = -1;
							P2x = -1;
							P2y = -1;
						}
					}
					break;
				case SDL_MOUSEMOTION:
					if (P1x != -1 && P1y != -1 && !rectangleFinished)  // Check if the user is holding a click and hasn't released yet
					{
						P2x = event.button.x;
						P2y = event.button.y;
					}
					break;
				case SDL_QUIT:
					running = false;  // End the loop to exit the program
					break;
			}
		}
		SDL_Flip(form);
	}
	return 0;
}

/// <summary>
/// Set the newly generated Mandelbrot image in the SDL hidden buffer
/// Calling SDL_Flip(form) is needed to swap the buffers and display the image
/// </summary>
void SetMandelbrotImage() {
	// Get the path of the Mandelbrot image
#if defined(WIN32) || defined(_WIN32) || defined(__WIN32) && !defined(__CYGWIN__)
	std::string path = std::filesystem::temp_directory_path().string() + "Mandelbrot.bmp";
#else
	std::string path = "/tmp/Mandelbrot.bmp";
#endif
	SDL_Surface* image = SDL_LoadBMP(path.c_str());
	if (!image) {
		throw std::runtime_error(std::string("Error loading image: ") + SDL_GetError());
	}
	
	rectangleFinished = false;  // Reset the rectangleFinished variable to allow the user to select a new area to zoom in

	// Display the image (in the hidden buffer)
	SDL_BlitSurface(image, NULL, form, NULL);
}

/// <summary>
/// Get the screen width in pixels
/// </summary>
/// <returns>screen width in pixels</returns>
int getScreenWidth() {
	SDL_Init(SDL_INIT_EVERYTHING);
	const SDL_VideoInfo* info = SDL_GetVideoInfo();
	return info->current_w;
}

/// <summary>
/// Get the screen height in pixels
/// </summary>
/// <returns>screen height in pixels</returns>
int getScreenHeight() {
	SDL_Init(SDL_INIT_EVERYTHING);
	const SDL_VideoInfo* info = SDL_GetVideoInfo();
	return info->current_h;
}
