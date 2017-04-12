using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Rest;

namespace NSD.Bot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            var response = Request.CreateResponse(HttpStatusCode.OK);
            try
            {
                switch (activity.Type)
                {
                    case ActivityTypes.ConversationUpdate:
                    case ActivityTypes.Message:
                    {
                        await Conversation.SendAsync(activity, () => new Dialogs.IntroDialog());
                        break;
                    }
                }
                return response;
            }
            catch (HttpOperationException)
            {
                // ignore
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                throw;
            }
            return response;
        }
    }
}