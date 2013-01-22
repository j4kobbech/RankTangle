namespace RankTangle.Models.ViewModels
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    using RankTangle.Models.Base;
    using RankTangle.Models.Domain;

    public class PlayerBaseDataViewModel : BaseViewModel
    {
        public PlayerBaseDataViewModel()
        {
            var genderList = new List<SelectListItem>
                                 {
                                     new SelectListItem { Text = "Male", Value = "Male" },
                                     new SelectListItem { Text = "Female", Value = "Female" }
                                 };

            this.Genders = genderList;
            this.ReferralUrl = "/Players";
        }

        public Player Player { get; set; }

        public string RepeatPassword { get; set; }

        public bool SaveSuccess { get; set; }

        public string ReferralUrl { get; set; }

        public List<SelectListItem> Genders { get; set; }
    }
}
