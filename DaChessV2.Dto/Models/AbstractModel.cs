namespace DaChessV2.Dto
{
    public abstract class AbstractModel
    {
        public bool IsError { get; set; }
        public string ErrorMsg { get; set; }
        public string ErrorDetails { get; set; }
        public string ResultText { get; set; }
    }
}
