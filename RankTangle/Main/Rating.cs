namespace RankTangle.Main
{
    using System;

    public static class Rating
    {
        private const int KModifier = 35;

        public static double GetRatingModifier(double winnerRating, double loserRating)
        {
            var winnerExpectedScore = 1 / (1 + Math.Pow(10, (loserRating - winnerRating) / 400));
            return KModifier * (1 - winnerExpectedScore);
        }
    }
}