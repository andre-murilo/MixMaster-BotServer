using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace MixMaster_Bot.LoginServer
{
    class Conn
    {
        public class LoginServer
        {
            public static Socket s;
            public static IPEndPoint EndPoint;
            public const int BufferSize = 2048;
            public static byte[] buffer = new byte[BufferSize];
            public static string ip = "145.14.134.121";
            public static int port = 22006;
            public static float LGS_VERSION;


            public static string Usuario = string.Empty;
            public static string Password = string.Empty;
            public static int id_idx = 0;
            public static string GMS_IP = string.Empty;
            public static int GMS_PORT = 0;
            public static int GMS_TOKEN = 0;
            public static bool logado = false;
        }

        public static void StartLGSConn()
        {
            try
            {
                LoginServer.Usuario = "FireSharker";
                LoginServer.Password = "9921323412";

                LoginServer.EndPoint = new IPEndPoint(IPAddress.Parse(LoginServer.ip), LoginServer.port);
                LoginServer.s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                LoginServer.s.BeginConnect(LoginServer.EndPoint, new AsyncCallback(LGSConnectCallBack), LoginServer.s);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error StartLGSConn: " + e.ToString());
                Console.Read();
                Environment.Exit(0);
                return;
            }
        }


        private static void LGSConnectCallBack(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;
            if (s.Connected)
            {
                Console.WriteLine("[LGS] Conectado com sucesso!");
                s.BeginReceive(LoginServer.buffer, 0, LoginServer.BufferSize, SocketFlags.None, new AsyncCallback(LGSReceiveCallBack), s);
                SendData.SendVersion(s, LoginServer.LGS_VERSION);
                //byte[] MyVersionPacket = MakeMyVersionPacket(LoginServer.LGS_VERSION);
                //LGSSend(s, VersionPacket); // Enviar pacote da versão ...
            }
            else
            {
                Console.WriteLine("[LGS] Erro na hora de conectar!");
                Console.Read();
                Environment.Exit(0);
            }
        }


        private static void LGSReceiveCallBack(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;
            if(s == null) { return; }
            if(!s.Connected) { return; }

            try
            {
                int ReceiveBytes = s.EndReceive(ar);
                if (ReceiveBytes > 0)
                {
                    byte[] data = new byte[ReceiveBytes];

                    Array.Copy(LoginServer.buffer, data, ReceiveBytes);
                    LGSHandlePacket(s, data);

                }
                else
                {
                    Console.WriteLine("[-] LoginServer Disconnected!");
                    s.Dispose();
                }
                LoginServer.buffer = new byte[LoginServer.BufferSize];
                s.BeginReceive(LoginServer.buffer, 0, LoginServer.BufferSize, SocketFlags.None, new AsyncCallback(LGSReceiveCallBack), s);
            }
            catch
            {
                // disconnect
                Console.WriteLine("[-] LoginServer Disconnected!");
                s.Dispose();
                return;
            }
        }

        public static void LGSSend(Socket s, byte[] data)
        {
            try
            {
                s.BeginSend(data, 0, data.Length, 0, new AsyncCallback(LGSSendCallBack), s);
            }
            catch
            {
                return;
            }
        }
        private  static void LGSSendCallBack(IAsyncResult ar)
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

        private static void LGSHandlePacket(Socket s, byte[] data)
        {
            try
            {
                if(!s.Connected) { return; }

                ReadPacket packet = new ReadPacket(data, XCRYPT.LoginServerPrivkey);
                if(!packet.IsInitialized()) { return; }

                Header packet_header = packet.GetHeader();
                byte[] packet_data = packet.GetBody();


                byte packet_type = packet.GetPacketType();


                switch(packet_type)
                {
                    case 11: // version okay
                        Console.WriteLine("LGS Version OK!");
                        SendData.SendLogin(s, LoginServer.Usuario, LoginServer.Password);
                        break;
                    case 23: // user and password incorrects
                        Console.WriteLine("Usuario e senhadas incorretos!");
                        break;
                    case 24: // password incorrect
                        Console.WriteLine("Senha incorreta");
                        break;
                    case 22: // login efetuado com sucesso, go go gms haha
                        Console.WriteLine("Login realizado com sucesso!");
                        ParseLoginResponse(s, packet_data);

                        byte[] EnterGMSPacket = { 0x02, 0x00, 0x2a, 0xa1, 0xd6 };
                        LGSSend(s, EnterGMSPacket);
                        //SendData.EnterServer(s);
                        break;
                    case 28: // token lgs para autenticar no gms
                        Console.WriteLine("Token GMS recebido!");
                        ParseTokenGMS(s, packet_data);

                        LoginServer.s.Disconnect(false);
                        LoginServer.s.Dispose();


                        GameManagerServer.Conn.GameManagerServer gms = new GameManagerServer.Conn.GameManagerServer(LoginServer.GMS_IP, LoginServer.GMS_PORT, LoginServer.id_idx, LoginServer.GMS_TOKEN, LoginServer.Usuario);


                        break;
                }
            }
            catch
            {
                return;
            }

        }

        private static void ParseLoginResponse(Socket s, byte[] body)
        {
            using (MemoryStream stream = new MemoryStream(body))
            {
                using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8))
                {
                    br.ReadByte(); // packet type
                    br.ReadByte(); // gms server count
                    br.ReadByte();
                    byte[] server_namebuff = br.ReadBytes(16);
                    br.ReadByte();
                    short players_online = br.ReadInt16();
                    br.ReadByte();
                    byte[] gms_ipbuff = br.ReadBytes(15);
                    br.ReadByte();
                    short gms_port = br.ReadInt16();


                    string gms_ip = PacketFunctions.ExtractStringFromBytes(gms_ipbuff);

                    LoginServer.GMS_IP = gms_ip;
                    LoginServer.GMS_PORT = gms_port;
                    Console.WriteLine("Servername: " + gms_ip);
                    Console.WriteLine("Players Online: " + players_online);
                    Console.WriteLine("GMS: " + PacketFunctions.ExtractStringFromBytes(gms_ipbuff) + ":" + gms_port);
                }
            }

        }

        private static void ParseTokenGMS(Socket s, byte[] body)
        {
            using (MemoryStream stream = new MemoryStream(body))
            {
                using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8))
                {
                    br.ReadByte(); // packet type
                    Int32 id_idx = br.ReadInt32();
                    Int32 token = br.ReadInt32();

                    LoginServer.id_idx = id_idx;
                    LoginServer.GMS_TOKEN = token;
                    Console.WriteLine("id_idx: " + id_idx + " | Token: " + token);
                }
            }
        }
    }
}
