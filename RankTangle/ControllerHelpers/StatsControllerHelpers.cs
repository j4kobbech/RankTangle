namespace RankTangle.ControllerHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;
    using RankTangle.Main;
    using RankTangle.Models.Custom;
    using RankTangle.Models.Domain;

    public static class StatsControllerHelpers
    {
        private static readonly MongoDatabase Dbh = new Db(AppConfig.GetEnvironment()).Dbh;

        public static Player GetMostFights()
        {
            return Dbh.GetCollection<Player>("Players")
                    .FindAll()
                    .SetSortOrder(SortBy.Descending("Played"))
                    .FirstOrDefault();
        }

        public static Player GetMostWins()
        {
            return Dbh.GetCollection<Player>("Players")
                    .FindAll()
                    .SetSortOrder(SortBy.Descending("Won"))
                    .FirstOrDefault();
        }

        public static Player GetMostLosses()
        {
            return Dbh.GetCollection<Player>("Players")
                    .FindAll()
                    .SetSortOrder(SortBy.Descending("Lost"))
                    .FirstOrDefault();
        }

        public static Player GetTopRanked()
        {
            return Dbh.GetCollection<Player>("Players")
                    .FindAll()
                    .SetSortOrder(SortBy.Descending("Ratings.OverAll"))
                    .FirstOrDefault();
        }

        public static Player GetBottomRanked()
        {
            return Dbh.GetCollection<Player>("Players")
                    .FindAll()
                    .SetSortOrder(SortBy.Ascending("Ratings.OverAll"))
                    .FirstOrDefault();
        }

        public static Player GetHighestOverAllRatingEver(List<Player> players)
        {
            return players.OrderByDescending(m => m.Ratings.OverAll).FirstOrDefault();
        }

        public static Player GetLowestOverAllRatingEver(List<Player> players)
        {
            return players.OrderBy(m => m.Ratings.OverAll).FirstOrDefault();
        }

        public static Streak GetLongestWinningStreak(List<Match> matches)
        {
            var streaks = new Streaks();

            foreach (var match in matches)
            {
                foreach (var teamPlayer in match.GetWinningTeam().TeamPlayers)
                {
                    streaks.Add(teamPlayer.Id);
                }

                foreach (var teamPlayer in match.GetLosingTeam().TeamPlayers)
                {
                    streaks.Reset(teamPlayer.Id);
                }
            }

            return streaks.GetLongestStreak();
        }

        public static Streak GetLongestLosingStreak(List<Match> matches)
        {
            var streaks = new Streaks();

            foreach (var match in matches)
            {
                foreach (var teamPlayer in match.GetWinningTeam().TeamPlayers)
                {
                    streaks.Reset(teamPlayer.Id);
                }

                foreach (var teamPlayer in match.GetLosingTeam().TeamPlayers)
                {
                    streaks.Add(teamPlayer.Id);
                }
            }

            return streaks.GetLongestStreak();
        }

        public static RatingDifference GetBiggestRatingWin()
        {
            var matches = Dbh.GetCollection<Match>("Matches").FindAll().SetSortOrder(SortBy.Ascending("GameOverTime"));
            var ratingDiff = new RatingDifference();
            
            foreach (var match in matches)
            {
                // Determine the winners and the losers
                var winners = match.GetWinningTeam();
                var losers = match.GetLosingTeam();

                // Get the rating modifier
                var overAllRatingModifier = Rating.GetRatingModifier(winners.GetTeamRatings.OverAll, losers.GetTeamRatings.OverAll);
                var singleRatingModifier = Rating.GetRatingModifier(winners.GetTeamRatings.OverAll, losers.GetTeamRatings.OverAll);
                var doubleRatingModifier = Rating.GetRatingModifier(winners.GetTeamRatings.OverAll, losers.GetTeamRatings.OverAll);

                var highestRatingModifier = Math.Max(Math.Max(overAllRatingModifier, singleRatingModifier), doubleRatingModifier);

                if (highestRatingModifier > ratingDiff.Rating)
                {
                    ratingDiff.Rating = highestRatingModifier;
                    ratingDiff.Match = match;
                }
            }

            return ratingDiff;
        }
    }
}