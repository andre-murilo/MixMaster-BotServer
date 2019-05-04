using MySql.Data;
using MySql.Data.Common;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MixMaster_Bot.Database
{
    public class gamedata
    {
		private string ip = "localhost"
		private string user = "root"
		private string pw = "toor"
		private string db = "gamedata"
		
        static string myConnectionString = $"Persist Security Info=False;server={ip};uid={user};pwd={pw};database={db};";
		
		
        static MySqlConnection conn;


        public static bool ConnectGamedata()
        {
            conn = new MySqlConnection(myConnectionString);
            try
            {
                conn.Open();
                Console.WriteLine("[I] Connect to gamedata DB Successfully...", ConsoleColor.Green);
                return true;
            }
            catch
            {
                Console.WriteLine("[-] Connect to gamedata DB Failed ...", ConsoleColor.Red);
                return false;
            }
        }


        public static int GetIdByCharacter(string charname)
        {
            int id = 0;
            if (conn.State == ConnectionState.Open)
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT id_idx FROM u_hero WHERE name = @CHARNAME;";
                cmd.Parameters.AddWithValue("@CHARNAME", charname);
                MySqlDataReader reader = cmd.ExecuteReader();
                if(reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int id_idx = reader.GetInt32(0);
                        id = id_idx;
                        reader.Close();
                        return id;
                    }
                }
                else
                {
                    Console.WriteLine("Character not found: " + charname);
                    reader.Close();
                    return 0;
                }
                reader.Close();
                
               
            }
          return id;
        }

        public static int GetPlayersOnline()
        {
            int online = 0;
            if (conn.State == ConnectionState.Open)
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT count(*) from u_hero where login > 0;";
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int players_online = reader.GetInt32(0);
                        online = players_online;
                        reader.Close();
                        return online;
                    }
                }
                else
                {
                    reader.Close();
                    return 0;
                }
                reader.Close();
            }
            return online;
        }


        public static List<Datas> GetTopGPRANK()
        {
            List<Datas> datas = new List<Datas>();
            if (conn.State == ConnectionState.Open)
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "select name, gold from u_hero WHERE class = 0 ORDER BY gold desc limit 5;";
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Datas temp = new Datas();
                        temp.nome = reader.GetString(0);
                        temp.value = reader.GetInt32(1);
                        datas.Add(temp);
                    }
                }
                else
                {
                    reader.Close();
                }
                reader.Close();
            }

            return datas;
        }


        public static List<Datas> GetTopViciados()
        {
            List<Datas> datas = new List<Datas>();
            if (conn.State == ConnectionState.Open)
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "select name, week_rank_time from u_hero WHERE class = 0 ORDER BY week_rank_time desc limit 5;";
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Datas temp = new Datas();
                        temp.nome = reader.GetString(0);
                        temp.value = reader.GetInt32(1);
                        datas.Add(temp);
                    }
                }
                else
                {
                    reader.Close();
                }
                reader.Close();
            }

            return datas;
        }

        public static int GetPlayerGameTime(string name)
        {
            int time = 0;
            if (conn.State == ConnectionState.Open)
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "select totalplayedtime from u_hero where name = @NAME;";
                cmd.Parameters.AddWithValue("@NAME", name);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int totalplayedtime = reader.GetInt32(0);
                        time = totalplayedtime;
                        reader.Close();
                        return time;
                    }
                }
                else
                {
                    reader.Close();
                    return 0;
                }
                reader.Close();
            }
            return time;
        }


        // money -> id_idx -> Member LC  (OKAY)
        // gamedata -> gold
        // last_trades -> id_idx -> LogDB
        // players_online -> gamedata -> login
        // castle_war -> S_Data -> S_Castleinfo
        // game_time -> gamedata
        // viciados -> gamedata -> horajogo



    }
}
