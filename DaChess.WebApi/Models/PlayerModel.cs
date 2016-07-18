namespace DaChess.WebApi.Models
{
    public class PlayerModel : AbstractModel
    {
        public bool IsWhite { get; set; }
        public bool IsBlack { get; set; }
        public string PartyName { get; set; }
    }
}