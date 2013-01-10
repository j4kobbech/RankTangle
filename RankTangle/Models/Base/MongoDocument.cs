namespace RankTangle.Models.Base
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public abstract class MongoDocument
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public BsonDateTime Created { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatedBy { get; set; }

        public BsonDateTime Updated { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UpdatedBy { get; set; }
    }
}