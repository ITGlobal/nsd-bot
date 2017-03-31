using System;

namespace NSD.Bot.Dialogs
{
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