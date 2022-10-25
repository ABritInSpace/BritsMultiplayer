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
        static void Main(string[] args)
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            TcpClient client = new TcpClient(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString(), 8080);
            NetworkStream ns = client.GetStream();
            string init = "0-"+username;
            ns.Write(ASCIIEncoding.ASCII.GetBytes(init), 0, ASCIIEncoding.ASCII.GetBytes(init).Length);
            byte[] code = new byte[client.ReceiveBufferSize];
            ns.Read(code, 0, client.ReceiveBufferSize);
            Console.WriteLine(BitConverter.ToInt32(code));
            while(true){
                if (BitConverter.ToInt32(code) == 1){
                    Console.WriteLine("Connection end!");
                    client.Close();
                    break;
                }
                byte[] buffer = ASCIIEncoding.ASCII.GetBytes("1-"+Console.ReadLine());
                ns.Write(buffer, 0, buffer.Length);
            }
        }
    }
}