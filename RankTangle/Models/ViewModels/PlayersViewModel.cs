namespace RankTangle.Models.ViewModels
{
    using System.Collections.Generic;
    using RankTangle.Models.Base;
    using RankTangle.Models.Domain;

    public class PlayersViewModel : BaseViewModel
    {
        public IEnumerable<Player> AllPlayers { get; set; }
    }
}