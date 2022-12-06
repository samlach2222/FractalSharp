#include <iostream>
#include <string>
#include <cmath>
#include <filesystem>
#include <mpi.h>
#include <SDL/SDL.h>
#undef main // Needed to overwrite the overwritten main method by SDL

#include "Complex.h"


typedef struct color {
    int r;
    int g;
    int b;
} color ;

/// <summary>
/// width of the image
/// </summary>
int pixelWidth;

/// <summary>
/// height of the image
/// </summary>
int pixelHeight;

int main(int, char* []);
bool IsDiverging(color);
void CreateMandelbrotImage(color**);
color GetPixelColor(int, int, int, int, double, double, double, double);
void defineStruct(MPI_Datatype* tstype);

/// <summary>
/// Main method of the program
/// </summary>
/// <param name="argc">number of arguments</param>
/// <param name="argv">
/// Arguments passed in parameters :
/// First is number of pixels per row
/// Second is number of pixels per column
/// Third is minRangeX
/// Fourth is maxRangeX
/// Fifth is minRangeY
/// Sixth is maxRangeY</param>
/// <returns>exit code</returns>
int main(int argc, char* argv[])
{
    // MPI vars
    int numtasks, rank;
    // initialize MPI
    MPI_Init(&argc, &argv);
    // get number of tasks
    MPI_Comm_size(MPI_COMM_WORLD, &numtasks);
    // get my rank
    MPI_Comm_rank(MPI_COMM_WORLD, &rank);
    // declare MPI type for the color struct
    MPI_Datatype colorType;
    // initialize MPI type for the color struct
    defineStruct(&colorType);
	// message parsing
    MPI_Status status;

#ifdef _DEBUG
    // test vals
    pixelWidth = std::stoi("1280");
    pixelHeight = std::stoi("720");
    double minRangeX = std::atof("-2.0");
    double maxRangeX = std::atof("2.0");
    double minRangeY = std::atof("-1.125");
    double maxRangeY = std::atof("1.125");
#else
    if (argc != 7) { // not 6 because in CPP argv[0] is the path of the exe file
        throw new std::invalid_argument("You must pass 6 arguments : number of pixels per row, number of pixels per column, minRangeX, maxRangeX, minRangeY, maxRangeY");
    }

    pixelWidth = std::stoi(argv[1]);
    pixelHeight = std::stoi(argv[2]);
    double minRangeX = std::atof(argv[3]);
    double maxRangeX = std::atof(argv[4]);
    double minRangeY = std::atof(argv[5]);
    double maxRangeY = std::atof(argv[6]);
#endif
       
    // Calculate the whole Mandelbrot
    int numberOfPixels = pixelWidth * pixelHeight;
    int nPerProc = numberOfPixels / numtasks;

    if (rank == 0) {
        // Display args
		std::cout << "Arguments ";
        for (int i = 1; i < argc; i++) {
            if (i != argc - 1) {
                std::cout << argv[i] << " | ";
            }
            else
            {
                std::cout << argv[i] << std::endl;
            }
        }
        std::cout << "Calculating the Mandelbrot set" << std::endl;
        std::cout << "----------------------------------------------" << std::endl;
		
        // Create array of pixels
        color** pixels = new color * [pixelWidth];
        for (int i = 0; i < pixelWidth; i++) {

            // Declare a memory block
            // of size n
            pixels[i] = new color[pixelHeight];
        }
		
        // Calculate rank 0's part
        for (int i = 0; i < nPerProc; i++)
        {
            int iXPos = i % pixelWidth;
            int iYPos = i / pixelWidth;

            color px = GetPixelColor(iXPos, iYPos, pixelWidth, pixelHeight, minRangeX, maxRangeX, minRangeY, maxRangeY);
            pixels[iXPos][iYPos] = px;
        }

        // Receive localPixels from other ranks
        for (int i = 1; i < numtasks; i++)
        {
            color* localPixels;
            int localPixelsSize = 0;

			if (rank == numtasks - 1) { // last task (nPerProc + numberOfPixels % nPerProc) pixels
                localPixelsSize = nPerProc + numberOfPixels % nPerProc + 1;
                localPixels = (color*)malloc(sizeof(color) * localPixelsSize);
                MPI_Recv(localPixels, localPixelsSize, colorType, i, 10, MPI_COMM_WORLD, &status);
            }
            else {
                localPixelsSize = nPerProc + 1;
                localPixels = (color*)malloc(sizeof(color) * localPixelsSize);
                MPI_Recv(localPixels, localPixelsSize, colorType, i, 10, MPI_COMM_WORLD, &status);
            }
            int rank = localPixels[0].r;
            int posFirstValue = rank * nPerProc;
            
            std::cout << "Rank 0 received " << localPixelsSize << " pixels from rank" << rank << std::endl;

            for (int j = 1; j < localPixelsSize; j++)
            {
                int iXPos = ((j - 1) + posFirstValue) % pixelWidth;
                int iYPos = ((j - 1) + posFirstValue) / pixelWidth;
                pixels[iXPos][iYPos] = localPixels[j]; // Error here, impossible to receive the struct
            }

            free(localPixels);
        }

        // Display pixels
        CreateMandelbrotImage(pixels);
    }
    else {
        int posFirstValue = rank * nPerProc;
        if (rank == numtasks - 1)
        {
            nPerProc += numberOfPixels % nPerProc;
        }

        color* localPixels = new color[nPerProc + 1]; // + 1 cell to include the rank number
		
        localPixels[0] = { rank, rank, rank }; // Put rank number in the first cell
        for (int i = 1; i < nPerProc + 1; i++)
        {
            int iXPos = ((i - 1) + posFirstValue) % pixelWidth;
            int iYPos = ((i - 1) + posFirstValue) / pixelWidth;
            color px = GetPixelColor(iXPos, iYPos, pixelWidth, pixelHeight, minRangeX, maxRangeX, minRangeY, maxRangeY);
            localPixels[i] = px;
        }

        // Send localPixels to rank 0
        std::cout << "Rank " << rank << " is ready to send " << nPerProc + 1 << " pixels." << std::endl;
        MPI_Send(localPixels, nPerProc + 1, colorType, 0, 10, MPI_COMM_WORLD); // (nPerProc + 1)*3 --> 3 is for r, g and b Int
    }

    // Done with MPI
    MPI_Finalize();
    return 0;
}

