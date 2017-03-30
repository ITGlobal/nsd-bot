using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using NSD.Bot.StateMachine;

namespace NSD.Bot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public XMLStateMachine Sm { get; } = new XMLStateMachine
        {
            StateTable = System.Web.Hosting.HostingEnvironment.MapPath("~/faq.xml"),
            CurrentState = "start"
        };

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            Trace.TraceInformation(activity.Text);
            await context.PostAsync("Привет!");
            ShowPromt(context, Sm.GetPrompt());
        }

        public async Task AfterInput(IDialogContext context, IAwaitable<Option> argument)
        {
            Sm.Next((await argument).Key);
            if (string.IsNullOrEmpty(Sm.Answer))
            {
                ShowPromt(context, Sm.GetPrompt());
                return;
            }
            await context.PostAsync(Sm.Answer);
        }

        private void ShowPromt(IDialogContext context, Prompt prompt)
        {
            PromptDialog.Choice(context, AfterInput,
                new PromptOptions<Option>(
                    prompt.Question,
                    options: prompt.Options,
                    promptStyler: new PromptStyler(PromptStyle.Keyboard)
                )
            );
        }
    }

    internal enum OptionEnum
    {
        SetupCS,
        LoginWebClient,
        GetCertificate
    }

    [Serializable]
    public class Option
    {
        public string Key { get; }
        public string FancyName { get; }
        public string Answer { get; }

        public Option(string key, string fancyName, string answer)
        {
            Key = key;
            FancyName = fancyName;
            Answer = answer;
        }

        public override string ToString()
        {
            return FancyName;
        }
    }
}