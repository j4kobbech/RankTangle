namespace RankTangle.Models.ViewModels
{
    using RankTangle.Models.Base;
    using RankTangle.Models.Custom;
    using RankTangle.Models.Domain;

    public class StatsAggregateViewModel : BaseViewModel
    {
        public long MatchCount { get; set; }

        public Player MostFights { get; set; }

        public Player MostWins { get; set; }

        public Player MostLosses { get; set; }

        public Player TopRanked { get; set; }

        public Player BottomRanked { get; set; }

        public Player HighestRatingEver { get; set; }

        public Player LowestRatingEver { get; set; }

        public Streak LongestWinningStreak { get; set; }

        public Streak LongestLosingStreak { get; set; }

        public RatingDifference BiggestRatingWin { get; set; }
    }
}
