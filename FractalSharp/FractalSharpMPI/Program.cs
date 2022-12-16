using FractalSharpMPI;
using MPI;
using SkiaSharp;
using System.Globalization;

/// <summary>
/// Main class of the program
/// </summary>
class Program
{
    /// <summary>
    /// Main method of the program
    /// </summary>
    /// <param name="args">
    /// Arguments passed in parameters :
    /// First is number of pixels per row
    /// Second is number of pixels per column
    /// Third is minRangeX
    /// Fourth is maxRangeX
    /// Fifth is minRangeY
    /// Sixth is maxRangeY
    /// </param>
    static void Main(string[] args)
    {
        // Check if we have all the necessary arguments
        if (args.Length != 6)
        {
#if DEBUG
            args = new string[] { "1280", "720", "-2.0", "2.0", "-1.125", "1.125" };
#else
            throw new ArgumentException("You must pass 6 arguments : number of pixels per row, number of pixels per column, minRangeX, maxRangeX, minRangeY, maxRangeY");
#endif
        }

        // Get values from arguments
        int pixelWidth = int.Parse(args[0]);
        int pixelHeight = int.Parse(args[1]);
        double minRangeX = double.Parse(args[2], CultureInfo.InvariantCulture);
        double maxRangeX = double.Parse(args[3], CultureInfo.InvariantCulture);
        double minRangeY = double.Parse(args[4], CultureInfo.InvariantCulture);
        double maxRangeY = double.Parse(args[5], CultureInfo.InvariantCulture);

        // Start MPI
        using MPI.Environment environment = new(ref args);
        Intracommunicator comm = Communicator.world;

        // Calculate the whole Mandelbrot
        int numberOfPixels = pixelWidth * pixelHeight;
        int nPerProc = numberOfPixels / comm.Size;

        if (comm.Rank == 0)
        {
            // Display args
            Console.WriteLine("Arguments :");
            foreach (string arg in args)
            {
                if (arg != args.Last())
                {
                    Console.Write(arg + " | ");
                }
                else
                {
                    Console.WriteLine(arg);
                }
            }

            Console.WriteLine("Calculating the Mandelbrot set");
            Console.WriteLine("----------------------------------------------");

            // Create array of pixels
            PixelColor[,] pixels = new PixelColor[pixelWidth, pixelHeight]; // Final array with all pixels

            // Calculate rank 0's part
            for (int i = 0; i < nPerProc; i++)
            {
                int iXPos = i % pixelWidth;
                int iYPos = i / pixelWidth;

                PixelColor px = GetPixelColor(iXPos, iYPos, pixelWidth, pixelHeight, minRangeX, maxRangeX, minRangeY, maxRangeY);
                pixels[iXPos, iYPos] = px;
            }

            // Receive localPixels from other ranks
            for (int i = 1; i < comm.Size; i++)
            {
                comm.Receive<PixelColor[]>(i, 0, out PixelColor[] localPixels);
                int rank = localPixels[0].Red;
                int posFirstValue = rank * nPerProc;

                Console.WriteLine("Rank 0 received {0} pixels from rank {1}", localPixels.Length - 1, rank);

                for (int j = 1; j < localPixels.Length; j++)
                {
                    int iXPos = ((j - 1) + posFirstValue) % pixelWidth;
                    int iYPos = ((j - 1) + posFirstValue) / pixelWidth;
                    pixels[iXPos, iYPos] = localPixels[j];
                }
            }

            // Display pixels
            CreateMandelbrotImage(pixels);
        }
        else
        {
            int posFirstValue = comm.Rank * nPerProc;

            if (comm.Rank == comm.Size - 1)
            {
                nPerProc += numberOfPixels % comm.Size;
            }

            PixelColor[] localPixels = new PixelColor[nPerProc + 1]; // + 1 cell to include the rank number

            localPixels[0] = new PixelColor(comm.Rank, comm.Rank, comm.Rank); // Put rank number in the first cell
            for (int i = 1; i < localPixels.Length; i++)
            {
                int iXPos = ((i - 1) + posFirstValue) % pixelWidth;
                int iYPos = ((i - 1) + posFirstValue) / pixelWidth;
                PixelColor px = GetPixelColor(iXPos, iYPos, pixelWidth, pixelHeight, minRangeX, maxRangeX, minRangeY, maxRangeY);
                localPixels[i] = px;
            }

            // Send localPixels to rank 0
            Console.WriteLine("Rank {0} is ready to send", comm.Rank);
            comm.Send(localPixels, 0, 0);
        }
    }

