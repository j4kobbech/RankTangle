namespace RankTangle.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using RankTangle.Models.Domain;
    using RankTangle.Models.ViewModels;

    using MongoDB.Driver.Builders;

    public class PlayersController : BaseController
    {
        public ActionResult Index()
        {
            var playerCollection = Dbh.GetCollection<Player>("Players")
                                        .FindAll()
                                        .SetSortOrder(SortBy.Descending("Rating"))
                                        .ToList();

            return this.View(new PlayersViewModel { AllPlayers = playerCollection });
        }
    }
}