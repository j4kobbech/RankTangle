namespace RankTangle.ControllerHelpers
{
    using System;
    using System.Web;
    using MongoDB.Driver;
    using RankTangle.Main;
    using RankTangle.Models.Domain;

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
    }
}