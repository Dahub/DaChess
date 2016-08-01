namespace DaChessV2.Dto
{
    public class PartyModel : AbstractModel
    {
        public string Name { get; set; }
        public string Board { get; set; }
        public string History { get; set; }
        public string LastCase { get; set; }
        public EnumPartyState PartyState { get; set; }
        public EnumPlayerState WhitePlayerState { get; set; }
        public EnumPlayerState BlackPlayerState { get; set; }
    }
}

