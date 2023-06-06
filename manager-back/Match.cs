using game.match;

namespace manager_back
{
    public class MatchRequestBody
    {
        public int Seed { get; set; }
        public string DeckList1 { get; set; }
        public bool P1IsBot{ get; set; }
        public string DeckList2 { get; set; }
        public bool P2IsBot{ get; set; }
    }

    public class MatchRecord
    {
        public string ID { get; }
        public int Seed { get; }
        public string Status { get; set; }
        public string Winner { get; set; } = "";
        public string TimeStart { get; }
        public string TimeEnd { get; set; } = "";

        public string ErrorMsg { get; set; } = "";

        public MatchRecord(Match match)
        {
            Guid g = Guid.NewGuid();
            
            ID = Convert.ToBase64String(g.ToByteArray());
            ID = ID.Replace("=","");
            ID = ID.Replace("+","");

            Seed = match.Config.Seed;
            TimeStart = DateTime.Now.ToString();
            Status = "IN PROGRESS";
        }
    }
}
