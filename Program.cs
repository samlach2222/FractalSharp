using FractalSharp;
using MPI;

class Program
{
    static void Main(string[] args)
    {
        using (MPI.Environment environment = new MPI.Environment(ref args))
        {
            Intracommunicator comm = Communicator.world;

            // variables
            double maxX = 1.0;
            double minX = -1.0;
            double maxY = 1.0;
            double minY = -1.0;

            //get width and height of the window

            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            double ratioWindow = 0.8;

            int pixelWidth = (int)(screenWidth * ratioWindow);
            int pixelHeight = (int)(screenHeight * ratioWindow);

            // create table of pixels
            PixelColor[,] pixels = new PixelColor[pixelWidth, pixelHeight]; // final table with all results

            int numberOfPixels = pixelWidth * pixelHeight;
            int nPerProc = numberOfPixels / comm.Size;

            if (comm.Rank == 0)
            {
                // create table of pixels
                PixelColor[] localPixels = new PixelColor[nPerProc + 1]; // 1st table cell contains rank
                localPixels[0] = new PixelColor(comm.Rank, comm.Rank, comm.Rank); // RANK 0
                for (int i = 1; i < localPixels.Length; i++)
                {
                    int iXPos = (i - 1) % pixelWidth;
                    int iYPos = (i - 1) / pixelWidth;
                    PixelColor px = GetPixelColor(iXPos, iYPos);
                    localPixels[i] = px;
                }

                // Merge table with rank 0 values
                for (int i = 1; i < localPixels.Length; i++)
                {
                    int iXPos = (i - 1) % pixelWidth;
                    int iYPos = (i - 1) / pixelWidth;
                    pixels[iXPos, iYPos] = localPixels[i];
                }

                // Receive localPixels from other ranks
                for (int i = 1; i < comm.Size; i++)
                {
                    comm.Receive<PixelColor[]>(i, 0, out localPixels);
                    int rank = localPixels[0].Red;
                    int posFirstValue = rank * nPerProc;

                    Console.WriteLine("Rank 0 received {0} values from rank {1}", localPixels.Length - 1, rank);

                    for (int j = 1; j < localPixels.Length; j++)
                    {
                        int iXPos = ((j - 1) + posFirstValue) % pixelWidth;
                        int iYPos = ((j - 1) + posFirstValue) / pixelWidth;
                        pixels[iXPos, iYPos] = localPixels[j];
                    }
                }

                // Display pixels
                DisplayPixels(pixels);
            }
            else
            {
                int posFirstValue = comm.Rank * nPerProc;

                if (comm.Rank == comm.Size - 1)
                {
                    nPerProc += numberOfPixels % comm.Size;
                }

                PixelColor[] localPixels = new PixelColor[nPerProc + 1]; // 1st table cell contains rank

                localPixels[0] = new PixelColor(comm.Rank, comm.Rank, comm.Rank); // RANK N
                for (int i = 1; i < localPixels.Length; i++)
                {
                    int iXPos = ((i - 1) + posFirstValue) % pixelWidth;
                    int iYPos = ((i - 1) + posFirstValue) / pixelWidth;
                    PixelColor px = GetPixelColor(iXPos, iYPos);
                    localPixels[i] = px;
                }

                // Send localPixels to rank 0
                Console.WriteLine("Rank {0} is ready to Send", comm.Rank);
                comm.Send(localPixels, 0, 0);
            }
        }
    }

    private static void DisplayPixels(PixelColor[,] pixels)
    {
        Bitmap bitmap = new(pixels.GetLength(0), pixels.GetLength(1));

        for (int i = 0; i < pixels.GetLength(0); i++)
        {
            for (int j = 0; j < pixels.GetLength(1); j++)
            {
                bitmap.SetPixel(i, j, Color.FromArgb(pixels[i, j].Red, pixels[i, j].Green, pixels[i, j].Blue));
            }
        }
        bitmap.Save("m.bmp");
    }

    private static PixelColor GetPixelColor(int iXPos, int iYpos)
    {
        // calculate if Mandelbrot suite diverge

        Complex c = new(iXPos, iYpos);
        Complex z = new(0, 0);

        int iteration = 0;
        int maxIteration = 1000;
        while (iteration < maxIteration && z.Modulus() <= 2) // AND Z mod 2 < 2
        {
            // max iteration --> if not diverge
            // z mod 2 < 2 --> if diverge
            z = z.Multiply(z).Add(c);
            iteration++;
        }
        if (iteration == maxIteration)
        {
            return new PixelColor(0, 0, 0);
        }
        else
        {
            return new PixelColor(255, 255, 255);
        }
    }
}


/*
 * LINKS :
 * https://stackoverflow.com/questions/20710851/how-can-i-scatter-an-object-array-using-mpi-net
 * https://nanohub.org/resources/5641/download/2008.09.04
 * 
 * TO RUN : 
 * mpiexec -n 2 FractalSharp.exe
 * 
 * TO DO : 
 * Restes à gérer !!
 */