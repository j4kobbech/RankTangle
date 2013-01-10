namespace RankTangle.Models.Domain
{
    using RankTangle.Models.Base;

    public class AutoLogin : MongoDocument
    {
        public string Email { get; set; }

        public string Token { get; set; }
    }
}