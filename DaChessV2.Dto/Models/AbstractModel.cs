namespace DaChessV2.Dto
{
    public abstract class AbstractModel
    {
        private string _errorDetails;

        public bool IsError { get; set; }
        public string ErrorMsg { get; set; }
        public string ErrorDetails
        {
            // On ne retourne les détails de l'erreur qu'en mode DEBUG
            get
            {
#if DEBUG
                return base.ToString();
#else
                return _errorDetails;
#endif
            }
            set
            {
                _errorDetails = value;
            }
        }
        public string ResultText { get; set; }
    }
}
