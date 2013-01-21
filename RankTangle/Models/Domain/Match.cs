namespace RankTangle.Models.Domain
{
    using System;
    using System.Collections.Generic;
    using MongoDB.Bson;
    using MongoDB.Driver;

    using RankTangle.Main;
    using RankTangle.Models.Base;

    public class Match : MongoDocument
    {
        private readonly MongoDatabase dbh = new Db(AppConfig.GetEnvironment()).Dbh;

        public Match()
        {
            TeamA = new Team();
            TeamB = new Team();
        }

        public MatchType Type { get; set; }
        
        public int TeamAScore { get; set; }

        public int TeamBScore { get; set; }

        public Team TeamA { get; set; }

        public Team TeamB { get; set; }

        public BsonDateTime CreationTime { get; set; }
        
        public BsonDateTime GameOverTime { get; set; }

        public bool WonTheMatch(string playerId)
        {
            return (TeamA.IsPlayerOnTeam(playerId) && TeamAScore > TeamBScore) || (TeamB.IsPlayerOnTeam(playerId) && TeamBScore > TeamAScore);
        }

        public bool MatchContainsPlayer(string playerId)
        {
            return TeamA.IsPlayerOnTeam(playerId) || TeamB.IsPlayerOnTeam(playerId);
        }

        public Player GetMatchPlayer(string playerId)
        {
            return TeamA.GetTeamPlayer(playerId) ?? TeamB.GetTeamPlayer(playerId);
        }

        public List<Player> GetMatchPlayers()
        {
            var listOfPlayers = new List<Player>();
            listOfPlayers.AddRange(TeamA.TeamPlayers);
            listOfPlayers.AddRange(TeamB.TeamPlayers);

            return listOfPlayers;
        }

        public Team GetWinningTeam()
        {
            return TeamAScore > TeamBScore ? TeamA : TeamB;
        }

        public Team GetLosingTeam()
        {
            return TeamAScore > TeamBScore ? TeamB : TeamA;
        } 

        public Team GetTeamByPlayerId(string playerId)
        {
            return TeamA.IsPlayerOnTeam(playerId) ? TeamA : TeamB;
        }

        public bool IsPlayerOnTeamA(string playerId)
        {
            return TeamA.IsPlayerOnTeam(playerId);
        }

        public bool IsPlayerOnTeamB(string playerId)
        {
            return TeamB.IsPlayerOnTeam(playerId);
        }

        public Match ResolveMatch()
        {
            var playerCollection = dbh.GetCollection<Player>("Players");
            var match = this;

            // Determine the winners and the losers
            var winningTeam = this.GetWinningTeam();
            var losingTeam = this.GetLosingTeam();

            // Get the rating modifier
            Rating.ModifyMatchRatings(ref match);

            winningTeam.AddWin();
            losingTeam.AddLoss();
            match.GetMatchPlayers().ForEach(x => playerCollection.Save(x));
            match.GameOverTime = new BsonDateTime(DateTime.Now);

            return this;
        }

        public SafeModeResult SaveMatch()
        {
            var matchCollection = dbh.GetCollection<Match>("Matches");
            var match = this;

            return matchCollection.Save(match);
        }
    }
}   
