using BLStats;
using BLStats.Components;
using MySqlConnector;
using Newtonsoft.Json;

DataDragon.InitDataDragon();

Thread dbupdate = new Thread(async () =>
{
    var configJson = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("config.json"));
    string API_KEY = configJson.APIKey;

    int curSeason = Utility.curSeason;

    while (true) //infinitely loop the update
    {
        // make sure datadragon is using latest info
        DataDragon.UpdateDataDragon();

        var matches = new List<string>();
        var timelines = new List<string>();

        using (MySqlConnection connection = new MySqlConnection(Database.connectionString))
        {
            connection.Open();

            MySqlCommand selectMatches = new MySqlCommand($"SELECT * FROM Matches WHERE matchData IS NULL OR matchData NOT LIKE '{{%'", connection); // no matchdata
            using (MySqlDataReader reader = selectMatches.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        matches.Add(reader.GetString(0)); // These matches need match data and timeline data
                    }
                }
            }

            //MySqlCommand selectTimelines = new MySqlCommand($"SELECT * FROM Matches WHERE (timeline IS NULL OR timeline NOT LIKE '{{%') AND matchData LIKE '{{%'", connection);
            //using (MySqlDataReader reader = selectTimelines.ExecuteReader())
            //{
            //    if (reader.HasRows)
            //    {
            //        while (reader.Read())
            //        {
            //            timelines.Add(reader.GetString(0)); // These matches have match data but not timeline data (updating legacy matches)
            //        }
            //    }
            //}

            foreach (var match in matches) // INSERT TIMELINE API CALL https://americas.api.riotgames.com/lol/match/v5/matches/NA1_5055582884/timeline?api_key=KEY
            {
                using (HttpClient client = new HttpClient())
                {
                    var matchResponse = await client.GetAsync($"https://americas.api.riotgames.com/lol/match/v5/matches/NA1_{match}?api_key={API_KEY}");

                    if (matchResponse.IsSuccessStatusCode)
                    {
                        var content = await matchResponse.Content.ReadAsStringAsync();
                        var jsonData = JsonConvert.DeserializeObject<RiotMatchData.RiotMatchData>(content);

                        MySqlCommand insertMatch = new MySqlCommand($"UPDATE Matches SET matchData = '{JsonConvert.SerializeObject(jsonData)}' WHERE matchId = '{match}'", connection);
                        insertMatch.ExecuteNonQuery();

                        foreach (var participant in jsonData.info.participants)
                        {
                            var puuid = participant.puuid;
                            var name = participant.riotIdGameName;
                            var tag = participant.riotIdTagline;
                            var champ = participant.championName;

                            MySqlCommand checkPlayer = new MySqlCommand($"SELECT * FROM Players WHERE puuid = '{puuid}'", connection);
                            using (MySqlDataReader readPlayer = checkPlayer.ExecuteReader())
                            {
                                if (!readPlayer.HasRows) //Player doesn't exist, insert into Players
                                {
                                    readPlayer.Close();

                                    MySqlCommand insertPlayer = new MySqlCommand($"INSERT INTO Players (puuid, name, tag) VALUES ('{puuid}', '{name}', '{tag}')", connection);
                                    insertPlayer.ExecuteNonQuery();
                                }
                                else // player exists, force update name + tag
                                {
                                    readPlayer.Close();

                                    MySqlCommand updatePlayer = new MySqlCommand($"UPDATE Players SET name = '{name}', tag = '{tag}' WHERE puuid = '{puuid}'", connection);
                                    updatePlayer.ExecuteNonQuery();
                                }
                            }

                            MySqlCommand checkPlayerChampMatch = new MySqlCommand($"SELECT * FROM PlayerChampMatch WHERE puuid = '{puuid}' AND matchId = '{match}'", connection);
                            using (MySqlDataReader checkPlayerChampMatchReader  = checkPlayerChampMatch.ExecuteReader())
                            {
                                if (!checkPlayerChampMatchReader.HasRows) // make sure this has not already been inserted
                                {
                                    checkPlayerChampMatchReader.Close();

                                    MySqlCommand insertPlayerChampMatch = new MySqlCommand("INSERT INTO PlayerChampMatch (puuid, matchId, participantData, champ) " +
                                        $"VALUES ('{puuid}', '{match}', '{JsonConvert.SerializeObject(participant)}', '{champ}')", connection);
                                    insertPlayerChampMatch.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    var timelineResponse = await client.GetAsync($"https://americas.api.riotgames.com/lol/match/v5/matches/NA1_{match}/timeline?api_key={API_KEY}");

                    if (timelineResponse.IsSuccessStatusCode)
                    {
                        var content = await timelineResponse.Content.ReadAsStringAsync();
                        var jsonData = JsonConvert.DeserializeObject<RiotMatchTimeline.RiotMatchTimeline>(content);

                        MySqlCommand insertTimeline = new MySqlCommand($"UPDATE Matches SET timeline = '{JsonConvert.SerializeObject(jsonData)}' WHERE matchId = '{match}'", connection);
                        insertTimeline.ExecuteNonQuery();
                    }
                }
            }

            foreach (var match in timelines) // insert timelines where data exists but timeline does not
            {
                using (HttpClient client = new HttpClient())
                {
                    var timelineResponse = await client.GetAsync($"https://americas.api.riotgames.com/lol/match/v5/matches/NA1_{match}/timeline?api_key={API_KEY}");

                    if (timelineResponse.IsSuccessStatusCode)
                    {
                        var content = await timelineResponse.Content.ReadAsStringAsync();
                        var jsonData = JsonConvert.DeserializeObject<dynamic>(content);

                        MySqlCommand insertTimeline = new MySqlCommand($"UPDATE Matches SET timeline = '{jsonData.ToString()}' WHERE id = '{match}'", connection);
                        insertTimeline.ExecuteNonQuery();
                    }
                }
            }
        }
        Console.WriteLine("Finished Processing All New Matches");
        int OneMin = 1 * 60 * 1000;
        Thread.Sleep(OneMin);
    }
    // check matches table and update it using Riot API if needed
    // loop every ten minutes
});
//#if !DEBUG
    dbupdate.Start();
//#endif

var builder = WebApplication.CreateBuilder(args);

#if !DEBUG
    builder.WebHost.UseUrls("http://*:5000");
#endif

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
