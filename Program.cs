using BLStats;
using BLStats.Components;
using MySqlConnector;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

DataDragon.InitDataDragon();

Thread dbupdate = new Thread(() =>
{
    var configJson = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("config.json"));
    string API_KEY = configJson.APIKey;

    int curSeason = Utility.curSeason;

    while (true) //infinitely loop the update
    {
        // make sure datadragon is using latest info
        DataDragon.UpdateDataDragon();

        List<List<string>> matches = Database.dbquery("SELECT matchId FROM Matches WHERE matchData IS NULL OR matchData NOT LIKE '{%'");

        if (matches[0][0] != "none")
        {
            foreach (var row in matches)
            {
                var matchId = row[0];
                RiotMatchData.RiotMatchData matchData = JsonConvert.DeserializeObject<RiotMatchData.RiotMatchData>(RiotAPI.rawMatchData(matchId).Result);
                RiotMatchTimeline.RiotMatchTimeline timelineData = JsonConvert.DeserializeObject<RiotMatchTimeline.RiotMatchTimeline>(RiotAPI.rawTimelineData(matchId).Result);
                Database.dbexecute($"UPDATE Matches SET matchData = '{JsonConvert.SerializeObject(matchData)}', " +
                    $"timelineData = '{JsonConvert.SerializeObject(timelineData)}' WHERE matchId = '{matchId}'");

                foreach (var participant in matchData.info.participants)
                {
                    var puuid = participant.puuid;
                    var name = participant.riotIdGameName;
                    var tag = participant.riotIdTagline;
                    var champ = participant.championName;

                    List<List<string>> players = Database.dbquery($"SELECT puuid, name, tag FROM Players WHERE puuid = '{puuid}'");
                    if (players[0][0] == "none") // insert player
                    {
                        Database.dbexecute($"INSERT INTO Players (puuid, name, tag) VALUES ('{puuid}', '{name}', '{tag}')");
                    }
                    else // update player if need be
                    {
                        if (players[0][1] != name || players[0][2] != tag)
                        {
                            Database.dbexecute($"UPDATE Players SET name = '{name}', tag = '{tag}' WHERE puuid = '{puuid}'");
                        }
                    }

                    List<List<string>> pcmCheck = Database.dbquery($"SELECT * FROM PlayerChampMatch WHERE puuid = '{puuid}' AND matchId = '{matchId}'");
                    if (pcmCheck[0][0] == "none")
                    {
                        Database.dbexecute("INSERT INTO PlayerChampMatch (puuid, matchId, participantData, champ) " +
                                            $"VALUES ('{puuid}', '{matchId}', '{JsonConvert.SerializeObject(participant)}', '{champ}')");
                    }
                }
            }
        }

        Console.WriteLine("Finished Processing All New Matches");
        int OneMin = 1 * 60 * 1000;
        Thread.Sleep(OneMin);
    }
    // check matches table and update it using Riot API if needed
    // loop every minute (changed to one minute now that there is less restrictive api access granted)
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
