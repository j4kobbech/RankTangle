namespace RankTangle.Models.ViewModels
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    using RankTangle.Models.Base;

    public class ConfigViewModel : BaseViewModel
    {
        public List<SelectListItem> Users { get; set; } 
    }
}