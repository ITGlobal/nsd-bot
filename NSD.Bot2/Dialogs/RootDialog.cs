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
        private int _badScore = 25;
        private int _goodScore = 60;
        private static List<QnAMakerAnswer> lastResults;

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

            List<QnAMakerAnswer> result = new List<QnAMakerAnswer>();
            if (lastResults != null)
                result.Add(lastResults.Find(m => m.KB.KBName.ToLowerInvariant() == input));
            else
                result = SearchAsync(((IMessageActivity)context.Activity).Text).Result;

            if (!result.Any(m => m.KnowledgeBaseResult.Answers.Any(n => n.Score > _badScore)))
            {
                await context.PostAsync(embeddedAnswers.notFound);
                await Done(context);
                return;
            }

            var KBWithBadAnswers = result.FindAll(m => m.KnowledgeBaseResult.Answers.Any(n => n.Score > _badScore && n.Score < _goodScore));
            var KBWithGoodAnswers = result.FindAll(m => m.KnowledgeBaseResult.Answers.Any(n => n.Score >= _goodScore));
            if (KBWithGoodAnswers != null && KBWithGoodAnswers.Count != 0)
            {
                if (KBWithGoodAnswers.Count() > 1)
                {
                    var reply = activity.CreateReply();
                    reply.AddKeyboardCard("Ответ найден в нескольких категориях. Пожалуйста, уточните область поиска", KBWithGoodAnswers.Select(m => m.KB.KBName));
                    await context.PostAsync(reply);
                    lastResults = KBWithGoodAnswers;

                    context.Wait(AfterInput);
                    return;
                }
                else
                    await context.PostAsync(CreateReply(KBWithGoodAnswers[0], _goodScore));
            }
            else if (KBWithBadAnswers != null && KBWithBadAnswers.Count != 0)
                if (KBWithBadAnswers.Count() > 1)
                {
                    var reply = activity.CreateReply();
                    reply.AddKeyboardCard("Ответ найден в нескольких категориях. Пожалуйста, уточните область поиска", KBWithBadAnswers.Select(m => m.KB.KBName));
                    await context.PostAsync(reply);
                    lastResults = KBWithBadAnswers;
                }
                else
                    await context.PostAsync(CreateReply(KBWithBadAnswers[0], _badScore));

            await Done(context);
        }

        private string CreateReply(QnAMakerAnswer qnaAnswer, int minScore)
        {
            var reply = $"Ответ найден в категории: {qnaAnswer.KB.KBName}";
            foreach (var answer in qnaAnswer.KnowledgeBaseResult.Answers)
            {
                var score = answer.Score;
                if (score >= minScore)
                {
                    reply += $"\n\n---\n\nВопросы:\n\n";
                    foreach (string question in answer.Questions)
                        reply += $"{question}\n\n";
                    reply += $"Ответ:\n\n{answer.Answer}\n\n*Релевантность {score:F0}%*";

                }
            }
            return reply;
        }

        private async Task Done(IDialogContext context)
        {
            lastResults = null;
            await context.PostAsync(embeddedAnswers.thank);
            context.Done<object>(null);
        }

        private async Task<List<QnAMakerAnswer>> SearchAsync(string input)
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

        [Serializable]
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