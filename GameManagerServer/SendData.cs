using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace MixMaster_Bot.GameManagerServer
{
    public class SendData
    {
        public static void SendInitial(Socket s, int idx, int token, string username)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                int len = 0;
                byte[] user_bytes = new byte[32];
                for(int i =0; i < username.Length; i++)
                {
                    user_bytes[i] = (byte)username[i];
                }
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    bw.Write((Int32)idx);
                    bw.Write((byte)0x00);
                    bw.Write((byte)0x00); // packet type
                    bw.Write((float)15.04f); // exe_ver
                    bw.Write((Int32)idx);
                    bw.Write((Int32)token);
                    bw.Write(user_bytes);
                    len = (int)bw.BaseStream.Length;
                }
                byte[] buffer = ms.GetBuffer();
                Array.Resize(ref buffer, len);
                PacketFunctions.GMSMakePacketAndSend(s, buffer);
            }
        }

        public static void SendListCharacters(Socket s, int idx)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                int len = 0;
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    bw.Write((Int32)idx);
                    bw.Write((byte)0x00);
                    bw.Write((byte)0x02); // packet type
                    len = (int)bw.BaseStream.Length;
                }
                byte[] buffer = ms.GetBuffer();
                Array.Resize(ref buffer, len);
                PacketFunctions.GMSMakePacketAndSend(s, buffer);
            }
        }

        public static void SelectEnterGameCharacter(Socket s, int idx, string hero_name, byte hero_order)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                int len = 0;

                byte[] hero_namebytes = new byte[13];
                for(int i = 0; i < hero_name.Length; i++)
                {
                    hero_namebytes[i] = (byte)hero_name[i];
                }
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    bw.Write((Int32)idx);
                    bw.Write((byte)0x00);
                    bw.Write((byte)0x06);
                    bw.Write((byte)hero_order);
                    bw.Write(hero_namebytes);
                    len = (int)bw.BaseStream.Length;
                }
                byte[] buffer = ms.GetBuffer();
                Array.Resize(ref buffer, len);
                PacketFunctions.GMSMakePacketAndSend(s, buffer);
            }
        }

    }
}
