using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace streamFile{
    public class Send
    {
        public static void ClientTcp(string ip, string toWrite){

        }
        public static void ServerTcp(string ip, string toWrite, TcpClient src){
            byte[] raw = File.ReadAllBytes(toWrite);
            NetworkStream ns = src.GetStream();
            ns.Write(BitConverter.GetBytes(raw.Length), 0, 4);
            for (int i = 0; i<raw.Length; i++){
                ns.WriteByte(raw[i]);
            }
            Console.WriteLine("Done!");
        }
    }
    public class Receive
    {
        public static void ClientTcp(string ip, string toRead, TcpClient src){
            NetworkStream ns = src.GetStream();
            byte[] bLen = new byte [4];
            ns.Read(bLen, 0, 4);
            int len = BitConverter.ToInt32(bLen);
            byte[] data = new byte[len];
            for (int i = 0; i<len; i++){
                int a;
                a = ns.ReadByte();
                byte[] as32 = BitConverter.GetBytes(a);
                data[i] = as32[0];
            }
            File.WriteAllBytes(toRead, data);
            Console.WriteLine("Done!");
        }
    }
}
