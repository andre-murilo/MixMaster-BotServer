using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace MixMaster_Bot
{
 
    public struct Datas
    {
        public string nome;
        public int value;
    }

    class Init
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Reading my info...");
            if(!Loader.LoadMyInfo())
            {
                Console.WriteLine("Failed to read my info data!");
                return;
            }
            Console.WriteLine("Version: " + LoginServer.Conn.LoginServer.LGS_VERSION);

            Console.WriteLine("[+] Conectando com o banco de dados ...");
            if(!Database.gamedata.ConnectGamedata()) { return; }
            if (!Database.member.ConnectMember()) { return; }


            Console.WriteLine("[+] Conectando com o LoginServer ...");
            LoginServer.Conn.StartLGSConn();

            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
        }
    }
}
