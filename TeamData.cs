﻿using Newtonsoft.Json;

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
            this.seriesList = new();
            matchList = new();

            List<List<string>> seriesList = Database.dbquery($"SELECT seriesId, title FROM Series WHERE (team1 = '{abbr}' OR team2 = '{abbr}') AND seriesId LIKE '{season}%'");
            foreach (var series in seriesList)
            {
                var seriesId = series[0];
                var seriesTitle = series[1];

                int seriesMatchWin = 0;
                int seriesMatchLoss = 0;
                foreach (var rawMatchData in Database.oneDimList(Database.dbquery($"SELECT matchData FROM Matches WHERE seriesId = '{seriesId}'")))
                {
                    matchList.Add(JsonConvert.DeserializeObject<RiotMatchData.RiotMatchData>(rawMatchData));
                    if (Utility.getTeamFromMatchByAbbr(abbr, rawMatchData).win)
                    {
                        seriesMatchWin += 1;
                    }
                    else { seriesMatchLoss += 1; }
                }
                matchWin += seriesMatchWin;
                matchLoss += seriesMatchLoss;

                if (seriesMatchWin > seriesMatchLoss) 
                { 
                    seriesWin += 1;
                    this.seriesList.Add(new List<string> { seriesId, seriesTitle, "win" });
                } else 
                { 
                    seriesLoss += 1;
                    this.seriesList.Add(new List<string> { seriesId, seriesTitle, "loss" });
                }
            }
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
        public List<List<string>> seriesList { get; set; }
        public List<RiotMatchData.RiotMatchData> matchList { get; set; }
    }
}