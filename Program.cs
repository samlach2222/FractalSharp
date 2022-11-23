using FractalSharp;
using MPI;

/// <summary>
/// Main class of the program
/// </summary>
class Program
{
    /// <summary>
    /// PictureBox where the fractal is drawn
    /// </summary>
    private static readonly PictureBox pictureBox = new();

    /// <summary>
    /// Form where the PictureBox is 
    /// </summary>
    private static readonly Form form = new();

    /// <summary>
    /// Width of the main screen in pixel
    /// </summary>
    private static readonly int screenWidth = Screen.PrimaryScreen.Bounds.Width; // get width of the screen

    /// <summary>
    /// Height of the main screen in pixel
    /// </summary>
    private static readonly int screenHeight = Screen.PrimaryScreen.Bounds.Height; // get height of the screen

    /// <summary>
    /// Ratio size of the window (form).
    /// Here 80% of the screen size.
    /// </summary>
    private const double ratioImage = 0.8; // ratio of the image (80% of the screen)

    /// <summary>
    /// Width in pixel of the image/form
    /// </summary>
    private static int pixelWidth = (int)(screenWidth * ratioImage); // get width of the mandelbrot image

    /// <summary>
    /// Height in pixel of the image/form
    /// </summary>
    private static int pixelHeight = (int)(screenHeight * ratioImage); // get height of the mandelbrot image

