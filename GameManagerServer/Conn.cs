using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace MixMaster_Bot.GameManagerServer
{
    public class Conn
    {
        public class GameManagerServer
        {
            public static Socket s;
            public static IPEndPoint EndPoint;
            public const int BufferSize = 2048;
            public static byte[] buffer = new byte[BufferSize];

            public static string ip = "";
            public static int port = 0;
            public static int token = 0;
            public static int id_idx = 0;
            public static string username;


            public static string charactername = "";
            public static byte hero_order = 0;

            public GameManagerServer(string _ip, int _port, int _idx, int _token, string _username)
            {
                ip = _ip;
                port = _port;
                id_idx = _idx;
                token = _token;
                username = _username;

                Console.Clear();
                Console.WriteLine("[*] Inicializando gms ...");
                Start();
            }


        }


        private static void Start()
        {
            GameManagerServer.s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint EndPoint = new IPEndPoint(IPAddress.Parse(GameManagerServer.ip), GameManagerServer.port);
            GameManagerServer.s.BeginConnect(EndPoint, new AsyncCallback(GMSConnectCallBack), GameManagerServer.s);
        }

        private static void GMSConnectCallBack(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;
            if (s.Connected)
            {
                Console.WriteLine("[GMS] Conectado com sucesso!");
                s.BeginReceive(GameManagerServer.buffer, 0, GameManagerServer.BufferSize, SocketFlags.None, new AsyncCallback(GMSReceiveCallBack), s);
                SendData.SendInitial(s, GameManagerServer.id_idx, GameManagerServer.token, GameManagerServer.username);
            }
            else
            {
                Console.WriteLine("[GMS] Erro na hora de conectar!");
                Console.Read();
                Environment.Exit(0);
            }
        }

        private static void GMSReceiveCallBack(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;
            if (s == null) { return; }
            if (!s.Connected) { return; }

            try
            {
                int ReceiveBytes = s.EndReceive(ar);
                if (ReceiveBytes > 0)
                {
                    byte[] data = new byte[ReceiveBytes];

                    Array.Copy(GameManagerServer.buffer, data, ReceiveBytes);
                    GMSHandlePacket(s, data);

                }
                else
                {
                    Console.WriteLine("[-] GMS Disconnected!");
                    s.Dispose();
                    return;
                }
                GameManagerServer.buffer = new byte[GameManagerServer.BufferSize];
                s.BeginReceive(GameManagerServer.buffer, 0, GameManagerServer.BufferSize, SocketFlags.None, new AsyncCallback(GMSReceiveCallBack), s);
            }
            catch
            {
                // disconnect
                Console.WriteLine("[-] GMS Disconnected!");
                s.Dispose();
                return;
            }
        }


        public static void GMSSend(Socket s, byte[] data)
        {
            try
            {
                s.BeginSend(data, 0, data.Length, 0, new AsyncCallback(GMSSendCallBack), s);
            }
            catch
            {
                return;
            }
        }
        private static void GMSSendCallBack(IAsyncResult ar)
        {
            try
            {
                Socket s = (Socket)ar.AsyncState;
                s.EndSend(ar);
            }
            catch
            {
                return;
            }
        }


        private static void GMSHandlePacket(Socket s, byte[] data)
        {
            try
            {
                if (!s.Connected) { return; }

                ReadPacket packet = new ReadPacket(data, XCRYPT.GameManagerServerPrivKey);
                if (!packet.IsInitialized()) { return; }

                Header packet_header = packet.GetHeader();
                byte[] packet_data = packet.GetBody();


                byte packet_type = packet.GetPacketType();
                Console.WriteLine("Received packet: " + packet_type);

                switch (packet_type)
                {
                    case 0:
                        Console.WriteLine("[*] Listando personagens ...");
                        SendData.SendListCharacters(s, GameManagerServer.id_idx);
                        break;
                    case 2: // my characters data
                        Console.WriteLine("[*] Personagens recebidos!");
                        ParseCharacters(packet_data);
                        break;
                    case 7: // connection aproved
                        Console.WriteLine("[*] Você logou no servidor com sucesso!");
                        break;
                    case 6: // select char
                        Console.WriteLine("Received: 6");
                        ParseEnterCharacter(packet_data);
                        break;
                    case 9: // gms info
                        Console.WriteLine("[*] Received gms info");
                        ParseGMSInfo(packet_data);
                        SendData.SelectEnterGameCharacter(s, GameManagerServer.id_idx, GameManagerServer.charactername, GameManagerServer.hero_order);
                        break;
                }
            }
            catch
            {
                return;
            }

        }


        private static void ParseCharacters(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8))
                {
                    br.ReadByte(); // packet type
                    byte characters_num = br.ReadByte();
                    for(int i = 0; i < characters_num; i++)
                    {
                        byte hero_order = br.ReadByte();
                        string hero_name = "";
                        char c;
                        while((c = br.ReadChar()) != (char)0x00)
                        {
                            hero_name += c;
                        }

                        byte hero_type = br.ReadByte();
                        short hero_lv = br.ReadInt16();
                        short hero_head = br.ReadInt16();
                        short hero_weapown = br.ReadInt16();
                        short hero_armour = br.ReadInt16();

                        byte hero_status = br.ReadByte();
                        br.ReadInt32();

                        byte hench_counts = br.ReadByte();
                        if (hench_counts > 0)
                        {
                            for (int k = 0; k < hench_counts; k++)
                            {
                                byte monster_order = br.ReadByte();
                                short monster_type = br.ReadInt16();
                            }
                        }

                        GameManagerServer.charactername = hero_name;
                        GameManagerServer.hero_order = hero_order;
                        Console.WriteLine("Hero: " + hero_name + " (" + hero_order + ") Type: " + hero_type + " LV: " + hero_lv);
                    }


                }
            }
        }

        private static void ParseEnterCharacter(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8))
                {
                    br.ReadByte(); // packet type
                    br.ReadByte(); // null
                    byte[] ip_zsbytes = br.ReadBytes(4);
                    Int16 zoneserver_port = br.ReadInt16();
                    byte zoneserver_crypto = br.ReadByte(); // null
                    Int32 zoneserver_token = br.ReadInt32();

                    IPAddress zoneserver_ip = new IPAddress(ip_zsbytes);


                    Console.WriteLine("[*] ZS-INFO: " + zoneserver_ip.ToString() + " Port: " + zoneserver_port + " Token: " + zoneserver_token);

                    ZoneServer.Conn.ZoneServer zs = new ZoneServer.Conn.ZoneServer(zoneserver_ip.ToString(), zoneserver_port, zoneserver_token, GameManagerServer.id_idx, GameManagerServer.charactername, GameManagerServer.hero_order, GameManagerServer.username);
                }
            }
        }

        private static void ParseGMSInfo(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8))
                {
                    br.ReadByte(); // packet type
                    int gms_num = br.ReadInt32();
                    Console.WriteLine("[+] GMS-NUM: " + gms_num);
                }
            }
        }
    }

}
