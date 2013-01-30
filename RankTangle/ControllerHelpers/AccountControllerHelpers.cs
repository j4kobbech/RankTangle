namespace RankTangle.ControllerHelpers
{
    using System;
    using System.Web;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;

    using RankTangle.Main;
    using RankTangle.Models.Domain;
    using RankTangle.Models.ViewModels;

    public class AccountControllerHelpers
    {
        private static readonly MongoDatabase Dbh = new Db(AppConfig.GetEnvironment()).Dbh;

        public static string GetAuthToken(Player player)
        {
            return Md5.CalculateMd5(player.Id + player.Email + "RankTangle4Ever");
        }

        // COOKIE DOUGH
        public static void CreateRememberMeCookie(Player player, HttpContextBase httpContext)
        {
            httpContext.Response.Cookies.Add(new HttpCookie("RankTangleAuth"));
            var httpCookie = httpContext.Response.Cookies["RankTangleAuth"];
            if (httpCookie != null)
            {
                httpCookie["Token"] = GetAuthToken(player);
            }

            if (httpCookie != null)
            {
                httpCookie.Expires = DateTime.Now.AddDays(30);
            }
        }

        public static void RemoveRememberMeCookie(HttpContextBase httpContext)
        {
            httpContext.Response.Cookies.Add(new HttpCookie("RankTangleAuth"));
            var httpCookie = httpContext.Response.Cookies["RankTangleAuth"];
            if (httpCookie != null)
            {
                httpCookie.Expires = DateTime.Now.AddDays(-1);
            }
        }

        public static RegisterViewModel ValidateRegisterViewModel(RegisterViewModel viewModel)
        {
            var player = viewModel.Player;
            viewModel.ListOfErrorMessages.Clear();

            if (string.IsNullOrEmpty(player.Email) || string.IsNullOrEmpty(player.Name)
                    || string.IsNullOrEmpty(player.NickName) || string.IsNullOrEmpty(player.Password))
            {
                viewModel.ListOfErrorMessages.Add("All fields are required to register.");
            } 
            
            if (!string.IsNullOrEmpty(viewModel.RepeatPassword) && (viewModel.RepeatPassword != player.Password))
            {
                viewModel.ListOfErrorMessages.Add("Your passwords do not match.");
            }

            if (PlayerEmailAlreadyInUse(player))
            {
                viewModel.ListOfErrorMessages.Add("A player with this name already exists");                
            }

            return viewModel;
        }

        private static bool PlayerEmailAlreadyInUse(Player player)
        {
            var resultCount = Dbh.GetCollection<Player>("Players").Find(Query.EQ("Email", player.Email.ToLower())).Count();

            return resultCount > 0;
        }
    }
}