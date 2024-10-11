using Newtonsoft.Json;
using RiotMatchData;
namespace BLStats
{
    public class ChampData
    {
        public ChampData(string champName, string seasonIn) 
        {
            name = champName;
            season = seasonIn;
            wins = 0;
            bans = 0;
            series = new();
            List<List<string>> matchList = matchesList();
            
            picks = matchList.Count;
            foreach (var entry in matchList)
            {
                var champData = JsonConvert.DeserializeObject<Participant>(entry[0]);

                if (champData.win)
                {
                    wins++;
                }
            }
            losses = picks - wins;
        }

        public void getBans() // get bans
        { 
            List<string> matchList = allMatchesList();
            totalMatches = matchList.Count;
            foreach (var rawMatch in matchList)
            {
                var match = JsonConvert.DeserializeObject<RiotMatchData.RiotMatchData>(rawMatch);
                foreach (var team in match.info.teams)
                {
                    foreach (var ban in team.bans)
                    {
                        if (DataDragon.champs[ban.championId] == name)
                        {
                            bans++;
                        }
                    }
                }
            }
        }

        public void getSeries() // get series (id, name) this champ was played in
        {
            var rawquery = "SELECT DISTINCT Series.seriesId, Series.title FROM PlayerChampMatch " +
                "INNER JOIN Matches ON Matches.matchId = PlayerChampMatch.matchId INNER JOIN Series ON Series.seriesId = Matches.seriesId " +
                $"WHERE Champ = '{name}'";

            if (season != "all")
            {
                rawquery += $" AND Matches.seriesId LIKE '{season}%'";
            }

            series = Database.dbquery(rawquery).ToList();
        }

        private List<List<string>> matchesList()
        {
            List<List<string>> matchList = new();

            if (season == "all")
            {
                matchList = Database.dbquery($"SELECT participantData, matchData FROM PlayerChampMatch " +
                    "INNER JOIN Matches on Matches.matchId = PlayerChampMatch.matchId " +
                    $"WHERE Champ = '{name}' AND matchData IS NOT NULL");
            } else
            {
                matchList = Database.dbquery($"SELECT participantData, matchData FROM PlayerChampMatch " +
                    "INNER JOIN Matches on Matches.matchId = PlayerChampMatch.matchId " +
                    $"WHERE Champ = '{name}' AND seriesId LIKE '{season}%' AND matchData IS NOT NULL");
            }

            return matchList;
        }

        private List<string> allMatchesList()
        {
            List<string> matchList = new();

            if (season == "all")
            {
                matchList = Database.oneColList(Database.dbquery($"SELECT matchData FROM Matches WHERE matchData IS NOT NULL"));
            } else
            {
                matchList = Database.oneColList(Database.dbquery($"SELECT matchData FROM Matches WHERE seriesId LIKE '{season}%' AND matchData IS NOT NULL"));
            }

            return matchList;
        }

        public string name { get; set; }
        public string season { get; set; }
        public int picks { get; set; }
        public int bans { get; set; }
        private int totalMatches { get; set; }
        public int wins { get; set; }
        public int losses { get; set; }

        public List<List<string>> series { get; set; }

        public int winrate()
        {
            return Utility.WinRate(wins, losses);
        }
        public int prescence()
        {
            decimal picksd = picks;
            decimal bansd = bans;
            decimal totalMatchesd = totalMatches;
            return (int)Math.Round(((picksd + bansd) / totalMatchesd) * 100);
        }
    }
}
