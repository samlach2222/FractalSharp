// FractalPlusPlusMPI.cpp : Ce fichier contient la fonction 'main'. L'exécution du programme commence et se termine à cet endroit.
//

#include <iostream>
#include <mpi.h>
#include <SDL/SDL.h>
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

int main()
{
    std::cout << "Hello World!\n";
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
