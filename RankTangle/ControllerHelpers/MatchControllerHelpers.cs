namespace RankTangle.ControllerHelpers
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.Mvc;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;
    using RankTangle.Main;
    using RankTangle.Models.Domain;

    using Match = RankTangle.Models.Domain.Match;

    public static class MatchControllerHelpers
    {
        private static readonly MongoDatabase Dbh = new Db(AppConfig.GetEnvironment()).Dbh;

        public static MatchType GetMatchType(Match match)
        {
            var gender = match.TeamA.TeamPlayers.First().Gender;

            if (match.TeamA.TeamPlayers.Count == 2 && match.TeamB.TeamPlayers.Count == 2)
            {
                return match.GetMatchPlayers().Any(x => x.Gender != gender) ? MatchType.MixedDouble : MatchType.Double;
            }

            if (match.TeamA.TeamPlayers.Count == 1 && match.TeamB.TeamPlayers.Count == 1 && match.GetMatchPlayers().All(x => x.Gender == gender))
            {
                return MatchType.Single;
            }
            
            return MatchType.Other;
        }

        public static Match CreateMatch(Player user, FormCollection formValues)
        {
            var newMatch = new Match
            {
                CreationTime = new BsonDateTime(DateTime.Now),
                GameOverTime = new BsonDateTime(DateTime.MinValue),
                Created = new BsonDateTime(DateTime.Now),
                CreatedBy = user.Id
            };

            var playerCollection = Dbh.GetCollection<Player>("Players");
            const string MatchPlayerRegEx = "^team-(a|b)-player-[0-9]+$";
            const string MatchTeamARegEx = "^team-a-player-[0-9]+$";

            foreach (var formValue in formValues.Keys)
            {
                var formElementName = formValue.ToString();
                var formElementValue = formValues.GetValue(formElementName).AttemptedValue;

                if (Regex.IsMatch(formElementName, MatchPlayerRegEx) && !string.IsNullOrEmpty(formElementValue))
                {
                    var player = playerCollection.FindOne(Query.EQ("_id", BsonObjectId.Create(formElementValue)));
 
                    if (Regex.IsMatch(formElementName, MatchTeamARegEx))
                    {
                        newMatch.TeamA.TeamPlayers.Add(player);
                    } 
                    else
                    {
                        newMatch.TeamB.TeamPlayers.Add(player);
                    }
                }
            }
            
            return newMatch;
        }
    }
}