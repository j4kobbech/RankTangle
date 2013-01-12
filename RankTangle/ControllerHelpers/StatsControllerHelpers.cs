namespace RankTangle.ControllerHelpers
{
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
                    .SetSortOrder(SortBy.Descending("Rating"))
                    .FirstOrDefault();
        }

        public static Player GetBottomRanked()
        {
            return Dbh.GetCollection<Player>("Players")
                    .FindAll()
                    .SetSortOrder(SortBy.Ascending("Rating"))
                    .FirstOrDefault();
        }

        public static Player GetHighestRatingEver(List<Player> players)
        {
            return players.OrderByDescending(m => m.Rating).FirstOrDefault();
        }

        public static Player GetLowestRatingEver(List<Player> players)
        {
            return players.OrderBy(m => m.Rating).FirstOrDefault();
        }

        public static Streak GetLongestWinningStreak(List<Match> matches)
        {
            var streaks = new Streaks();

            foreach (var match in matches)
            {
                if (match.WonTheMatch(match.TeamAPlayer1.Id))
                {
                    streaks.Add(match.TeamAPlayer1.Id);
                    streaks.Add(match.TeamAPlayer2.Id);

                    streaks.Reset(match.TeamBPlayer1.Id);
                    streaks.Reset(match.TeamBPlayer2.Id);
                }
                else
                {
                    streaks.Add(match.TeamBPlayer1.Id);
                    streaks.Add(match.TeamBPlayer2.Id);

                    streaks.Reset(match.TeamAPlayer1.Id);
                    streaks.Reset(match.TeamAPlayer2.Id);
                }
            }

            return streaks.GetLongestStreak();
        }

        public static Streak GetLongestLosingStreak(List<Match> matches)
        {
            var streaks = new Streaks();

            foreach (var match in matches)
            {
                if (!match.WonTheMatch(match.TeamAPlayer1.Id))
                {
                    streaks.Add(match.TeamAPlayer1.Id);
                    streaks.Add(match.TeamAPlayer2.Id);

                    streaks.Reset(match.TeamBPlayer1.Id);
                    streaks.Reset(match.TeamBPlayer2.Id);
                }
                else
                {
                    streaks.Add(match.TeamBPlayer1.Id);
                    streaks.Add(match.TeamBPlayer2.Id);

                    streaks.Reset(match.TeamAPlayer1.Id);
                    streaks.Reset(match.TeamAPlayer2.Id);
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
                var winners = new Team();
                var losers = new Team();

                if (match.TeamAScore > match.TeamBScore)
                {
                    winners.MatchTeam.Add(match.TeamAPlayer1);
                    winners.MatchTeam.Add(match.TeamAPlayer2);
                    losers.MatchTeam.Add(match.TeamBPlayer1);
                    losers.MatchTeam.Add(match.TeamBPlayer2);
                }
                else
                {
                    winners.MatchTeam.Add(match.TeamBPlayer1);
                    winners.MatchTeam.Add(match.TeamBPlayer2);
                    losers.MatchTeam.Add(match.TeamAPlayer1);
                    losers.MatchTeam.Add(match.TeamAPlayer2);
                }

                // Get the rating modifier
                var ratingModifier = Rating.GetRatingModifier(winners.GetTeamRating(), losers.GetTeamRating());

                if (ratingModifier > ratingDiff.Rating)
                {
                    ratingDiff.Rating = ratingModifier;
                    ratingDiff.Match = match;
                }
            }

            return ratingDiff;
        }
    }
}