namespace RankTangle.Controllers
{
    using System.Web.Mvc;
    using MongoDB.Driver;
    using RankTangle.Main;
    using RankTangle.Models.Domain;

    public class BaseController : Controller
    {
        protected readonly MongoDatabase Dbh;

        public BaseController()
        {
            var environment = AppConfig.GetEnvironment();
            this.Settings = new Config();

            this.Dbh = new Db(environment).Dbh;
            this.Settings.Environment = environment;

            ViewBag.Settings = this.Settings;
        }

        protected Config Settings { get; set; }
    }
}
