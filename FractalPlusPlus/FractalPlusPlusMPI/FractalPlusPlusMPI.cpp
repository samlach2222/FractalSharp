#include "mpi.h"
#include <SDL/SDL.h>
#include <iostream>
#include <string>
#include <cmath>

#include "Complex.h"

struct color {
    int r;
    int g;
    int b;
};

bool IsDiverging(color);
void CreateMandelbrotImage(color);
color GetPixelColor(int, int, int, int, double, double, double, double);

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
	// message parsing
    MPI_Status status;
	
    if (argc != 6) {
        throw new std::invalid_argument("You must pass 6 arguments : number of pixels per row, number of pixels per column, minRangeX, maxRangeX, minRangeY, maxRangeY");
    }
	
    const int pixelWidth = std::stoi(argv[0]);
    const int pixelHeight = std::stoi(argv[1]);
    double minRangeX = std::atof(argv[2]);
    double maxRangeX = std::atof(argv[3]);
    double minRangeY = std::atof(argv[4]);
    double maxRangeY = std::atof(argv[5]);
       
    // Calculate the whole Mandelbrot
    int numberOfPixels = pixelWidth * pixelHeight;
    int nPerProc = numberOfPixels / numtasks;

    if (rank == 0) {
        // Display args
		std::cout << "Arguments ";
		for (int i = 0; i < argc; i++) {
            if (i != argc - 1) {
                std::cout << argv[i] << " | ";
            }
            else
            {
                std::cout << argv[i] << "\n";
            }
            std::cout << "Calculating the Mandelbrot set\n";
            std::cout << "----------------------------------------------\n";
			
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
				if (rank == numtasks - 1) { // last task (nPerProc + numberOfPixels % nPerProc) pixels
                    localPixels = new color[nPerProc + numberOfPixels % nPerProc + 1];
                }
                else {
                    localPixels = new color[nPerProc + 1];
                }
                MPI_Recv(&localPixels, nPerProc, MPI_INT, 1, 10, MPI_COMM_WORLD, &status);
                int rank = localPixels[0].r;
                int posFirstValue = rank * nPerProc;
                
                std::cout << "Rank 0 received " << localPixels - 1 << " pixels from rank" << rank << "\n";


				int localPixelsSize = *(&localPixels + 1) - localPixels;
                for (int j = 1; j < localPixelsSize; j++)
                {
                    int iXPos = ((j - 1) + posFirstValue) % pixelWidth;
                    int iYPos = ((j - 1) + posFirstValue) / pixelWidth;
                    pixels[iXPos][iYPos] = localPixels[j];
                }
            }

            // Display pixels
            CreateMandelbrotImage(pixels);
		}
    }
    else {
        int posFirstValue = rank * nPerProc;
        if (rank == numtasks - 1)
        {
            nPerProc += numberOfPixels % numtasks;
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
        std::cout << "Rank " << rank << " is ready to send\n";
        MPI_Send(&localPixels, nPerProc + 1, MPI_INT, 0, 10, MPI_COMM_WORLD);
    }
}

bool IsDiverging(color pixel)
{
	return (pixel.r != 0 || pixel.g != 0 || pixel.b != 0);
}

void CreateMandelbrotImage(color pixels[])
{
	
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

// Exécuter le programme : Ctrl+F5 ou menu Déboguer > Exécuter sans débogage
// Déboguer le programme : F5 ou menu Déboguer > Démarrer le débogage

// Astuces pour bien démarrer : 
//   1. Utilisez la fenêtre Explorateur de solutions pour ajouter des fichiers et les gérer.
//   2. Utilisez la fenêtre Team Explorer pour vous connecter au contrôle de code source.
//   3. Utilisez la fenêtre Sortie pour voir la sortie de la génération et d'autres messages.
//   4. Utilisez la fenêtre Liste d'erreurs pour voir les erreurs.
//   5. Accédez à Projet > Ajouter un nouvel élément pour créer des fichiers de code, ou à Projet > Ajouter un élément existant pour ajouter des fichiers de code existants au projet.
//   6. Pour rouvrir ce projet plus tard, accédez à Fichier > Ouvrir > Projet et sélectionnez le fichier .sln.
