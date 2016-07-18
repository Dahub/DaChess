using System;

namespace DaChess.Business
{
    public class Factory
    {
        private static volatile Factory _instance;
        private static object _syncRoot = new Object();

        private Factory() { }

        public IPartyManager GetPartyManager()
        {
            return new PartyManager();
        }

        public IPlayerManager GetPlayerManager()
        {
            return new PlayerManager();
        }

        public static Factory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new Factory();
                    }
                }

                return _instance;
            }
        }
    }
}
