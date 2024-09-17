using MySqlConnector;
using Newtonsoft.Json;
using RiotMatchData;

namespace BLStats
{
    public static class Database
    {

        static dynamic configJson = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("config.json"));

        static MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
        {
            Server = configJson.server,
            UserID = configJson.userId,
            Password = configJson.password,
            Database = configJson.database
        };
        static public string connectionString = builder.ConnectionString;
        public static List<KeyValuePair<string, Participant>> playerDataList(string name, string season)
        {
            var playerData = new List<KeyValuePair<string, Participant>>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var puuid = "";
                MySqlCommand aliasFind = new MySqlCommand($"SELECT puuid FROM Players WHERE name = '{name}'", connection);
                using (MySqlDataReader aliasReader = aliasFind.ExecuteReader())
                {
                    if (!aliasReader.HasRows) { return playerData; } // empty list
                    aliasReader.Read();
                    puuid = aliasReader.GetString(0);
                    aliasReader.Close();
                }
                var cmd = $"SELECT matchId, participantData FROM PlayerChampMatch WHERE puuid = '{puuid}'";
                if (season != "all")
                {
                    cmd += $" AND season = {season}";
                }
                MySqlCommand sqlCmd = new MySqlCommand(cmd, connection);
                using (MySqlDataReader reader = sqlCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        playerData.Add(new KeyValuePair<string, Participant>(reader.GetString(0), JsonConvert.DeserializeObject<Participant>(reader.GetString(1))));
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return playerData;
        }
        public static List<KeyValuePair<string,string>> ListPlayerNameId()
        {
            var playerNames = new List<KeyValuePair<string, string>>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                MySqlCommand getNames = new MySqlCommand("SELECT name, puuid FROM Players", connection);
                using (MySqlDataReader readNames = getNames.ExecuteReader())
                {
                    while (readNames.Read())
                    {
                        playerNames.Add(new KeyValuePair<string, string>(readNames.GetString(0), readNames.GetString(1)));
                    }
                    readNames.Close();
                }

                connection.Close();
            }
            return playerNames;//.OrderBy(pair => pair.Key).ToList(); //Order after receiving, no need for this function to do the sorting
        }
        public static string SeriesTitle(string seriesId)
        {
            string returnVal = "";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand getTitle = new MySqlCommand($"SELECT team1, team2 FROM Series WHERE seriesId = '{seriesId}'", conn);
                using (MySqlDataReader reader = getTitle.ExecuteReader()) { 
                    while (reader.Read())
                    {
                        returnVal = reader.GetString(0) + " vs " + reader.GetString(1);
                    }
                    reader.Close();
                }
                conn.Close();
            }
            return returnVal;
        }
        public static List<RiotMatchData.RiotMatchData> SeriesMatchData(string seriesId) 
        {
            List<RiotMatchData.RiotMatchData> returnVal = new();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand getTitle = new MySqlCommand($"SELECT matchData FROM Matches WHERE seriesId = '{seriesId}'", conn);
                using (MySqlDataReader reader = getTitle.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        returnVal.Add(JsonConvert.DeserializeObject<RiotMatchData.RiotMatchData>(reader.GetString(0)));
                    }
                    reader.Close();
                }
                conn.Close();
            }
            return returnVal;
        }
        public static List<List<string>> getSeriesForSeason(string? season)
        {
            List<List<string>> retVal = new List<List<string>>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand getMatches = new MySqlCommand($"SELECT seriesId, team1, team2 FROM Series WHERE seriesId LIKE '{season}%'", conn);
                using (MySqlDataReader readMatches = getMatches.ExecuteReader())
                {
                    while (readMatches.Read())
                    {
                        retVal.Add(new List<string>() { readMatches.GetInt32(0).ToString(), readMatches.GetString(1), readMatches.GetString(2) });
                    }
                    readMatches.Close();
                }
                conn.Close();
            }
            return retVal;
        }
        public static List<KeyValuePair<string, int>> getSeriesForpuuid(string? puuid)
        {
            List<KeyValuePair<string, int>> retVal = new();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand getSeries = new MySqlCommand($"SELECT DISTINCT Series.seriesId, team1, team2 FROM PlayerChampMatch " +
                    $"INNER JOIN Matches ON Matches.matchId = PlayerChampMatch.matchId " +
                    $"INNER JOIN Series ON Series.seriesId = Matches.seriesId " +
                    $"WHERE puuid = '{puuid}'", conn);
                using (MySqlDataReader readSeries = getSeries.ExecuteReader())
                {
                    while (readSeries.Read())
                    {
                        retVal.Add(new KeyValuePair<string, int>("Week " + 
                            Int32.Parse(readSeries.GetInt32(0).ToString().Substring(2,2)).ToString() + 
                            " " + readSeries.GetString(1) + " vs " + readSeries.GetString(2),
                            readSeries.GetInt32(0)));
                    }
                    readSeries.Close();
                }

                conn.Close();
            }
            return retVal;
        }
        public static string playerTeamName(string playerName)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand getTeamName = new MySqlCommand("SELECT Teams.name FROM Teams " +
                    "INNER JOIN PlayerTeam on PlayerTeam.teamId = Teams.teamId " +
                    "INNER JOIN Players ON Players.puuid = PlayerTeam.puuid " +
                    $"WHERE Players.name = '{playerName}'", conn);
                using (MySqlDataReader readTeamName = getTeamName.ExecuteReader())
                {
                    while (readTeamName.Read())
                    {
                        return readTeamName.GetString(0);
                    }
                }
            }
            return "None";
        }
        /*public static void DBExecute(string sqlCmd)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlCommand SqlCmd = new MySqlCommand(sqlCmd, connection);
                SqlCmd.ExecuteNonQuery();
                connection.Close();
            }
        }*/
    }
}