    /// <summary>
    /// This method creates a Bitmap image with the pixels and display it in the form.
    /// This method also let the user zoom in the Mandelbrot image by selecting an area.
    /// </summary>
    /// <param name="pixels">2D array of PixelColor (r,g,b) which contains the color of each pixel</param>
    private static void CreateMandelbrotImage(PixelColor[,] pixels)
    {
        // Create the Bitmap
        SKBitmap bitmap = new(pixels.GetLength(0), pixels.GetLength(1), true);

        // Fill the Bitmap with black, so we only need to set the pixels where Mandelbrot is diverging
        bitmap.Erase(SKColors.Black);

        // Set diverging pixels
        for (int i = 0; i < pixels.GetLength(0); i++)
        {
            for (int j = 0; j < pixels.GetLength(1); j++)
            {
                if (pixels[i, j].IsDiverging())
                {
                    bitmap.SetPixel(i, j, new SKColor((byte)pixels[i, j].Red, (byte)pixels[i, j].Green, (byte)pixels[i, j].Blue));
                }
            }
        }

        // Save the bitmap as PNG (SkiaSharp can't save as bitmap)
        string path = Path.GetTempPath() + "Mandelbrot.png";
        using (SKWStream fileStream = new SKFileWStream(path))
        {
            bitmap.Encode(fileStream, SKEncodedImageFormat.Png, 100);
        }
        Console.WriteLine("Mandelbrot image saved in {0}", path);
        Console.WriteLine("----------------------------------------------");
    }

    /// <summary>
    /// This method calculates the color of a pixel and returns it.
    /// The pixel is black if the sequence converge.
    /// The pixel is in other colors (gray scale) if the sequence diverge.
    /// </summary>
    /// <param name="iXpos">X position of the pixel</param>
    /// <param name="iYpos">Y position of the pixel</param>
    /// <param name="pixelWidth">number of pixels in width</param>
    /// <param name="pixelHeight">number of pixels in height</param>
    /// <param name="rangeX">range of the X axis (example : 4 is for [-2, 2])</param>
    /// <param name="rangeY">range of the Y axis (example : 4 is for [-2, 2])</param>
    /// <returns>color of the pixel</returns>
    private static PixelColor GetPixelColor(int iXpos, int iYpos, int pixelWidth, int pixelHeight, double minRangeX, double maxRangeX, double minRangeY, double maxRangeY)
    {
        // Calculate if Mandelbrot sequence diverge

        double rangeXPos = (double)iXpos / (double)pixelWidth * (maxRangeX - minRangeX) + minRangeX;
        double rangeYPos = (double)iYpos / (double)pixelHeight * (maxRangeY - minRangeY) + minRangeY;
        // DEBUG : print rangeXPos and rangeYPos
        //Console.WriteLine("rangeXPos = {0}, rangeYPos = {1}", rangeXPos, rangeYPos);

        Complex c = new(rangeXPos, rangeYPos);
        Complex z = new(0, 0);

        int iteration = 0;
        const int maxIteration = 1000;
        while (iteration < maxIteration && z.Modulus() <= 2) // AND Z mod 2 < 2
        {
            // Max iteration --> If not diverge
            // z mod 2 < 2 --> If diverge
            z = z.NextIteration(c);
            iteration++;
        }
        if (iteration == maxIteration)
        {
            return new PixelColor(0, 0, 0);
        }
        else
        {
            // Color smoothing Mandelbrot (a little bit)
            double log_zn = Math.Log(z.Modulus());
            double nu = Math.Log(log_zn / Math.Log(2)) / Math.Log(2);
            iteration = iteration + 1 - (int)nu;

            // Gray gradient with color smoothing
            int color = (int)(255.0 * Math.Sqrt((double)iteration / (double)maxIteration));
            return new PixelColor(color, color, color);
        }
    }
}
