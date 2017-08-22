using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace NSD.Bot2.Dialogs
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
            var activity = (Activity)await result;

            if (!state.TryGetValue(activity.Conversation.Id, out bool sentGreeting))
            {
                state[activity.Conversation.Id] = true;
                await context.PostAsync(embeddedAnswers.intro);
                context.Done<object>(null);
            }
            else if (activity.Type == ActivityTypes.Message)
            {
                await context.Forward(new RootDialog(), async (dialogContext, res) => dialogContext.Done(await res), activity, CancellationToken.None);
            }
        }
    }
}