/// <summary>
/// define MPI type with struct color
/// </summary>
/// <param name="tstype">MPI type</param>
void defineStruct(MPI_Datatype* colorType) {
    const int count = 3;
    int blocklens[count] = { 1, 1, 1 };
    MPI_Datatype types[count] = { MPI_INT, MPI_INT, MPI_INT };
    MPI_Aint disps[count] = { offsetof(color,r), offsetof(color,g), offsetof(color,b) };

    MPI_Type_create_struct(count, blocklens, disps, types, colorType);
    MPI_Type_commit(colorType);
}

/// <summary>
/// Method to know if the Mandelbrot sequence diverge
/// </summary>
/// <returns>true if diverging (color isn't black)</returns>
bool IsDiverging(color pixel)
{
	return (pixel.r != 0 || pixel.g != 0 || pixel.b != 0);
}

/// <summary>
/// This method creates a Bitmap image with the pixels and display it in the form.
/// This method also let the user zoom in the Mandelbrot image by selecting an area.
/// </summary>
/// <param name="pixels">2D array of PixelColor (r,g,b) which contains the color of each pixel</param>
void CreateMandelbrotImage(color** pixels)
{
	// Create the surface
    SDL_Surface* surface;
    Uint32 rmask, gmask, bmask, amask;

#if SDL_BYTEORDER == SDL_BIG_ENDIAN
    rmask = 0xff000000;
    gmask = 0x00ff0000;
    bmask = 0x0000ff00;
    amask = 0x00000000;
#else
    rmask = 0x000000ff;
    gmask = 0x0000ff00;
    bmask = 0x00ff0000;
    amask = 0x00000000;
#endif
    surface = SDL_CreateRGBSurface(SDL_SWSURFACE, pixelWidth, pixelHeight, 32, rmask, gmask, bmask, amask);
    if (surface == NULL) {
        fprintf(stderr, "CreateRGBSurface failed: %s\n", SDL_GetError());
        exit(1);
    }
	
    // Fill the Bitmap with black, so we only need to set the pixels where Mandelbrot is diverging
	SDL_FillRect(surface, NULL, SDL_MapRGB(surface->format, 0, 0, 0));

    // Set diverging pixels
    for (int i = 0; i < pixelWidth; i++)
    {
        for (int j = 0; j < pixelHeight; j++)
        {
            if (IsDiverging(pixels[i][j]))
            {
				unsigned char* surfacePixels = (unsigned char*)surface->pixels;
                surfacePixels[4 * (j * pixelWidth + i) + 0] = pixels[i][j].b; // blue
                surfacePixels[4 * (j * pixelWidth + i) + 1] = pixels[i][j].g; // green
                surfacePixels[4 * (j * pixelWidth + i) + 2] = pixels[i][j].r; // red
            }
        }
    }
    
    // save file
#if defined(WIN32) || defined(_WIN32) || defined(__WIN32) && !defined(__CYGWIN__)
    std::string path = std::filesystem::temp_directory_path().string() + "Mandelbrot.bmp";
#else
    std::string path = "/tmp/Mandelbrot.bmp";
#endif
	SDL_SaveBMP(surface, path.c_str());
	std::cout << "Mandelbrot image saved in " << path << std::endl;
    std::cout << "----------------------------------------------" << std::endl;
}



