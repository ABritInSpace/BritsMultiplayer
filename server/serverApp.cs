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

        public static bool tcpDecode(Player current, int type, string data, int len, TcpClient src)
        {
            bool good = true; //assume good unless otherwise
            switch(type)
            {
                case 0:
                    log(3, "Received chat message of "+len.ToString()+" characters");
                    string msg = current.Username + ": " + data;
                    log(0, "[Chat] "+msg);
                    break;
                case 1:
                    log(3, String.Format("Sending {0} to a remote client on {1}...",data,current.srcAddress));
                    if(streamFile.Send.Tcp(current.srcAddress, data, src)){
                        break; //don't change good - success
                    }
                    good = false; //change good - failure
                    break;
                case 2:
                    log(3, String.Format("Receiving to {0} from a remote client on {1}...",data,current.srcAddress));
                    if (streamFile.Receive.Tcp(current.srcAddress, data, src) != null){
                        break; //don't change good - success
                    }
                    good = false; //change good - failure
                    break;
                default: //anything outside of case range considered a heartbeat
                    break;
            }
            return good;
        }
        static void playerMgr()
        {
            //list to keep track of players
            listener.Start();
            while(!closed)
            {
                TcpClient cl = listener.AcceptTcpClient();
                log(3, String.Format("New client connection from {0}...", (cl.Client.RemoteEndPoint as IPEndPoint).Address.ToString()));
                //assign client thread
                var cThread = new Thread(()=> {
                    TcpClient client = cl; //move TcpClient into thread for access
                    Player current = null;
                    NetworkStream ns = client.GetStream(); //get network stream for writing and reading
                    ns.ReadTimeout = 5000; //set read timeout to 5 seconds
                    ns.WriteTimeout = 5000; //set write timeout to 5 seconds
                    bool heartbeat = true; //heartbeat
                    //len - length of buffer received, data - data buffer received, type - type of request
                    byte[] type = new byte[4];
                    byte[] len = new byte [4];
                    try{ns.Read(len, 0, 4);}catch{goto timeout;} //read length
                    byte[] data = new byte[BitConverter.ToInt32(len)];
                    try{ns.Read(data, 0, BitConverter.ToInt32(len));}catch{goto timeout;} //read data
                    //on init, data is client username, to set up a player object
                    string name = ASCIIEncoding.ASCII.GetString(data);
                    current = new Player((client.Client.RemoteEndPoint as IPEndPoint).Address.ToString(), name, client);
                    foreach (Player p in list){
                        if (p.Username == name){ //check if username is already taken
                            try{ns.Write(BitConverter.GetBytes(1), 0, 4);}catch{goto timeout;} //return code 1 (not accepted)
                            current.Disconnect("name already taken"); //disconnect client
                            goto end; //kill thread
                        }
                    }
                    list.Add(current); //add current client player to list
                    try{ns.Write(BitConverter.GetBytes(0), 0, 4);}catch{goto timeout;} //return code 0 (accepted)
                    log(2,String.Format("Client connected successfully!\n  - Username: {0}\n  - UUID: {1}\n  - Address: {2}",current.Username,current.UUID,current.srcAddress));

                    //main read loop
                    while(true){
                        //timeout if client not connected
                        if(!client.Connected){
                            goto timeout;
                        }
                        // *** ORDER ***
                        // 1. Read length of data buffer
                        // 2. Read type of request
                        // 3. Read data buffer
                        // 4. Write code (0 = no errors, 1 = errors)
                        try{ns.Read(len, 0, 4);}catch{goto tcheck;} //read length
                        try{ns.Read(type, 0, 4);}catch{goto tcheck;} //read type
                        data = new byte[BitConverter.ToInt32(len)]; //set data buffer to length of buffer to read
                        try{ns.Read(data, 0, BitConverter.ToInt32(len));}catch{goto timeout;} //read data
                        //if good, send code 0, else send failure code 1
                        if(tcpDecode(current, BitConverter.ToInt32(type), ASCIIEncoding.ASCII.GetString(data), BitConverter.ToInt32(len), client)){
                            try{ns.Write(BitConverter.GetBytes(0), 0, 4);}catch{goto tcheck;} //return code 0 (accepted)
                            heartbeat = true; //consider as heartbeat
                        }
                        else{
                            try{ns.Write(BitConverter.GetBytes(1), 0, 4);}catch{if(client.Connected){goto tcheck;}} //return code 1 (not accepted)
                                                                                                                     //if cannot write and client is connected, timeout
                                                                                                                     //if not connected, will disconnect on next loop
                        }
                        continue;

                        //check if real timeout, or if heartbeat has been sent in the meantime
                        tcheck: 
                        if (heartbeat){
                            heartbeat = false;
                            continue;
                        }
                        goto timeout;
                    }

                    //timeout goto if real timeout is caught, or client disconnects unexpectedly
                    //if current.tO is flagged, client has already disconnected
                    timeout:
                    if (current == null){
                        log(1, String.Format("Client on {0} timed out before initialising.", (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString()));
                        ns.Close();
                    }else if (!current.tO){
                        current.Disconnect("timed out.");
                        ns.Close();
                    }
                    //end goto if client is disposed of within / behind loop
                    end: ;
                });
                cThread.Start();
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