namespace RankTangle.Models.Custom
{
    using RankTangle.Models.Domain;

    public class Streak
    {
        public Player Player { get; set; }

        public int Count { get; set; }
    }
}