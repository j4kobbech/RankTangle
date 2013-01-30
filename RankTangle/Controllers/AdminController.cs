namespace RankTangle.Controllers
{
    using System.Linq;
    using System.Web.Mvc;
    using MongoDB.Bson;
    using MongoDB.Driver.Builders;
    using RankTangle.Main;
    using RankTangle.Models.Base;
    using RankTangle.Models.Domain;
    using RankTangle.Models.ViewModels;
    
    public class AdminController : BaseController
    {
        public ActionResult Index()
        {
            var currentUser = (Player)Session["User"];
            var config = Dbh.GetCollection<Config>("Config").FindOne();

            if (currentUser != null && currentUser.Email == this.Settings.AdminAccount)
            {
                var playerCollection = Dbh.GetCollection<Player>("Players")
                        .FindAll()
                        .SetSortOrder(SortBy.Ascending("Name"))
                        .ToList()
                        .Select(team => new SelectListItem { Selected = false, Text = team.Name, Value = team.Id })
                        .ToList();

                return View(new ConfigViewModel { Settings = config, Users = playerCollection });
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
                var dbConfig = configCollection.FindOne();

                Settings.Id = dbConfig.Id;
                Settings.Name = viewModel.Settings.Name;
                Settings.TeamALabel = viewModel.Settings.TeamALabel;
                Settings.TeamBLabel = viewModel.Settings.TeamBLabel;
                Settings.Domain = viewModel.Settings.Domain;
                Settings.AdminAccount = viewModel.Settings.AdminAccount;
                Settings.MinTeamNumberOfTeamMembers = viewModel.Settings.MinTeamNumberOfTeamMembers;
                Settings.MaxTeamNumberOfTeamMembers = viewModel.Settings.MaxTeamNumberOfTeamMembers;

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
                var allMatches = Dbh.GetCollection<Match>("Matches").FindAll().SetSortOrder(SortBy.Ascending("GameOverTime")).ToList();
                var allPlayers = Dbh.GetCollection<Player>("Players").FindAll();

                var temporaryCopyOfMatches = Dbh.GetCollection<Match>("CopyMatches");
                var temporaryCopyOfPlayers = Dbh.GetCollection<Player>("CopyPlayers");

                // Empty the Copies
                temporaryCopyOfMatches.RemoveAll();
                temporaryCopyOfPlayers.RemoveAll();

                // Reset all Players
                foreach (var player in allPlayers)
                {
                    player.Lost = 0;
                    player.Won = 0;
                    player.Played = 0;
                    player.Ratings = new Ratings();

                    temporaryCopyOfPlayers.Save(player);
                }

                /////////////////////////////////////////////////////////////////////////////////////////////////////////
                // replay each match in chronological order
                /////////////////////////////////////////////////////////////////////////////////////////////////////////
                foreach (var match in allMatches)
                {
                    var replayedMatch = match;
                    
                    // Update each player in the match with data from the database.
                    replayedMatch.GetMatchPlayers().ForEach(x => x = temporaryCopyOfPlayers.FindOne(Query.EQ("_id", BsonObjectId.Parse(x.Id))));

                    // Get the rating modifier
                    Rating.ModifyMatchRatings(ref replayedMatch);

                    // Save the data to Db
                    temporaryCopyOfMatches.Save(replayedMatch);
                }

                // Copy data into Production tables
                var matchesCollection = Dbh.GetCollection<Match>("Matches");
                var playersCollection = Dbh.GetCollection<Match>("Players");
                matchesCollection.RemoveAll();
                playersCollection.RemoveAll();

                foreach (var player in temporaryCopyOfPlayers.FindAll())
                {
                    playersCollection.Save(player);
                }

                foreach (var match in temporaryCopyOfMatches.FindAll())
                {
                    matchesCollection.Save(match);
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
