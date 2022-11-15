using Microsoft.VisualBasic;
using System.Collections.Concurrent;
using System.Collections;
using MPI;

class Program
{
    static void Main(string[] args)
    {
        using (MPI.Environment environment = new MPI.Environment(ref args))
        {
            Intracommunicator comm = Communicator.world;

            if (comm.Rank == 0)
            {
                int[] numbers = new int[comm.Size];

                for (int k = 0; k < numbers.Length; k++)
                    numbers[k] = k * k;

                int r = comm.Scatter(numbers);

                Console.WriteLine("Received {0} at {1}", r, comm.Rank);
            }
            else
            {
                int r = comm.Scatter<int>(0);
                Console.WriteLine("Received {0} at {1}", r, comm.Rank);
            }
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
 */