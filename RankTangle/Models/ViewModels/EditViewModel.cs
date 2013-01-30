namespace RankTangle.Models.ViewModels
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    using RankTangle.Models.Base;
    using RankTangle.Models.Domain;

    public class EditViewModel : BaseViewModel
    {
        public EditViewModel()
        {
            this.ReferralUrl = "/Players";

            var genderList = new List<SelectListItem>
                                 {
                                     new SelectListItem { Text = "Male", Value = "Male" },
                                     new SelectListItem { Text = "Female", Value = "Female" }
                                 };

            this.Genders = genderList;
        }

        public List<SelectListItem> Genders { get; set; }

        public bool SaveSuccess { get; set; }

        public bool FormIsInvalid { get; set; }

        public string ReferralUrl { get; set; }

        public Player Player { get; set; }

        public string NewPassword { get; set; }
    }
}