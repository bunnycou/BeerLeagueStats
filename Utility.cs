using Newtonsoft.Json;
using RiotMatchData;

namespace BLStats
{
    public static class Utility
    {
        public static dynamic configJson = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("config.json"));
        public static int curSeason = 10;
        public static int WinRate(int win, int loss)
        {
            decimal wins = (decimal)win;
            decimal losses = (decimal)loss;
            return (int)Math.Round((wins / (wins+losses))*100);
        }
        public static string roleShort(string role) //convert role to short role top/jg/mid/bot/sup 
        {
            role = role.ToLower();
            return role == "top" ? "top" : role == "jungle" ? "jg" : role == "middle" ? "mid" : role == "bottom" ? "bot" : "sup";
        }
        public static string determineRole (string puuid) //determine player most played role
        {
            List<string> playerMatches = Database.oneColList(Database.dbquery($"SELECT participantData FROM PlayerChampMatch WHERE puuid = '{puuid}'"));
            List<Participant> playerMatchData = new();
            foreach (var playerMatch in playerMatches)
            {
                playerMatchData.Add(JsonConvert.DeserializeObject<Participant>(playerMatch));
            }

            Dictionary<string, int> roleCt = new();
            foreach (var playerMatch in playerMatchData)
            {
                if (roleCt.ContainsKey(playerMatch.teamPosition))
                {
                    roleCt[playerMatch.teamPosition] += 1;
                } else
                {
                    roleCt.Add(playerMatch.teamPosition, 1);
                }
            }
            return roleShort(roleCt.OrderByDescending(x => x.Value).First().Key);

        }
        public static Participant getTeamRole(string team, string role, List<Participant> list) //blue/red, top/jg/mid/bot/sup, list of participants 
        {
            int teamId;
            if (team == "blue")
            {
                teamId = 100;
            } else
            {
                teamId = 200;
            }
            string roleId;
            if (role == "top")
            {
                roleId = "TOP";
            } else  if (role == "jg")
            {
                roleId = "JUNGLE";
            } else if (role == "mid")
            {
                roleId = "MIDDLE";
            } else if (role == "bot")
            {
                roleId = "BOTTOM";
            } else
            {
                roleId = "UTILITY";
            }

            foreach (var player in list)
            {
                if (player.teamId == teamId && player.teamPosition == roleId) { return player; }
            }

            return null; // shouldn't happen unless function is called wrong
        }
        public static Team getTeamFromMatchByAbbr(string teamAbbr, string rawmatch)
        {
            RiotMatchData.RiotMatchData match = JsonConvert.DeserializeObject<RiotMatchData.RiotMatchData>(rawmatch);
            List<string> teamPuuids = Database.oneColList(Database.dbquery($"SELECT Players.puuid FROM Players " +
                $"INNER JOIN PlayerTeam ON PlayerTeam.puuid = Players.puuid " +
                $"INNER JOIN Teams ON Teams.teamId = PlayerTeam.teamId " +
                $"WHERE Teams.abbreviation = '{teamAbbr}'"));

            foreach(var player in match.info.participants)
            {
                if (teamPuuids.Contains(player.puuid))
                {
                    return match.info.teams.Find(x => x.teamId == player.teamId);
                }
            }
            return null;
        }
        public static string GameLength(int duration)
        {
            int mins = duration / 60;
            int secs = duration % 60;
            
            string ret;

            ret = mins + ":";
            if (secs < 10) 
            {
                ret += "0" + secs;
            } else { ret += secs; }

            return ret;
        }
        public static string GameVersion(string version)
        {
            return String.Join(".", version.Split(".").Take(2));
        }
    }
}
