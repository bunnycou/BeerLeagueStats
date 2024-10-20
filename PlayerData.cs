using RiotMatchData;
using Newtonsoft.Json;
using BLStats.Components.Pages;

namespace BLStats
{
    public class PlayerData
    {
        public PlayerData(List<List<string>> data)
        {
            win = 0;
            loss = 0;
            kills = 0;
            deaths = 0;
            assists = 0;
            playerSeries = new();
            playerChamps = new();
            Dictionary<string, int> roles = new Dictionary<string, int>();

            bool first = true;
            foreach (var row in data)
            {
                var gamedata = row[0];
                var seriesId = row[1];
                var Game = JsonConvert.DeserializeObject<Participant>(gamedata);
                var champ = Game.championName;
                if (first)
                {
                    first = false;
                    name = Game.riotIdGameName;
                    tag = Game.riotIdTagline;
                    puuid = Game.puuid;
                }
                if (Game.win) { win++; } else { loss++; }
                kills += Game.kills;
                deaths += Game.deaths;
                assists += Game.assists;
                if (roles.ContainsKey(Game.teamPosition)) { roles[Game.teamPosition] += 1; } else { roles.Add(Game.teamPosition, 1); }
                if (playerSeries.ContainsKey(seriesId))
                {
                    playerSeries[seriesId].Add(champ);
                } else
                {
                    playerSeries.Add(seriesId, new());
                    playerSeries[seriesId].Add(champ);
                }
                if (playerChamps.ContainsKey(champ))
                {
                    playerChamps[champ]++;
                } else
                {
                    playerChamps.Add(champ, 1);
                }
            }

            var role = roles.OrderByDescending(pair => pair.Value).First().Key;

            primaryRole = role == "TOP" ? "Top" : role == "JUNGLE" ? "Jungle" : role == "MIDDLE" ? "Middle" : role == "BOTTOM" ? "Bottom" : "Support";
        }
        public string name { get; set; }
        public string tag { get; set; }
        public string puuid { get; set; }
        public int win { get; set; }
        public int loss { get; set; }
        public decimal winrate()
        {
            return Utility.WinRate(win, loss);
        }
        public int kills { get; set; }
        public int deaths { get; set; }
        public int assists { get; set; }
        public decimal kda()
        {
            return Utility.WinRate(kills + assists, deaths);
        }
        public string primaryRole { get; set; }
        public Dictionary<string, List<string>> playerSeries { get; set; }
        public Dictionary<string, int> playerChamps { get; set; }
    }
}
