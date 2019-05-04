using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace MixMaster_Bot.ZoneServer
{
    public class SendData
    {
        public static void SendAUTH(Socket s, int idx, int token, byte hero_order, string username)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                int len = 0;
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    Console.WriteLine("IDX: " + idx + " TOken: " + token + " HeroOrder: " + hero_order + " Username: " + username);

                    bw.Write((int)Conn.ZoneServer.base_key); // token
                    bw.Write((int)idx);
                    bw.Write((byte)hero_order);
                    bw.Write((byte)100); // packet type
                    bw.Write((int)idx);
                    bw.Write((int)token);
                    bw.Write((byte)0x00);
                    foreach(char c in username)
                    {
                        bw.Write((byte)c);
                    }
                    bw.Write(new byte[] { 0x00, 0xBF, 0xB5, 0xC8, 0xF1, 0x00 });

                    len = (int)bw.BaseStream.Length;
                }
                byte[] buffer = ms.GetBuffer();
                Array.Resize(ref buffer, len);
                PacketFunctions.ZSMakePacketAndSend(s, buffer);
            }
        }



        public static void EnterGame(Socket s, int idx, byte hero_order)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                int len = 0;
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    bw.Write((Int32)Conn.ZoneServer.base_key);
                    bw.Write((int)idx);
                    bw.Write((byte)hero_order);
                    bw.Write((byte)0x69);

                    len = (int)bw.BaseStream.Length;
                }
                byte[] buffer = ms.GetBuffer();
                Array.Resize(ref buffer, len);
                PacketFunctions.ZSMakePacketAndSend(s, buffer);
            }
        }

        public static void SendWhispper(Socket s, string to, string message)
        {
            if(to.Length >= 12) { Console.WriteLine("[Whispper] Character name is big"); return; }
            if (message.Length >= 48) { Console.WriteLine("[Whispper] Message is big");  return; }

            using (MemoryStream ms = new MemoryStream())
            {
                int len = 0;
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    bw.Write((Int32)Conn.ZoneServer.base_key);
                    bw.Write((int)Conn.ZoneServer.id_idx);
                    bw.Write((byte)Conn.ZoneServer.hero_order);
                    bw.Write((byte)126); // packet type

                    // send to
                    foreach(char c in to)
                    {
                        bw.Write((byte)c);
                    }
                    bw.Write((byte)0x00);

                    // message
                    foreach(char c in message)
                    {
                        bw.Write((byte)c);
                    }

                    bw.Write((byte)0x00);

                    len = (int)bw.BaseStream.Length;
                }
                byte[] buffer = ms.GetBuffer();
                Array.Resize(ref buffer, len);
                PacketFunctions.ZSMakePacketAndSend(s, buffer);
            }
        }

        public static void SendGlobal(Socket s, string message)
        {
            if (message.Length >= 48) { Console.WriteLine("[Global] Message is big"); return; }
            using (MemoryStream ms = new MemoryStream())
            {
                int len = 0;
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    bw.Write((Int32)Conn.ZoneServer.base_key);
                    bw.Write((int)Conn.ZoneServer.id_idx);
                    bw.Write((byte)Conn.ZoneServer.hero_order);
                    bw.Write((byte)128); // packet type
                    bw.Write((byte)0x03); // gm command id

                    // message
                    foreach (char c in message)
                    {
                        bw.Write((byte)c);
                    }

                    bw.Write((byte)0x00);

                    len = (int)bw.BaseStream.Length;
                }
                byte[] buffer = ms.GetBuffer();
                Array.Resize(ref buffer, len);
                PacketFunctions.ZSMakePacketAndSend(s, buffer);
            }



        }



    }
}
