namespace RankTangle.ControllerHelpers
{
    using System;

    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;

    using RankTangle.Main;
    using RankTangle.Models.Domain;

    public static class DbHelper
    {
        private static readonly MongoDatabase Dbh = new Db(AppConfig.GetEnvironment()).Dbh;

        public static Player GetPlayer(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new Exception("<id> parameter cannot be null or empty");
            }

            return Dbh.GetCollection<Player>("Players").FindOne(Query.EQ("_id", BsonObjectId.Parse(id)));
        }

        public static Player GetPlayerByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("<name> parameter cannot be null or empty");
            }

            return Dbh.GetCollection<Player>("Players").FindOne(Query.EQ("Name", name));
        }

        public static SafeModeResult SavePlayer(Player player)
        {
            if (player == null)
            {
                throw new Exception("<player> parameter cannot be null");
            }

            return Dbh.GetCollection<Player>("Players").Save(player);
        }
    }
}