namespace RankTangle.Controllers
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using RankTangle.Main;
    using RankTangle.Models.Custom;
    using RankTangle.Models.Domain;
    using RankTangle.Models.ViewModels;

    using MongoDB.Bson;
    using MongoDB.Driver.Builders;

    public class MatchesController : BaseController
    {       
        // GET: /Matches/
        public ActionResult Index()
        {
            // Fetch all players to display in a <select>
            var playerCollection = this.Dbh.GetCollection<Player>("Players").FindAll().SetSortOrder(SortBy.Ascending("Name")).ToList();
            
            // Fetch all RankTangle matches
            var playedMatches =
                this.Dbh.GetCollection<Match>("Matches")
                    .Find(Query.NE("GameOverTime", BsonDateTime.Create(DateTime.MinValue)))
                    .ToList()
                    .OrderByDescending(x => x.GameOverTime)
                    .Take(30);

            var pendingMatches =
                this.Dbh.GetCollection<Match>("Matches")
                    .Find(Query.EQ("GameOverTime", BsonDateTime.Create(DateTime.MinValue)))
                    .ToList()
                    .OrderByDescending(x => x.CreationTime);

            // Create content for the <select> 
            var selectItems = playerCollection
                .Select(x => new CustomSelectListItem { Selected = false, Text = x.Name, Value = x.Id, CssClass = x.Gender })
                .ToList();

            var played = playedMatches.OrderByDescending(x => x.GameOverTime);
            var pending = pendingMatches.OrderByDescending(x => x.CreationTime);

            return View(new MatchesViewModel
                            {
                                PlayedMatches = played, 
                                PendingMatches = pending, 
                                SelectPlayers = selectItems,
                                Settings = this.Settings
                            });
        }

        // POST: /Matches/RegisterMatch
        [HttpPost]
        public ActionResult RegisterMatch(FormCollection formCollection)
        {
            var currentUser = (Player)Session["User"];
            var unresolvedMatch = CreateMatch(currentUser, formCollection);
            var resolvedMatch = SaveMatchResult(unresolvedMatch, formCollection);
            var matchCollection = this.Dbh.GetCollection<Match>("Matches");

            matchCollection.Save(resolvedMatch);
            Events.SubmitEvent("Register", "Match", resolvedMatch, currentUser.Id);

            return this.RedirectToAction("Index");
        }

        // POST: /Matches/Delete/{id}
        [HttpGet]
        public ActionResult Delete(string id)
        {
            var currentUser = (Player)Session["User"];

            if (currentUser != null)
            {
                var matchCollection = this.Dbh.GetCollection<Match>("Matches");
                var query = Query.EQ("_id", BsonObjectId.Parse(id));
                var match = matchCollection.FindOne(query);

                Events.SubmitEvent("Delete", "Match", match, currentUser.Id);
                matchCollection.Remove(query);
            }

            return RedirectToAction("Index");
        }

        // POST: /Matches/Create/{FormCollection}
        private Match CreateMatch(Player user, FormCollection formValues)
        {
            Match newMatch = null;

            if (user != null)
            {
                var r1 = formValues.GetValue("team-a-player-1").AttemptedValue;
                var r2 = formValues.GetValue("team-a-player-2").AttemptedValue;
                var b1 = formValues.GetValue("team-b-player-1").AttemptedValue;
                var b2 = formValues.GetValue("team-b-player-2").AttemptedValue;

                // only try to create a match if properties are set correctly
                if (!string.IsNullOrEmpty(r1) && !string.IsNullOrEmpty(b1))
                {
                    // var matchCollection = this.Dbh.GetCollection<Match>("Matches");
                    var playerCollection = this.Dbh.GetCollection<Player>("Players");
                    var teamAPlayer1 = string.IsNullOrEmpty(r1)
                                         ? new Player()
                                         : playerCollection.FindOne(Query.EQ("_id", BsonObjectId.Create(r1)));
                    var teamAPlayer2 = string.IsNullOrEmpty(r2)
                                         ? new Player()
                                         : playerCollection.FindOne(Query.EQ("_id", BsonObjectId.Create(r2)));
                    var teamBPlayer1 = string.IsNullOrEmpty(b1)
                                          ? new Player()
                                          : playerCollection.FindOne(Query.EQ("_id", BsonObjectId.Create(b1)));
                    var teamBPlayer2 = string.IsNullOrEmpty(b2)
                                          ? new Player()
                                          : playerCollection.FindOne(Query.EQ("_id", BsonObjectId.Create(b2)));

                    newMatch = new Match
                                    {
                                        TeamAPlayer1 = teamAPlayer1,
                                        TeamAPlayer2 = teamAPlayer2,
                                        TeamBPlayer1 = teamBPlayer1,
                                        TeamBPlayer2 = teamBPlayer2,
                                        CreationTime = new BsonDateTime(DateTime.Now),
                                        GameOverTime = new BsonDateTime(DateTime.MinValue),
                                        Created = new BsonDateTime(DateTime.Now),
                                        CreatedBy = user.Id
                                    };
                }
            }

            return newMatch;
        }

        // POST: /Matches/SaveMatchResult/{FormCollection}
        [HttpPost]
        private Match SaveMatchResult(Match match, FormCollection form)
        {
            var teamAScore = form.GetValue("team-a-score").AttemptedValue;
            var teamBScore = form.GetValue("team-b-score").AttemptedValue;
            
            if (string.IsNullOrEmpty(teamAScore) == false && string.IsNullOrEmpty(teamBScore) == false)
            {
                var playerCollection = this.Dbh.GetCollection<Player>("Players");

                // Update players from the match with players from the Db.
                if (match.TeamAPlayer1.Id != null)
                {
                    match.TeamAPlayer1 = playerCollection.FindOne(Query.EQ("_id", BsonObjectId.Parse(match.TeamAPlayer1.Id)));
                }

                if (match.TeamAPlayer2.Id != null)
                {
                    match.TeamAPlayer2 = playerCollection.FindOne(Query.EQ("_id", BsonObjectId.Parse(match.TeamAPlayer2.Id)));
                }
                
                if (match.TeamBPlayer1.Id != null)
                {
                    match.TeamBPlayer1 = playerCollection.FindOne(Query.EQ("_id", BsonObjectId.Parse(match.TeamBPlayer1.Id)));
                }
                
                if (match.TeamBPlayer2.Id != null)
                {
                    match.TeamBPlayer2 = playerCollection.FindOne(Query.EQ("_id", BsonObjectId.Parse(match.TeamBPlayer2.Id)));
                }

                var currentUser = (Player)Session["User"];
                if (currentUser != null)
                {
                    // Get the scores
                    var intTeamAScore = int.Parse(teamAScore, NumberStyles.Float);
                    var intTeamBScore = int.Parse(teamBScore, NumberStyles.Float);
                    match.TeamAScore = intTeamAScore;
                    match.TeamBScore = intTeamBScore;

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

                    // Propagate the rating and stats to the team members of both teams
                    foreach (var member in winners.MatchTeam.Where(member => member.Id != null))
                    {
                        member.Rating += ratingModifier;
                        member.Won++;
                        member.Played++;
                        playerCollection.Save(member);
                    }

                    foreach (var member in losers.MatchTeam.Where(member => member.Id != null))
                    {
                        member.Rating -= ratingModifier;
                        member.Lost++;
                        member.Played++;
                        playerCollection.Save(member);
                    }

                    // Update match time stats
                    match.GameOverTime = new BsonDateTime(DateTime.Now);
                }
            }
            
            return match;
        }
    }
}