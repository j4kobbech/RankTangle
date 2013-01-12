namespace RankTangle.ViewHelpers
{
    using System.Web.Mvc;

    using RankTangle.Main;

    public static class Md5Extensions
    {
        public static string GetGravatarEmailHash(this HtmlHelper helper, string email)
        {
            var gravatarUrl = string.IsNullOrEmpty(email) 
                    ? "http://www.gravatar.com/avatar/fbbf176095d4cb476b84bb4188a26ab1?d=mm" 
                    : string.Format("http://www.gravatar.com/avatar/{0}", Md5.CalculateMd5(email.ToLower().Trim()));

            return gravatarUrl.ToLower();
        }
    }
}