using System.Runtime.Serialization;

namespace FractalSharp
{
    [Serializable]
    internal class PixelColor : ISerializable
    {
        private int red;
        private int green;
        private int blue;

        public PixelColor(int red, int green, int blue)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
        }

        public PixelColor()
        {
            this.red = 0;
            this.green = 0;
            this.blue = 0;
        }

        public PixelColor(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            red = (int)info.GetValue("Red", typeof(int));
            green = (int)info.GetValue("Green", typeof(int));
            blue = (int)info.GetValue("Blue", typeof(int));
        }

        public int Red
        {
            get { return red; }
            set { red = value; }
        }

        public int Green
        {
            get { return green; }
            set { green = value; }
        }

        public int Blue
        {
            get { return blue; }
            set { blue = value; }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Red", red);
            info.AddValue("Green", green);
            info.AddValue("Blue", blue);
        }

        public Boolean IsDiverging()
        {
            return (red != 0 || green != 0 || blue != 0);
        }
    }
}
