namespace RankTangle.Models.Domain
{
    public class Ratings
    {
        public Ratings()
        {
            this.Single = 1000;
            this.Double = 1000;
            this.All = 1000;
        }

        public double Single { get; set; }

        public double Double { get; set; }

        public double All { get; set; }
    }
}