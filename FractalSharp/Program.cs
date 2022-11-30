using System.Diagnostics;
using System.Globalization;

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
    private static readonly int screenWidth = Screen.PrimaryScreen.Bounds.Width;

    /// <summary>
    /// Height of the main screen in pixel
    /// </summary>
    private static readonly int screenHeight = Screen.PrimaryScreen.Bounds.Height;

    /// <summary>
    /// Ratio size of the image.
    /// Here 80% of the screen size.
    /// </summary>
    private const double ratioImage = 0.8;

    /// <summary>
    /// Width in pixel of the image
    /// </summary>
    private static readonly int pixelWidth = (int)(screenWidth * ratioImage);

    /// <summary>
    /// Height in pixel of the image
    /// </summary>
    private static readonly int pixelHeight = (int)(screenHeight * ratioImage);

    private static double P1XinAxe = -2.0;
    private static double P1YinAxe = P1XinAxe * pixelHeight / pixelWidth;
    private static double P2XinAxe = 2.0;
    private static double P2YinAxe = P2XinAxe * pixelHeight / pixelWidth;


    /// <summary>
    /// Main method of the program
    /// </summary>
    /// <param name="args">Arguments passed in parameters (unused in our program)</param>
    static void Main()
    {
        InitializeForm(pixelWidth, pixelHeight);
        CalculateMandelbrot(0, 0, pixelWidth, pixelHeight); // Calculate the whole Mandelbrot
    }

    /// <summary>
    /// Calculate Mandelbrot in MPI. The rank 0 is the main process and the others work for him.
    /// All ranks calculate a part of the Mandelbrot image and send it to the main process.
    /// </summary>
    /// <param name="P1x">Optional parameter which is the x coordinate of the first point after selecting an area to zoom in</param>
    /// <param name="P1y">Optional parameter which is the y coordinate of the first point after selecting an area to zoom in</param>
    /// <param name="P2x">Optional parameter which is the x coordinate of the second point after selecting an area to zoom in</param>
    /// <param name="P2y">Optional parameter which is the y coordinate of the second point after selecting an area to zoom in</param>
    private static void CalculateMandelbrot(double P1x = 0, double P1y = 0, double P2x = 0, double P2y = 0)
    {
        // Debug line
        Console.WriteLine("P1x = {0}, P1y = {1}, P2x = {2}, P2y = {3}", P1x, P1y, P2x, P2y);

        // ERROR HERE RE-CALCULATE

        // calculate the new range of the image
        double rangeX = Math.Abs(P2XinAxe - P1XinAxe);
        double rangeY = Math.Abs(P2YinAxe - P1YinAxe);
        //display the new range
        Console.WriteLine("rangeX = {0}, rangeY = {1}", rangeX, rangeY);

        double localP1XinAxe = P1x / pixelWidth * rangeX - rangeX / 2;
        double localP1YinAxe = P1y / pixelHeight * rangeY - rangeY / 2;
        double localP2XinAxe = P2x / pixelWidth * rangeX - rangeX / 2;
        double localP2YinAxe = P2y / pixelHeight * rangeY - rangeY / 2;

        // stock the new range of the image for the next image
        if(localP1XinAxe < localP2XinAxe)
        {
            P1XinAxe = localP1XinAxe;
            P2XinAxe = localP2XinAxe;
        }
        else
        {
            P1XinAxe = localP2XinAxe;
            P2XinAxe = localP1XinAxe;
        }
        if(localP1YinAxe < localP2YinAxe)
        {
            P1YinAxe = localP1YinAxe;
            P2YinAxe = localP2YinAxe;
        }
        else
        {
            P1YinAxe = localP2YinAxe;
            P2YinAxe = localP1YinAxe;
        }
        
        DisplayLoadingScreen();

        if (File.Exists(Path.GetTempPath() + "Mandelbrot.bmp"))
        {
            File.Delete(Path.GetTempPath() + "Mandelbrot.bmp");
        }

        // exec EXE FILE
        // TODO : Change launch to use MPI (maybe tell to user how many processes he want)
        string exePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.Parent!.Parent!.FullName, "FractalSharpMPI\\bin\\x64\\Release\\net6.0-windows\\FractalSharpMPI.exe");
        Console.WriteLine("exePath = {0}", exePath);
        string[] args = new string[] { pixelWidth.ToString(), pixelHeight.ToString(), P1XinAxe.ToString(CultureInfo.InvariantCulture), P2XinAxe.ToString(CultureInfo.InvariantCulture), P1YinAxe.ToString(CultureInfo.InvariantCulture), P2YinAxe.ToString(CultureInfo.InvariantCulture) };
        ProcessStartInfo startInfo = new(exePath, string.Join(" ", args));
        Process.Start(startInfo);

        // wait for the end of the process
        while (!File.Exists(Path.GetTempPath() + "Mandelbrot.bmp"))
        {
            Thread.Sleep(100);
        }
        while (IsFileLocked(new FileInfo(Path.GetTempPath() + "Mandelbrot.bmp")))
        {
            Thread.Sleep(100);
        }
        DisplayPixels();
    }

    /// <summary>
    /// Check if the file is occupied by another process
    /// </summary>
    /// <param name="file">file name we want to check</param>
    /// <returns>true if occupied by another program, false else</returns>
    private static bool IsFileLocked(FileInfo file)
    {
        try
        {
            using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
            {
                stream.Close();
            }
        }
        catch (IOException)
        {
            //the file is unavailable because it is:
            //still being written to
            //or being processed by another thread
            //or does not exist (has already been processed)
            return true;
        }

        //file is not locked
        return false;
    }

    /// <summary>
    /// This method initialize the form where the user is able to see and zoom in the Mandelbrot image
    /// </summary>
    /// <param name="pixelWidth">the width of the Mandelbrot image in pixels. It's also the width of the form's content</param>
    /// <param name="pixelHeight">the height of the Mandelbrot image in pixels. It's also the height of the form's content</param>
    private static void InitializeForm(int pixelWidth, int pixelHeight)
    {
        // Use the width and height of the Mandelbrot image for the form content
        // /!\ This is different from form.Size, which includes borders and titlebar /!\
        form.ClientSize = new Size(pixelWidth, pixelHeight);
        // Change form name
        form.Text = "FractalSharpMPI";
        // Set pictureBox to fill the form
        pictureBox.Dock = DockStyle.Fill;
        // Add the pictureBox to the form
        form.Controls.Add(pictureBox);
        // Form create zone for display image
        new Thread(delegate ()
            {
                Application.Run(form);
            }).Start();
    }
    /// <summary>
    /// This method display loading.gif in the form while waiting for the end of the calculation
    /// </summary>
    private static void DisplayLoadingScreen()
    {
        pictureBox.BackColor = Color.FromArgb(40, 44, 52); // Set background color of pictureBox
        pictureBox.ImageLocation = "loading.gif"; // Set loading.gif as pictureBox image
        pictureBox.SizeMode = PictureBoxSizeMode.Zoom; // Center and fit image in pictureBox
    }

    /// <summary>
    /// This method creates a Bitmap image with the pixels and display it in the form.
    /// This method also let the user zoom in the Mandelbrot image by selecting an area.
    /// </summary>
    /// <param name="pixels">2D array of PixelColor (r,g,b) which contains the color of each pixel</param>
    private static void DisplayPixels()
    {
        // Change the image to the generated Mandelbrot image
        pictureBox.ImageLocation = Path.GetTempPath() + "Mandelbrot.bmp";

        int P1x = 0;
        int P1y = 0;
        int P2x = 0;
        int P2y = 0;
        bool rectangleFinished = false;
        pictureBox.MouseDown += new MouseEventHandler((object? sender, MouseEventArgs e) =>
        {
            if (!rectangleFinished)
            {
                P1x = e.X;
                P1y = e.Y;
                Console.WriteLine("P1 point at ({0}, {1})", P1x, P1y);
            }
        });

        pictureBox.MouseMove += new MouseEventHandler((object? sender, MouseEventArgs e) =>
        {
            if (P1x != 0 && P1y != 0 && !rectangleFinished)
            {
                P2x = e.X;
                P2y = e.Y;
                Console.WriteLine("Mouse move at ({0}, {1})", P2x, P2y);
                pictureBox.Refresh();
            }
        });

        pictureBox.MouseUp += new MouseEventHandler((object? sender, MouseEventArgs e) =>
        {
            if (!rectangleFinished)
            {
                if (e.X < 0 || e.X > pictureBox.Width || e.Y < 0 || e.Y > pictureBox.Height)
                {
                    rectangleFinished = false;
                }
                else
                {
                    Console.WriteLine("P2 point up at ({0}, {1})", P2x, P2y);
                    rectangleFinished = true;

                    CalculateMandelbrot(P1x, P1y, P2x, P2y); // Execute CalculateMandelbrot in Main Thread (possible memory problem here)
                }
            }

            // TODO : Recalculate image
            // TODO : ZOOM IN AND OUT

        });

        pictureBox.Paint += new PaintEventHandler((object? sender, PaintEventArgs e) =>
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

                // Interpolate P3 and P4
                int P3x = P1x;
                int P3y = P2y;
                int P4x = P2x;
                int P4y = P1y;

                // Draw rectangle P1 P4 P2 P3
                e.Graphics.DrawLine(Pens.Purple, P1x, P1y, P4x, P4y);
                e.Graphics.DrawLine(Pens.Purple, P4x, P4y, P2x, P2y);
                e.Graphics.DrawLine(Pens.Purple, P2x, P2y, P3x, P3y);
                e.Graphics.DrawLine(Pens.Purple, P3x, P3y, P1x, P1y);
            }
        });

    }
}


/*
 * LINKS :
 * https://stackoverflow.com/questions/20710851/how-can-i-scatter-an-object-array-using-mpi-net
 * https://nanohub.org/resources/5641/download/2008.09.04
 * 
 * TO RUN : 
 * mpiexec -n 8 FractalSharpMPI.exe
 * 
 * TO DO : 
 * User have to be able to Zoom in the bitmap
 * Optimizations
 */
