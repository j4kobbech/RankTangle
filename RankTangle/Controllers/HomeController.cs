namespace RankTangle.Controllers
{
    using System.Web.Mvc;

    using RankTangle.Models.ViewModels;

    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var viewModel = new HomeViewModel { Settings = Settings };
            return View(viewModel);
        }

        public ActionResult Features()
        {
            return View(new FeaturesViewModel { Settings = this.Settings });
        }
    }
}