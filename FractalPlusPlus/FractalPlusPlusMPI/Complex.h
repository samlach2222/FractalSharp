#pragma once
class Complex
{
public:
	Complex(double real, double imag);
	Complex();
	double Modulus();
	double NextIteration(Complex c);
};

