namespace RankTangle.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using RankTangle.ControllerHelpers;
    using RankTangle.Models;
    using RankTangle.Models.Domain;
    using RankTangle.Models.ViewModels;

    using MongoDB.Bson;
    using MongoDB.Driver.Builders;

    using StackExchange.Profiling;

    public class StatsController : BaseController
    {
        protected readonly MiniProfiler Profiler = MiniProfiler.Current;

        public ActionResult Index()
        {
            var viewModel = new StatsAggregateViewModel
                                {
                                    MostFights = StatsControllerHelpers.GetStatMostFights(),
                                    MostWins = StatsControllerHelpers.GetStatMostWins(),
                                    MostLosses = StatsControllerHelpers.GetStatMostLosses(),
                                    TopRanked = StatsControllerHelpers.GetStatTopRanked(),
                                    BottomRanked = StatsControllerHelpers.GetStatBottomRanked()
                                };

            var matches = Dbh.GetCollection<Match>("Matches").FindAll();
            var matchesList = matches.SetSortOrder(SortBy.Ascending("GameOverTime")).ToList();
            var players = matches.Select(m => m.TeamBPlayer1).ToList();
            players.AddRange(matches.Select(m => m.TeamBPlayer2).ToList());
            players.AddRange(matches.Select(m => m.TeamAPlayer1).ToList());
            players.AddRange(matches.Select(m => m.TeamAPlayer2).ToList());

            viewModel.HighestRatingEver = StatsControllerHelpers.GetStatHighestRatingEver(players);
            viewModel.LowestRatingEver = StatsControllerHelpers.GetStatLowestRatingEver(players);

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
                    var stats = new PlayerStatsViewModel { Player = player };

                    var matches =
                        Dbh.GetCollection<Match>("Matches")
                           .FindAll()
                           .SetSortOrder(SortBy.Ascending("GameOverTime"))
                           .ToList()
                           .Where(match => match.ContainsPlayer(playerId))
                           .Where(match => match.GameOverTime != DateTime.MinValue);

                    var playedMatches = matches as List<Match> ?? matches.ToList();
                    stats.PlayedMatches = playedMatches.OrderByDescending(x => x.GameOverTime);
                    stats.LatestMatch = playedMatches.Last();

                    foreach (var match in playedMatches)
                    {
                        var teamMate = match.GetTeamMate(playerId);
                        if (teamMate.Id != null)
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

                        if (match.IsOnTeamA(playerId))
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
                                var goalDiff = match.TeamBScore - match.TeamAScore;
                                if (eae.ContainsKey(match.TeamBPlayer1.Id))
                                {
                                    eae[match.TeamBPlayer1.Id].Occurrences++;
                                    eae[match.TeamBPlayer1.Id].GoalDiff += goalDiff;
                                }
                                else
                                {
                                    eae.Add(
                                        match.TeamBPlayer1.Id,
                                        new EvilArchEnemy { Player = match.TeamBPlayer1, GoalDiff = goalDiff });
                                }

                                if (match.TeamBPlayer2.Id != null)
                                {
                                    if (eae.ContainsKey(match.TeamBPlayer2.Id))
                                    {
                                        eae[match.TeamBPlayer2.Id].Occurrences++;
                                        eae[match.TeamBPlayer2.Id].GoalDiff += goalDiff;
                                    }
                                    else
                                    {
                                        eae.Add(
                                            match.TeamBPlayer2.Id,
                                            new EvilArchEnemy { Player = match.TeamBPlayer2, GoalDiff = goalDiff });
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
                                var goalDiff = match.TeamBScore - match.TeamAScore;
                                if (eae.ContainsKey(match.TeamAPlayer1.Id))
                                {
                                    eae[match.TeamAPlayer1.Id].Occurrences++;
                                    eae[match.TeamAPlayer1.Id].GoalDiff = goalDiff;
                                }
                                else
                                {
                                    eae.Add(
                                        match.TeamAPlayer1.Id,
                                        new EvilArchEnemy { Player = match.TeamAPlayer1, GoalDiff = goalDiff });
                                }

                                if (match.TeamAPlayer2.Id != null)
                                {
                                    if (eae.ContainsKey(match.TeamAPlayer2.Id))
                                    {
                                        eae[match.TeamAPlayer2.Id].Occurrences++;
                                        eae[match.TeamAPlayer2.Id].GoalDiff = goalDiff;
                                    }
                                    else
                                    {
                                        eae.Add(
                                            match.TeamAPlayer2.Id,
                                            new EvilArchEnemy { Player = match.TeamAPlayer2, GoalDiff = goalDiff });
                                    }
                                }
                            }
                        }

                        stats.Played++;
                        stats.Won = match.WonTheMatch(playerId) ? ++stats.Won : stats.Won;
                        stats.Lost = !match.WonTheMatch(playerId) ? ++stats.Lost : stats.Lost;
                        stats.PlayedToday = match.GameOverTime.ToLocalTime().Day == DateTime.Now.Day
                                                ? ++stats.PlayedToday
                                                : stats.PlayedToday;
                        stats.PlayedLast7Days = match.GameOverTime.ToLocalTime() > DateTime.Now.AddDays(-7)
                                                    ? ++stats.PlayedLast7Days
                                                    : stats.PlayedLast7Days;
                        stats.PlayedLast30Days = match.GameOverTime.ToLocalTime() > DateTime.Now.AddDays(-30)
                                                     ? ++stats.PlayedLast30Days
                                                     : stats.PlayedLast30Days;
                        stats.Ranking = playerCollection.FindAll()
                                            .SetSortOrder(SortBy.Descending("Rating"))
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
                    .Where(match => match.ContainsPlayer(playerId))
                    .Where(match => match.GameOverTime != DateTime.MinValue);

                var playedMatches = matches as List<Match> ?? matches.ToList();
                double minRating = 10000;
                double maxRating = 0;

                foreach (var match in playedMatches)
                {
                    var matchPlayer = match.GetPlayer(playerId);
                    minRating = (minRating > matchPlayer.Rating) ? matchPlayer.Rating : minRating;
                    maxRating = (maxRating < matchPlayer.Rating) ? matchPlayer.Rating : maxRating;

                    var time = new List<string>
                        {
                            match.GameOverTime.ToLocalTime().Year.ToString(CultureInfo.InvariantCulture),
                            match.GameOverTime.ToLocalTime().Month.ToString(CultureInfo.InvariantCulture),
                            match.GameOverTime.ToLocalTime().Day.ToString(CultureInfo.InvariantCulture),
                            match.GameOverTime.ToLocalTime().Hour.ToString(CultureInfo.InvariantCulture),
                            match.GameOverTime.ToLocalTime().Minute.ToString(CultureInfo.InvariantCulture),
                            match.GameOverTime.ToLocalTime().Second.ToString(CultureInfo.InvariantCulture)
                        };

                    chartData.DataPoints.Add(new PlayerRatingChartDataPoint { TimeSet = time, Rating = matchPlayer.Rating });
                }

                chartData.MinimumValue = minRating;
                chartData.MaximumValue = maxRating;
            }
          
            return Json(chartData, JsonRequestBehavior.AllowGet);
        }
    }
}
