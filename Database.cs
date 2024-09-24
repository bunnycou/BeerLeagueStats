using MySqlConnector;
using Newtonsoft.Json;
using RiotMatchData;

namespace BLStats
{
    public static class Database
    {
        static MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
        {
            Server = Utility.configJson.server,
            UserID = Utility.configJson.userId,
            Password = Utility.configJson.password,
            Database = Utility.configJson.database
        };
        static public string connectionString = builder.ConnectionString;
        public static List<List<string>> dbquery(string rawquery) // select matchid, seriesid will return list[0][0] as first row matchid, list[0][1] as first row seriesid etc
        {
            List<List<string>> retVal = new();

            string query = rawquery;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand(query, conn);
                conn.Open();

                using (MySqlDataReader dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.HasRows)
                    {
                        int row = 0;
                        while(dataReader.Read())
                        {
                            retVal.Add(new List<string>()); //blank row
                            for (int i = 0; i < dataReader.FieldCount; i++)
                            {
                                try
                                {
                                    retVal[row].Add(dataReader.GetString(i));
                                } catch
                                {
                                    retVal[row].Add(dataReader.GetInt32(i).ToString());
                                }
                            }
                            row++;
                        }
                    } else
                    {
                        retVal.Add(new List<string> { "none" }); // check if list[0][0] is none when calling this function
                    }
                }

                conn.Close();

                return retVal;
            }
        }
        public static void dbexecute(string rawsql) // execute command in database without any returned data
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlCommand SqlCmd = new MySqlCommand(rawsql, connection);
                SqlCmd.ExecuteNonQuery();
                connection.Close();
            }
        }
        public static List<string> oneDimList(List<List<string>> list) // convert 2d list to 1d for when there is only one element per row
        {
            List<string> result = new();
            foreach(var row in list)
            {
                result.Add(row[0]);
            }

            return result; // will be result[0] none if orignal was list[0][0] none
        }
    }
}
