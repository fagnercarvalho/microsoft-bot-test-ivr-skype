using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;

namespace Test_IVR_Skype
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
            if (activity.Type == ActivityTypes.Message)
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                string replyMessage;
                if (activity.Text.Trim() == "1 + 1")
                {
                    replyMessage = "2";
                } else
                {
                    replyMessage = "Unknown command. Please type something else.";
                }

                var reply = activity.CreateReply(replyMessage);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
    }
}