using RiotMatchData;

namespace BLStats
{
    public class PlayerOverallStats
    {
        public PlayerOverallStats() { }
        public PlayerOverallStats(string alias, string season) {
            var dataList = Database.playerDataList(alias, season);

            win = 0;
            loss = 0;
            kills = 0;
            deaths = 0;
            assists = 0;
            Dictionary<string, int> roles = new Dictionary<string, int>();

            bool first = true;
            foreach (dynamic Game in dataList)
            {
                if (first)
                {
                    first = false;
                    name = Game.riotIdGameName;
                    tag = Game.riotIdGameTag;
                    puuid = Game.riotIdGamePuuid;
                }
                if ((bool)Game.win) { win++; } else { loss++; }
                kills += (int)Game.kills;
                deaths += (int)Game.deaths;
                assists += (int)Game.assists;
                if (roles.ContainsKey((string)Game.teamPosition)) { roles[(string)Game.teamPosition] += 1; } else { roles[(string)Game.teamPosition] = 0; }
            }

            var role = roles.OrderByDescending(pair => pair.Value).First().Key;

            primaryRole = role == "TOP" ? "Top" : role == "JUNGLE" ? "Jungle" : role == "MIDDLE" ? "Middle" : role == "BOTTOM" ? "Bottom" : "Support";
        }

        public PlayerOverallStats(List<KeyValuePair<string, Participant>> PlayerGames) 
        {
            win = 0;
            loss = 0;
            kills = 0;
            deaths = 0;
            assists = 0;
            Dictionary<string, int> roles = new Dictionary<string, int>();

            bool first = true;
            foreach (var data in PlayerGames) 
            {
                var Game = data.Value;
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
                if (roles.ContainsKey(Game.teamPosition)) { roles[Game.teamPosition] += 1; } else { roles[Game.teamPosition] = 0; }
            }

            var role = roles.OrderByDescending(pair => pair.Value).First().Key;

            primaryRole = role == "TOP" ? "Top" : role == "JUNGLE" ? "Jungle" : role == "MIDDLE" ? "Middle" : role == "BOTTOM" ? "Bottom" : "Support" ;
        }
        public string name { get; set; }
        public string tag { get; set; }
        public string puuid { get; set; }
        public int win { get; set; }
        public int loss { get; set; }
        public decimal winrate()
        {
            return Math.Round(((decimal)win/(win+loss))*100);
        }
        public int kills { get; set; }
        public int deaths { get; set; }
        public int assists { get; set; }
        public decimal kda()
        {
            return Math.Round(((decimal)(kills+assists)/deaths), 1);
        }
        public string primaryRole { get; set; }
    }
}
