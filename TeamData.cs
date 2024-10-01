using Newtonsoft.Json;

namespace BLStats
{
    public class TeamData
    {
        public TeamData(string TeamAbbr, string season)
        {
            abbr = TeamAbbr;
            name = Database.dbquery($"SELECT name FROM Teams WHERE abbreviation = '{abbr}' AND season = '{season}'")[0][0];
            matchWin = 0;
            matchLoss = 0;
            seriesWin = 0;
            seriesLoss = 0;
            matchList = new();

            seriesList = Database.dbquery($"SELECT seriesId, title, winner FROM Series WHERE (team1 = '{abbr}' OR team2 = '{abbr}') AND seriesId LIKE '{season}%'");
            foreach (var row in seriesList)
            {
                var seriesId = row[0];
                var title = row[1];
                var winner = row[2];
                if (winner != "none")
                {
                    if (abbr == winner)
                    {
                        seriesWin++;
                    }
                    else
                    {
                        seriesLoss++;
                    }
                }       
            }
            //foreach (var series in seriesList)
            //{
            //    var seriesId = series[0];
            //    var seriesTitle = series[1];

            //    int seriesMatchWin = 0;
            //    int seriesMatchLoss = 0;
            //    var matchDataList = Database.oneColList(Database.dbquery($"SELECT matchData FROM Matches WHERE seriesId = '{seriesId}'"));
            //    if (matchDataList[0] != "none")
            //    {
            //        foreach (var rawMatchData in matchDataList)
            //        {
            //            matchList.Add(JsonConvert.DeserializeObject<RiotMatchData.RiotMatchData>(rawMatchData));
            //            if (Utility.getTeamFromMatchByAbbr(abbr, rawMatchData).win)
            //            {
            //                seriesMatchWin += 1;
            //            }
            //            else { seriesMatchLoss += 1; }
            //        }
            //    }
            //    matchWin += seriesMatchWin;
            //    matchLoss += seriesMatchLoss;

            //    if (seriesMatchWin > seriesMatchLoss) 
            //    { 
            //        seriesWin += 1;
            //        this.seriesList.Add(new List<string> { seriesId, seriesTitle, "win" });
            //    } else 
            //    { 
            //        seriesLoss += 1;
            //        this.seriesList.Add(new List<string> { seriesId, seriesTitle, "loss" });
            //    }
            //}
        }
        public string name { get; set; }
        public string abbr { get; set; }
        public int matchWin { get; set; }
        public int matchLoss { get; set; }
        public int seriesWin { get; set; }
        public int seriesLoss { get; set; }
        public int matchWR()
        {
            return Utility.WinRate(matchWin, matchLoss);
        }
        public int seriesWR()
        {
            return Utility.WinRate(seriesWin, seriesLoss);
        }
        public List<List<string>> seriesList { get; set; } // id, title, winnerAbbr
        public List<RiotMatchData.RiotMatchData> matchList { get; set; }
    }
}
