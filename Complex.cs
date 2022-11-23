namespace FractalSharp
{
    /// <summary>
    /// Class to represent a complex number
    /// </summary>
    internal class Complex
    {
        /// <summary>
        /// Real part of the complex
        /// </summary>
        private double real = 0; // default value 0

        /// <summary>
        /// Imag part of the complex
        /// </summary>
        private double imag = 0; // default value 0

        /// <summary>
        /// Empty constructor to create a 0 + 0i number
        /// </summary>
        public Complex() { }

        /// <summary>
        /// Constructor of the complex number
        /// </summary>
        /// <param name="real">Real part of the complex</param>
        /// <param name="imag">Imag part of the complex</param>
        public Complex(double real, double imag)
        {
            this.real = real;
            this.imag = imag;
        }

        /// <summary>
        /// Calculate the modulus of the current complex number
        /// </summary>
        /// <returns>return modulus of the current complex number</returns>
        public double Modulus()
        {
            return Math.Sqrt(real * real + imag * imag);
        }

        /// <summary>
        /// Return a new complex using the formula z = z^2 + c
        /// </summary>
        /// <param name="c">complex added for calculating the next iteration</param>
        /// <returns>next iteration of mandelbrot</returns>
        public Complex NextIteration(Complex c)
        {
            // Do the multiplication and addition at the same time to gain time
            return new Complex((this.real * this.real) - (this.imag * this.imag) + c.real, (this.real * this.imag) + (this.imag * this.real) + c.imag);
        }
    }
}