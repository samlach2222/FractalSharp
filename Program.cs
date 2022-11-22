using FractalSharp;
using MPI;
using System.Runtime.InteropServices;

class Program
{
    private static readonly Form form = new();
    private static readonly PictureBox pictureBox = new();
    private static readonly float scaleFactor = GetScalingFactor();  // get the scaling factor
    private static readonly int screenWidth = (int)(Screen.PrimaryScreen.Bounds.Width * scaleFactor); // get true width of the screen
    private static readonly int screenHeight = (int)(Screen.PrimaryScreen.Bounds.Height * scaleFactor); // get true height of the screen

    private static readonly int screenWidthWithDpi = Screen.PrimaryScreen.Bounds.Width; // get width of the screen scaled with dpi
    private static readonly int screenHeightWithDpi = Screen.PrimaryScreen.Bounds.Height; // get height of the screen scaled with dpi

    private static readonly double ratioWindow = 0.8; // ratio of the window (80% of the screen)

    private static int pixelWidth = (int)(screenWidth * ratioWindow);  // get width of the window
    private static int pixelHeight = (int)(screenHeight * ratioWindow);    // get height of the window


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
            CalculateMandelbroth(); // calculate the whole mandelbroth
        }
    }

    private static void CalculateMandelbroth(int P1x = 0, int P1y = 0, int P2x = 0, int P2y = 0)
    {
        Intracommunicator comm = Communicator.world;

        if (P1x == 0 && P1y == 0 && P2x == 0 && P2y == 0)
        {
            // calculate the whole mandelbroth
            
            double rangeX = 4; // range of axis (exemple 4 is for [-2, 2])
            double rangeY = rangeX * pixelHeight / pixelWidth;

            int numberOfPixels = pixelWidth * pixelHeight;
            int nPerProc = numberOfPixels / comm.Size;

            if (comm.Rank == 0)
            {
                // create table of pixels
                PixelColor[,] pixels = new PixelColor[pixelWidth, pixelHeight]; // final table with all results

                // initialize form
                InitializeForm(pixelWidth, pixelHeight);

                // create table of pixels
                for (int i = 0; i < nPerProc; i++)
                {
                    int iXPos = i % pixelWidth;
                    int iYPos = i / pixelWidth;
                    PixelColor px = GetPixelColor(iXPos, iYPos, pixelWidth, pixelHeight, rangeX, rangeY);
                    pixels[iXPos, iYPos] = px;
                }

                // Receive localPixels from other ranks
                for (int i = 1; i < comm.Size; i++)
                {
                    comm.Receive<PixelColor[]>(i, 0, out PixelColor[] localPixels);
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
        else
        {
            Console.WriteLine("Coucou");
            // calculate the mandelbroth between P1 and P2
            if (P1x > P2x)
            {
                pixelWidth = P1x - P2x;
            }
            else
            {
                pixelWidth = P2x - P1x;
            }
            if (P1y > P2y)
            {
                pixelHeight = P1y - P2y;
            }
            else
            {
                pixelHeight = P2y - P1y;
            }

            // TODO : PROBABLY WE HAVE TO MODIFY THE RANGE
            double rangeX = 4; // range of axis (exemple 4 is for [-2, 2])
            double rangeY = rangeX * pixelHeight / pixelWidth;

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

        int P1x = 0;
        int P1y = 0;
        int P2x = 0;
        int P2y = 0;
        bool rectangleFinished = false;
        pictureBox.MouseDown += new MouseEventHandler((object sender, MouseEventArgs e) =>
        {
            P1x = e.X;
            P1y = e.Y;
            Console.WriteLine("Mouse down at ({0}, {1})", P1x, P1y);
        });
        pictureBox.MouseMove += new MouseEventHandler((object sender, MouseEventArgs e) =>
        {
            if(!rectangleFinished)
            {
                P2x = e.X;
                P2y = e.Y;
                Console.WriteLine("Mouse move at ({0}, {1})", P2x, P2y);
                pictureBox.Refresh();
            }
        });
        pictureBox.MouseUp += new MouseEventHandler((object sender, MouseEventArgs e) =>
        {
            P2x = e.X;
            P2y = e.Y;
            Console.WriteLine("Mouse up at ({0}, {1})", P2x, P2y);
            rectangleFinished = true;
            
            // Invoke CalculateMandelbroth
            form.Invoke((MethodInvoker)delegate { CalculateMandelbroth(P1x, P1y, P2x, P2y); }); // Execute CalculateMandelbroth in Main Thrad (possibly memory problem here)


            // TODO : recalculate image
            // TODO : ZOOM IN AND OUT

        });

        pictureBox.Paint += new PaintEventHandler((object sender, PaintEventArgs e) =>
        {
            if (P1x != 0 && P1y != 0 && P2x != 0 && P2y != 0)
            {
                // interpolate P3 and P4
                int P3x = P1x;
                int P3y = P2y;
                int P4x = P2x;
                int P4y = P1y;
                // draw rectangle P1 P4 P2 P3
                e.Graphics.DrawLine(Pens.Purple, P1x, P1y, P4x, P4y);
                e.Graphics.DrawLine(Pens.Purple, P4x, P4y, P2x, P2y);
                e.Graphics.DrawLine(Pens.Purple, P2x, P2y, P3x, P3y);
                e.Graphics.DrawLine(Pens.Purple, P3x, P3y, P1x, P1y);
            }
        });

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
