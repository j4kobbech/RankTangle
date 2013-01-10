namespace RankTangle.Models.Domain
{
    using RankTangle.Models.Base;

    public class Event : MongoDocument
    {
        public Event()
        {
            this.Index = System.DateTime.Now.Ticks;
        }

        public long Index { get; set; }

        public string Action { get; set; }

        public string Type { get; set; }

        public Player Player { get; set; }

        public Match Match { get; set; }
    }
}