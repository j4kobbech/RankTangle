namespace RankTangle.ControllerHelpers
{
    using System;

    using RankTangle.Main;
    using RankTangle.Models.Domain;

    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;

    public static class DbHelper
    {
        private static readonly MongoDatabase Dbh = new Db(AppConfig.GetEnvironment()).Dbh;

        public static Player GetPlayer(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new Exception("id parameter cannot be null or empty");
            }

            return Dbh.GetCollection<Player>("Players").FindOne(Query.EQ("_id", BsonObjectId.Parse(id)));
        }

        public static SafeModeResult SavePlayer(Player player)
        {
            if (player == null)
            {
                throw new Exception("player parameter cannot be null");
            }

            return Dbh.GetCollection<Player>("Players").Save(player);
        }
    }
}