namespace RankTangle.Models.ViewModels
{
    using System.Collections.Generic;

    using RankTangle.Models.Base;
    using RankTangle.Models.Domain;

    public class PlayerStatsViewModel : BaseViewModel
    {
        public Player Player { get; set; }

        public int Played { get; set; }

        public int Won { get; set; }

        public int Lost { get; set; }

        public int Ranking { get; set; }

        public int WinningStreakMatches { get; set; }

        public double WinningStreakPoints { get; set; }

        public int LosingStreakMatches { get; set; }

        public double LosingStreakPoints { get; set; }

        public BestFriendForever Bff { get; set; }

        public RealBestFriendForever Rbff { get; set; }

        public EvilArchEnemy Eae { get; set; }

        public PreferredColor PreferredColor { get; set; }

        public WinningColor WinningColor { get; set; }

        public Match LatestMatch { get; set; }

        public int TotalNumberOfPlayers { get; set; }

        public IEnumerable<Match> PlayedMatches { get; set; }
    }
}