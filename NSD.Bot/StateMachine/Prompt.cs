using NSD.Bot.Dialogs;

namespace NSD.Bot.StateMachine
{
    public class Prompt
    {
        public Prompt(string question, Option[] options)
        {
            Question = question;
            Options = options;
        }

        public string Question { get; }
        public Option[] Options { get; }
    }
}