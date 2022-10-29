using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Reflection;
using static logMgr.logMgr;

namespace clientApp
{
    class threads
    {
        static char bell = Encoding.ASCII.GetString(new byte[]{ 7 })[0];
        static bool hbsend = false;
        static NetworkStream cns;
        static TcpClient cls;
        static bool valid = true;
        static bool tO = false;
        static List<string[]> tasklist = new List<string[]>();
        static TcpClient client;
        static void Main(string[] args)
        {
            start: ;
            Console.Clear();
            //get username + address and port of server
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Address: ");
            string address = Console.ReadLine();
            Console.Write("Port: ");
            int port;
            while (true){
                try{port = int.Parse(Console.ReadLine());break;}catch{Console.WriteLine("Port invalid - try again");} //validate port as int
            }
            client = new TcpClient(address, port);
            NetworkStream ns = client.GetStream();
            ns.ReadTimeout = 5000; //set read timeout to 5 seconds
            ns.WriteTimeout = 5000; //set write timeout to 5 seconds
            byte[] data = ASCIIEncoding.ASCII.GetBytes(username);
            byte[] code = new byte[4];
            try{ns.Write(BitConverter.GetBytes(data.Length), 0, 4);}catch{goto timeout;} //write length of data buffer
            try{ns.Write(data, 0, data.Length);}catch{goto timeout;} //write data buffer
            try{ns.Read(code, 0, 4);}catch{goto timeout;} //catch returned code
            if(BitConverter.ToInt32(code) == 1){
                ns.Close();
                client.Close();
                Console.WriteLine("Connection closed - error initializing. Hit enter to return.");
                goto start;
            }

            Console.WriteLine("Connection established!");

            var chatThread = new Thread(() => {
                while (client.Connected){
                    string cmd = Console.ReadLine();
                    if (client.Connected && cmd != null){
                        string[] toAppend = {"0", cmd};
                        tasklist.Add(toAppend);
                        Console.WriteLine("Broadcasting: '"+cmd+"'");
                    }
                }
            });
            chatThread.Start();

            //main task executor
            while (true){
                bool idle = true;
                if (tasklist != null){
                    foreach (string[] task in tasklist.ToList()){
                        int type = int.Parse(task[0]);
                        data = ASCIIEncoding.ASCII.GetBytes(task[1].ToString());
                        //try 3 times if failed, else continue (server fault?)
                        for (int i = 0; i<3; i++){
                            try{ns.Write(BitConverter.GetBytes(data.Length), 0, 4);}catch{goto timeout;} //send length of data buffer
                            try{ns.Write(BitConverter.GetBytes(type), 0, 4);}catch{goto timeout;} //send type of request
                            try{ns.Write(data, 0, data.Length);}catch{goto timeout;} //send data buffer
                            try{ns.Read(code, 0, 4);}catch{goto timeout;} //read code returned
                            if (BitConverter.ToInt32(code) != 1){
                                break;
                            }
                        }
                        tasklist.Remove(task);
                        idle = false;
                    }
                }
                //send heartbeat if idle
                if(idle){
                    Thread.Sleep(10);
                    data = ASCIIEncoding.ASCII.GetBytes("Heartbeat");
                    try{ns.Write(BitConverter.GetBytes(data.Length), 0, 4);}catch{goto timeout;} //send length of data buffer
                    try{ns.Write(BitConverter.GetBytes(80085), 0, 4);}catch{goto timeout;} //send type of request
                    try{ns.Write(data, 0, data.Length);}catch{goto timeout;} //send data buffer
                    try{ns.Read(code, 0, 4);}catch{goto timeout;} //read code returned
                }
            }

            //timeout goto if timeout is detected
            timeout: ;
            ns.Close();
            client.Close();
            Console.WriteLine("Connection closed - timeout with server. Hit enter to return.");
            Console.ReadLine();
            goto start;
        }
    }
}