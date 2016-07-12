using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Linq;
using System.Web.Http.Cors;
using System.Collections.Generic;
using DaChess.Api.Models;
using Newtonsoft.Json;
using DaChess.Business;

namespace DaChess.Api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class PlayerController : ApiController
    {
        private static readonly IDictionary<string, ConcurrentQueue<StreamWriter>> _subscribers = new Dictionary<string, ConcurrentQueue<StreamWriter>>();

        public HttpResponseMessage Get(HttpRequestMessage request)
        {
            string partyName = request.GetQueryNameValuePairs().Where(k => k.Key.ToLower().Equals("name")).First().Value;

            HttpResponseMessage response = request.CreateResponse();

            response.Content = new PushStreamContent((stream, httpContent, context) =>
            {
                OnStreamAvailable(stream, httpContent, context, partyName);
            }, "text/event-stream");

            return response;
        }

        public void Post(PartyModel party)
        {
            if (party.WhiteAskToPlay && String.IsNullOrEmpty(party.WhiteToken))
            {
                var myParty = Factory.Instance.GetPartyManager().AddPlayerToParty(party.Id, Colors.WHITE);
                party.WhiteToken = myParty.WhiteLink;
            }
            else if (party.BlackAskToPlay && String.IsNullOrEmpty(party.BlackToken))
            {
                var myParty = Factory.Instance.GetPartyManager().AddPlayerToParty(party.Id, Colors.BLACK);
                party.BlackToken = myParty.BlackLink;
            }

            MessageCallback(party);
        }

        public static void OnStreamAvailable(Stream stream, HttpContent headers, TransportContext context, string partyName)
        {
            StreamWriter streamwriter = new StreamWriter(stream);
            if (_subscribers.ContainsKey(partyName))
            {
                _subscribers[partyName].Enqueue(streamwriter);
            }
            else
            {
                var toAdd = new ConcurrentQueue<StreamWriter>();
                toAdd.Enqueue(streamwriter);
                _subscribers.Add(partyName, toAdd);
            }
        }

        private static void MessageCallback(PartyModel m)
        {
            if (_subscribers.ContainsKey(m.Name))
            {
                foreach (var s in _subscribers[m.Name])
                {
                    try
                    {
                        s.WriteLine("data:" + JsonConvert.SerializeObject(m) + Environment.NewLine);
                        s.Flush();
                    }
                    catch // si exception, on n'arrête pas
                    {

                    }
                }
            }
        }
    }
}
