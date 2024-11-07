using Newtonsoft.Json;
using System.Text;

namespace BLStats
{
    public static class DataDragon
    {
        public static Dictionary<int, string> summoners = new();
        public static Dictionary<int, List<string>> runes = new();
        public static Dictionary<int, string> champs = new();
        private static string latest { get; set; }
        private static List<string> versions { get; set; }
        public static string champIcon(string champ)
        {
            return $"https://ddragon.leagueoflegends.com/cdn/{latest}/img/champion/{champ}.png";
        }
        public static string champIcon(string champ, string version)
        {
            if (champ == "FiddleSticks") champ = "Fiddlesticks"; // the champ name has a capitol S but icon does not and the url is case sensitive
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
        public static string runeIcon(int rune)
        {
            return $"https://ddragon.leagueoflegends.com/cdn/img/{runes[rune][0]}";
        }
        public static string runeName(int rune)
        {
            return runes[rune][1];
        }
        public async static void InitDataDragon()
        {
            using (HttpClient client = new HttpClient()) // version
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

            using (HttpClient client = new HttpClient()) // summoners
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

            using (HttpClient client = new HttpClient()) // champs
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

            using (HttpClient client = new HttpClient()) // runes
            {
                var response = await client.GetAsync($"https://ddragon.leagueoflegends.com/cdn/{latest}/data/en_US/runesReforged.json");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonData = JsonConvert.DeserializeObject<dynamic>(content);
                    foreach (var child in jsonData)
                    {
                        runes.Add((int)child.id, new() { (string)child.icon, (string)child.name });
                        foreach (var rune in child.slots[0].runes)
                        {
                            runes.Add((int)rune.id, new() { (string)rune.icon, (string)rune.name});
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
