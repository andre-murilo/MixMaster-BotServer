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
    public class Conn
    {
        public class ZoneServer
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
            public static string hero_name;
            public static byte hero_order;
            public static int base_key = 1431655765;


            public ZoneServer(string _ip, int _port, int _token, int _idx, string _hero_name, byte _hero_order, string _username)
            {
                ip = _ip;
                port = _port;
                token = _token;
                id_idx = _idx;
                hero_name = _hero_name;
                hero_order = _hero_order;
                username = _username;

                Console.Clear();
                Console.WriteLine("---------- ZoneServer ---------", ConsoleColor.Red);
                Start();
            }

        }


        private static void Start()
        {
            ZoneServer.s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ZoneServer.s.NoDelay = true;
            IPEndPoint EndPoint = new IPEndPoint(IPAddress.Parse(ZoneServer.ip), ZoneServer.port);
            ZoneServer.s.BeginConnect(EndPoint, new AsyncCallback(ZSConnectCallBack), ZoneServer.s);
        }

        private static void ZSConnectCallBack(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;
            if (s.Connected)
            {
                Console.WriteLine("[ZS] Conectado com sucesso!");
                Python.Start();
                s.BeginReceive(ZoneServer.buffer, 0, ZoneServer.BufferSize, SocketFlags.None, new AsyncCallback(ZSReceiveCallBack), s);
            }
            else
            {
                Console.WriteLine("[ZS] Erro na hora de conectar!");
                Console.Read();
                Environment.Exit(0);
            }
        }

        private static void ZSReceiveCallBack(IAsyncResult ar)
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

                    Array.Copy(ZoneServer.buffer, data, ReceiveBytes);
                    ZSHandlePacket(s, data);

                }
                else
                {
                    Console.WriteLine("[-] GMS Disconnected!");
                    s.Dispose();
                }
                ZoneServer.buffer = new byte[ZoneServer.BufferSize];
                s.BeginReceive(ZoneServer.buffer, 0, ZoneServer.BufferSize, SocketFlags.None, new AsyncCallback(ZSReceiveCallBack), s);
            }
            catch
            {
                // disconnect
                Console.WriteLine("[-] GMS Disconnected!");
                s.Dispose();
                return;
            }
        }

        private static void ZSHandlePacket(Socket s, byte[] data)
        {
            try
            {
                if (!s.Connected) { return; }

                Packet[] packets = ReadPackets.ReadAllPackets(data);

                for(int i = 0; i < packets.Count(); i++)
                {
                    byte[] body = packets[i].body;
                    byte packet_type = body[0];

                    switch(packet_type)
                    {
                        case 101:
                            SendData.SendAUTH(s, ZoneServer.id_idx, ZoneServer.token, ZoneServer.hero_order, ZoneServer.username);
                            break;
                        case 206:
                            SendData.EnterGame(s, ZoneServer.id_idx, ZoneServer.hero_order);
                            Thread antikick = new Thread(new ThreadStart(AntiKick));
                            antikick.Start();
                            break;
                        case 126:
                            ParseWhispperChat(s, body);
                            break;
                    }
                }
            }
            catch
            {
                return;
            }

        }


        public static void ParseWhispperChat(Socket s, byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    br.ReadByte();
                    string PlayerSource = "";
                    char c;
                    while ((c = br.ReadChar()) != (char)0x00)
                    {
                        PlayerSource += c;
                    }

                    string Mensagem = string.Empty;
                    while ((c = br.ReadChar()) != (char)0x00)
                    {
                        Mensagem += c;
                    }

                    ParseCommand(s, PlayerSource, Mensagem);
                }
            }
        }


        public static bool IsHaveSpaceInString(string s)
        {
            foreach(char c in s)
            {
                if(c == (char)0x20)
                {
                    return true;
                }
            }
            return false;
        }

        public static void ParseCommand(Socket s, string source, string message)
        {
            try
            {
                // # player
                    // money
                    // gp_top
                    // last_trades
                    // formula [Dragoer]
                    // players_online
                    // castle_war
                    // gametime
                    // viciados
                    
                // verificar se tem espaco no comando
                if(IsHaveSpaceInString(message))
                {
                    string[] commands = message.Split((char)0x20);
                    switch(commands[0])
                    {
                        // GM COMMANDS
                        case "add_cash":
                            break;
                        case "add_item":
                            break;
                        case "ban":
                            break;
                        case "desban":
                            break;
                        case "start_event":
                            break;

                        // Player commands
                        case "formula":
                            string hench_name_nolow = commands[1];
                            string hench_name = commands[1].ToLower();
                            string no_formula = " nao tem formula!";
                            switch (hench_name)
                            {
                                #region dragon
                                case "draco":
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "pikey":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Draco + Flowco");
                                    break;
                                case "imon":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Draco + Devilco");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Draco + Chicoe");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Draco + Rabbo");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Pikey + Pee");
                                    break;
                                case "noa":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Draco + Birdco");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Pikey + Metalocks");
                                    break;
                                case "draki":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Pikey + Beasco");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Pikey + Ukki");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Imon + Goa");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Imon + Bebe");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Noa + Goa");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Noa + Rurrabbi");
                                    break;
                                case "gago":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Imon + Inseco");
                                    break;
                                case "bigon":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Gago + Oilpot");
                                    break;
                                case "sq-21":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Imon + MechaBall");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Imon + Yeon");
                                    break;
                                case "kurma":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Gago + Mosky");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Bigon + Bota");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Bigon + Sumping");
                                    break;
                                case "deepsuffer":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Gago + Beanie");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Gago + Bota");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Gago + Sumping");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Pikey + MechaBall");
                                    break;
                                case "mitra":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Gago + Pengie");
                                    break;
                                case "armaball":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Bigon + NTV");
                                    break;
                                case "haedragon":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Kurma + CheezDogg");
                                    break;
                                case "battledragon":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Mitra + HighLife");
                                    break;
                                case "battleroyale":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> BattleDragon + MysticYoyo");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> BattleDragon + Asakayza");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> BatleDragon + Gamerika (1)");
                                    break;
                                case "lausta":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Haedragon + Synicks");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Haedragon + Cactusing");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> BatleDragon + Skullwiser");
                                    break;
                                case "laustar":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Haedragon + Synicks");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Haedragon + Cactusing");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> BatleDragon + Skullwiser");
                                    break;
                                case "greentail":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> BattleDragon + Synicks");
                                    break;
                                case "silverlausta":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> BattleRoyale (1) + Gamerika (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> BattleDragon (1) + Chowie (1)");
                                    break;
                                case "silverlaustar":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> BattleRoyale (1) + Gamerika (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> BattleDragon (1) + Chowie (1)");
                                    break;
                                case "dragoer":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> SilverLaustar (1) + Manta");
                                    break;
                                case "neosilver":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> SilverLaustar (1) + Kuguto (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> SilverLaustar (1) + KingDusty (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> SilverLaustar (1) + MintRabie");
                                    break;
                                case "braki":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Neo Silver (1) + Gokuma (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Neo Silver (1) + Snogyun");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Neo Silver (1) + BladeDasha");
                                    break;
                                case "blazerhino": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "Sneaky": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "seki-shu-ryu":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> BlazeRhino (2) + Ordevil (2)");
                                    break;
                                case "seki": // seki-shu-ryu abreviation
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> BlazeRhino (2) + Ordevil (2)");
                                    break;
                                case "boardgon":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> BlazeRhino (2) + Wagstuff (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> BlazeRhino (2) + KimeBleu (2)");
                                    break;
                                case "armored":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Braki (2) + GreenTravel (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Braki (2) + BomberGun (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Braki (2) + RockyRush (2)");
                                    break;
                                case "ki-ryu":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Seki-Shu-Ryu (2) + BomberGun (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Seki-Shu-Ryu (2) + Rhine (2)");
                                    break;
                                case "fairudo":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Seki-Shu-Ryu (2) + NeoGokuma (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Seki-Shu-Ryu (2) + WildBallza (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Ky-Ryu (2) + WildBallza (2)");
                                    break;
                                case "loisy":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Sneaky (2) + EvilClaw (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Sneaky (2) + DevilStone (2)");
                                    break;
                                case "wypin":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> MerryBongBong (2) + Seki-Shu-Ryu (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> BoxingTower (2) + Seki-Shu-Ryu (2)");
                                    break;
                                case "djbraki":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> TheUnknown (2) + Fairudo (3)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> NeoOrdevil (2) + Fairudo (3)");
                                    break;
                                case "fireduke":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Ki-Ryu (2) + Gunsmash (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Ki-Ryu (2) + WingThunder (2)");
                                    break;
                                case "fairudojaune":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Seki-Shu-Ryu (2) + NewDelcoi (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Sneaky (2) + EvilClaw (2)");
                                    break;
                                case "drilldra":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> Fairudo (2) + Armored (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " -> WhingThunder (2) + Armored (2)");
                                    break;
                                case "neoseki-shu-ryu":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Fairudo (2) + DarkTravel (2)");
                                    break;
                                case "neoseki":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Fairudo (2) + DarkTravel (2)");
                                    break;
                                case "mirnoir":
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "puppleqoon":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  FairudoJaune (2) + FrankenNo1");
                                    break;
                                case "infernoduke":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Fireduke (2) + KillerZill (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Fireduke (2) + MintLionKing (2)");
                                    break;
                                case "punchdra":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Onestep (3) + FrankenNo2 (3)");
                                    break;
                                case "prozenrhino":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  InfernoDuke (2) + XT-Bone (2)");
                                    break;
                                case "frozenrhino":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  InfernoDuke (2) + XT-Bone (2)");
                                    break;
                                case "newfiredragon":
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "rfairudo":
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "darkqoon":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  FrankenNo3 (3) + DustyElder (3)");
                                    break;
                                case "bluegarugon":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  DarkQoon (3) + KalinAngel (3)");
                                    break;
                                case "garugon":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BlueGarugon (3) + MadOrchid (3)");
                                    break;
                                case "kinggarugon":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Garugon (3) + RooflileGeneral (3)");
                                    break;
                                #endregion dragon

                                #region planta
                                case "flowco": // no have formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "manglock":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Flowco + Draco");
                                    break;
                                case "jamoo":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Manglock + Devilco");
                                    break;
                                case "mameo":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Manglock + Rabbo");
                                    break;
                                case "beanie":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Jamoo + Chicoe");
                                    break;
                                case "yeon":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Jamoo + Bookworm");
                                    break;
                                case "crutchie":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Mameo + NortsNcross");
                                    break;
                                case "nars":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Mameo + SnakeEye");
                                    break;
                                case "timtona":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Yeon + Soo");
                                    break;
                                case "spinemush":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Yeon + Cabbager");
                                    break;
                                case "lemoni":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Crutchie + Buma");
                                    break;
                                case "douda":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Crutchie + Kurma");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Crutchie + StarBird");
                                    break;
                                case "pana":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Nars + Starbird");
                                    break;
                                case "yassie":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  TimTona + Sumping");
                                    break;
                                case "cactusing":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Douda + Delcoi");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Douda + Oscar");
                                    break;
                                case "hornmameo":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Yassei + Liddy");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Yassei + Monoeye");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Yassei + Synicks");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Yassei + Delcoi");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Yassei + OneStump");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Yassei + Oscar");
                                    break;
                                case "tenkaki":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Douda + OneStump");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Douda + Monoeye");
                                    break;
                                case "chikaki":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Pana + Rabie");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Yassei + Asakayza");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Cactusing + Bolarish");
                                    break;
                                case "ballza":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Chikaki (1) + ArmaBall");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Chikaki (1) + HornKing (1)");
                                    break;
                                case "mantyplant":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Ballza (1) +SwordTail (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Ballza (1) + Dragoer (1)");
                                    break;
                                case "shin-chikaki":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Mantyplant (1) + Madtailor (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MantyPlant (1) + Ayaya (1)");
                                    break;
                                case "shinchikaki": // abreviacao shin-chikaki
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Mantyplant (1) + Madtailor (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MantyPlant (1) + Ayaya (1)");
                                    break;
                                case "wildballza":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Shin-Chikaki + WingStormer");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Ballza (1) + InsaneDoctor (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Ballza (1) + Pumped (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Shin-Chikaki + NeoSilver (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Shin-Chikaki + Ordevil (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Shin-Chikaki + Stoner (1)");
                                    break;
                                case "cabid": // no have formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "merrybongbong": // no have formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "sunflower":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  WildBallza (2) + NeoMad (2)");
                                    break;
                                case "palmboy": // no have formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "nauren":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Hellboy (2) + Cabit");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MacT-Bone (2) + Cabit");
                                    break;
                                case "newundergi": // no have formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "elder":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SunFlower (2) + WhiteWags (2)");
                                    break;
                                case "petitchikaki":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Faibizu (2) + Chikaki");
                                    break;
                                case "eldering":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Elder (2) + Snowball (2)");
                                    break;
                                case "dustyelder":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Nauren (2) + Mr.Rupert (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Nauren (2) + DeadlyBamudar (2)");
                                    break;
                                case "petittentaki":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  RBanshe (2) + Tentaki");
                                    break;
                                case "neosunflower":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Eldering (2) + Banshee (2)");
                                    break;
                                case "redleaf":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Elder (2) + MintLionKing (2)");
                                    break;
                                case "tinkerbell":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BigHead (3) + GateToDeath (3)");
                                    break;
                                case "madorchid":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Avenger (3) + Cupid (3)");
                                    break;
                                case "garlingz":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoBanshee (2) + KingRhine (2)");
                                    break;
                                case "gaga": // abreviaçao garlingz
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoBanshee (2) + KingRhine (2)");
                                    break;
                                case "napenthes":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Garlingz (3) + PersonaSoul (3)");
                                    break;
                                #endregion planta

                                #region animal
                                case "beasco": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "rabbo":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Beasco + Draco");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Beasco + Rurabbi");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Beasco + Birdco");
                                    break;
                                case "goa":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Beasco + Flowco");
                                    break;
                                case "nosie":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabbo + Chicoe");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabbo + Ukki");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabbo + Rurabbi");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Goa + Bebe");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Beasco + Buma");
                                    break;
                                case "crybop":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabbo + MetaLocks");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabbo + Beasco");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabbo + Pee");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Nossie + SnakeEye");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Nossie + Pengie");
                                    break;
                                case "turton":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Goa + NortsNcross");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Goa + BicMaq");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Nosie + Mameo");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  CryBop + MechaBall");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  CryBop + PoochDev");
                                    break;
                                case "suneack":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Goa + Jamoo");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Nosie + Beanie");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Nosie + Nippa");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  CryBop + Mosky");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  CryBop + MelonFlyer");
                                    break;
                                case "cheezdogg":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Goa + BookWorm");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Nosie +RingWorm #1");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  CryBop + Yeon");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SunEack + ForceFlyer");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SunEack + NTV");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Turton + Gago");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Turton + Oilpot");
                                    break;
                                case "rabie":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  CheezDog + Kurma");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  CheezDog + ForceFlyer");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  CryBop + Pengie");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SunEack + CheezDog");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Turton + Buma");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Turton + RingWorm #2");
                                    break;
                                case "mimieack":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabie + DeepSuffer");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabie + Monggyal");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  CheezDog + CheezDog");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  CheezDog + Bota");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Turton + MechaBall");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Turton + MelonFlyer");
                                    break;
                                case "cowty":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabie + Chowie");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabie + Dr.Calvin");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabie + Bubri");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabie + Mitra");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabie + PayaPaya");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  CheezDog + Yeon");
                                    break;
                                case "onestump":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabie + TankTortus");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabie + HighLife");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabie + Bangie");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabie + TimTona");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MummyEack + SpineMuch");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MummyEack + NightIme");
                                    break;
                                case "mintrabie": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "liddy":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rabie + Headragon");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MummyEack + Lemoni");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MummyEack + RingWorm#1");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MummyEack + Baccho");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  OneStump + ArmaBall");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  OneStump + Pana");
                                    break;
                                case "torra":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  OneStump + PayaPaya");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  OneStump + MonoEye");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  OneStump + DayCrawl");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  OneStump + Cactusing");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  OneStump + Cowty");
                                    break;
                                case "clawless":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Liddy + Bota");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Liddy + Oscar");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Liddy + Sinicks");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Liddy + Douda");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Liddy + Pana");
                                    break;
                                case "rrainova":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Torra + BatleDragon");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Torra + Clawless");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Torra + Cactusing");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Torra + HornKing (1)");
                                    break;
                                case "rainova": // Rrainova abreviacao
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Torra + BatleDragon");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Torra + Clawless");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Torra + Cactusing");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Torra + HornKing (1)");
                                    break;
                                case "dashabell":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rrainova (1) + Thunderbird (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rrainova (1) + BatleRoyale (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rrainova (1) + BeetlePete (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rrainova (1) + SwordTail (1)");
                                    break;
                                case "mintclaw":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Dashabell (1) + WingCrush (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Dashabell (1) + Ballza (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Dashabell (1) + KingDusty (1)");
                                    break;
                                case "torrax":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Dashabell (1) + Phoenix (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Torra + InsaneDoctor (1)");
                                    break;
                                case "bladedashabell": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "wagstuff":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MintClaw (1) + PhantomWing-X (1)");
                                    break;
                                case "greentravel":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Wagstuff (2) + Snogyun (2)");
                                    break;
                                case "evilclaw":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MintClaw (1) + Gokuma(1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MintClaw (1) + MintClaw (1)");
                                    break;
                                case "whitewags":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  GreenTravel (2) + CrimsonMetal (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  GreenTravel (2) + Seki-Shu-Ryu (2)");
                                    break;
                                case "mintlion":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MintClaw  (2) + Shin-Chikaki (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MintClaw  (2) + MacT-Bone (2)");
                                    break;
                                case "snowfoxy":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  WhiteWags (2) + Siper (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  WhiteWags (2) + WildBallza (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  GreenTravel (2) + BomberGun (2)");
                                    break;
                                case "rhine":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  EvilClaw (2) + NeoMad (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  EvilClaw (2) + Maleki (2)");
                                    break;
                                case "bluepigy":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Snowfoxy (2) + TheUnknown (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Snowfoxy (2) + Harpy (2)");
                                    break;
                                case "darktravel": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "mintlionking":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rhine (2) + DadlyBamude (2)");
                                    break;
                                case "supercat":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Blue Piggy (3) + WingThunder (3)");
                                    break;
                                case "kingrhine": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "neomintlionking": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "windgirl": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "lightingman":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoMintLion (3) + NapheleMan (3)");
                                    break;
                                case "cutiecat":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SuperCat (3) + Old Metal (3)");
                                    break;
                                case "hellfard":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  CutieCat (3) + Garugon (3)");
                                    break;
                                #endregion animal

                                #region demon
                                case "devilco": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "rurabbi":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Devilco + Draco");
                                    break;

                                case "bebe":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Devilco + Rabbo");
                                    break;

                                case "buma":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Devilco + Caminho");
                                    break;

                                case "poochdev":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rurabbi + Pee");
                                    break;

                                case "marogni":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Rurabbi + Mameo");
                                    break;

                                case "pirosty":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Bebe + NortsNcross");
                                    break;

                                case "monggyal":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Bebe + Oilpot");
                                    break;

                                case "wildbuma":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Pirosty + Sumping");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Pirosty + Bota");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Marogni + Doodoo");
                                    break;

                                case "moonraid":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Marogni + CryBop");
                                    break;

                                case "arndev":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Pirosty + Rabbo");
                                    break;

                                case "mahollow":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Monggyal + Sumping");
                                    break;

                                case "mysticyoyo": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "skullwiser":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Mahollow + NightIme");
                                    break;

                                case "amazonez":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MysticYoyo + Tenkaki (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Skullwiser + MysticYoyo");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Skullwiser + AcientKilla");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MysticYoyo + Azakaysa");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MysticYoyo + Torra");
                                    break;

                                case "wiesha": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "pumped":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Skullwiser + Asakayza");
                                    break;

                                case "tomated":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Pumped (1) + ClawLess (1)");
                                    break;

                                case "pumpedcurse":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Pumped (1) + BattleRoyale (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Pumped (1) + Thunderbird (1)");
                                    break;

                                case "kuguto":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Amazonez (1) + Laustar (1)");
                                    break;

                                case "blackened":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Kugutu (1) + GreenTail (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Kugutu (1) + KingDusty (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Kugutu (1) + Phoenix (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Kugutu (1) + InsaneDoctor(1)");
                                    break;

                                case "phantomwing": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "ordevil":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  PhantomWing (1) + Dashabell (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  PhantomWing (1) + SoulBreaker (1)");
                                    break;

                                case "succubus": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "devilstone": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "pbanshe": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "hellboy":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Blackned (1) + WingStormer (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Blackned (1) + CrimsonMetal (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Blackned (1) + TorraX (2)");
                                    break;

                                case "maleki": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "hornmaleki": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "neoordevil":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Ordevil (2) + EvilClaw (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Ordevil (2) + MacT-Bone (2)");
                                    break;

                                case "battleamazonez":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Amazonez (1) + HellBoy (2)");
                                    break;

                                case "neodevilstone":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Devilstone (2) + WildBallza (2)");
                                    break;

                                case "failbizu":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  HellBoy (2) + MintLion (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  HellBoy (2) + NeoMad (2)");
                                    break;

                                case "saruff":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoOrdevil (2) + Ziller (2)");
                                    break;

                                case "bdevilwing": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "frankenno1":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  PBanshee (2) + SunFlower (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  PBanshee (2) + SnowFoxy (2)");
                                    break;

                                case "onestep":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Failbizu (3) + Loisy (3)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Wypin (3) + Loisy (3)");
                                    break;

                                case "rbanshee":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  HornMaleki (2) + PalmBoy (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  HornMaleki (2) + SunFlower (2)");
                                    break;

                                case "banshee":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Saruff (2) + Rhine (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Saruff (2) + Mr.Rupert (2)");
                                    break;

                                case "frankenno2":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoDevilStone (3) + TopDrum (3)");
                                    break;

                                case "boneqoontra":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Saruff (2) + DevilStone (2)");
                                    break;

                                case "gatetodeath":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Banshee (2) + Siperous (2)");
                                    break;

                                case "frankenno3":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  FrankenNo1 (3) + NeoSeki-Shu (3)");
                                    break;

                                case "neobanshee": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "bluesaurph": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "bluesaruff": // abreviacao de bluesaurph // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "pdevilwing":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  RedLeaf (3) + BoneQoomtra (3)");
                                    break;

                                case "dalsipper":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  QueenSting (2) + NeoSunFlower (2)");
                                    break;

                                case "qoomtra":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  InfernoDuke (3) + Blue Saurph (3)");
                                    break;

                                case "avenger":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Punchdra (3) + PurpleQoon (3)");
                                    break;

                                case "ybanshe":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Scorpasis (3) + NewMintmeul (3)");
                                    break;

                                case "ninjagirl":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  GateToDeath”Droped” (2) + BomberGunX (2)");
                                    break;

                                case "anubis":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NinjaGirl (3) + Soul Eater (3)");
                                    break;

                                #endregion demon

                                #region bird
                                case "birdco": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "chicoe":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Birdco + Draco");
                                    break;

                                case "caminho":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Birdco + Devilco");
                                    break;

                                case "pengie":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Chicoe + Rabbo");
                                    break;

                                case "melonflyer":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Chicoe + Pee");
                                    break;

                                case "doodoo":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Caminho + Manglock");
                                    break;

                                case "starbird":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Pengie + Haneping");
                                    break;

                                case "forceflyer":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Birdco + MechaBall");
                                    break;

                                case "highlife":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Starbird + Pikey");
                                    break;

                                case "brubli":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Caminho + Jamoo");
                                    break;

                                case "nightime":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  DooDoo + Buma");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  HightLife + PayaPaya");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  DooDoo + Yeon");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  ForceFlyer + BellyLady");
                                    break;

                                case "oscar":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Pengie + Pirosty");
                                    break;

                                case "synicks":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  HighLife + ArnDev");
                                    break;

                                case "manta":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Brubli + Kurma");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Brubli + CrezzDog");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  ForceFlyer + Chutchie");
                                    break;

                                case "thunderbird":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Synicks + BattleDragon");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Synicks + Asakayza");
                                    break;

                                case "swordtail":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Thunderbird + Thunderbird (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Thunderbird + Amazones (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Thunderbird + Tentaki (1)");
                                    break;

                                case "phoenix":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SwordTail (1) + BattleRoyale (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SwordTail (1) + ThunderBird(1)");
                                    break;

                                case "madtailor":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SwordTail (1) + Rrainova (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SwordTail (1) + BeetlePete (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SwordTail (1) + Phoenix (1)");
                                    break;

                                case "wingcrusher":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Phoenix (1) + SwordTail (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Phoenix (1) + Rrainova (1)");
                                    break;

                                case "wingstormer":  // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "redballon":  // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "jone":  // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "rockyrush":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  WingStormer+ TorraX (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  WingStormer + PhantonWing-X (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  WingStormer + NeoSoul (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  WingStormer + Snogyun (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  WingStormer + BladeDasha (2)");
                                    break;

                                case "firebird":  // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "neomad":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  RockyRush (2) + BomberGun (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  RockyRush (2) + BladeDasha (2)");
                                    break;

                                case "yellowballon":  // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "harpy":  // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "wingthunder":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoMad (2) + NeoOrdevil (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoMad (2) + Siper (2)");
                                    break;

                                case "soldierhawk":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  FireBird (2) + WhiteWags (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  FireBird (2) + Seki-Shu-Ryu (2)");
                                    break;

                                case "jack":  // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "griffin":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Harpy (2) + TheUnknown (2)");
                                    break;

                                case "blueballon":  // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "orangeballon":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  RedBallon (2) + YellowBallon (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  YellowBallon (2) + RedBallon (2)");
                                    break;

                                case "chickenfight":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  WingThunder (2) + SunFlower (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  WingThunder (2) + Hornz (2)");
                                    break;

                                case "purpleballon":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  RedBallon (2) + BlueBallon (2)");
                                    break;

                                case "blueharpy":  // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "pioki":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  WingThunder (2) + BomberGunMk2 (2)");
                                    break;

                                case "pelocanduo":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Jack (3) + Jone (2)");
                                    break;

                                case "greenballon":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  YellowBalloon (2) + BlueBalloon (3)");
                                    break;

                                case "persona":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  ChickenFighter (3) + SirRupert (3)");
                                    break;

                                case "gladyhawk":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SoldierHawk (3) + Pioki (3)");
                                    break;

                                case "chuck": // nao possui formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "cupid":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  ChampLava (3) + GladyHawk (3)");
                                    break;

                                case "pelicancrew":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  PelocanDuo (3) + Chuck (3)");
                                    break;

                                case "pioking": // nao possui formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "darkpitt": // nao possui formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "kalinangel":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Cupid (3) + PDevil Wing (3)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Cupid (3) + Dark Pitt (3)");
                                    break;

                                case "personasoul":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  PelicanCrew (3) + Dalsipper (3)");
                                    break;

                                case "pphoenix":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  KalinAngel (3) + Avenger (3)");
                                    break;

                                #endregion bird

                                #region insecte
                                case "inseco": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "pee":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Inseco + Draco");
                                    break;

                                case "bookworm":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Inseco + Beasco");
                                    break;

                                case "snakeeye":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Pee + Rurabbi");
                                    break;

                                case "mosky":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Pee + Chicoe");
                                    break;

                                case "cabbagefighter":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Bookworm + Jamoo");
                                    break;

                                case "bellylady":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SnakeEye + RingWorm #1");
                                    break;

                                case "ntv":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Mosky + BigMaq");
                                    break;

                                case "payapaya":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Pee + Nosie");
                                    break;

                                case "baccho":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BellyLady + Jamoo");
                                    break;

                                case "bolarish":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NTV + Manglock");
                                    break;

                                case "daycrawl":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  CabbageFighter + ForceFlyer");
                                    break;

                                case "delcoi":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Baccho + TimTona");
                                    break;

                                case "asakayza":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  PayaPaya + TimTona");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  PayaPaya + Bubri");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Baccho + NighTime");
                                    break;

                                case "hornking":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Delcoi + Asakayza");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Delcoi + Synicks");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Delcoi + Liddy");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Delcoi + Cactusing");
                                    break;

                                case "beetlepete":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Asakayza + Skullwiser");
                                    break;

                                case "kingdusty":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BeetlePete (1) + KingDusty (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BeetlePete (1) + SilverLaustar (1)");
                                    break;

                                case "bloodstinger":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  KingDusty (1) + DashaBell (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  KingDusty (1) + ThunderBird (1)");
                                    break;

                                case "killerpete":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BeetlePete (1) + MintClaw (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BeetlePete (1) + WingCrusher (1)");
                                    break;

                                case "punchlava": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "handbomb": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "bamudar":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  KillerPete (1) + Dragoer (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  KillerPete (1) + InsaneDoctor (1)");
                                    break;

                                case "newdelcoi": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "siper":  // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "devildusty":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  KillerPete (1) + Stoner (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BloodStinger (1) + NeoSoul (1)");
                                    break;

                                case "beetleger": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "hornz":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  DevilDusty (2) + Bamudar (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  DevilDusty (2) + MacT-Bone (2)");
                                    break;

                                case "ziller":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Bamudar (2) + BomberGun (2)");
                                    break;

                                case "deadlybamude":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Ziller (2) + NeoOrdevil (2)");
                                    break;

                                case "clonebeetle":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NewDelcoi (2) + HornMaleki (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NewDelcoi (2) + NeoOrdevil (2)");
                                    break;

                                case "poisoner":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Ziller (2) + Harpy (2)");
                                    break;

                                case "siperous":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  DeadlyBamude (2) + Fairudo (2)");
                                    break;

                                case "killerzill":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Ziller (2) + Mr.Rupert (2)");
                                    break;

                                case "gaunterbomb":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  HandBomb (2) + BDevilWind (3)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  HandBomb (2) + CloneBettle (2)");
                                    break;

                                case "queensting":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  KillerZill (2) + FireDuke (2)");
                                    break;

                                case "beetleclass":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Beetleger (2) + OrangeBallon (3)");
                                    break;

                                case "alertspider":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Poisoner (3) + Siperous (3)");
                                    break;

                                case "scorpasis":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  DarkTravel (2) + Harpy (2)");
                                    break;

                                case "champlava":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SilverKun (3) + Minoir (3)");
                                    break;

                                case "megahornz":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  QueenSting (3) + Infamous (3)");
                                    break;

                                case "beetleknight":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BeetleClass (3) + MegaHornz (3)");
                                    break;

                                case "nipperking":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BeetleKnight (3) + PersonaSoul (3)");
                                    break;

                                case "cuttermantis":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NipperKing (3) + CutieCat (3)");
                                    break;


                                #endregion insect

                                #region mystery
                                case "mysco": // noa tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "ukki":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Mysco + Draco");
                                    break;

                                case "nortsncross":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Ukki + Devilco");
                                    break;

                                case "ringworm":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Ukki + Goa");
                                    break;

                                case "haneping":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NortsNcross + Caminho");
                                    break;

                                case "bangie":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Mysco + Mosky");
                                    break;

                                case "soo":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  RingWorm + Jamoo");
                                    break;

                                case "sumping":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Bangie + Metalocks");
                                    break;

                                case "dr.calvin":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Sumping + Monggyal");
                                    break;

                                case "aquaping":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Dr.Calvin + Delcoi");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Dr.Calvin + Synicks");
                                    break;

                                case "chowie":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Dr.Calvin + Cactusing");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Dr.Calvin + Torra");
                                    break;

                                case "insanedoctor":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Chowie (1) + Wiesha (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Dr.Calvin + SkullWisher");
                                    break;

                                case "stoner": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "gokuma":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  InsaneDoctor (1) + PumpedCurse (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  InsaneDoctor (1) + NeoSilver (1)");
                                    break;

                                case "snogyun":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  InsaneDoctor (1) + WingCrusher (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  InsaneDoctor (1) + SilverLausta (1)");
                                    break;

                                case "ringrookpawn": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "hellcrown": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "mact-bone":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Stoner (2) + NeoGamerika (2)");
                                    break;

                                case "boxingtower": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "neogokuma":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Gokuma (2) + Siper (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Gokuma (1) + Ayaya (1)");
                                    break;

                                case "hetti":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  KingRookPawn (2) + BomberGun (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  KingRookPawn (2) + Bamudar (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  KingRookPawn (2) + CrimsonMetal (2)");
                                    break;

                                case "binocchio":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Gokuma (1) + BomberGun (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Gokuma (1) + RockyRush (1)");
                                    break;

                                case "mr.rupert":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MacT-Bone (2) + NeoMad (2)");
                                    break;

                                case "snowball":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoGokuma (2) + Snowfoxy (2)");
                                    break;

                                case "topdrum":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Binnochio (2) + Snowfoxy (2)");
                                    break;

                                case "giant": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "swordtower": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "sirpupert":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Mr.Rupert (2) + WingThunder (2)");
                                    break;

                                case "xt-bone":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Mr.Rupert (2) + WingThunder (2)");
                                    break;

                                case "bloodshadow": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "bighead":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Mirnoir (2) + PetitChikaki (2)");
                                    break;

                                case "mintmal":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  XT-Bone (2) + QueenSting (2)");
                                    break;

                                case "nouveaumintmeul": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "shadowmagic":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Griffin “Droped” (2) + Banshee (2)");
                                    break;

                                case "persoz":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BloodShadow (3) + PetitTentaki (2)");
                                    break;

                                case "topcymbals": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;

                                case "armiris":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Scorpasis (2) + Cupid (2)");
                                    break;

                                case "souleater":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  ArmIris (3) + NinjaGirl (3)");
                                    break;

                                case "rooflilegeneral":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SoulEater (3) + MixDestroyer (3)");
                                    break;
                                #endregion mystery

                                #region metal
                                case "metaco": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "metalocks":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Metaco + Draco");
                                    break;
                                case "bigmaq":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Metaco + Rurabbi");
                                    break;
                                case "nippa":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Metalocks + Rabbo");
                                    break;
                                case "oilpot":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BigMaq + Chicoe");
                                    break;
                                case "mechaball":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Nippa + Bookworm");
                                    break;
                                case "waterball":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Oilpot + Mameo");
                                    break;
                                case "bota":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MechaBall + NortsNcross");
                                    break;
                                case "monoeye":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Waterball + Gago");
                                    break;
                                case "tanktortus":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Bota + BellyLady");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Bota + Sumping");
                                    break;
                                case "ancientkilla":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Monoeye + Delcoi");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Monoeye + Yassei");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Bota + Dr.Calvin");
                                    break;
                                case "killa":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Monoeye + Delcoi");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Monoeye + Yassei");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Bota + Dr.Calvin");
                                    break;
                                case "gamerika":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  TankTortus  + Chikaki");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  TankTortus  + Headragon");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  TankTortus  + Armaball");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  TankTortus  + Malreino");
                                    break;
                                case "soulbreaker":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Gamerika (1) + Greentail (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Gamerika (1) + Pumped (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Gamerika (1) + SwordTail (1)");
                                    break;
                                case "ayaya":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Gamerika (1) + Tenkaki (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Gamerika (1) + DashaBell (1)");
                                    break;
                                case "neosoul":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SoulBreaker (1) + Dragoer (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SoulBreaker (1) + Stoner (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SoulBreaker (1) + DashaBell (1)");
                                    break;
                                case "neogamerika":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SoulBreaker (1) + Tomated (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Ayaya (1) + MintClaw (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Ayaya (1) + Gokuma (1)");
                                    break;
                                case "crimsonmetal":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoSoul (1) + PhantonWing (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoSoul (1) + WingStormer (1)");
                                    break;
                                case "bombergun":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoSoul (2) + BladeDashabell (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoSoul (1) + DashaBell (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoSoul (1) + InsaneDoctor (1)");
                                    break;
                                case "bomber": // abreviação de bombergun
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoSoul (2) + BladeDashabell (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoSoul (1) + DashaBell (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoSoul (1) + InsaneDoctor (1)");
                                    break;
                                case "neoayaya":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Ayaya (1) + WingCrusher (1)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Ayaya (1) + MantyPlant (1)");
                                    break;
                                case "theunknown":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BomberGun (2) + Ordevil (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BomberGun (2) + Snogyun (1)");
                                    break;
                                case "shield":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoAyaya (2) + NeoAyaya (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoAyaya (2) + Ki-Ryu (2)");
                                    break;
                                case "gunsmash":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BomberGun (2) + EvilClaw (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoAyaya (2) + Wagstuff (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoAyaya (2) + Siper (2)"); 
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoAyaya (2) + Maleki (2)");
                                    break;
                                case "minivulcan":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  TheUnknown (2) + TheUnknown (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  TheUnknown (2) + Malmon (2)");
                                    break;
                                case "spiker":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Shield (2) + SunFlower (2)");
                                    break;
                                case "plugnsocket":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  LampNSocket (2) + NeoDevilStone (2)");
                                    break;
                                case "signalneon": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "bombergunmk2":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Gunsmash (2) + Ziller (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Gunsmash (2) + NeoGokuma (2)");
                                    break;
                                case "mk2":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Gunsmash (2) + Ziller (2)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Gunsmash (2) + NeoGokuma (2)");
                                    break;
                                case "silverkun":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BomberGunMk2 (2) + Siperous (2)");
                                    break;
                                case "tweestonga":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Spiker (3) + BomberGunMk2 (3)");
                                    break;
                                case "infamous":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SignalNeon (3) + Siperous (3)");
                                    break;
                                case "speenity":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  QueenSting (3) + SilverKun (3)");
                                    break;
                                case "bluemetal":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  TheUnknown (2) + Siperous (2)");
                                    break;
                                case "neovulcan":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  SignalNeon (3) + MiniVulcan (2)");
                                    break;
                                case "BomberGunX":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BlueMetal (2) + BomberGunMk2 (2)");
                                    break;
                                case "bbx":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  BlueMetal (2) + BomberGunMk2 (2)");
                                    break;
                                case "oldmetal": // nao tem formula
                                    SendData.SendWhispper(s, source, hench_name_nolow + no_formula);
                                    break;
                                case "darktoonga":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  TweesToonga (3) + Scorpasis (3)");
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  NeoVulcan (3) + AlertSpider (3)");
                                    break;
                                case "blacknity":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  Speenity (3) + Pioking (3)");
                                    break;
                                case "mixdestroyer":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  DarkToonga (3) + Garlingz (3)");
                                    break;
                                case "mixd":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  DarkToonga (3) + Garlingz (3)");
                                    break;
                                case "destroyer":
                                    SendData.SendWhispper(s, source, hench_name_nolow + " ->  MixDestroyer (3) + NipperKing (3)");
                                    break;

                                    #endregion metal
                            }

                            break;
                        default:
                            Console.WriteLine("Comando inexistente: " + message);
                            break;
                    }

                }
                else
                {
                    switch(message)
                    {
                        case "money":
                            int id_idx = Database.gamedata.GetIdByCharacter(source);
                            if(id_idx == 0) { break; }
                            int lcs = Database.member.GetLCFromididx(id_idx);
                            SendData.SendWhispper(s, source, "Voce possui: " + lcs.ToString() + " LCS!");
                            break;
                        case "ricos":
                            List<Datas> datas = Database.gamedata.GetTopGPRANK();
                            if(datas.Count > 0)
                            {
                                for(int i = 0; i < datas.Count; i++)
                                {
                                    string nome = datas[i].nome;
                                    int gp = datas[i].value;
                                    string gp_str = string.Format("{0:#,##}", gp);
                                    // 1º FireSharker - 500.000.000 GP
                                    SendData.SendWhispper(s, source, (i + 1).ToString() + "º " + nome + " - " + gp_str + " GP");
                                }
                            }
                            break;
                        /*
                        case "last_trades":
                            SendData.SendWhispper(s, source, "Ultimo trade com: Fulano 16/03/2018 14:20");
                            break;
                        */
                        case "online":
                            int players_online = Database.gamedata.GetPlayersOnline();
                            SendData.SendWhispper(s, source, "Players Online: " + players_online.ToString());
                            break;
                        /*
                        case "castle_war":
                            SendData.SendWhispper(s, source, "[Castle War] Inico: 21:00; Termino: 22:00");
                            break;
                        */
                        case "gametime":
                            float time = Database.gamedata.GetPlayerGameTime(source);
                            SendData.SendWhispper(s, source, "Voce possui: " + (time / 60).ToString("0.00") + " horas de jogo!");
                            break;
                        case "viciados":
                            List<Datas> times = Database.gamedata.GetTopViciados();
                            if (times.Count > 0)
                            {
                                for (int i = 0; i < times.Count; i++)
                                {
                                    string nome = times[i].nome;
                                    float Week = times[i].value;
                                    SendData.SendWhispper(s, source, (i + 1).ToString() + "º " + nome + " - " + (Week / 60).ToString("0.00") + " horas.");
                                }
                            }
                            break;
                        default:
                            Console.WriteLine("Comando inexistente: " + message);
                            break;
                    }
                }
           
            }
            catch
            {
                Console.WriteLine("[ParseCommand] Erro!");
                return;
            }


            Console.WriteLine("[" + source + "] " + message);

            
        }


        public static void AntiKick()
        {
            while (true)
            {
                Thread.Sleep(60000);
                ZSend(ZoneServer.s, new byte[] { 0x00, 0x00, 0xFD });
            }
        }


        public static void ZSend(Socket s, byte[] data)
        {
            try
            {
                s.BeginSend(data, 0, data.Length, 0, new AsyncCallback(ZSSendCallBack), s);
            }
            catch
            {
                return;
            }
        }
        private static void ZSSendCallBack(IAsyncResult ar)
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


    }
}
