using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Reflection;
using players;
using static players.playerList;
using static logMgr.logMgr;

namespace serverApp
{
    static class threads
    {
        static bool closed = false;
        static int port = 8080;
        static char bell = Encoding.ASCII.GetString(new byte[]{ 7 })[0];

        static TcpListener listener =  new TcpListener(IPAddress.Any, port);

        public static void tcpDecode(Player current, int code, string data, int len, TcpClient src)
        {
            switch(code)
            {
                case 1:
                    log(3, "Received chat message of "+(len-2).ToString()+" characters");
                    string msg = current.Username + ": " + msgProc.Bounds.GetBounds(data, 2, len);
                    log(0, "[Chat] "+msg);
                    break;
                case 2:
                    string toWrite = msgProc.Bounds.GetBounds(data, 2, len);
                    log(3, String.Format("Sending {0} to a remote client on {1}...",toWrite,current.srcAddress));
                    current.tO = true;
                    streamFile.Send.Tcp(current.srcAddress, toWrite, src);
                    current.tO = false;
                    break;
                case 3:
                    string toRead = msgProc.Bounds.GetBounds(data, 2, len);
                    log(3, String.Format("Receiving to {0} from a remote client on {1}...",toRead,current.srcAddress));
                    current.tO = true;
                    streamFile.Receive.Tcp(current.srcAddress, toRead, src);
                    current.tO = false;
                    break;
            }
        }
        static void playerMgr()
        {
            //list to keep track of players
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
                            try{
                            Thread.Sleep(5000);
                            if (hbrec && !current.tO){
                                hbrec = false;
                            }
                            else if (current != null && !current.tO){
                                current.Disconnect("Timed out");
                                valid = false;
                                break;
                            }
                            else{
                                continue;
                            }
                            }
                            catch
                            {
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
                                inName = inName.Split(null)[0]; //remove null characters from string
                                log(3, "aaa");
                                foreach (Player p in list)
                                {
                                    if (p.Username == inName){
                                        ns.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(0).Length);
                                        current = new Player(null, inName, client);
                                        current.Disconnect("name already taken");
                                        current = null;
                                        valid = false;
                                        client.Close();
                                        break;
                                    }
                                }
                                if (valid){
                                    current = new Player(remote.Address.ToString(), inName, client);
                                    established = true;
                                    ns.Write(BitConverter.GetBytes(0), 0, BitConverter.GetBytes(0).Length);
                                    list.Add(current);
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
                                tcpDecode(current, code, data, BitConverter.ToInt32(len), client);
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
                        
                        //end
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
    
    
}