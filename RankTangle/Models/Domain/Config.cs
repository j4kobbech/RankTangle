namespace RankTangle.Models.Domain
{
    using MongoDB.Bson.Serialization.Attributes;
    using RankTangle.Models.Base;

    [BsonIgnoreExtraElements]
    public class Config : MongoDocument
    {
        public Config()
        {
            this.Name = "RankTangle";
            this.Domain = "gmail.com";
            this.AdminAccount = "j4kobbech@gmail.com";
            this.EnableDomainValidation = true;
            this.EnableOneOnOneMatches = true;
            this.EnableGenderSpecificMatches = false;
        }

        public string Name { get; set; }

        public string Domain { get; set; }

        public string AdminAccount { get; set; } 

        public bool EnableDomainValidation { get; set; }

        public bool EnableOneOnOneMatches { get; set; }

        public bool EnableGenderSpecificMatches { get; set; }

        public Environment Environment { get; set; }
    }
}