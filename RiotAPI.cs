using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace BLStats
{
    public class RiotAPI
    {
        public static async Task<string> rawMatchData(string matchId)
        {
            using (HttpClient client = new HttpClient())
            {
                var matchResponse = await client.GetAsync($"https://americas.api.riotgames.com/lol/match/v5/matches/NA1_{matchId}?api_key={Utility.configJson.APIKey}");

                if (matchResponse.IsSuccessStatusCode)
                {
                    var content = await matchResponse.Content.ReadAsStringAsync();
                    return content;
                } else
                {
                    return "error";
                }
            }
        }
        public static async Task<string> rawTimelineData(string matchId)
        {
            using (HttpClient client = new HttpClient())
            {
                var matchResponse = await client.GetAsync($"https://americas.api.riotgames.com/lol/match/v5/matches/NA1_{matchId}/timeline?api_key={Utility.configJson.APIKey}");

                if (matchResponse.IsSuccessStatusCode)
                {
                    var content = await matchResponse.Content.ReadAsStringAsync();
                    return content;
                }
                else
                {
                    return "error";
                }
            }
        }
    }
}
