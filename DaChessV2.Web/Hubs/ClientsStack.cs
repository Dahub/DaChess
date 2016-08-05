using DaChessV2.Dto;
using System.Collections.Generic;

namespace DaChessV2.Web.Hubs
{
    public static class ClientsStack
    {
        private readonly static IDictionary<string, ClientInfo> _clients =
            new Dictionary<string, ClientInfo>();

        public static void Add(string connectionId, string partyName, bool isPlayer, Color? color)
        {
            Add(connectionId, new ClientInfo()
            {
                Color = color,
                IsPlayer = isPlayer,
                PartyName = partyName
            });
        }

        public static void Add(string connectionId, ClientInfo infos)
        {
            lock (_clients)
            {
                if (!_clients.ContainsKey(connectionId))
                {
                    _clients.Add(connectionId, infos);
                }
            }
        }

        public static void Update(string connectionId, string partyName, bool isPlayer, Color? color)
        {
            Update(connectionId, new ClientInfo()
            {
                Color = color,
                IsPlayer = isPlayer,
                PartyName = partyName
            });
        }

        public static void Update(string connectionId, ClientInfo infos)
        {
            lock (_clients)
            {
                if (!_clients.ContainsKey(connectionId))
                {
                    _clients.Add(connectionId, infos);
                }
                else
                {
                    _clients[connectionId] = infos;
                }
            }
        }

        public static void Remove(string connectionId)
        {
            lock (_clients)
            {
                if (_clients.ContainsKey(connectionId))
                {
                    _clients.Remove(connectionId);
                }
            }
        }

        public static ClientInfo Get(string connectionId)
        {
            ClientInfo toReturn = new ClientInfo();

            lock (_clients)
            {
                if (_clients.ContainsKey(connectionId))
                {
                    toReturn = _clients[connectionId];
                }
            }

            return toReturn;
        }
    }
}