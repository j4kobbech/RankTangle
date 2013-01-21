namespace RankTangle.Models.Domain
{
    using MongoDB.Bson.Serialization.Attributes;
    using RankTangle.Models.Base;

    [BsonIgnoreExtraElements]
    public class Config : MongoDocument
    {
        public Config()
        {
            Name = "RankTangle";
            Domain = "gmail.com";
            AdminAccount = "j4kobbech@gmail.com";
            EnableDomainValidation = true;
            MinTeamNumberOfTeamMembers = 1;
            MaxTeamNumberOfTeamMembers = 2;
            TeamALabel = "Yellow Team";
            TeamBLabel = "Brown Team";
        }

        public string Name { get; set; }

        public string Domain { get; set; }

        public string AdminAccount { get; set; } 

        public bool EnableDomainValidation { get; set; }

        public int MinTeamNumberOfTeamMembers { get; set; }
 
        public int MaxTeamNumberOfTeamMembers { get; set; }

        public Environment Environment { get; set; }

        public string TeamALabel { get; set; }

        public string TeamBLabel { get; set; }
    }
}