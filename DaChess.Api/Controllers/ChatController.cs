using DaChess.Api.Models;
using System;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.Net;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.Web.Http.Cors;

namespace DaChess.Api.Controllers
{
    // http://www.strathweb.com/2012/05/native-html5-push-notifications-with-asp-net-web-api-and-knockout-js/
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ChatController : ApiController
    {
        private static readonly ConcurrentQueue<StreamWriter> _streammessage = new ConcurrentQueue<StreamWriter>();

        public HttpResponseMessage Get(HttpRequestMessage request)
        {
            HttpResponseMessage response = request.CreateResponse();
            response.Content = new PushStreamContent(
                (Action<Stream, HttpContent, TransportContext>)OnStreamAvailable, "text/event-stream");

            return response;
        }

        public void Post(Message m)
        {
            m.Dt = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            MessageCallback(m);
        }

        public static void OnStreamAvailable(Stream stream, HttpContent headers, TransportContext context)
        {
            
            StreamWriter streamwriter = new StreamWriter(stream);
            _streammessage.Enqueue(streamwriter);
        }

        private static void MessageCallback(Message m)
        {
            foreach (var subscriber in _streammessage)
            {
                subscriber.WriteLine("data:" + JsonConvert.SerializeObject(m) + "\n");
                subscriber.Flush();
            }
        }
    }
}