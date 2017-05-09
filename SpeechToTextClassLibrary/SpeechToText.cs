using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace SpeechToTextClassLibrary
{
    public class SpeechToText
    {
        #region StaticVariables
        #endregion

        #region Constants
        #endregion

        #region Variables
        private SpeechRecognizer speechRecognizer;
        private SpeechToTextEventArgs speechToTextEventArgs;
        #endregion

        #region Constructors
        #region Singleton
        private static SpeechToText instance;

        private SpeechToText()
        {
            speechToTextEventArgs = new SpeechToTextEventArgs();
        }

        public static SpeechToText Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SpeechToText();
                }
                return instance;
            }
        }

        #endregion
        #endregion

        #region StaticFunctions
        #endregion

        #region Functions
        public async void StartOverlayRecognization()
        {
            OnstartEvent(new EventArgs());
            // Create an instance of SpeechRecognizer.
            speechRecognizer = InitSpeechRecognizer();

            // Listen for audio input issues.
            speechRecognizer.RecognitionQualityDegrading += speechRecognizer_RecognitionQualityDegrading;

            // Add a web search grammar to the recognizer.
            var webSearchGrammar = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.WebSearch, "webSearch");

            speechRecognizer.UIOptions.AudiblePrompt = "Say what you want to search for...";
            speechRecognizer.UIOptions.ExampleText = @"Ex. 'weather for London'";
            speechRecognizer.Constraints.Add(webSearchGrammar);

            // Compile the constraint.
            await speechRecognizer.CompileConstraintsAsync();

            // Start recognition.
            SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync();
            //await speechRecognizer.RecognizeWithUIAsync();

            speechToTextEventArgs.SpeechResult = speechRecognitionResult.Text;
            OnHaveResultEvent(speechToTextEventArgs);

            //// Do something with the recognition result.
            //var messageDialog = new Windows.UI.Popups.MessageDialog(speechRecognitionResult.Text, "Text spoken");

            //await messageDialog.ShowAsync();
        }

        public async void StartRecognization()
        {
            OnstartEvent(new EventArgs());
            // Create an instance of SpeechRecognizer.
            speechRecognizer = InitSpeechRecognizer();

            // Listen for audio input issues.
            speechRecognizer.RecognitionQualityDegrading += speechRecognizer_RecognitionQualityDegrading;

            // Add a web search grammar to the recognizer.
            var webSearchGrammar = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.WebSearch, "webSearch");

            speechRecognizer.UIOptions.AudiblePrompt = "Say what you want to search for...";
            speechRecognizer.UIOptions.ExampleText = @"Ex. 'weather for London'";
            speechRecognizer.Constraints.Add(webSearchGrammar);

            // Compile the constraint.
            await speechRecognizer.CompileConstraintsAsync();

            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;

            if (speechRecognizer.State == SpeechRecognizerState.Idle)
            {
                await speechRecognizer.ContinuousRecognitionSession.StartAsync();
            }
        }

        private void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
                args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                speechToTextEventArgs.SpeechResult = args.Result.Text;
                OnHaveResultEvent(speechToTextEventArgs);
            }
        }

        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (args.Status != SpeechRecognitionResultStatus.Success)
            {
                if (args.Status == SpeechRecognitionResultStatus.TimeoutExceeded)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                       Windows.UI.Core.CoreDispatcherPriority.Normal,
                       () =>
                       {
                           MessageDialog dialog = new MessageDialog("Voice recognization time out and stop");
                           dialog.ShowAsync();
                       });
                }
                else
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                       Windows.UI.Core.CoreDispatcherPriority.Normal,
                       () =>
                       {
                           //MessageDialog dialog = new MessageDialog("Voice recognization ended");
                           //dialog.ShowAsync();
                           if (speechRecognizer.State == SpeechRecognizerState.Idle)
                           {
                               speechRecognizer.ContinuousRecognitionSession.StartAsync();
                           }
                       });
                }
            }
        }

        public async void StopRecognization()
        {
            if (speechRecognizer.State != SpeechRecognizerState.Idle)
            {
                await speechRecognizer.ContinuousRecognitionSession.CancelAsync();
            }
            OnStopEvent(speechToTextEventArgs);
            speechToTextEventArgs = new SpeechToTextEventArgs();
        }

        private SpeechRecognizer InitSpeechRecognizer()
        {
            Language language = SpeechRecognizer.SystemSpeechLanguage;
            SpeechRecognizer speechRecognizer = new SpeechRecognizer(language);

            return speechRecognizer;
        }

        private void speechRecognizer_RecognitionQualityDegrading(SpeechRecognizer sender, SpeechRecognitionQualityDegradingEventArgs args)
        {
        }
        #endregion

        #region Events

        public event EventHandler Start;

        protected virtual void OnstartEvent(EventArgs e)
        {
            EventHandler handler = Start;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        
        public event SpeechToTextEventHandler Stop;
        public delegate void OnStopEventHandler(object sender, SpeechToTextEventArgs e);

        protected virtual void OnStopEvent(SpeechToTextEventArgs e)
        {
            SpeechToTextEventHandler handler = Stop;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event SpeechToTextEventHandler HaveResult;
        public delegate void SpeechToTextEventHandler(object sender, SpeechToTextEventArgs e);

        protected virtual void OnHaveResultEvent(SpeechToTextEventArgs e)
        {
            SpeechToTextEventHandler handler = HaveResult;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public class SpeechToTextEventArgs : EventArgs
        {
            private StringBuilder speechResult = new StringBuilder();

            public SpeechToTextEventArgs()
            {
            }

            public String SpeechResult
            {
                get
                {
                    String[] lines = speechResult.ToString().Split('\0');
                    return lines.Last();
                }
                set
                {
                    this.speechResult.AppendLine(value);
                }
            }

            public StringBuilder SpeechResultAll
            {
                get { return speechResult; }
            }

        }
        #endregion
    }
}
