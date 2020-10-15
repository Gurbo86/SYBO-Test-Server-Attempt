using System;
using GameServer;
using System.Threading;

namespace ServerTest
{
    class Constants
    {
        public const int TICKS_PER_SEC = 30;
        public const int MS_PER_TICK = 1000 / TICKS_PER_SEC;
    }

    class Program
    {
        private static bool isRunning = false;
        private static bool serverOn = false;

        private const string CLOSE_CONECTION = "CloseConection";
        private const string CLOSE_SERVER = "Shutdown";

        static void Main(string[] args)
        {
            Console.Title = "ServerTest";
            string command;

            serverOn = true;
            // Starting the server
            isRunning = true;
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(1, 33328);

            while (serverOn)
            {
                command = Console.ReadLine().ToString();

                switch (command)
                {
                    case CLOSE_CONECTION:
                        Console.WriteLine($"Parar la ejecucion del thread. Deberia solo cambiar el estado de isRunnning, pero no se como impacta. TODO");
                        isRunning = false;
                        break;
                    case CLOSE_SERVER:
                        serverOn = false;
                        isRunning = false;
                        break;
                    default:
                        Console.WriteLine($"Wrong Command. The valid commands available are:");
                        Console.WriteLine($"{ CLOSE_CONECTION } : Stop the threads comunicating");
                        Console.WriteLine($"{ CLOSE_SERVER } : Stop the server from comunicating");
                        break;
                }
            }
        }

        private static void MainThread()
        {
            Console.WriteLine($"Server main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
            DateTime nextLoop = DateTime.Now;

            while (isRunning)
            {
                // Este loop me genera sospecha. Me suena al re pedo.
                while (nextLoop < DateTime.Now)
                {
                    GameLogic.Update();

                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if (nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }

            Console.WriteLine($"Conections Closed.");
        }
    }
}
