using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            context.Wait(AfterInput);
            return Task.CompletedTask;
        }

        public async Task AfterInput(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = (Activity)await argument;
            var input = activity.Text.Trim().ToLowerInvariant();

            if (input == "помощь" || input == "помощь")
            {
                await context.PostAsync(embeddedAnswers.help);
                return;
            }

            if (input == "меню")
            {
                Sm.CurrentState = "start";
                ShowPromt(context, Sm.GetPrompt(), activity);
                context.Wait(AfterInput);
                return;
            }

            if (input == "сбросить")
            {
                await Done(context);
                return;
            }

            // если нажал кнопку
            if (Sm.Next(activity.Text))
            {
                if (string.IsNullOrEmpty(Sm.Answer))
                {
                    ShowPromt(context, Sm.GetPrompt(), activity);
                    context.Wait(AfterInput);
                }
                else
                {
                    await context.PostAsync(Sm.Answer);
                    await Done(context);
                }
                return;
            }

            // выполнить полнотекстовый поиск
            var answers = Search(((IMessageActivity) context.Activity).Text);
            if (answers[0].Score > 80)
            {
                var answer = answers[0].Answer;
                var score = answers[0].Score;
                var reply = $"{answer}\n\n---\n\n*Релевантность {score:F0}%*";
                await context.PostAsync(reply);
                await Done(context);
            }
            else if (answers[0].Questions == null)
            {
                await context.PostAsync(embeddedAnswers.notFound);
                await Done(context);
            }
            else
            {
                var reply = activity.CreateReply();
                reply.AddKeyboardCard("Утоните, пожалуйста", answers.Select(x => x.Questions[0]));
                await context.PostAsync(reply);
            }
        }

        private static async Task Done(IDialogContext context)
        {
            await context.PostAsync(embeddedAnswers.thank);
            context.Done<object>(null);
        }

        private static IList<QnAMakerAnswer> Search(string input)
        {
            var client = new RestClient("https://westus.api.cognitive.microsoft.com/qnamaker/v2.0");
            var request = new RestRequest("/knowledgebases/8619aa1d-fe00-4a68-9ffc-0a2a8979bc29/generateAnswer", Method.POST);
            request.AddHeader("Ocp-Apim-Subscription-Key", "dc6b8b6a65804c21b30ead4225af6f59");
            request.AddJsonBody(new
            {
                question = input,
                top = 3
            });
            var response = client.Execute<QnAMakerResponse>(request);
            return response.Data.Answers;
        }

        private async void ShowPromt(IDialogContext context, Prompt prompt, Activity activity)
        {
            var reply = activity.CreateReply();
            reply.AddKeyboardCard(prompt.Question, prompt.Options, prompt.Options.Select(x => x.ToString()));
            await context.PostAsync(reply);
        }
    }

    public class QnAMakerResponse
    {
        public List<QnAMakerAnswer> Answers { get; set; }
    }

    public class QnAMakerAnswer
    {
        public string Answer { get; set; }
        public List<string> Questions { get; set; }
        public double Score { get; set; }
    }
}