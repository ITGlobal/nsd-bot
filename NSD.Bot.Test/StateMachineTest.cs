using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NSD.Bot.Test
{
    [TestFixture]
    public class StateMachineTest
    {
        [TestCase("start", "next state", "nextState")]
        [TestCase("start", "give answer", "start")]
        public void Next_Test(string currentState, string input, string nextState)
        {
            var sm = new XMLStateMachine
            {
                CurrentState = currentState,
                StateTable = @"C:\Users\dizel\OneDrive\документы\visual studio 2017\Projects\NSD.Bot\NSD.Bot.Test\sm.xml"
            };
            sm.Next(input);
            Assert.AreEqual(nextState, sm.CurrentState);
        }

        [Test]
        public void GetPrompt_Test()
        {
            var sm = new XMLStateMachine
            {
                CurrentState = "start",
                StateTable = @"C:\Users\dizel\OneDrive\документы\visual studio 2017\Projects\NSD.Bot\NSD.Bot.Test\sm.xml"
            };
            var prompt = sm.GetPrompt();
            Assert.AreEqual(2, prompt.Options.Length);
        }
    }
}
