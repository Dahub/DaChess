using System;
using System.Web.Configuration;

namespace DaChess.Ui
{
    public class ConfigurationWrapper
    {
        private static volatile ConfigurationWrapper _instance;
        private static object _syncRoot = new Object();

        public string WebApiUrl
        {
            get
            {
                if (WebConfigurationManager.AppSettings["WebApiUrl"] == null || String.IsNullOrEmpty(WebConfigurationManager.AppSettings["WebApiUrl"].ToString()))
                    throw new Exception("Element WebApiUrl manquant ou vide");
                return WebConfigurationManager.AppSettings["WebApiUrl"].ToString();
            }
        }

        public static ConfigurationWrapper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new ConfigurationWrapper();
                    }
                }

                return _instance;
            }
        }
    }
}