    /// <summary>
    /// Main method of the program
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args)
    {
        using (MPI.Environment environment = new MPI.Environment(ref args))
        {
            CalculateMandelbroth(); // calculate the whole mandelbroth
        }
    }

    /// <summary>
    /// The Method calculate Mandelbroth in MPI. The rank 0 is the main process and the others work for him.
    /// All nodes calculate a part of the mandelbroth and send it to the main process. (Each part is a part of the pixels in the image)
    /// </summary>
    /// <param name="P1x">Optional parameter who is the x coordinate of the first point after select an area to zoom in</param>
    /// <param name="P1y">Optional parameter who is the y coordinate of the first point after select an area to zoom in</param>
    /// <param name="P2x">Optional parameter who is the x coordinate of the second point after select an area to zoom in</param>
    /// <param name="P2y">Optional parameter who is the y coordinate of the second point after select an area to zoom in</param>
    private static void CalculateMandelbroth(int P1x = 0, int P1y = 0, int P2x = 0, int P2y = 0)
    {
        displayLoadingScreen();

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

    /// <summary>
    /// This method initialize the form where the user be able to see and zoom in the mandelbroth
    /// </summary>
    /// <param name="pixelWidth">The Width of the Mandelbroth image in pixels. It's also the Width of the form</param>
    /// <param name="pixelHeight">The Height of the Mandelbroth image in pixels. It's also the Height of the form</param>
    private static void InitializeForm(int pixelWidth, int pixelHeight)
    {
        // Use the width and height of the mandelbrot image for the window content
        // /!\ This is different from form.Size, which includes borders and titlebar /!\
        form.ClientSize = new Size(pixelWidth, pixelHeight);
        // change form name
        form.Text = "FractalSharp";
        // set pictureBox to fill the form
        pictureBox.Dock = DockStyle.Fill;
        // add the pictureBox to the form
        form.Controls.Add(pictureBox);
        // form create zone for display image
        new Thread(delegate ()
            {
                Application.Run(form);
            }).Start();
    }
    /// <summary>
    /// This method display loading.gif in the form to wait the end of the calculation
    /// </summary>
    private static void displayLoadingScreen()
    {
        pictureBox.BackColor = Color.FromArgb(40, 44, 52); // set background color of pictureBox
        pictureBox.ImageLocation = "loading.gif"; // set loading.gif as pictureBox image
        pictureBox.SizeMode = PictureBoxSizeMode.Zoom; // center and fit image in pictureBox
    }

    /// <summary>
    /// This method create a Bitmap image with the pixels and display it in the form.
    /// This method also permise to zoom in the mandelbroth by selecting an area.
    /// </summary>
    /// <param name="pixels">2D array of PixelColor (r,g,b) who contains the color of each pixel</param>
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

        /*
        // Invoke displayThread to pass image to the form
        form.Invoke(new Action(() =>
        {
            pictureBox.Image = bitmap;
        }));
        */
        // change the image to the generated mandelbrot image
        pictureBox.Image = bitmap;

        int P1x = 0;
        int P1y = 0;
        int P2x = 0;
        int P2y = 0;
        bool rectangleFinished = false;
        pictureBox.MouseDown += new MouseEventHandler((object sender, MouseEventArgs e) =>
        {
            if (!rectangleFinished)
            {
                P1x = e.X;
                P1y = e.Y;
                Console.WriteLine("P1 point at ({0}, {1})", P1x, P1y);
            }   
        });

        pictureBox.MouseMove += new MouseEventHandler((object sender, MouseEventArgs e) =>
        {
            if (P1x != 0 && P1y != 0 && !rectangleFinished)
            {
                P2x = e.X;
                P2y = e.Y;
                Console.WriteLine("Mouse move at ({0}, {1})", P2x, P2y);
                pictureBox.Refresh();
            }
        });
        
        pictureBox.MouseUp += new MouseEventHandler((object sender, MouseEventArgs e) =>
        {
            if(!rectangleFinished)
            {
                if (e.X < 0 || e.X > pictureBox.Width || e.Y < 0 || e.Y > pictureBox.Height)
                {
                    rectangleFinished = false;
                }
                else
                {
                    Console.WriteLine("P2 point up at ({0}, {1})", P2x, P2y);
                    rectangleFinished = true;

                    form.Invoke((MethodInvoker)delegate { CalculateMandelbroth(P1x, P1y, P2x, P2y); }); // Execute CalculateMandelbroth in Main Thrad (possibly memory problem here)
                }
            }
            
            // TODO : recalculate image
            // TODO : ZOOM IN AND OUT

        });

        pictureBox.Paint += new PaintEventHandler((object sender, PaintEventArgs e) =>
        {
            /*      P1------P4
             *       |       |
             *       |       |
             *      P3------P2
             */

            if (P1x != 0 && P1y != 0 && P2x != 0 && P2y != 0 && !rectangleFinished)
            { // BUG : The ratio is not always respected (why ?)
                double r = (double)pixelWidth / (double)pixelHeight;
                int width;
                int height;
                if (P1x > P2x)
                {
                    width = P1x - P2x;
                }
                else
                {
                    width = P2x - P1x;
                }
                if (P1y > P2y)
                {
                    height = P1y - P2y;
                    if (width < height)
                    {
                        int widthWanted = (int)(height * r);
                        int distance = widthWanted - width;
                        P2x -= distance;
                    }
                    else
                    {
                        int heightWanted = (int)(width / r);
                        int distance = heightWanted - height;
                        P2y -= distance;
                    }
                }
                else
                {
                    height = P2y - P1y;
                    if (width < height)
                    {
                        int widthWanted = (int)(height * r);
                        int distance = widthWanted - width;
                        P2x += distance;
                    }
                    else
                    {
                        int heightWanted = (int)(width / r);
                        int distance = heightWanted - height;
                        P2y += distance;
                    }
                }
              
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

    /// <summary>
    /// The method calculate the color of a pixel and return the R, G, B values of it.
    /// The pixel is black if the suite converge.
    /// The pixel is in other colors (gray scale) if the suite diverge.
    /// </summary>
    /// <param name="iXpos">X pos of the pixel</param>
    /// <param name="iYpos">Y pos of the pixel</param>
    /// <param name="pixelWidth">Number of pixels in width</param>
    /// <param name="pixelHeight">Number of pixels in Height</param>
    /// <param name="rangeX">Range of the X axis (exemple 4 if [-2, 2])</param>
    /// <param name="rangeY">Range of the Y axis (exemple 4 if [-2, 2])</param>
    /// <returns>Color of the pixel</returns>
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
 * Optimizations
 */
