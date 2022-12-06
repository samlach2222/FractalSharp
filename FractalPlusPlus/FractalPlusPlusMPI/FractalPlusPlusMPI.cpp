// FractalPlusPlusMPI.cpp : Ce fichier contient la fonction 'main'. L'exécution du programme commence et se termine à cet endroit.
//

#include <iostream>
#include <mpi.h>
#include <SDL/SDL.h>

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

color GetPixelColor(int iXpos, int iYpos, int pixelWidth, int pixelHeight, double minRangeX, double maxRangeX, double minRangeY, double maxRangeY)
{
	return color();
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
