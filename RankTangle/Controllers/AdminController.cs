namespace RankTangle.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using RankTangle.Main;
    using RankTangle.Models.Base;
    using RankTangle.Models.Domain;
    using RankTangle.Models.ViewModels;

    using MongoDB.Bson;
    using MongoDB.Driver.Builders;

    public class AdminController : BaseController
    {
        public ActionResult Index()
        {
            var currentUser = (Player)Session["User"];

            if (currentUser != null && currentUser.Email == this.Settings.AdminAccount)
            {
                var playerCollection = Dbh.GetCollection<Player>("Players")
                        .FindAll()
                        .SetSortOrder(SortBy.Ascending("Name"))
                        .ToList()
                        .Select(team => new SelectListItem { Selected = false, Text = team.Name, Value = team.Id })
                        .ToList();

                return View(new ConfigViewModel { Settings = this.Settings, Users = playerCollection });
            }

            return this.Redirect("/Home/Index");
        }

        [HttpGet]
        public JsonResult GetConfig()
        {
            var currentUser = (Player)Session["User"];

            if (currentUser != null && currentUser.Email == this.Settings.AdminAccount)
            {
                var playerCollection = Dbh.GetCollection<Player>("Players")
                        .FindAll()
                        .SetSortOrder(SortBy.Ascending("Name"))
                        .ToList()
                        .Select(team => new SelectListItem { Selected = false, Text = team.Name, Value = team.Id })
                        .ToList()
                        .ToJson();

                return Json(new { Settings = this.Settings.ToJson(), Users = playerCollection }, JsonRequestBehavior.AllowGet);
            }

            return Json(null);
        }

        [HttpPost]
        public ActionResult Save(ConfigViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var configCollection = Dbh.GetCollection<Config>("Config");

                this.Settings.Name = viewModel.Settings.Name;
                this.Settings.Domain = viewModel.Settings.Domain;
                this.Settings.AdminAccount = viewModel.Settings.AdminAccount;
                this.Settings.EnableDomainValidation = viewModel.Settings.EnableDomainValidation;
                this.Settings.EnableOneOnOneMatches = viewModel.Settings.EnableOneOnOneMatches;
                this.Settings.EnableGenderSpecificMatches = viewModel.Settings.EnableGenderSpecificMatches;

                configCollection.Save(this.Settings);
            }
            
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public JsonResult CopyProdData()
        {
            var dbhTo = new Db(Environment.Staging).Dbh;
            var dbhFrom = new Db().Dbh;

            var allMatches = dbhFrom.GetCollection<Match>("Matches").FindAll();
            var allPlayers = dbhFrom.GetCollection<Player>("Players").FindAll();

            var destinationMatches = dbhTo.GetCollection<Match>("Matches");
            var destinationPlayers = dbhTo.GetCollection<Player>("Players");

            destinationMatches.RemoveAll();
            destinationPlayers.RemoveAll();

            foreach (var match in allMatches)
            {
                destinationMatches.Save(match);
            }

            foreach (var player in allPlayers)
            {
                destinationPlayers.Save(player);
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult ReplayMatches()
        {
            var currentUser = (Player)Session["User"];

            if (currentUser != null && currentUser.Email == this.Settings.AdminAccount)
            {
                var allMatches = Dbh.GetCollection<Match>("Matches").FindAll().SetSortOrder(SortBy.Ascending("GameOverTime"));
                var allPlayers = Dbh.GetCollection<Player>("Players").FindAll();

                var copyMatches = Dbh.GetCollection<Match>("CopyMatches");
                var copyPlayers = Dbh.GetCollection<Player>("CopyPlayers");

                // Empty the Copies
                copyMatches.RemoveAll();
                copyPlayers.RemoveAll();

                // Reset all Players
                foreach (var player in allPlayers)
                {
                    player.Lost = 0;
                    player.Won = 0;
                    player.Played = 0;
                    player.Rating = 1000;

                    copyPlayers.Save(player);
                }

                /////////////////////////////////////////////////////////////////////////////////////////////////////////
                // replay each match in chronological order
                /////////////////////////////////////////////////////////////////////////////////////////////////////////
                foreach (var match in allMatches)
                {
                    // Update players from the match with players from the Db.
                    match.TeamAPlayer1 = copyPlayers.FindOne(Query.EQ("_id", BsonObjectId.Parse(match.TeamAPlayer1.Id)));
                    
                    if (match.CountTeamAPlayers() == 2)
                    {
                        match.TeamAPlayer2 = copyPlayers.FindOne(Query.EQ("_id", BsonObjectId.Parse(match.TeamAPlayer2.Id)));
                    }

                    match.TeamBPlayer1 = copyPlayers.FindOne(Query.EQ("_id", BsonObjectId.Parse(match.TeamBPlayer1.Id)));
                
                    if (match.CountTeamBPlayers() == 2)
                    {
                        match.TeamBPlayer2 = copyPlayers.FindOne(Query.EQ("_id", BsonObjectId.Parse(match.TeamBPlayer2.Id)));
                    }

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
                        copyPlayers.Save(member);
                    }

                    foreach (var member in losers.MatchTeam.Where(member => member.Id != null))
                    {
                        member.Rating -= ratingModifier;
                        member.Lost++;
                        member.Played++;
                        copyPlayers.Save(member);
                    }

                    // Save the data to Db
                    copyMatches.Save(match);
                }

                // Copy data into Production tables
                var matches = Dbh.GetCollection<Match>("Matches");
                var players = Dbh.GetCollection<Match>("Players");
                matches.RemoveAll();
                players.RemoveAll();

                foreach (var player in copyPlayers.FindAll())
                {
                    players.Save(player);
                }

                foreach (var match in copyMatches.FindAll())
                {
                    matches.Save(match);
                }
            }

            return RedirectToAction("Index", "Admin");
        }

        [HttpGet]
        public JsonResult GetPlayerEmails()
        {
            var allEmails = Dbh.GetCollection<Player>("Players").FindAll().Select(x => x.Email).ToList();
            return Json(allEmails, JsonRequestBehavior.AllowGet);
        }
    }
}
