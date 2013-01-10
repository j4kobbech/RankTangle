namespace RankTangle.Models.ViewModels
{
    using System.Collections.Generic;

    using RankTangle.Models.Base;
    using RankTangle.Models.Custom;
    using RankTangle.Models.Domain;

    public class MatchesViewModel : BaseViewModel
    {
        public IEnumerable<Match> PlayedMatches { get; set; }

        public IEnumerable<Match> PendingMatches { get; set; }

        public IEnumerable<CustomSelectListItem> SelectPlayers { get; set; }
    }
}