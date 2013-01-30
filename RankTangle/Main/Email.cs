namespace RankTangle.Main
{
    using System.Collections.Generic;
    using System.Configuration;
    using RestSharp;

    public static class Email
    {
        public static IRestResponse SendSimpleEmail(List<string> emailTo, string subject = "", string textBody = "", string emailFrom = "j4kobbech@gmail.com")
        {
            var client = new RestClient();
            var request = new RestRequest();
            var mailGunApiKey = ConfigurationManager.AppSettings["MAILGUN_API_KEY"];
            var mailGunAccount = ConfigurationManager.AppSettings["MAILGUN_SMTP_LOGIN"];

            client.BaseUrl = "https://api.mailgun.net/v2";
            client.Authenticator = new HttpBasicAuthenticator("api", mailGunApiKey);
            request.AddParameter("domain", mailGunAccount, ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", emailFrom);
            emailTo.ForEach(x => request.AddParameter("to", x));
            request.AddParameter("subject", subject);
            request.AddParameter("text", textBody);
            request.Method = Method.POST;

            return client.Execute(request);
        }

        public static IRestResponse SendSimpleEmail(string emailTo, string subject = "", string textBody = "", string emailFrom = "j4kobbech@gmail.com")
        {
            var client = new RestClient();
            var request = new RestRequest();
            var mailGunApiKey = ConfigurationManager.AppSettings["MAILGUN_API_KEY"];
            var mailGunAccount = ConfigurationManager.AppSettings["MAILGUN_SMTP_LOGIN"];

            client.BaseUrl = "https://api.mailgun.net/v2";
            client.Authenticator = new HttpBasicAuthenticator("api", mailGunApiKey);
            request.AddParameter("domain", mailGunAccount, ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", emailFrom);
            request.AddParameter("to", emailTo);    
            request.AddParameter("subject", subject);
            request.AddParameter("text", textBody);
            request.Method = Method.POST;

            return client.Execute(request);
        }
    }
}