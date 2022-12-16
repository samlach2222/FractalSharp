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

int getScreenWidth();
int getScreenHeight();
void AskUserNbProcessMpi();
void CalculateMandelbrot(double, double, double, double);
void InitializeForm(int, int);
void DisplayPixels();

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

int main()
{
	AskUserNbProcessMpi();
	InitializeForm(pixelWidth, pixelHeight);
	CalculateMandelbrot(0, 0, pixelWidth, pixelHeight); // Calculate the whole Mandelbrot
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
	form = SDL_SetVideoMode(pixelWidth, pixelHeight, 0, SDL_OPENGL | SDL_DOUBLEBUF);
	if (!form) {
		throw std::runtime_error("Unable to set " + std::to_string(pixelWidth) + "x" + std::to_string(pixelHeight) + " video: " + SDL_GetError());
	}
	// Change form name
	SDL_WM_SetCaption("FractalPlusPlus", "FractalSharp logo.ico");
	// set background color of the SDL_Surface
	SDL_FillRect(form, NULL, SDL_MapRGB(form->format, 40, 44, 52));

	// TODO : do something with drawRectangleThreadFunc

}

/// <summary>
/// Call the MPI program to calculate the Mandelbrot with all the parameters
/// </summary>
/// <param name="P1x">Optional parameter which is the x coordinate of the first point after selecting an area to zoom in</param>
/// <param name="P1y">Optional parameter which is the y coordinate of the first point after selecting an area to zoom in</param>
/// <param name="P2x">Optional parameter which is the x coordinate of the second point after selecting an area to zoom in</param>
/// <param name="P2y">Optional parameter which is the y coordinate of the second point after selecting an area to zoom in</param>
void CalculateMandelbrot(double P1x = 0, double P1y = 0, double P2x = 0, double P2y = 0) {
	// Debug line
	std::cout << "----------------------------------------------" << std::endl;
	std::cout << "Points to calculate Mandelbrot :" << std::endl;
	std::cout << "P1x = " << P1x << ", P1y = " << P1y << "\nP2x = " << P2x << ", P2y = " << P2y << std::endl;
	std::cout << "----------------------------------------------" << std::endl;

	// Calculate the new range of the image
	double rangeX = abs(P2XinAxe - P1XinAxe);
	double rangeY = abs(P2YinAxe - P1YinAxe);
	// Display the new range
	std::cout << "Range of the Mandelbrot set :" << std::endl;
	std::cout << "rangeX = " << rangeX << ", rangeY = " << rangeY << std::endl;
	std::cout << "----------------------------------------------" << std::endl;

	double localP1XinAxe = P1x / pixelWidth * rangeX - rangeX / 2;
	double localP1YinAxe = P1y / pixelHeight * rangeY - rangeY / 2;
	double localP2XinAxe = P2x / pixelWidth * rangeX - rangeX / 2;
	double localP2YinAxe = P2y / pixelHeight * rangeY - rangeY / 2;

	// Stock the new range of the image for the next image
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

	DisplayPixels();
}

/// <summary>
/// This method creates a Bitmap image with the pixels and display it in the form.
/// This method also let the user zoom in the Mandelbrot image by selecting an area.
/// </summary>
void DisplayPixels() {
	// Change the image to the generated Mandelbrot image
#if defined(WIN32) || defined(_WIN32) || defined(__WIN32) && !defined(__CYGWIN__)
	std::string path = std::filesystem::temp_directory_path().string() + "Mandelbrot.bmp";
#else
	std::string path = "/tmp/Mandelbrot.bmp";
#endif
	SDL_Surface* image = SDL_LoadBMP(path.c_str());
	if (!image) {
		throw std::runtime_error(std::string("Error loading image: ") + SDL_GetError());
	}
	rectangleFinished = false;

	// Display the image in the form
	SDL_BlitSurface(image, NULL, form, NULL);
	SDL_Flip(form);
}

/// <summary>
/// Method of the SDL window to draw the rectangle when the user is selecting an area to zoom in
/// and to calculate the Mandelbrot image when the user has finished selecting an area.
/// </summary>
/// <param name="data"></param>
/// <returns>exit code</returns>
int drawRectangleThreadFunc(void* data) {
	// user mouse events
	int P1x = 0;
	int P1y = 0;
	int P2x = 0;
	int P2y = 0;
	bool mouseDown = false;
	bool mouseUp = false;
	bool mouseMove = false;
	int mouseX = 0;
	int mouseY = 0;
	SDL_Event event;

	while (true) {
		// wait for mouse click event
		while (SDL_PollEvent(&event)) {
			switch (event.type) {
			case SDL_MOUSEBUTTONDOWN:
				if (!rectangleFinished) {
					P1x = event.button.x;
					P1y = event.button.y;
					std::cout << "P1 points at (" + std::to_string(P1x) + ", " + std::to_string(P1y) + ")" << std::endl;
				}
				//mouseDown = true;
				//mouseX = event.button.x;
				//mouseY = event.button.y;
				break;
			case SDL_MOUSEBUTTONUP:
				if (!rectangleFinished) {
					if (event.button.x < 0 || event.button.x > pixelWidth || event.button.y < 0 || event.button.y > pixelHeight) {
						rectangleFinished = false;
					}
					else
					{
						std::cout << "P2 points at (" + std::to_string(P2x) + ", " + std::to_string(P2y) + ")" << std::endl;
						std::cout << "----------------------------------------------" << std::endl;
						std::cout << "\n\n\n\n" << std::endl;

						rectangleFinished = true;

						CalculateMandelbrot(P1x, P1y, P2x, P2y); // Execute CalculateMandelbrot in Main Thread (possible memory problem here)

						P1x = 0;
						P1y = 0;
						P2x = 0;
						P2y = 0;
					}
				}
				//mouseUp = true;
				//mouseX = event.button.x;
				//mouseY = event.button.y;
				break;
			case SDL_MOUSEMOTION:
				if (P1x != 0 && P1y != 0 && !rectangleFinished)
				{
					P2x = event.button.x;
					P2y = event.button.y;
				}
				//mouseMove = true;
				//mouseX = event.button.x;
				//mouseY = event.button.y;
				break;
			case SDL_QUIT:
				break;
			}
		}
		// DRAWING STARTS HERE
	}
	return 0;
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
