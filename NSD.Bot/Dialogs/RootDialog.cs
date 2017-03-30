using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using NSD.Bot.StateMachine;
using RestSharp;

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
            ShowPromt(context, Sm.GetPrompt(), activity);
            context.Wait(AfterInput);
        }

        public async Task AfterInput(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            Trace.TraceInformation(nameof(AfterInput));

            var activity = await argument;
            var result = Sm.Next(activity.Text);

            if (result)
            {
                Trace.TraceInformation($"result {activity.Text}");

                if (string.IsNullOrEmpty(Sm.Answer))
                {
                    ShowPromt(context, Sm.GetPrompt(), activity as Activity);
                    context.Wait(AfterInput);
                    return;
                }
                await context.PostAsync(Sm.Answer);
                context.Done<object>(null);
            }
            else
            {
                Trace.TraceInformation($"false {activity.Text}");

                await context.PostAsync(Search(((IMessageActivity)context.Activity).Text));
                context.Done<object>(null);
            }
        }

        private string Search(string input)
        {
            var client = new RestClient("https://westus.api.cognitive.microsoft.com/qnamaker/v2.0");
            var request = new RestRequest("/knowledgebases/8619aa1d-fe00-4a68-9ffc-0a2a8979bc29/generateAnswer", Method.POST);
            request.AddHeader("Ocp-Apim-Subscription-Key", "dc6b8b6a65804c21b30ead4225af6f59");
            request.AddJsonBody(new
            {
                question = input
            });
            var response = client.Execute<dynamic>(request);
            dynamic answer = response.Data["answers"][0];
            var score = answer["score"];
            var ret = score < 45 ? "Не могу ответить на вопрос. Обратитесь в soed@nsd.ru" : answer["answer"];
            return $"{ret}\n(Инфа {score}%)";
        }

        private async void ShowPromt(IDialogContext context, Prompt prompt, Activity activity)
        {
            var reply = activity.CreateReply();
            reply.AddKeyboardCard(prompt.Question, prompt.Options, prompt.Options.Select(x => x.ToString()));
            await context.PostAsync(reply);
        }
    }

    [Serializable]
    public class Option
    {
        public string Input { get; }
        public string Answer { get; }

        public Option(string input, string answer)
        {
            Input = input;
            Answer = answer;
        }

        public override string ToString()
        {
            return Input;
        }
    }
}