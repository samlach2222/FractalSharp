using System.Runtime.Serialization;

namespace FractalSharpMPI
{
    /// <summary>
    /// Class to represent a pixel color
    /// </summary>
    [Serializable]
    internal class PixelColor : ISerializable
    {
        private const string redSerializedName = "Red";
        private const string greenSerializedName = "Green";
        private const string blueSerializedName = "Blue";

        /// <summary>
        /// Red component of the color
        /// </summary>
        private int red;

        /// <summary>
        /// Green component of the color
        /// </summary>
        private int green;

        /// <summary>
        /// Blue component of the color
        /// </summary>
        private int blue;

        /// <summary>
        /// Constructor of the color
        /// </summary>
        /// <param name="red">red component of the color</param>
        /// <param name="green">green component of the color</param>
        /// <param name="blue">blue component of the color</param>
        public PixelColor(int red, int green, int blue)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
        }

        /// <summary>
        /// Constructor of the color without parameters (black color)
        /// </summary>
        public PixelColor()
        {
            this.red = 0;
            this.green = 0;
            this.blue = 0;
        }

        /// <summary>
        /// Constructor of the color used by MPI (with serialization)
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public PixelColor(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            red = (int)info.GetValue(redSerializedName, typeof(int))!;
            green = (int)info.GetValue(greenSerializedName, typeof(int))!;
            blue = (int)info.GetValue(blueSerializedName, typeof(int))!;
        }

        /// <summary>
        /// Method to get the red component of the color
        /// </summary>
        public int Red
        {
            get { return red; }
            set { red = value; }
        }

        /// <summary>
        /// Method to get the green component of the color
        /// </summary>
        public int Green
        {
            get { return green; }
            set { green = value; }
        }

        /// <summary>
        /// Method to get the blue component of the color
        /// </summary>
        public int Blue
        {
            get { return blue; }
            set { blue = value; }
        }

        /// <summary>
        /// Method used by MPI to add colors
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(redSerializedName, red);
            info.AddValue(greenSerializedName, green);
            info.AddValue(blueSerializedName, blue);
        }

        /// <summary>
        /// Method to know if the Mandelbrot sequence diverge
        /// </summary>
        /// <returns>true if diverging (color isn't black)</returns>
        public Boolean IsDiverging()
        {
            return (red != 0 || green != 0 || blue != 0);
        }
    }
}
