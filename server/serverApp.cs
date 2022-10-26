using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Reflection;

namespace serverApp
{
    static class threads
    {
        static bool closed = false;
        static int port = 8080;
        static char bell = Encoding.ASCII.GetString(new byte[]{ 7 })[0];

        public static List<Player> pList = new List<Player>();

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
            return;
        }
        public static void tcpDecode(Player current, int code, string data, int len)
        {
            switch(code)
            {
                case 1:
                    log(3, "Received chat message of "+(len-2).ToString()+" characters");
                    string msg = current.Username + ": ";
                    for(int i = 0; i<=len; i++)
                    {
                        if (i > 1){
                            msg += data[i];
                        }else if (data[i].ToString() == string.Empty){
                            break;
                        }
                    }
                    log(0, "[Chat] "+msg);
                    break;
            }
        }
        static void playerMgr()
        {
            //list to keep track of players
            TcpListener listener =  new TcpListener(IPAddress.Any, port);
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
                    bool hbrec = true;
                    var timeout = new Thread(() => {
                        while (true){
                            Thread.Sleep(5000);
                            if (hbrec){
                                hbrec = false;
                            }
                            else if (current != null){
                                current.Disconnect("Timed out");
                                valid = false;
                                break;
                            }
                        }
                    });
                    timeout.Start();
                    while (valid)
                    {
                        //get buffer
                        try{
                        byte[] buffer = new byte[client.ReceiveBufferSize];
                        ns.Read(buffer, 0, client.ReceiveBufferSize);
                        string data = Encoding.ASCII.GetString(buffer);
                        try{
                        int code = int.Parse(data[0].ToString());
                        switch(code){
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
                                    current = new Player(remote.Address.ToString(), inName, client);
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
                                byte[] len = new byte [4];
                                ns.Read(len, 0, 4);
                                tcpDecode(current, code, data, BitConverter.ToInt32(len));
                                break;
                        }
                        }
                        catch{
                            if (BitConverter.ToInt32(buffer) == 80085){
                                Int32 ping = 80085;
                                ns.Write(BitConverter.GetBytes(ping), 0, 4);
                                hbrec = true;
                            }/*
                            else{
                                current.Disconnect("Escape signal pressed");
                                valid = false;
                            }
                            */
                        }
                        }catch{}
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
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(String.Format("Starting ABMP Server v{0}", Assembly.GetEntryAssembly().GetName().Version));
            Console.BackgroundColor = ConsoleColor.Black;
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

            log(2, "Loading Complete!");

            //check for user input
            while (!closed)
            {
                string d = Console.ReadLine();
            }
        }
    }
    
    class Player 
    {
        public Player (string address, string name, TcpClient cl)
        {
            srcAddress = address;
            Username = name;
            UUID = Guid.NewGuid().ToString();
            client = cl;
        }
        public string srcAddress { get; }
        public string Username { get; }
        public string UUID { get; }
        public TcpClient client { get; }
        public bool tO { get; set; } = false;
        public void Disconnect (string reason = "N/A")
        {
            IPEndPoint ip = client.Client.RemoteEndPoint as IPEndPoint;
            client.Close();
            threads.pList.Remove(this);
            threads.log(1, String.Format("A remote client from {0} disconnected - {1}\n  - Username = {2}\n  - UUID = {3}", ip.Address.ToString(), reason, this.Username, this.UUID));
        }
    }
}