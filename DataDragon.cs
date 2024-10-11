using Newtonsoft.Json;
using System.Text;

namespace BLStats
{
    public static class DataDragon
    {
        public static Dictionary<int, string> summoners = new Dictionary<int, string>();
        public static Dictionary<int, string> runes = new Dictionary<int, string>();
        public static Dictionary<int, string> champs = new Dictionary<int, string>();
        private static string latest { get; set; }
        private static List<string> versions { get; set; }
        public static string champIcon(string champ)
        {
            return $"https://ddragon.leagueoflegends.com/cdn/{latest}/img/champion/{champ}.png";
        }
        public static string champIcon(string champ, string version)
        {
            return $"https://ddragon.leagueoflegends.com/cdn/{ddVersion(version)}/img/champion/{champ}.png";
        }
        public static string champIcon(int champ, string version)
        {
            return $"https://ddragon.leagueoflegends.com/cdn/{ddVersion(version)}/img/champion/{champs[champ]}.png";
        }
        public static string itemIcon(int item, string version)
        {
            if (item == 0)
            {
                return "https://ddragon.leagueoflegends.com/cdn/img/bg/A6000000.png";
            }

            return $"https://ddragon.leagueoflegends.com/cdn/{ddVersion(version)}/img/item/{item}.png";
        }
        public static string summonerIcon(int summoner, string version)
        {
            return $"https://ddragon.leagueoflegends.com/cdn/{ddVersion(version)}/img/spell/{summoners[summoner]}.png";
        }
        public async static void InitDataDragon()
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync("https://ddragon.leagueoflegends.com/api/versions.json");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonData = JsonConvert.DeserializeObject<List<string>>(content);
                    latest = jsonData.First();
                    versions = jsonData;
                }
            }

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync($"https://ddragon.leagueoflegends.com/cdn/{latest}/data/en_US/summoner.json");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonData = JsonConvert.DeserializeObject<dynamic>(content);
                    foreach (var summoner in jsonData.data.Children())
                    {
                        foreach (var summon in summoner.Children()) //i blame riot for this
                        {
                            summoners.Add((int)summon.key, (string)summon.id);
                        }
                    }
                }
            }

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync($"https://ddragon.leagueoflegends.com/cdn/{latest}/data/en_US/champion.json");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonData = JsonConvert.DeserializeObject<dynamic>(content);
                    foreach (var champion in jsonData.data.Children())
                    {
                        foreach (var champ in champion.Children()) //i blame riot for this
                        {
                            champs.Add((int)champ.key, (string)champ.id);
                        }
                    }
                }
            }
        }
        public async static void UpdateDataDragon()
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync("https://ddragon.leagueoflegends.com/api/versions.json");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonData = JsonConvert.DeserializeObject<List<string>>(content);
                    latest = jsonData.First();
                    versions = jsonData;
                }
            }
        }
        private static string ddVersion(string version) // version input would be (example) 14.15 and need to return 14.15.1 or 14.15.2 if it exists etc
        {
            return versions.Find(ver => { return ver.StartsWith(version); });
        }
    }
}
