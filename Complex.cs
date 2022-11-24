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
        private readonly double real;

        /// <summary>
        /// Imaginary part of the complex
        /// </summary>
        private readonly double imag;

        /// <summary>
        /// Constructor without parameters to create a 0 + 0i number
        /// </summary>
        public Complex()
        {
            real = 0;
            imag = 0;
        }

        /// <summary>
        /// Constructor of the complex number
        /// </summary>
        /// <param name="real">Real part of the complex</param>
        /// <param name="imag">Imaginary part of the complex</param>
        public Complex(double real, double imag)
        {
            this.real = real;
            this.imag = imag;
        }

        /// <summary>
        /// Calculate the modulus of the current complex number
        /// </summary>
        /// <returns>modulus of the current complex number</returns>
        public double Modulus()
        {
            return Math.Sqrt(real * real + imag * imag);
        }

        /// <summary>
        /// Return a new complex using the formula z^2 + c
        /// With z the current complex and c the parameter
        /// </summary>
        /// <param name="c">complex added for calculating the next iteration</param>
        /// <returns>next iteration of Mandelbrot</returns>
        public Complex NextIteration(Complex c)
        {
            // Do the multiplication and addition at the same time to gain time
            return new Complex((this.real * this.real) - (this.imag * this.imag) + c.real, (this.real * this.imag) + (this.imag * this.real) + c.imag);
        }
    }
}