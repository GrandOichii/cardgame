namespace manager_back
{
    public class MatchRequestBody
    {
        public string Seed { get; set; }
        public string DeckList1 { get; set; }
        public bool P1IsBot{ get; set; }
        public string DeckList2 { get; set; }
        public bool P2IsBot{ get; set; }
    }

    public class MatchTrace
    {
        public string ID { get; set; }
        public int Seed { get; set; }
        public string Status { get; set; }
        public string Winner { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
    }
}
