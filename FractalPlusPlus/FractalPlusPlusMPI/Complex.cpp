#include "Complex.h"
#include <cmath>

/// <summary>
/// Constructor without parameters to create a 0 + 0i number
/// </summary>
Complex::Complex() {
	this->real = 0;
	this->imag = 0;
}

/// <summary>
/// Constructor of the complex number
/// </summary>
/// <param name="real">Real part of the complex</param>
/// <param name="imag">Imaginary part of the complex</param>
Complex::Complex(double real, double imag)
{
	this->real = real;
	this->imag = imag;
}

/// <summary>
/// Calculate the modulus of the current complex number
/// </summary>
/// <returns>modulus of the current complex number</returns>
double Complex::Modulus()
{
	return sqrt(real * real + imag * imag);
}

/// <summary>
/// Return a new complex using the formula z^2 + c
/// With z the current complex and c the parameter
/// </summary>
/// <param name="c">complex added for calculating the next iteration</param>
/// <returns>next iteration of Mandelbrot</returns>
Complex Complex::NextIteration(Complex c) {
	// Do the multiplication and addition at the same time to gain time
	return Complex((real * real) - (imag * imag) + c.real, (real * imag) + (imag * real) + c.imag);
}