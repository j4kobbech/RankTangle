namespace RankTangle.Models.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    
    public class Team
    {
        public Team()
        {
            this.TeamPlayers = new List<Player>();
        }
        
        public List<Player> TeamPlayers { get; set; }

        public string TeamPlayersSignature 
        { 
            get
            {
                return this.TeamPlayers.OrderBy(x => x.Id).Select(x => x.Id).ToString();
            }
        }

        public Ratings GetTeamRatings
        {
            get
            {
                return new Ratings
                           {
                               OverAll = this.TeamPlayers.Sum(player => player.Ratings.OverAll),
                               Single = this.TeamPlayers.Sum(player => player.Ratings.Single),
                               Double = this.TeamPlayers.Sum(player => player.Ratings.Double)
                           };
            }
        }

        public bool IsPlayerOnTeam(string id)
        {
            return this.TeamPlayers.Exists(x => x.Id == id);
        }

        public List<Player> GetTeamMates(string id)
        {
            return this.TeamPlayers.FindAll(x => x.Id != id);
        }

        public Player GetTeamPlayer(string id)
        {
            return this.TeamPlayers.Find(x => x.Id == id);
        }

        public void AddWin()
        {
            this.TeamPlayers.ForEach(x =>
                {
                    x.Won++;
                    x.Played++;
                });
        }

        public void AddLoss()
        {
            this.TeamPlayers.ForEach(x =>
                {
                    x.Lost++;
                    x.Played++;
                });
        }
    }
}