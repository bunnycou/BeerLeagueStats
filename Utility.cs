using RiotMatchData;

namespace BLStats
{
    public static class Utility
    {
        public static int curSeason = 10;
        public static string roleShort(string role)
        {
            return role == "Top" ? "top" : role == "Jungle" ? "jg" : role == "Middle" ? "mid" : role == "Bottom" ? "bot" : "sup";
        }
        public static Participant getTeamRole(string team, string role, List<Participant> list) 
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
