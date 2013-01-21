namespace RankTangle.Main
{
    using System;

    using RankTangle.Models.Domain;

    public static class Rating
    {
        private const double KModifier = 35;

        public static double GetRatingModifier(double winnerRating, double loserRating)
        {
            var winnerExpectedScore = ExpectedScore(winnerRating, loserRating);
            return KModifier * (1 - winnerExpectedScore);
        }

        public static void ModifyMatchRatings(ref Match match)
        {
            var winningTeam = match.GetWinningTeam();
            var losingTeam = match.GetLosingTeam();
            var overAllModifer = GetRatingModifier(winningTeam.GetTeamRatings.OverAll, losingTeam.GetTeamRatings.OverAll);
            winningTeam.TeamPlayers.ForEach(x => x.Ratings.OverAll += overAllModifer);
            losingTeam.TeamPlayers.ForEach(x => x.Ratings.OverAll -= overAllModifer);

            if (match.Type == MatchType.Single)
            {
                var modifier = GetRatingModifier(winningTeam.GetTeamRatings.Single, losingTeam.GetTeamRatings.Single);
                winningTeam.TeamPlayers.ForEach(x => x.Ratings.Single += modifier);
                losingTeam.TeamPlayers.ForEach(x => x.Ratings.Single -= modifier);
            }

            if (match.Type == MatchType.Double)
            {
                var modifier = GetRatingModifier(winningTeam.GetTeamRatings.Double, losingTeam.GetTeamRatings.Double);
                winningTeam.TeamPlayers.ForEach(x => x.Ratings.Double += modifier);
                losingTeam.TeamPlayers.ForEach(x => x.Ratings.Double -= modifier);
            }
        }

        private static double ExpectedScore(double winnerRating, double loserRating)
        {
            return 1 / (1 + Math.Pow(10, (loserRating - winnerRating) / 400));
        }
    }
}