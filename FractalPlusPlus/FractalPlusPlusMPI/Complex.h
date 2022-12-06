#pragma once
/// <summary>
/// Class to represent a complex number
/// </summary>
class Complex
{
private:
	/// <summary>
	/// Real part of the complex
	/// </summary>
	double real;

	/// <summary>
	/// Imaginary part of the complex
	/// </summary>
	double imag;
public:
	Complex();
	Complex(double, double);
	double Modulus();
	Complex NextIteration(Complex);
};

