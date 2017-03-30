using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;

namespace NSD.Bot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var options = new List<Option>
            {
                new Option("setupCS", "Настроить КС"),
                new Option("loginWebClient", "Войти в веб-кабинет НРД"),
                new Option("getCert", "Получить сертификат для веб-кабинета КД")
            };

            PromptDialog.Choice(context, AfterResetAsync,
                new PromptOptions<Option>(
                    "Чем я могу вам помочь?",
                    options: options,
                    promptStyler: new PromptStyler(PromptStyle.Keyboard)
                )
            );

            //context.Wait(MessageReceivedAsync);
        }

        public async Task AfterResetAsync(IDialogContext context, IAwaitable<Option> argument)
        {
            await context.PostAsync((await argument).ToString());
            context.Wait(MessageReceivedAsync);
        }
    }

    enum OptionEnum
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

        public Option(string key, string fancyName)
        {
            Key = key;
            FancyName = fancyName;
        }

        public override string ToString()
        {
            return FancyName;
        }
    }
}