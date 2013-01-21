namespace RankTangle.Models.Custom
{
    using System.Collections.Generic;
    using System.Linq;

    using RankTangle.Models.Domain;

    public class Streaks
    {
        public Streaks()
        {
            this.AggregatedStreaks = new Dictionary<string, int>();
            this.FinalStreaks = new Dictionary<string, int>();
        }

        public Dictionary<string, int> AggregatedStreaks { get; set; }

        public Dictionary<string, int> FinalStreaks { get; set; }

        public void Add(string playerId)
        {
            if (!string.IsNullOrEmpty(playerId))
            {
                if (this.AggregatedStreaks.ContainsKey(playerId))
                {
                    this.AggregatedStreaks[playerId]++;
                }
                else
                {
                    this.AggregatedStreaks.Add(playerId, 1);
                }
            }   
        }

        public void Reset(string playerId)
        {
            if (!string.IsNullOrEmpty(playerId))
            {
                if (this.AggregatedStreaks.ContainsKey(playerId))
                {
                    if (this.FinalStreaks.ContainsKey(playerId))
                    {
                        if (this.FinalStreaks[playerId] < this.AggregatedStreaks[playerId]) 
                        {
                            this.FinalStreaks[playerId] = this.AggregatedStreaks[playerId];
                        }
                    }
                    else
                    {
                        this.FinalStreaks.Add(playerId, this.AggregatedStreaks[playerId]);
                    }

                    this.AggregatedStreaks[playerId] = 0;
                }
            }
        }

        public Streak GetLongestStreak()
        {
            var finalStreaksId = this.FinalStreaks.OrderByDescending(x => x.Value).First().Key;
            var aggrStreaksId = this.AggregatedStreaks.OrderByDescending(x => x.Value).First().Key;

            if (this.FinalStreaks.ContainsKey(finalStreaksId))
            {
                if (this.FinalStreaks[finalStreaksId] > this.AggregatedStreaks[aggrStreaksId])
                {
                    return new Streak { Player = new Player(finalStreaksId), StreakCount = this.FinalStreaks[finalStreaksId] };
                }
            }

            return new Streak { Player = new Player(aggrStreaksId), StreakCount = this.AggregatedStreaks[aggrStreaksId] };
        }
    }
}