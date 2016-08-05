namespace DaChessV2.Dto
{
    public class MoveModel : AbstractModel
    {
        public string FirstCase { get; set; }
        public string SecondCase { get; set; }
        public bool Promote { get; set; }
        public string PromotePiece { get; set; }
        public string PartyName { get; set; }
        public string Token { get; set; }
    }
}
