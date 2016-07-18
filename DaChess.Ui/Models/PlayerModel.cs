namespace DaChess.Ui.Models
{
    public class PlayerModel
    {
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }

        public bool IsWhite { get; set; }
        public bool IsBlack { get; set; }
        public string PartyName { get; set; }
    }
}