namespace RankTangle.Main
{
    using System;

    using RankTangle.Models.Domain;

    using MongoDB.Bson;

    public static class Events
    {
        public static void SubmitEvent(string action, string type, object targetObject, string userId) 
        {
            SaveEvent(action, type, targetObject, userId);
        }

        private static void SaveEvent(string action, string type, object targetObject, string userId)
        {
            if (string.IsNullOrEmpty(action) == false && string.IsNullOrEmpty(type) == false && targetObject != null)
            {
                var dbh = new Db(AppConfig.GetEnvironment()).Dbh;
                var eventCollection = dbh.GetCollection<Event>("Events");

                var newEvent = new Event
                {
                    Action = action,
                    Type = type,
                    Created = new BsonDateTime(DateTime.Now),
                    CreatedBy = userId
                };

                if (type.ToLower() == "player")
                {
                    newEvent.Player = (Player)targetObject;
                }

                if (type.ToLower() == "match")
                {
                    newEvent.Match = (Match)targetObject;
                }

                eventCollection.Insert(newEvent);
            }
        }
    }
}