using FractalSharp;
using MPI;
using System.Runtime.InteropServices;

class Program
{
    public static Form form = new();
    public static PictureBox pictureBox = new();

    // Necessary for getting the scaling factor
    [DllImport("gdi32.dll")]
    static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
    public enum DeviceCap
    {
        VERTRES = 10,
        DESKTOPVERTRES = 117,
    }

    static void Main(string[] args)
    {
        using (MPI.Environment environment = new MPI.Environment(ref args))
        {
            Intracommunicator comm = Communicator.world;

            float scaleFactor = GetScalingFactor();  // get the scaling factor

            // get width and height of the window and screen

            int screenWidth = (int)(Screen.PrimaryScreen.Bounds.Width * scaleFactor); // get true width of the screen
            int screenHeight = (int)(Screen.PrimaryScreen.Bounds.Height * scaleFactor); // get true height of the screen

            int screenWidthWithDpi = Screen.PrimaryScreen.Bounds.Width; // get width of the screen scaled with dpi
            int screenHeightWithDpi = Screen.PrimaryScreen.Bounds.Height; // get height of the screen scaled with dpi


            const double ratioWindow = 0.8; // ratio of the window (80% of the screen)

            int pixelWidth = (int)(screenWidth * ratioWindow);  // get width of the window
            int pixelHeight = (int)(screenHeight * ratioWindow);    // get height of the window

            double rangeX = 4; // range of axis (exemple 4 is for [-2, 2])
            double rangeY = rangeX * pixelHeight / pixelWidth;

            // create table of pixels
            PixelColor[,] pixels = new PixelColor[pixelWidth, pixelHeight]; // final table with all results

            int numberOfPixels = pixelWidth * pixelHeight;
            int nPerProc = numberOfPixels / comm.Size;

            if (comm.Rank == 0)
            {
                // initialize form
                InitializeForm(pixelWidth, pixelHeight);

                // create table of pixels
                PixelColor[] localPixels = new PixelColor[nPerProc + 1]; // 1st table cell contains rank
                localPixels[0] = new PixelColor(comm.Rank, comm.Rank, comm.Rank); // RANK 0
                for (int i = 1; i < localPixels.Length; i++)
                {
                    int iXPos = (i - 1) % pixelWidth;
                    int iYPos = (i - 1) / pixelWidth;
                    PixelColor px = GetPixelColor(iXPos, iYPos, pixelWidth, pixelHeight, rangeX, rangeY);
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
                    PixelColor px = GetPixelColor(iXPos, iYPos, pixelWidth, pixelHeight, rangeX, rangeY);
                    localPixels[i] = px;
                }

                // Send localPixels to rank 0
                Console.WriteLine("Rank {0} is ready to Send", comm.Rank);
                comm.Send(localPixels, 0, 0);
            }
        }
    }

    private static void InitializeForm(int pixelWidth, int pixelHeight)
    {
        int titleBarHeight = form.Height - form.ClientSize.Height; // get the size of the form title bar
        form.Size = new(pixelWidth, pixelHeight + titleBarHeight);
        // change form name
        form.Text = "FractalSharp";
        // form create zone for display image
        pictureBox.Size = new(pixelWidth, pixelHeight);
        pictureBox.Location = new(0, 0);
        new Thread(delegate ()
            {
                Application.Run(form);
            }).Start();
    }

    private static void DisplayPixels(PixelColor[,] pixels)
    {
        // Create the Bitmap

        Bitmap bitmap = new(pixels.GetLength(0), pixels.GetLength(1));
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.Black);
        }

        for (int i = 0; i < pixels.GetLength(0); i++)
        {
            for (int j = 0; j < pixels.GetLength(1); j++)
            {
                if (pixels[i, j].IsDiverging())
                {
                    bitmap.SetPixel(i, j, Color.FromArgb(pixels[i, j].Red, pixels[i, j].Green, pixels[i, j].Blue));
                }
            }
        }

        // Invoke displayThread to pass image to the form
        form.Invoke(new Action(() =>
        {
            pictureBox.Image = bitmap;
            form.Controls.Add(pictureBox);
        }));

        // TODO : ZOOM IN AND OUT
    }

    private static PixelColor GetPixelColor(int iXpos, int iYpos, int pixelWidth, int pixelHeight, double rangeX, double rangeY)
    {
        // calculate if Mandelbrot suite diverge

        double rangeXPos = (double)iXpos / (double)pixelWidth * rangeX - rangeX / 2.0;
        double rangeYPos = (double)iYpos / (double)pixelHeight * rangeY - rangeY / 2.0;

        Complex c = new(rangeXPos, rangeYPos);
        Complex z = new(0, 0);

        int iteration = 0;
        const int maxIteration = 1000;
        while (iteration < maxIteration && z.Modulus() <= 2) // AND Z mod 2 < 2
        {
            // max iteration --> if not diverge
            // z mod 2 < 2 --> if diverge
            z = z.NextIteration(c);
            iteration++;
        }
        if (iteration == maxIteration)
        {
            return new PixelColor(0, 0, 0);
        }
        else
        {

            // color smoothing mandelbroth (a little bit)
            double log_zn = Math.Log(z.Modulus());
            double nu = Math.Log(log_zn / Math.Log(2)) / Math.Log(2);
            iteration = iteration + 1 - (int)nu;

            // gray gradient with color smoothing
            int color = (int)(255.0 * Math.Sqrt((double)iteration / (double)maxIteration));
            return new PixelColor(color, color, color);
        }
    }

    /// <summary>
    /// Get the scaling factor of the current screen
    /// </summary>
    /// <returns>the scaling factor</returns>
    private static float GetScalingFactor()
    {
        Graphics g = Graphics.FromHwnd(IntPtr.Zero);
        IntPtr desktop = g.GetHdc();
        int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
        int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

        float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;

        return ScreenScalingFactor; // 1.25 = 125%
    }
}


/*
 * LINKS :
 * https://stackoverflow.com/questions/20710851/how-can-i-scatter-an-object-array-using-mpi-net
 * https://nanohub.org/resources/5641/download/2008.09.04
 * 
 * TO RUN : 
 * mpiexec -n 8 FractalSharp.exe
 * 
 * TO DO : 
 * User have to be able to Zoom in the bitmap
 * Loading animation before the bitmap is displayed
 * Optimizations
 */
