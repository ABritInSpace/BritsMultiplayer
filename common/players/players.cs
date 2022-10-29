using System;
using System.Net.Sockets;
using System.Net;
using static logMgr.logMgr;

namespace players{
    public class Player
    {
        public Player (string address, string name, TcpClient cl)
        {
            srcAddress = address;
            Username = name;
            UUID = Guid.NewGuid().ToString();
            client = cl;
            tO = false;
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
            try{playerList.list.Remove(this);}catch{}
            log(1, String.Format("A remote client from {0} disconnected - {1}\n  - Username = {2}\n  - UUID = {3}", ip.Address.ToString(), reason, this.Username, this.UUID));
            this.tO = true;
        }
    }
    public static class playerList
    {
        public static List<players.Player> list = new List<Player>{};
    }
}