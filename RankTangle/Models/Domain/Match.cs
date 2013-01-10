namespace RankTangle.Models.Domain
{
    using System.Collections.Generic;
    using RankTangle.Models.Base;
    using MongoDB.Bson;

    public class Match : MongoDocument
    {
        public int TeamAScore { get; set; }

        public int TeamBScore { get; set; }
        
        public Player TeamAPlayer1 { get; set; }
        
        public Player TeamAPlayer2 { get; set; }
        
        public Player TeamBPlayer1 { get; set; }
        
        public Player TeamBPlayer2 { get; set; }
        
        public BsonDateTime CreationTime { get; set; }
        
        public BsonDateTime GameOverTime { get; set; }

        public bool WonTheMatch(string id)
        {
            return (IsOnTeamA(id) && TeamAScore > TeamBScore) || (IsOnTeamB(id) && TeamBScore > TeamAScore);
        }

        public bool ContainsPlayer(string id)
        {
            return id == TeamAPlayer1.Id || id == TeamAPlayer2.Id || id == TeamBPlayer1.Id || id == TeamBPlayer2.Id;
        }

        public bool IsOnTeamA(string id)
        {
            return id == TeamAPlayer1.Id || id == TeamAPlayer2.Id;
        }

        public bool IsOnTeamB(string id)
        {
            return id == TeamBPlayer1.Id || id == TeamBPlayer2.Id;
        }

        public Player GetTeamMate(string id)
        {
            if (id == TeamAPlayer1.Id)
            {
                return TeamAPlayer2;
            }

            if (id == TeamAPlayer2.Id)
            {
                return TeamAPlayer1;
            }
            
            if (id == TeamBPlayer1.Id)
            {
                return TeamBPlayer2;
            }
            
            if (id == TeamBPlayer2.Id)
            {
                return TeamBPlayer1;
            }
            
            return null;
        }

        public int CountTeamAPlayers()
        {
            return (TeamAPlayer2.Id == null) ? 1 : 2;
        }
        
        public int CountTeamBPlayers()
        {
            return (TeamBPlayer2.Id == null) ? 1 : 2;
        }
        
        public double GetTeamARating()
        {
            return (TeamAPlayer2.Id == null) ? TeamAPlayer1.Rating : (TeamAPlayer1.Rating + TeamAPlayer2.Rating);
        }

        public double GetTeamBRating()
        {
            return (TeamBPlayer2.Id == null) ? TeamBPlayer1.Rating : (TeamBPlayer1.Rating + TeamBPlayer2.Rating);
        }

        public Player GetPlayer(string id)
        {
            if (id == TeamAPlayer1.Id)
            {
                return TeamAPlayer1;
            }

            if (id == TeamAPlayer2.Id)
            {
                return TeamAPlayer2;
            }

            if (id == TeamBPlayer1.Id)
            {
                return TeamBPlayer1;
            }

            return id == this.TeamBPlayer2.Id ? this.TeamBPlayer2 : null;
        }

        public List<Player> GetPlayers()
        {
            var listOfPlayers = new List<Player> { this.TeamAPlayer1 };
            if (this.CountTeamAPlayers() == 2)
            {
                listOfPlayers.Add(this.TeamAPlayer2);
            }

            listOfPlayers.Add(this.TeamBPlayer1);
            if (this.CountTeamBPlayers() == 2)
            {
                listOfPlayers.Add(this.TeamBPlayer2);
            }

            return listOfPlayers;
        }

        public List<Player> GetWinners()
        {
            var listOfWinners = new List<Player>();

            if (this.WonTheMatch(this.TeamAPlayer1.Id))
            {
                listOfWinners.Add(this.TeamAPlayer1);
                if (this.CountTeamAPlayers() == 2)
                {
                    listOfWinners.Add(this.TeamAPlayer2);
                }
            }
            else
            {
                listOfWinners.Add(this.TeamBPlayer1);
                if (this.CountTeamBPlayers() == 2)
                {
                    listOfWinners.Add(this.TeamBPlayer2);
                }
            }

            return listOfWinners;
        }

        public List<Player> GetLosers()
        {
            var listOfLosers = new List<Player>();

            if (!this.WonTheMatch(this.TeamAPlayer1.Id))
            {
                listOfLosers.Add(this.TeamAPlayer1);
                if (this.CountTeamAPlayers() == 2)
                {
                    listOfLosers.Add(this.TeamAPlayer2);
                }
            }
            else
            {
                listOfLosers.Add(this.TeamBPlayer1);
                if (this.CountTeamBPlayers() == 2)
                {
                    listOfLosers.Add(this.TeamBPlayer2);
                }
            }

            return listOfLosers;
        } 
    }
}   