/// <summary>
/// This method calculates the color of a pixel and returns it.
/// The pixel is black if the sequence converge.
/// The pixel is in other colors (gray scale) if the sequence diverge.
/// </summary>
/// <param name="iXpos">X position of the pixel</param>
/// <param name="iYpos">Y position of the pixel</param>
/// <param name="pixelWidth">number of pixels in width</param>
/// <param name="pixelHeight">number of pixels in height</param>
/// <param name="rangeX">range of the X axis (example : 4 is for [-2, 2])</param>
/// <param name="rangeY">range of the Y axis (example : 4 is for [-2, 2])</param>
/// <returns>color of the pixel</returns>
color GetPixelColor(int iXpos, int iYpos, int pixelWidth, int pixelHeight, double minRangeX, double maxRangeX, double minRangeY, double maxRangeY)
{
    // Calculate if Mandelbrot sequence diverge

    double rangeXPos = (double)iXpos / (double)pixelWidth * (maxRangeX - minRangeX) + minRangeX;
    double rangeYPos = (double)iYpos / (double)pixelHeight * (maxRangeY - minRangeY) + minRangeY;
    // DEBUG : print rangeXPos and rangeYPos
    //Console.WriteLine("rangeXPos = {0}, rangeYPos = {1}", rangeXPos, rangeYPos);

    Complex c = Complex(rangeXPos, rangeYPos);
    Complex z = Complex(0, 0);

    int iteration = 0;
    const int maxIteration = 1000;
    while (iteration < maxIteration && z.Modulus() <= 2) // AND Z mod 2 < 2
    {
        // Max iteration --> If not diverge
        // z mod 2 < 2 --> If diverge
        z = z.NextIteration(c);
        iteration++;
    }
    if (iteration == maxIteration)
    {
        return color{ 0, 0, 0 };
    }
    else
    {
        // Color smoothing Mandelbrot (a little bit)
        double log_zn = log(z.Modulus());
        double nu = log(log_zn / log(2)) / log(2);
        iteration = iteration + 1 - (int)nu;

        // Gray gradient with color smoothing
        int colorValue = (int)(255.0 * sqrt((double)iteration / (double)maxIteration));
        return color{colorValue, colorValue, colorValue};
    }
}
