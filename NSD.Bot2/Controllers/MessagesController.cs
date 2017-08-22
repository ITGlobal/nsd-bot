using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace NSD.Bot2.Controllers
{
    [BotAuthentication]
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        public async Task Post([FromBody]Activity activity)
        {

            if (activity.Type == ActivityTypes.Message || activity.Type == ActivityTypes.ConversationUpdate)
            {
                await Conversation.SendAsync(activity, () => new Dialogs.IntroDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing that the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}