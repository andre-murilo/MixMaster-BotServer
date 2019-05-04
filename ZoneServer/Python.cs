using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixMaster_Bot.ZoneServer
{
    public class XClient
    {
        public Socket s;
        public const int buffer_size = 2048;
        public byte[] buffer = new byte[2048];
        public int id;
    }

    public class ClientFunctions
    {
        public static XClient GetInstance(int id)
        {
            for(int i = 0; i < Python.clients.Count; i++)
            {
                if(Python.clients[i].id == id)
                {
                    return Python.clients[i];
                }
            }
            return null;
        }

        public static void DisconnectClient(int id)
        {
            for (int i = 0; i < Python.clients.Count; i++)
            {
                if (Python.clients[i].id == id)
                {
                    if(Python.clients[i].s.Connected)
                        Python.clients[i].s.Disconnect(false);
                    Python.clients[i].s.Dispose();
                    Python.clients.Remove(Python.clients[i]);
                }
            }
        }

    }

 
    public class Python
    {
        public static List<XClient> clients;


        public static void Start()
        {
            Console.WriteLine("[Python] Iniciando modulos ...");
            clients = new List<XClient>();
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.NoDelay = true;
            IPEndPoint EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55670);
            s.Bind(EndPoint);
            s.Listen(5);
            s.BeginAccept(new AsyncCallback(AcceptCallback), s);
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;
            if(s == null) { return; }

            try
            {
                Socket connClient = s.EndAccept(ar);
                XClient client = new XClient();
                client.s = connClient;
                client.id = connClient.GetHashCode();
                clients.Add(client);

                Console.WriteLine("[Python] Client connected!");
                client.s.BeginReceive(client.buffer, 0, XClient.buffer_size, SocketFlags.None, new AsyncCallback(ReceiveCallback), client);
            }
            catch
            {
                Console.WriteLine("[Python] Accept client error!");
                return;
            }
            finally
            {
                s.BeginAccept(new AsyncCallback(AcceptCallback), s);
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            XClient client = (XClient)ar.AsyncState;
            if(client == null) { return; }
            if(!client.s.Connected) { return; }
            try
            {
                int bytes_received = client.s.EndReceive(ar);
                if(bytes_received > 0)
                {
                    byte[] data = new byte[bytes_received];
                    Array.Copy(client.buffer, data, bytes_received);

                    // process data
                    ProcessData(client, data);

                    client.buffer = new byte[XClient.buffer_size];
                    client.s.BeginReceive(client.buffer, 0, XClient.buffer_size, SocketFlags.None, new AsyncCallback(ReceiveCallback), client);
                }
                else
                {
                    // client disconnected
                    ClientFunctions.DisconnectClient(client.id);        
                }
            }
            catch
            {
                // disconnect client
                ClientFunctions.DisconnectClient(client.id); 
                return;
            }
        }

        private static void SendString(XClient client, string data)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                client.s.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, new AsyncCallback(SendCallback), client.s);
            }
            catch
            {
                return;
            }
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket s = ar.AsyncState as Socket;
                s.EndSend(ar);
            }
            catch
            {
                return;
            }
        }

        private static void ProcessData(XClient client, byte[] data)
        {
            try
            {
                string raw_message = Encoding.UTF8.GetString(data);
                string message = string.Join(",", data);
                Console.WriteLine("[Python] Received: " + raw_message);

                using (MemoryStream ms = new MemoryStream(data))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        char packet_type = br.ReadChar();
                        byte[] data_body = br.ReadBytes(data.Length - 1);
                        string body = Encoding.UTF8.GetString(data_body);

                        try
                        {
                            switch (packet_type)
                            {
                                case 'a':
                                    string[] splited = body.Split('|');
                                    string target = splited[0];
                                    string msg = splited[1];
                                    Console.WriteLine("[" + target + "] " + msg);
                                    SendData.SendWhispper(Conn.ZoneServer.s, target, msg);
                                    SendString(client, "received");
                                    break;
                                case 'b':
                                    SendData.SendGlobal(Conn.ZoneServer.s, body);
                                    SendString(client, "received");
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch
                        {
                            return;
                        }
                    }
                }
            }
            catch
            {
                return;
            }
        }





    }
}
