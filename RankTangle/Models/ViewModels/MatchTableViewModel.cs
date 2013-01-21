namespace RankTangle.Models.ViewModels
{
    using System.Collections.Generic;

    using RankTangle.Models.Base;
    using RankTangle.Models.Domain;

    public class MatchTableViewModel : BaseViewModel
    {
        public IEnumerable<Match> Matches { get; set; }

        public Player User { get; set; }
    }
}