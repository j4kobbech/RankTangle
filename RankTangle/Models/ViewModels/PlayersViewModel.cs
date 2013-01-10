namespace RankTangle.Models.ViewModels
{
    using System.Collections.Generic;

    using RankTangle.Models.Domain;
    using RankTangle.Models.Base;

    public class PlayersViewModel : BaseViewModel
    {
        public IEnumerable<Player> AllPlayers { get; set; }
    }
}