namespace RankTangle.Main
{
    using RestSharp;

    public static class Email
    {
        public static IRestResponse SendSimpleEmail() 
        {
            var client = new RestClient();
            var request = new RestRequest();
               
            client.BaseUrl = "https://api.mailgun.net/v2";
            client.Authenticator = new HttpBasicAuthenticator("api", "key-13nkt7ndx4pd7ys0hv4770blihhjv3k2");
            request.AddParameter("domain", "app13163.mailgun.org", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "RankTangle <info@ranktangle.com>");
            request.AddParameter("to", "j4kobbech@gmail.com");
            request.AddParameter("to", "j4kobbechspam@gmail.com");
            request.AddParameter("subject", "Hello from my MailGun");
            request.AddParameter("text", "Testing some Mailgun awesomness!");
            request.Method = Method.POST;

            return client.Execute(request);
        }
    }
}