using System;

namespace PrimalDevistation
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (PrimalDevistation game = new PrimalDevistation())
            {
                game.Run();
            }
        }
    }
}

