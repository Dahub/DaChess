namespace DaChessV2.Dto
{
    public class PlayerModel : AbstractModel
    {
        public string PartyName { get; set; }
        public Color PlayerColor { get; set; }
        public string Token { get; set; }
    }
}
