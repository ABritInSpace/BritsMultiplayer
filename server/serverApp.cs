using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace serverApp
{
    class threads
    {
        static bool closed = false;
        static int port = 8080;

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
            //list to keep track of players
            Player[] pList = {};
            while(!closed)
            {
                IPAddress localadd = IPAddress.Parse(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString());
                TcpListener listener =  new TcpListener(localadd, port);
                listener.Start();
                Socket client = listener.AcceptSocket();
                //assign client thread
                var userT = new Thread(() => {
                    while (true)
                    {
                        //get length of buffer
                        byte[] len = new byte [4];
                        client.Receive(len);
                        int length = BitConverter.ToInt32(len);
                        //get buffer
                        byte[] buffer = new byte[length];
                        client.Receive(buffer);
                        //print buffer text (tesing purposes)
                        log(0, Encoding.ASCII.GetString(buffer));
                    }
                });
                userT.Start(); 
            }
        }
        static void chatMgr()
        {
            string[] chat = {};

        }
        static void Main(string[] args)
        {
            //run cleanup on ctrl-c
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
    
    class Player 
    {
        public Player (string address, string name)
        {
            srcAddress = address;
            Username = name;
            UUID = Guid.NewGuid().ToString();
        }
        public string srcAddress { get; }
        public string Username { get; }
        public string UUID { get; }
    }
}