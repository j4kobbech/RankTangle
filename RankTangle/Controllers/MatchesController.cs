namespace RankTangle.Controllers
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;
    using MongoDB.Bson;
    using MongoDB.Driver.Builders;
    using RankTangle.ControllerHelpers;
    using RankTangle.Models.Custom;
    using RankTangle.Models.Domain;
    using RankTangle.Models.ViewModels;

    public class MatchesController : BaseController
    {
        public ActionResult Index()
        {
            // Fetch all players to display in a <select>
            var playerCollection = Dbh.GetCollection<Player>("Players").FindAll().SetSortOrder(SortBy.Ascending("Name")).ToList();
            
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
                .Select(x => new CustomSelectListItem { Selected = false, Text = x.Name, Value = x.Id, CssClass = x.Gender.ToString() })
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

        [HttpPost]
        public ActionResult RegisterMatch(FormCollection formCollection)
        {
            var currentUser = (Player)Session["User"];
            
            if (currentUser != null)
            {
                var teamAScore = formCollection.GetValue("team-a-score").AttemptedValue;
                var teamBScore = formCollection.GetValue("team-b-score").AttemptedValue;
                var newMatch = MatchControllerHelpers.CreateMatch(currentUser, formCollection);

                // Get the scores
                newMatch.TeamAScore = int.Parse(teamAScore, NumberStyles.Float);
                newMatch.TeamBScore = int.Parse(teamBScore, NumberStyles.Float);
                newMatch.ResolveMatch().SaveMatch();
            }

            return this.RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            var currentUser = (Player)Session["User"];

            if (currentUser != null)
            {
                var matchCollection = this.Dbh.GetCollection<Match>("Matches");
                var query = Query.EQ("_id", BsonObjectId.Parse(id));

                matchCollection.Remove(query);
            }

            return RedirectToAction("Index");
        }
    }
}