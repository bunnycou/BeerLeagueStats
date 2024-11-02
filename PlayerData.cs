using RiotMatchData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace BLStats
{
   public class PlayerData
   {
       // Basic Info
       public string name { get; set; }
       public string tag { get; set; }
       public string puuid { get; set; }
       public string primaryRole { get; set; }
       public int win { get; set; }
       public int loss { get; set; }
       private int TotalGames => win + loss;
       public double timePlayed { get; set; }
       public double WinRate => TotalGames > 0 ? (win * 100.0 / TotalGames) : 0;

        public int kills { get; set; }
        public int deaths { get; set; }
        public int assists { get; set; }
        public int largestKillingSpree { get; set; }
        public int doubleKills { get; set; }
        public int tripleKills { get; set; }
        public int quadraKills { get; set; }
        public int pentaKills { get; set; }

       // Carry Stats
       public double totalDamageDealtToChampions { get; set; }
       public double damageSelfMitigated { get; set; }
       public int firstBloodKills { get; set; }
       public int firstBloodAssists { get; set; }
       public int totalMinionsKilled { get; set; }
       public int killsNearEnemyTurret { get; set; }
       public int totalLaneMinionsFirst10 { get; set; }
       public double goldSpent { get; set; }
       public double maxCsAdvantageOnLaneOpponent { get; set; }
       public double teamDamagePercentage { get; set; }
       public int soloKills { get; set; }

       // Objective Stats
       public double damageDealtToBuildings { get; set; }
       public int turretTakedowns { get; set; }
       public int inhibitorTakedowns { get; set; }
       public int objectivesStolen { get; set; }
       public int turretPlatesTaken { get; set; }
       public int baronKills { get; set; }

       // Utility Stats
       public int visionScore { get; set; }
       public int wardsPlaced { get; set; }
       public int wardsKilled { get; set; }
       public double totalTimeCCDealt { get; set; }
       public double totalDamageShieldedOnTeammates { get; set; }
       public double totalHealsOnTeammates { get; set; }

       // Fun Stats
       public int spell1Casts { get; set; }
       public int spell2Casts { get; set; }
       public int spell3Casts { get; set; }
       public int spell4Casts { get; set; }
       public int totalSpellCasts => spell1Casts + spell2Casts + spell3Casts + spell4Casts;
       public int fistBumpParticipation { get; set; }
       public int flawlessAces { get; set; }
       public int skillshotsDodged { get; set; }
       public int skillshotsHit { get; set; }
       public int takedownsInAlcove { get; set; }

        // New Properties for Series and Champion Tracking
        public Dictionary<string, List<string>> playerSeries { get; private set; } = new();
        public Dictionary<string, int> playerChamps { get; private set; } = new();

       public PlayerData(List<List<string>> data)
       {
           InitializeStats();
                       playerSeries = new();
            playerChamps = new();
           bool first = true;
           Dictionary<string, int> roles = new();

           foreach (var row in data)
           {    
               var gamedata = row[0];
               var seriesId = row.Count > 1 ? row[1] : "defaultSeries" ;
               var Game = JsonConvert.DeserializeObject<Participant>(gamedata);
               var champ = Game.championName;
               if (first)
               {
                   first = false;
                   name = Game.riotIdGameName;
                   tag = Game.riotIdTagline;
                   puuid = Game.puuid;
               }

               // Basic Stats
               if (Game.win) win++; else loss++;
               timePlayed += Game.timePlayed;

               // Combat Stats
               kills += Game.kills;
               deaths += Game.deaths;
               assists += Game.assists;
               largestKillingSpree = Math.Max(largestKillingSpree, Game.largestKillingSpree);
               doubleKills += Game.doubleKills;
               tripleKills += Game.tripleKills;
               quadraKills += Game.quadraKills;
               pentaKills += Game.pentaKills;

               // Carry Stats
               totalDamageDealtToChampions += Game.totalDamageDealtToChampions;
               damageSelfMitigated += Game.damageSelfMitigated;
               if (Game.firstBloodKill) firstBloodKills++;
               if (Game.firstBloodAssist) firstBloodAssists++;
               totalMinionsKilled += Game.totalMinionsKilled;
               goldSpent += Game.goldSpent;

               // Objective Stats
               damageDealtToBuildings += Game.damageDealtToBuildings;
               turretTakedowns += Game.turretTakedowns;
               inhibitorTakedowns += Game.inhibitorTakedowns;
               baronKills += Game.baronKills;

               // Utility Stats
               visionScore += Game.visionScore;
               wardsPlaced += Game.wardsPlaced;
               wardsKilled += Game.wardsKilled;
               totalTimeCCDealt += Game.totalTimeCCDealt;
               totalDamageShieldedOnTeammates += Game.totalDamageShieldedOnTeammates;
               totalHealsOnTeammates += Game.totalHealsOnTeammates;

               // Fun Stats
               spell1Casts += Game.spell1Casts;
               spell2Casts += Game.spell2Casts;
               spell3Casts += Game.spell3Casts;
               spell4Casts += Game.spell4Casts;

               // Challenge Stats
               if (Game.challenges != null)
               {
                   killsNearEnemyTurret += Game.challenges.killsNearEnemyTurret;
                   totalLaneMinionsFirst10 += Game.challenges.laneMinionsFirst10Minutes;
                   maxCsAdvantageOnLaneOpponent = Math.Max(maxCsAdvantageOnLaneOpponent, Game.challenges.maxCsAdvantageOnLaneOpponent);
                   teamDamagePercentage += Game.challenges.teamDamagePercentage;
                   soloKills += Game.challenges.soloKills;
                   objectivesStolen += Game.challenges.epicMonsterSteals;
                   turretPlatesTaken += Game.challenges.turretPlatesTaken;
                   fistBumpParticipation += Game.challenges.fistBumpParticipation;
                   flawlessAces += Game.challenges.flawlessAces;
                   skillshotsDodged += Game.challenges.skillshotsDodged;
                   skillshotsHit += Game.challenges.skillshotsHit;
                   takedownsInAlcove += Game.challenges.takedownsInAlcove;
               }

                // Track Role
                if (roles.ContainsKey(Game.teamPosition)) roles[Game.teamPosition]++;
                else roles[Game.teamPosition] = 1;

                // Series and Champion Tracking

                if (!playerSeries.ContainsKey(seriesId))
                {
                    playerSeries[seriesId] = new List<string>();
                }
                playerSeries[seriesId].Add(champ);

                if (playerChamps.ContainsKey(champ))
                {
                    playerChamps[champ]++;
                }
                else
                {
                    playerChamps[champ] = 1;
                }
           }

           // Calculate Role
           var role = roles.OrderByDescending(pair => pair.Value).First().Key;
           primaryRole = role switch
           {
               "TOP" => "Top",
               "JUNGLE" => "Jungle",
               "MIDDLE" => "Middle",
               "BOTTOM" => "Bottom",
               _ => "Support"
           };

           // Calculate averages that need game count division
           if (TotalGames > 0)
           {
               teamDamagePercentage /= TotalGames;
           }
       }

        public PlayerData(List<string> data)
            : this(data.Select(d => new List<string> { d }).ToList())
        {
            // This wraps List<string> as List<List<string>> and calls the main constructor
        }
       private void InitializeStats()
       {
           // Basic Stats
           win = loss = 0;
           timePlayed = 0;

           // Combat Stats
           kills = deaths = assists = 0;
           largestKillingSpree = doubleKills = tripleKills = quadraKills = pentaKills = 0;

           // Carry Stats
           totalDamageDealtToChampions = damageSelfMitigated = goldSpent = 0;
           firstBloodKills = firstBloodAssists = totalMinionsKilled = killsNearEnemyTurret = totalLaneMinionsFirst10 = 0;
           maxCsAdvantageOnLaneOpponent = teamDamagePercentage = 0;
           soloKills = 0;

           // Objective Stats
           damageDealtToBuildings = 0;
           turretTakedowns = inhibitorTakedowns = objectivesStolen = turretPlatesTaken = baronKills = 0;

           // Utility Stats
           visionScore = wardsPlaced = wardsKilled = 0;
           totalTimeCCDealt = totalDamageShieldedOnTeammates = totalHealsOnTeammates = 0;

           // Fun Stats
           spell1Casts = spell2Casts = spell3Casts = spell4Casts = 0;
           fistBumpParticipation = flawlessAces = skillshotsDodged = skillshotsHit = takedownsInAlcove = 0;
       }

        public double GetKillsPerMinute() => timePlayed > 0 ? kills / (timePlayed / 60.0) : 0;
        public double GetKillsPerGame() => TotalGames > 0 ? (double)kills / TotalGames : 0;
        public double GetDeathsPerMinute() => timePlayed > 0 ? deaths / (timePlayed / 60.0) : 0;
        public double GetDeathsPerGame() => TotalGames > 0 ? (double)deaths / TotalGames : 0;
        public double GetAssistsPerMinute() => timePlayed > 0 ? assists / (timePlayed / 60.0) : 0;
        public double GetAssistsPerGame() => TotalGames > 0 ? (double)assists / TotalGames : 0;
        public double GetKDA() => deaths > 0 ? (kills + assists) / (double)deaths : kills + assists;

       // Carry Stats Calculations
       public double DamageDealtToChampionsPerMinute => timePlayed > 0 ? totalDamageDealtToChampions / (timePlayed / 60) : 0;
       public double DamageDealtToChampionsPerGame => TotalGames > 0 ? totalDamageDealtToChampions / TotalGames : 0;
       public double DamageMitigatedPerGame => TotalGames > 0 ? damageSelfMitigated / TotalGames : 0;
       public int TotalFirstBloods => firstBloodKills + firstBloodAssists;
       public double MinionsPerMinute => timePlayed > 0 ? totalMinionsKilled / (timePlayed / 60) : 0;
       public double MinionsPerGame => TotalGames > 0 ? (double)totalMinionsKilled / TotalGames : 0;
       public double KillsNearTurretPerGame => TotalGames > 0 ? (double)killsNearEnemyTurret / TotalGames : 0;
       public double LaneMinionsFirst10PerGame => TotalGames > 0 ? (double)totalLaneMinionsFirst10 / TotalGames : 0;
       public double DamagePerGold => goldSpent > 0 ? totalDamageDealtToChampions / goldSpent : 0;
       public double SoloKillsPerGame => TotalGames > 0 ? (double)soloKills / TotalGames : 0;

        // Objective Stats Calculations
        public double BuildingDamagePerMinute => timePlayed > 0 ? damageDealtToBuildings / (timePlayed / 60) : 0;
        public double BuildingDamagePerGame => TotalGames > 0 ? damageDealtToBuildings / TotalGames : 0;
        public double TurretTakedownsPerGame => TotalGames > 0 ? (double)turretTakedowns / TotalGames : 0;
        public double ObjectivesStolenPerGame => TotalGames > 0 ? (double)objectivesStolen / TotalGames : 0;
        public double TurretPlatesPerGame => TotalGames > 0 ? (double)turretPlatesTaken / TotalGames : 0;
        public double BaronKillsPerGame => TotalGames > 0 ? (double)baronKills / TotalGames : 0;
        public double DamageDealtToObjectivesPerMinute => timePlayed > 0 ? damageDealtToBuildings / (timePlayed / 60) : 0;
        public double DamageDealtToObjectivesPerGame => TotalGames > 0 ? damageDealtToBuildings / TotalGames : 0;

       // Utility Stats Calculations
       public double VisionScorePerMinute => timePlayed > 0 ? visionScore / (timePlayed / 60) : 0;
       public double VisionScorePerGame => TotalGames > 0 ? (double)visionScore / TotalGames : 0;
       public double WardsPerMinute => timePlayed > 0 ? wardsPlaced / (timePlayed / 60) : 0;
       public double WardsPerGame => TotalGames > 0 ? (double)wardsPlaced / TotalGames : 0;
       public double WardsKilledPerGame => TotalGames > 0 ? (double)wardsKilled / TotalGames : 0;
       public double CCTimePerGame => TotalGames > 0 ? totalTimeCCDealt / TotalGames : 0;
       public double ShieldingPerGame => TotalGames > 0 ? totalDamageShieldedOnTeammates / TotalGames : 0;
       public double HealingPerGame => TotalGames > 0 ? totalHealsOnTeammates / TotalGames : 0;

       // Fun Stats Calculations
       public double SpellCastsPerMinute => timePlayed > 0 ? totalSpellCasts / (timePlayed / 60) : 0;
       public double SpellCastsPerGame => TotalGames > 0 ? (double)totalSpellCasts / TotalGames : 0;
       public double SkillshotsDodgedPerGame => TotalGames > 0 ? (double)skillshotsDodged / TotalGames : 0;
       public double SkillshotsHitPerGame => TotalGames > 0 ? (double)skillshotsHit / TotalGames : 0;
   }
}