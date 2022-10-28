using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Reflection;
using static logMgr.logMgr;

namespace streamFile{
    public class Send
    {
        public static bool Tcp(string ip, string toWrite, TcpClient src){
            byte[] raw = File.ReadAllBytes(toWrite);
            NetworkStream ns = src.GetStream();
            ns.Write(BitConverter.GetBytes(raw.Length), 0, 4);
            ns.WriteTimeout = 5000;
            for (int i = 0; i<raw.Length; i++){
                try{
                    ns.WriteByte(raw[i]);
                }
                catch{
                    log(1, "Failed to send all bytes - write abort.");
                }
            }
            log(2, String.Format("Done! Sent {0} bytes.", raw.Length.ToString()));
            return true;
        }
        public static bool Tcp(string ip, byte[] toWrite, TcpClient src){
            byte[] raw = toWrite;
            NetworkStream ns = src.GetStream();
            ns.Write(BitConverter.GetBytes(raw.Length), 0, 4);
            ns.WriteTimeout = 5000;
            for (int i = 0; i<raw.Length; i++){
                try{
                    ns.WriteByte(raw[i]);
                }
                catch{
                    log(1, "Failed to send all bytes - write abort.");
                }
            }
            log(2, String.Format("Done! Sent {0} bytes.", raw.Length.ToString()));
            return true;
        }
    }
    public class Receive
    {
        public static byte[] Tcp(string ip, string toRead, TcpClient src){
            NetworkStream ns = src.GetStream();
            byte[] bLen = new byte [4];
            ns.Read(bLen, 0, 4);
            int len = BitConverter.ToInt32(bLen);
            ns.ReadTimeout = 5000;
            byte[] data = new byte[len];
            for (int i = 0; i<len; i++){
                try{
                    int a;
                    a = ns.ReadByte();
                    byte[] as32 = BitConverter.GetBytes(a);
                    data[i] = as32[0];
                }
                catch{
                    log(1, "Failed to receive all bytes - read abort.");
                    return null;
                }
            }
            if(!File.Exists(toRead) && toRead != null){
                File.Create(toRead).Close();
                File.WriteAllBytes(toRead, data);
            }else if (toRead != null){
                File.Delete(toRead);
                File.Create(toRead).Close();
                File.WriteAllBytes(toRead, data);
            }
            log(2, String.Format("Done! Received {0} bytes.", data.Length));
            return data;
        }
    }
}