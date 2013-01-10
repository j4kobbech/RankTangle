namespace RankTangle.Models.ViewModels
{
    using RankTangle.Models.Base;

    public class LogOnViewModel : BaseViewModel
    {
        public LogOnViewModel()
        {
            this.LogOnError = false;
            this.RememberMe = true;
            this.Email = string.Empty;
        }
        
        public string Email { get; set; }
        
        public string Password { get; set; }
        
        public string RefUrl { get; set; }
        
        public bool RememberMe { get; set; }
        
        public bool LogOnError { get; set; }
    }
}