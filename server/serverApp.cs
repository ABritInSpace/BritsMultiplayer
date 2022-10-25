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
        static char bell = Encoding.ASCII.GetString(new byte[]{ 7 })[0];

        static List<Player> pList = new List<Player>();

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
            IPAddress localadd = IPAddress.Parse(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString());
            TcpListener listener =  new TcpListener(localadd, port);
            listener.Start();
            while(!closed)
            {
                TcpClient cl = listener.AcceptTcpClient();
                //assign client thread
                var userT = new Thread(() => {
                    TcpClient client = cl;
                    NetworkStream ns = client.GetStream();
                    bool established = false;
                    bool valid = true;
                    Player current = null;
                    while (valid)
                    {
                        //get buffer
                        byte[] buffer = new byte[client.ReceiveBufferSize];
                        ns.Read(buffer, 0, client.ReceiveBufferSize);
                        string data = Encoding.ASCII.GetString(buffer);
                        switch(int.Parse(data[0].ToString())){
                            case 0:
                                IPEndPoint remote = client.Client.RemoteEndPoint as IPEndPoint;
                                log(3, String.Format("New client connection from {0}...", remote.Address.ToString()));
                                string inName = data.Split('-')[1];
                                foreach (Player p in pList)
                                {
                                    if (p.Username == inName){
                                        ns.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(0).Length);
                                        log(1, String.Format("A remote client from {0} disconnected - username taken!", remote.Address.ToString()));
                                        valid = false;
                                        client.Close();
                                        break;
                                    }
                                }
                                if (valid){
                                    current = new Player(remote.Address.ToString(), inName);
                                    established = true;
                                    ns.Write(BitConverter.GetBytes(0), 0, BitConverter.GetBytes(0).Length);
                                    pList.Add(current);
                                    log(2, String.Format("A remote client from {0} connected!\n  - Username = {1}\n  - UUID = {2}", remote.Address.ToString(), current.Username, current.UUID));
                                }
                                break;
                            default:
                                if (established == false){
                                    client.Close();
                                    valid = false; //client becomes invalid and therefore thread clears
                                    break;
                                }
                                log(0, data);
                                break;
                        }
                        //check for ship appends / deappends
                        //check for chat rebroadcast
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