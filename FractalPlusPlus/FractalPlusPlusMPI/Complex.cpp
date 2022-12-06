#include "Complex.h"
#include <cmath>

/// <summary>
/// Class to represent a complex number
/// </summary>
class Complex {
	private :
		/// <summary>
		/// Real part of the complex
		/// </summary>
		double real;

		/// <summary>
		/// Imaginary part of the complex
		/// </summary>
		double imag;
		
	public :
		/// <summary>
		/// Constructor without parameters to create a 0 + 0i number
		/// </summary>
		Complex() {
			this->real = 0;
			this->imag = 0;
		}

		/// <summary>
		/// Constructor of the complex number
		/// </summary>
		/// <param name="real">Real part of the complex</param>
		/// <param name="imag">Imaginary part of the complex</param>
		Complex(double real, double imag) {
			this->real = real;
			this->imag = imag;
		}

		/// <summary>
		/// Calculate the modulus of the current complex number
		/// </summary>
		/// <returns>modulus of the current complex number</returns>
		double Modulus() {
			return sqrt(real * real + imag * imag);
		}
		
		/// <summary>
		/// Return a new complex using the formula z^2 + c
        /// With z the current complex and c the parameter
		/// </summary>
		/// <param name="c">complex added for calculating the next iteration</param>
		/// <returns>next iteration of Mandelbrot</returns>
		Complex NextIteration(Complex c) {
			// Do the multiplication and addition at the same time to gain time
			return Complex((this->real * this->real) - (this->imag * this->imag) + c.real, (this->real * this->imag) + (this->imag * this->real) + c.imag);
		}
};
