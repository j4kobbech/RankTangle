namespace RankTangle.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;
    using MongoDB.Bson;
    using MongoDB.Driver.Builders;
    using RankTangle.ControllerHelpers;
    using RankTangle.Models;
    using RankTangle.Models.Domain;
    using RankTangle.Models.ViewModels;
    using StackExchange.Profiling;

    public class StatsController : BaseController
    {
        protected readonly MiniProfiler Profiler = MiniProfiler.Current;

        public ActionResult Index()
        {
            var matches = Dbh.GetCollection<Match>("Matches").FindAll();
            var matchesList = matches.SetSortOrder(SortBy.Ascending("GameOverTime")).ToList();
            var players = new List<Player>();
            var viewModel = new StatsAggregateViewModel { MatchCount = matchesList.Count(), Settings = this.Settings };

            matchesList.ForEach(x => x.GetMatchPlayers().ForEach(players.Add));

            if (matchesList.Any())
            {
                using (Profiler.Step("Step A"))
                {
                    viewModel.MostFights = StatsControllerHelpers.GetMostFights();
                    viewModel.MostWins = StatsControllerHelpers.GetMostWins();
                    viewModel.MostLosses = StatsControllerHelpers.GetMostLosses();
                    viewModel.TopRanked = StatsControllerHelpers.GetTopRanked();
                    viewModel.BottomRanked = StatsControllerHelpers.GetBottomRanked();
                    viewModel.HighestRatingEver = StatsControllerHelpers.GetHighestOverAllRatingEver(players);
                    viewModel.LowestRatingEver = StatsControllerHelpers.GetLowestOverAllRatingEver(players);
                }

                var winningStreak = StatsControllerHelpers.GetLongestWinningStreak(matchesList);
                winningStreak.Player = DbHelper.GetPlayer(winningStreak.Player.Id);
                viewModel.LongestWinningStreak = winningStreak;

                var losingStreak = StatsControllerHelpers.GetLongestLosingStreak(matchesList);
                losingStreak.Player = DbHelper.GetPlayer(losingStreak.Player.Id);
                viewModel.LongestLosingStreak = losingStreak;

                using (Profiler.Step("Calculating BiggestRatingWin"))
                {
                    viewModel.BiggestRatingWin = StatsControllerHelpers.GetBiggestRatingWin();
                }
            }

            return this.View(viewModel);
        }

        public ActionResult Player(string playerId)
        {
            using (Profiler.Step("Calculating Player Statistics"))
            {
                if (playerId != null)
                {
                    var bff = new Dictionary<string, BestFriendForever>();
                    var rbff = new Dictionary<string, RealBestFriendForever>();
                    var eae = new Dictionary<string, EvilArchEnemy>();
                    var preferredColor = new Dictionary<string, PreferredColor>();
                    var winningColor = new Dictionary<string, WinningColor>();
                    const string TeamA = "TeamA";
                    const string TeamB = "TeamB";

                    var playerCollection = Dbh.GetCollection<Player>("Players");
                    var player = playerCollection.FindOne(Query.EQ("_id", BsonObjectId.Parse(playerId)));
                    var stats = new PlayerStatsViewModel { Player = player, Settings = Settings };

                    var matches =
                        Dbh.GetCollection<Match>("Matches")
                           .FindAll()
                           .SetSortOrder(SortBy.Ascending("GameOverTime"))
                           .ToList()
                           .Where(match => match.MatchContainsPlayer(playerId))
                           .Where(match => match.GameOverTime != DateTime.MinValue);

                    if (!matches.Any())
                    {
                        return this.View(stats);
                    }  

                    var playedMatches = matches as List<Match> ?? matches.ToList();
                    stats.PlayedMatches = playedMatches.OrderByDescending(x => x.GameOverTime);
                    stats.LatestMatch = playedMatches.Last();

                    foreach (var match in playedMatches)
                    {
                        var teamMates = match.GetTeamByPlayerId(playerId).GetTeamMates(playerId);
                        
                        foreach (var teamMate in teamMates)
                        {
                            if (bff.ContainsKey(teamMate.Id))
                            {
                                bff[teamMate.Id].Occurrences++;
                            }
                            else
                            {
                                bff.Add(teamMate.Id, new BestFriendForever { Player = teamMate });
                            }

                            if (match.WonTheMatch(playerId))
                            {
                                if (rbff.ContainsKey(teamMate.Id))
                                {
                                    rbff[teamMate.Id].Occurrences++;
                                }
                                else
                                {
                                    rbff.Add(teamMate.Id, new RealBestFriendForever { Player = teamMate });
                                }
                            }
                        }

                        if (match.IsPlayerOnTeamA(playerId))
                        {
                            if (preferredColor.ContainsKey(TeamA))
                            {
                                preferredColor[TeamA].Occurrences++;
                            }
                            else
                            {
                                preferredColor.Add(TeamA, new PreferredColor { Color = TeamA, Occurrences = 1 });
                            }

                            if (match.WonTheMatch(playerId))
                            {
                                if (winningColor.ContainsKey(TeamA))
                                {
                                    winningColor[TeamA].Occurrences++;
                                }
                                else
                                {
                                    winningColor.Add(TeamA, new WinningColor { Color = TeamA, Occurrences = 1 });
                                }
                            }
                            else
                            {
                                foreach (var teamPlayer in match.TeamB.TeamPlayers)
                                {
                                    var goalDiff = match.TeamBScore - match.TeamAScore;
                                    if (eae.ContainsKey(teamPlayer.Id))
                                    {
                                        eae[teamPlayer.Id].Occurrences++;
                                        eae[teamPlayer.Id].GoalDiff += goalDiff;
                                    }
                                    else
                                    {
                                        eae.Add(teamPlayer.Id, new EvilArchEnemy { Player = teamPlayer, GoalDiff = goalDiff });
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (preferredColor.ContainsKey(TeamB))
                            {
                                preferredColor[TeamB].Occurrences++;
                            }
                            else
                            {
                                preferredColor.Add(TeamB, new PreferredColor { Color = TeamB, Occurrences = 1 });
                            }

                            if (match.WonTheMatch(playerId))
                            {
                                if (winningColor.ContainsKey(TeamB))
                                {
                                    winningColor[TeamB].Occurrences++;
                                }
                                else
                                {
                                    winningColor.Add(TeamB, new WinningColor { Color = TeamB, Occurrences = 1 });
                                }
                            }
                            else
                            {
                                foreach (var teamPlayer in match.TeamA.TeamPlayers)
                                {
                                    var goalDiff = match.TeamBScore - match.TeamAScore;
                                    if (eae.ContainsKey(teamPlayer.Id))
                                    {
                                        eae[teamPlayer.Id].Occurrences++;
                                        eae[teamPlayer.Id].GoalDiff = goalDiff;
                                    }
                                    else
                                    {
                                        eae.Add(teamPlayer.Id, new EvilArchEnemy { Player = teamPlayer, GoalDiff = goalDiff });
                                    }
                                }
                            }
                        }

                        stats.Played++;
                        stats.Won = match.WonTheMatch(playerId) ? ++stats.Won : stats.Won;
                        stats.Lost = !match.WonTheMatch(playerId) ? ++stats.Lost : stats.Lost;
                        stats.Ranking = playerCollection.FindAll()
                                            .SetSortOrder(SortBy.Descending("Ratings.OverAll"))
                                            .ToList()
                                            .FindIndex(x => x.Id == playerId) + 1; // convert zero-based to 1-based index

                        stats.TotalNumberOfPlayers = (int)playerCollection.Count();
                    }

                    stats.Bff = bff.OrderByDescending(i => i.Value.Occurrences).Select(i => i.Value).FirstOrDefault();
                    stats.Rbff = rbff.OrderByDescending(i => i.Value.Occurrences).Select(i => i.Value).FirstOrDefault();
                    stats.Eae =
                        eae.OrderByDescending(i => i.Value.Occurrences)
                           .ThenByDescending(i => i.Value.GoalDiff)
                           .Select(i => i.Value)
                           .FirstOrDefault();
                    stats.PreferredColor =
                        preferredColor.OrderByDescending(i => i.Value.Occurrences).Select(i => i.Value).FirstOrDefault();
                    stats.WinningColor =
                        winningColor.OrderByDescending(i => i.Value.Occurrences).Select(i => i.Value).FirstOrDefault();

                    return View(stats);
                }
            }

            return this.RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public JsonResult GetPlayerRatingData(string playerId)
        {
            var chartData = new PlayerRatingChartData(); 

            if (playerId != null)
            {
                var matches = Dbh.GetCollection<Match>("Matches")
                    .FindAll()
                    .SetSortOrder(SortBy.Ascending("GameOverTime"))
                    .ToList()
                    .Where(match => match.MatchContainsPlayer(playerId))
                    .Where(match => match.GameOverTime != DateTime.MinValue);

                var playedMatches = matches as List<Match> ?? matches.ToList();
                double minRating = 10000;
                double maxRating = 0;

                foreach (var match in playedMatches)
                {
                    var matchPlayer = match.GetMatchPlayer(playerId);
                    minRating = (minRating > matchPlayer.Ratings.OverAll) ? matchPlayer.Ratings.OverAll : minRating;
                    maxRating = (maxRating < matchPlayer.Ratings.OverAll) ? matchPlayer.Ratings.OverAll : maxRating;

                    var time = new List<string>
                        {
                            match.GameOverTime.ToLocalTime().Year.ToString(CultureInfo.InvariantCulture),
                            match.GameOverTime.ToLocalTime().Month.ToString(CultureInfo.InvariantCulture),
                            match.GameOverTime.ToLocalTime().Day.ToString(CultureInfo.InvariantCulture),
                            match.GameOverTime.ToLocalTime().Hour.ToString(CultureInfo.InvariantCulture),
                            match.GameOverTime.ToLocalTime().Minute.ToString(CultureInfo.InvariantCulture),
                            match.GameOverTime.ToLocalTime().Second.ToString(CultureInfo.InvariantCulture)
                        };

                    chartData.DataPoints.Add(new PlayerRatingChartDataPoint { TimeSet = time, Rating = matchPlayer.Ratings.OverAll });
                }

                chartData.MinimumValue = minRating;
                chartData.MaximumValue = maxRating;
            }
          
            return Json(chartData, JsonRequestBehavior.AllowGet);
        }
    }
}
