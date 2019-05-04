using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MixMaster_Bot
{
    public class Loader
    {
        public static bool LoadMyInfo()
        {
            bool response = false;
            string dir = Directory.GetCurrentDirectory() + @"/ver.cfg";
            if (!File.Exists(dir))
            {
                response = false;
                return response;
            }

            using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(dir)))
            {
                using (BinaryReader br = new BinaryReader(ms, Encoding.UTF8))
                {
                    float version = br.ReadSingle();
                    LoginServer.Conn.LoginServer.LGS_VERSION = version;
                    response = true;
                }
                // dispose binary reader
            }
            // dispose memory stream
            return response;
        }
    }
}
