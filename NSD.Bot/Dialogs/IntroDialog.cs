using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Rest;

namespace NSD.Bot.Dialogs
{
    [Serializable]
    public class IntroDialog : IDialog<object>
    {
        private static readonly IDictionary<string, bool> state = new Dictionary<string, bool>();

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(Resume);
            return Task.CompletedTask;
        }

        private async Task Resume(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = (Activity)(await result);

            bool sentGreeting;
            if (!state.TryGetValue(activity.Conversation.Id, out sentGreeting))
            {
                state[activity.Conversation.Id] = true;
                await context.PostAsync(embeddedAnswers.intro);
                context.Call(new RootDialog(), async (dialogContext, res) => dialogContext.Done(await res));
            }
            else
            {
                await context.Forward(new RootDialog(), async (dialogContext, res) => dialogContext.Done(await res), activity, CancellationToken.None);
            }
        }
    }
}