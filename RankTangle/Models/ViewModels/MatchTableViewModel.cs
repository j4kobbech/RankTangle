namespace RankTangle.Models.ViewModels
{
    using System.Collections.Generic;

    using RankTangle.Models.Domain;

    public class MatchTableViewModel
    {
        public IEnumerable<Match> Matches { get; set; }

        public Player User { get; set; }
    }
}