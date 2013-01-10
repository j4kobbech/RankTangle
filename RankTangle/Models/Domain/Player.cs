﻿namespace RankTangle.Models.Domain
{
    using System;
    using RankTangle.Models.Base;
    using MongoDB.Bson; 
    using MongoDB.Bson.Serialization.Attributes;

    [BsonIgnoreExtraElements]
    public class Player : MongoDocument
    {
        public Player()
        {
            Rating = 1000;
            Deactivated = false;
            RememberMe = true;
            Won = 0;
            Lost = 0;
            Played = 0;
            Created = new BsonDateTime(DateTime.Now);
        }

        public Player(string id)
        {
            Id = id;
            Rating = 1000;
            Deactivated = false;
            RememberMe = true;
            Won = 0;
            Lost = 0;
            Played = 0;
            Created = new BsonDateTime(DateTime.Now);
        }

        public string Email { get; set; }

        public string Name { get; set; }
        
        public string NickName { get; set; }

        public string Gender { get; set; }

        public string Password { get; set; }
        
        public int Won { get; set; }
        
        public int Lost { get; set; }
        
        public int Played { get; set; }

        public double Rating { get; set; }

        public bool Deactivated { get; set; }
        
        public bool RememberMe { get; set; }

        public double Ratio
        {
            get
            {
                double ratio;

                if (this.Played == 0)
                {
                    ratio = 0;
                }
                else
                {
                    ratio = (this.Won / (double)this.Played) * 100;
                }

                return ratio;                
            }
        }
    }
}