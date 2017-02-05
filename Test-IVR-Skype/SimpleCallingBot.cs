using Microsoft.Bot.Builder.Calling;
using Microsoft.Bot.Builder.Calling.Events;
using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace Test_IVR_Skype
{
    public class SimpleCallingBot : ICallingBot
    {
        public ICallingBotService CallingBotService
        {
            get; private set;
        }

        public SimpleCallingBot(ICallingBotService callingBotService)
        {
            if (callingBotService == null)
                throw new ArgumentNullException(nameof(callingBotService));

            this.CallingBotService = callingBotService;

            CallingBotService.OnIncomingCallReceived += OnIncomingCallReceived;
            CallingBotService.OnPlayPromptCompleted += OnPlayPromptCompleted;
            CallingBotService.OnRecognizeCompleted += OnRecognizeCompleted;
            CallingBotService.OnHangupCompleted += OnHangupCompleted;
        }

        private Task OnIncomingCallReceived(IncomingCallEvent incomingCallEvent)
        {
            var id = Guid.NewGuid().ToString();
            incomingCallEvent.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    new Answer { OperationId = id },
                    GetPromptForText("Welcome to my test bot!")
                };

            return Task.FromResult(true);
        }

        private Task OnPlayPromptCompleted(PlayPromptOutcomeEvent playPromptOutcomeEvent)
        {
            playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
            {
                CreateIvrOptions("Press 1 to see how much is 1 + 1 or 9 to exit.")
            };
            return Task.FromResult(true);
        }

        private Task OnRecognizeCompleted(RecognizeOutcomeEvent recognizeOutcomeEvent)
        {
            switch (recognizeOutcomeEvent.RecognizeOutcome.ChoiceOutcome.ChoiceName)
            {
                case "How much is 1 + 1?":
                    recognizeOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetPromptForText("1 + 1 is 2")
                    };
                    break;
                case "Bye":
                    recognizeOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetPromptForText("Goodbye!"),
                        new Hangup { OperationId = Guid.NewGuid().ToString() }
                    };
                    break;
                default:
                    recognizeOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        CreateIvrOptions("Press 1 to see how much is 1 + 1 or 9 to exit.")
                    };
                    break;
            }
            return Task.FromResult(true);
        }

        private Task OnHangupCompleted(HangupOutcomeEvent hangupOutcomeEvent)
        {
            hangupOutcomeEvent.ResultingWorkflow = null;
            return Task.FromResult(true);
        }

        private static Recognize CreateIvrOptions(string textToBeRead)
        {
            var id = Guid.NewGuid().ToString();

            var choices = new List<RecognitionOption>
            {
                new RecognitionOption
                {
                    Name = "How much is 1 + 1?",
                    SpeechVariation = new List<string> { "How much is 1 + 1?", "1 + 1" },
                    DtmfVariation = (char)('0' + 1)
                },

                new RecognitionOption
                {
                    Name = "Bye",
                    SpeechVariation = new List<string> { "Bye", "Bye bye", "Goodbye" },
                    DtmfVariation = (char)('0' + 9)
                }
            };

            var recognize = new Recognize
            {
                OperationId = id,
                PlayPrompt = GetPromptForText(textToBeRead),
                BargeInAllowed = true,
                Choices = choices
            };

            return recognize;
        }

        private static PlayPrompt GetPromptForText(string text)
        {
            var prompt = new Prompt { Value = text, Voice = VoiceGender.Female };
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }
    }
}