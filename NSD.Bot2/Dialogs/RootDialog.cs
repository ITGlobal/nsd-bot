using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using RestSharp;
using QnaMakerApi;
using NSD.Bot2.Models;
using QnaMakerApi.Responses;

namespace NSD.Bot2.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(AfterInput);
            return Task.CompletedTask;
        }

        public async Task AfterInput(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = (Activity)await argument;
            var input = activity.Text.Trim().ToLowerInvariant();

            if (input == "помощь")
            {
                await context.PostAsync(embeddedAnswers.help);
                return;
            }

            if (input == "сбросить")
            {
                await Done(context);
                return;
            }

            // выполнить полнотекстовый поиск
            var result = SearchAsync(((IMessageActivity)context.Activity).Text).Result;
            foreach (QnAMakerAnswer kbres in result)
            {
                var reply = $"Ответы из базы знаний - {kbres.KB.KBName}:\n\n---\n\n";
                foreach (var answer in kbres.KnowledgeBaseResult.Answers)
                {
                    var score = answer.Score;
                    if (score >= 25)
                        reply += $"{answer.Answer}\n\n---\n\n*Релевантность {score:F0}%*";
                    else
                        reply += embeddedAnswers.notFound;
                }
                await context.PostAsync(reply);
            }
            
            await Done(context);
            /*if (answers[0].Score > 80)
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
            }*/
        }

        private static async Task Done(IDialogContext context)
        {
            await context.PostAsync(embeddedAnswers.thank);
            context.Done<object>(null);
        }

        private static async Task<List<QnAMakerAnswer>> SearchAsync(string input)
        {
            List<QnAMakerAnswer> result = new List<QnAMakerAnswer>();
            List<KB> KBs = new List<KB>();
            using (var db = new KnowledgeBasesContext())
            {
                KBs = db.KB.ToList();
            }

            foreach (var KB in KBs)
            { 
                using (var client = new QnaMakerClient("e6a93449fd254d11a7857b500be15be3"))
                {
                    var res = await client.GenerateAnswer(KB.KBId, input, 3);
                    result.Add(new QnAMakerAnswer(res, KB));
                }
            }
            
            return result;
        }

        public class QnAMakerAnswer
        {
            public GenerateAnswerResponse KnowledgeBaseResult { get; set; }
            public KB KB { get; set; }

            public QnAMakerAnswer(GenerateAnswerResponse answers, KB kb)
            {
                KnowledgeBaseResult = answers;
                KB = kb;
            }
        }
    }
}