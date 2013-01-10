namespace RankTangle.Models
{
    using System.Collections.Generic;

    using RankTangle.Models.Domain;

    public class BestFriendForever
    {
        public BestFriendForever()
        {
            this.Occurrences = 1;
        }

        public Player Player { get; set; }

        public int Occurrences { get; set; }
    }

    public class RealBestFriendForever
    {
        public RealBestFriendForever()
        {
            this.Occurrences = 1;
        }

        public Player Player { get; set; }

        public int Occurrences { get; set; }
    }

    public class EvilArchEnemy
    {
        public EvilArchEnemy()
        {
            this.Occurrences = 1;
            this.GoalDiff = 0;
        }

        public Player Player { get; set; }

        public int GoalDiff { get; set; }

        public int Occurrences { get; set; }
    }

    public class PreferredColor
    {
        public string Color { get; set; }

        public int Occurrences { get; set; }
    }

    public class WinningColor
    {
        public string Color { get; set; }

        public int Occurrences { get; set; }
    }

    public class PlayerRatingChartDataPoint
    {
        public List<string> TimeSet { get; set; }

        public double Rating { get; set; }
    }

    public class PlayerRatingChartData
    {
        public PlayerRatingChartData()
        {
            this.DataPoints = new List<PlayerRatingChartDataPoint>();
        }

        public List<PlayerRatingChartDataPoint> DataPoints { get; set; }

        public double MinimumValue { get; set; }

        public double MaximumValue { get; set; }
    }


}