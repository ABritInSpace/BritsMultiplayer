using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace clientApp
{
    class threads
    {
        static char bell = Encoding.ASCII.GetString(new byte[]{ 7 })[0];
        static bool hbsend = false;
        static NetworkStream cns;
        static TcpClient cls;
        static bool valid = true;
        static void Main(string[] args)
        {
            Console.Clear();
            Console.Write("Username: ");
            string username = Console.ReadLine();
            TcpClient client = new TcpClient(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString(), 8080);
            NetworkStream ns = client.GetStream();
            string init = "0-"+username;
            ns.Write(ASCIIEncoding.ASCII.GetBytes(init));
            byte[] code = new byte[client.ReceiveBufferSize];
            ns.Read(code, 0, client.ReceiveBufferSize);
            Console.WriteLine(BitConverter.ToInt32(code));
            if (BitConverter.ToInt32(code) == 0){
                cns = ns;
                cls = client;
                timeout.Start();
            }
            while(true){
                if (BitConverter.ToInt32(code) == 1){
                    Console.WriteLine("Connection end!");
                    client.Close();
                    ns.Close();
                    break;
                }
                while (hbsend){}
                string input = "1-"+Console.ReadLine();
                byte[] buffer = ASCIIEncoding.ASCII.GetBytes(input);
                while (hbsend){}
                while (!valid){goto End;}
                ns.Write(buffer, 0, buffer.Length);
                while (hbsend){}
                while (!valid){goto End;}
                ns.Write(BitConverter.GetBytes(input.Length), 0, 4);
            }
            End: ;
        }
        static Thread timeout = new Thread(() => {
            while(true){
            Thread.Sleep(100);
            hbsend = true;
            Int32 ping = 80085;
            cns.Write(BitConverter.GetBytes(ping));
            //cns.ReadTimeout = 5000;
            byte[] returned = new byte[4];
            try{cns.Read(returned, 0, 4);}
            catch{returned = BitConverter.GetBytes(0);}
            if (BitConverter.ToInt32(returned) != 80085){
                cns.Close();
                cls.Close();
                valid = false;
                hbsend = false;
                break;
            }
            hbsend = false;
            }
        });
    }
}