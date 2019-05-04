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
    public class member
    {
		private string ip = "localhost"
		private string user = "root"
		private string pw = "toor"
		private string db = "Member"
		
        static string myConnectionString = $"Persist Security Info=False;server={ip};uid={user};pwd={pw};database={db};";
		
		
        static MySqlConnection conn;


        public static bool ConnectMember()
        {
            conn = new MySqlConnection(myConnectionString);
            try
            {
                conn.Open();
                Console.WriteLine("[I] Connect to Member DB Successfully...", ConsoleColor.Green);
                return true;
            }
            catch
            {
                Console.WriteLine("[-] Connect to Member DB Failed ...", ConsoleColor.Red);
                return false;
            }
        }


        public static int GetLCFromididx(int id_idx)
        {
            int lc = 0;
            if (conn.State == ConnectionState.Open)
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT lc from Player WHERE id_idx = @ID;";
                cmd.Parameters.AddWithValue("@ID", id_idx);
                MySqlDataReader reader = cmd.ExecuteReader();

                if(reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int LCs = reader.GetInt32(0);
                        lc = LCs;
                        reader.Close();
                        return lc;
                    }
                }
                else
                {
                    reader.Close();
                    return lc;
                }

                reader.Close();
            }
          return lc;
        }


    }
}
