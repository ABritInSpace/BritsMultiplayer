using System;
using System.Threading;

namespace serverApp
{
    class threads
    {
        static bool closed = false;

        public static void log(int type, string txt)
        {
            switch(type){
                //cases:
                //  - 0: general
                //  - 1: error
                //  - 2: success
                //  - 3: init
                case 0:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case 1:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case 2:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case 3:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
            }
            Console.WriteLine(txt);
            Console.ForegroundColor = ConsoleColor.White;
        }
        static void playerMgr()
        {
            while(!closed)
            {
                
            }
        }
        static void chatMgr()
        {
            string[] chat = {};

        }
        static void Main(string[] args)
        {
            Console.CancelKeyPress += delegate{
                closed = true;
                Console.WriteLine("\nAdios!");
            };
            //start chat thread
            ThreadStart chatref = new ThreadStart(chatMgr);
            log(3, "Starting chat manager thread...");
            Thread chatThread = new Thread(chatref);
            chatThread.Start();
            log(2, "Chat manager thread started!");

            //start players thread
            ThreadStart playersref = new ThreadStart(playerMgr);
            log(3, "Starting players manager thread...");
            Thread playersThread = new Thread(playersref);
            playersThread.Start();
            log(2, "Players manager thread started!");

            //check for user input
            while (!closed)
            {
                string d = Console.ReadLine();
            }
        }
    }
}