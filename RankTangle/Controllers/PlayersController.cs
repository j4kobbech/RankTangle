namespace RankTangle.Controllers
{
    using System.Linq;
    using System.Web.Mvc;
    using MongoDB.Driver.Builders;
    using RankTangle.Models.Domain;
    using RankTangle.Models.ViewModels;

    public class PlayersController : BaseController
    {
        public ActionResult Index()
        {
            var playerCollection = Dbh.GetCollection<Player>("Players")
                                        .FindAll()
                                        .SetSortOrder(SortBy.Descending("Ratings.OverAll"))
                                        .ToList();

            return this.View(new PlayersViewModel { AllPlayers = playerCollection, Settings = this.Settings});
        }
    }
}