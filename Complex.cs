namespace FractalSharp
{
    internal class Complex
    {
        private double real = 0; // default value 0
        private double imag = 0; // default value 0

        public Complex() { } // empty constructor --> 0 + 0i 

        public Complex(double real, double imag)
        {
            this.real = real;
            this.imag = imag;
        }

        /**
        * Method to get conjugate
        */
        public void Conjugate()
        {
            imag = -imag;
        }

        /**
         * Method to add a complex number in current complex number
         * @param c complex to add
         * @return sum of the 2 complexes
         */
        public Complex Add(Complex c)
        {
            // Le résultat est aussi un complexe, il faut donc introduire une autre variable de type Complex
            Complex sum = new()
            {
                real = this.real + c.real,
                imag = this.imag + c.imag
            };
            return sum;
        }

        /**
         * Method to divide current complex number with param passed complex number
         * @param c divider of the division
         * @return quotient of the division
         */
        public Complex Divide(Complex c)
        {
            double x = this.real * c.real + this.imag * c.imag;
            double y = this.imag * c.real - this.real * c.imag;
            double z = c.real * c.real + c.imag * c.imag;
            this.real = x / z;
            this.imag = y / z;
            return this;
        }

        /**
         * Method to substract current complex number with param passed complex number
         * @param c complex to substract
         * @return substract
         */
        public Complex Minus(Complex c)
        {
            // The result is also a complex number, so we have to introduce a new Complex
            Complex sum = new()
            {
                real = this.real - c.real,
                imag = this.imag - c.imag
            };
            return sum;
        }

        /**
         * Method to multiply current complex number with param passed complex number
         * @param c multiplier
         * @return product of the 2 complexes
         */
        public Complex Multiply(Complex c)
        {
            Complex product = new()
            {
                real = (this.real * c.real) - (this.imag * c.imag),
                imag = (this.real * c.imag) + (this.imag * c.real)
            };
            return product;
        }

        /**
         * Method to multiply current complex number with a parm passed double number
         * @param d multiplier
         * @return product of the complex number and the double number
         */
        public Complex Multiply(double d)
        {
            Complex product = new()
            {
                real = this.real * d,
                imag = this.imag * d
            };
            return product;
        }

        /**
         * Getter of the imaginary part of the current complex number
         * @return imaginary part of the current complex number
         */
        public double GetImag()
        {
            return imag;
        }

        /**
         * Setter of the imaginary part of the current complex number
         * @param imaginary part of the current complex number
         */
        public void SetImag(double imag)
        {
            this.imag = imag;
        }

        /**
         * Getter of the real part of the current complex number
         * @return real part of the current complex number
         */
        public double GetReal()
        {
            return real;
        }

        /**
         * Getter of the real part of the current complex number
         * @param real part of the current complex number
         */
        public void SetReal(double real)
        {
            this.real = real;
        }

        /**
         * Return the modulus fot the current complex number
         * @return modulus of the current complex number
         */
        public double Modulus()
        {
            return Math.Sqrt(real * real + imag * imag);
        }
    }
}