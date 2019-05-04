using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace MixMaster_Bot.LoginServer
{
    public class SendData
    {
        public static void SendVersion(Socket s, float version)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                int len = 0;
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    bw.Write((byte)10); // packet 
                    bw.Write((float)version);
                    len = (int)bw.BaseStream.Length;
                }
                byte[] buffer = ms.GetBuffer();
                Array.Resize(ref buffer, len);
                PacketFunctions.MakePacketAndSend(s, buffer);
            }
        }


        public static void SendLogin(Socket s, string Username, string Password)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                int len = 0;

                // user
                byte[] user_bytes = new byte[21];
                for(int i=0; i < Username.Length; i++)
                {
                    user_bytes[i] = (byte)Username[i];
                }
                // pass
                byte[] pass_bytes = new byte[21];
                for (int i = 0; i < Password.Length; i++)
                {
                    pass_bytes[i] = (byte)Password[i];
                }


                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    bw.Write((byte)21); // packet 
                    bw.Write(user_bytes);
                    bw.Write(pass_bytes);
                    len = (int)bw.BaseStream.Length;
                }
                byte[] buffer = ms.GetBuffer();
                Array.Resize(ref buffer, len);
                PacketFunctions.MakePacketAndSend(s, buffer);
            }
        }

        public static void EnterServer(Socket s)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                int len = 0;
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    bw.Write((byte)27); // packet 
                    bw.Write((byte)0x00);
                }
                byte[] buffer = ms.GetBuffer();
                Array.Resize(ref buffer, len);
                PacketFunctions.MakePacketAndSend(s, buffer);
            }
        }
    }
}
