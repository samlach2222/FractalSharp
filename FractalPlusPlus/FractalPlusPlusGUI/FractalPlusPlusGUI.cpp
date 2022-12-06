#include <iostream>
#include <SDL/SDL.h>
#undef main

int getScreenWidth();
int getScreenHeight();
void AskUserNbProcessMpi();
void CalculateMandelbrot(double, double, double, double);
void InitializeForm(int, int);
void DisplayLoadingScreen();
void DisplayPixels();

/// <summary>
/// PictureBox where the fractal is drawn
/// </summary>
//PictureBox pictureBox = new();

/// <summary>
/// Form where the PictureBox is 
/// </summary>
//Form form = new();

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
	
	return 0;
}

int getScreenWidth() {
	SDL_Init(SDL_INIT_EVERYTHING);
	const SDL_VideoInfo* info = SDL_GetVideoInfo();  
	return info->current_w;
}

int getScreenHeight() {
	SDL_Init(SDL_INIT_EVERYTHING);
	const SDL_VideoInfo* info = SDL_GetVideoInfo();
	return info->current_h;
}

/// <summary>
/// Ask the user the number of process to use
/// 1 is without MPI
/// </summary>
void AskUserNbProcessMpi() {

}

/// <summary>
/// Call the MPI program to calculate the Mandelbrot with all the parameters
/// </summary>
/// <param name="P1x">Optional parameter which is the x coordinate of the first point after selecting an area to zoom in</param>
/// <param name="P1y">Optional parameter which is the y coordinate of the first point after selecting an area to zoom in</param>
/// <param name="P2x">Optional parameter which is the x coordinate of the second point after selecting an area to zoom in</param>
/// <param name="P2y">Optional parameter which is the y coordinate of the second point after selecting an area to zoom in</param>
void CalculateMandelbrot(double P1x = 0, double P1y = 0, double P2x = 0, double P2y = 0) {

}

/// <summary>
/// This method initialize the form where the user is able to see and zoom in the Mandelbrot image
/// </summary>
/// <param name="pixelWidth">the width of the Mandelbrot image in pixels. It's also the width of the form's content</param>
/// <param name="pixelHeight">the height of the Mandelbrot image in pixels. It's also the height of the form's content</param>
void InitializeForm(int pixelWidth, int pixelHeight) {

}

/// <summary>
/// This method display loading.gif in the form while waiting for the end of the calculation
/// </summary>
void DisplayLoadingScreen() {
	
}

/// <summary>
/// This method creates a Bitmap image with the pixels and display it in the form.
/// This method also let the user zoom in the Mandelbrot image by selecting an area.
/// </summary>
/// <param name="pixels">2D array of PixelColor (r,g,b) which contains the color of each pixel</param>
void DisplayPixels() {
	
}