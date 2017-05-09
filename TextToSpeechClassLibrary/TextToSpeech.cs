using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;
using Windows.Media.Playback;
using Windows.ApplicationModel.Core;
using System.Threading;
using Windows.Media.Core;

namespace TextToSpeechClassLibrary
{
    public class TextToSpeech
    {
        #region StaticVariables
        private const string TEXT_TO_SPEECH_WORKER = "TEXT_TO_SPEECH";
        #endregion

        #region Constants
        #endregion

        #region Variables
        MediaPlayerManager playerManager;
        CommandWorker.CommandWorker commandWorker;
        List<ManualResetEvent> resetEvents;
        #endregion

        #region Constructors

        #region Singleton
        private static TextToSpeech instance;

        private TextToSpeech()
        {
            commandWorker = new CommandWorker.CommandWorker(TEXT_TO_SPEECH_WORKER);
            resetEvents = new List<ManualResetEvent>();
        }

        public static TextToSpeech Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TextToSpeech();
                }
                return instance;
            }
        }

        #endregion
        #endregion

        #region StaticFunctions
        #endregion

        #region Functions
        public void Play(String textToRead)
        {
            playerManager = new MediaPlayerManager(textToRead);
            playerManager.Slicer(new String[] { "." });
            playerManager.PlaySlicedText();
            OnPlayEvent(new MediaPlayerEventArgs(playerManager));
        }

        public void Pause()
        {
            playerManager.Pause();
            OnPauseEvent(new MediaPlayerEventArgs(playerManager));
        }

        public void Resume()
        {
            playerManager.Resume();
            OnResumeEvent(new MediaPlayerEventArgs(playerManager));
        }

        public void Stop()
        {
            playerManager.Stop();
            OnStopEvent(new MediaPlayerEventArgs(playerManager));
        }

        private void Mediaplayer_MediaEnded(MediaPlayer sender, object args)
        {
            OnStopEvent(new MediaPlayerEventArgs(playerManager));
        }

        public async void RunOnUI(Action action)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        action();
                    });
        }
        #endregion

        #region Events

        public event EventHandler PlayEvent;

        protected virtual void OnPlayEvent(MediaPlayerEventArgs e)
        {
            EventHandler handler = PlayEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }


        public event EventHandler ResumeEvent;

        protected virtual void OnResumeEvent(MediaPlayerEventArgs e)
        {
            EventHandler handler = ResumeEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }


        public event EventHandler PauseEvent;

        protected virtual void OnPauseEvent(MediaPlayerEventArgs e)
        {
            EventHandler handler = PauseEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler StopEvent;

        protected virtual void OnStopEvent(MediaPlayerEventArgs e)
        {
            EventHandler handler = StopEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public class MediaPlayerEventArgs : EventArgs
        {
            private MediaPlayerManager mediaPlayerManager;


            public MediaPlayerEventArgs(MediaPlayerManager mediaPlayerManager)
            {
                this.mediaPlayerManager = mediaPlayerManager;
            }

            public MediaPlayerManager MediaPlayerManager
            {
                get { return mediaPlayerManager; }
            }

        }
        #endregion

        #region Classes
        public class MediaPlayerManager
        {

            #region StaticVariables
            #endregion

            #region Constants
            #endregion

            #region Variables
            private MediaPlayer mediaPlayer;
            private ManualResetEvent mre;
            private String text;
            private String[] slicedText;
            private Int32 indexOfRead;
            #endregion

            #region Constructors
            public MediaPlayerManager(String toRead)
            {
                indexOfRead = -1;
                text = toRead;
                Slicer();
            }
            #endregion

            #region StaticFunctions
            #endregion

            #region Functions
            public void PlaySlicedText()
            {
                Task.Factory.StartNew(() =>
                {
                    foreach (var item in this.slicedText)
                    {
                        mre = new ManualResetEvent(false);
                        SpeechSynthesizer reader = new SpeechSynthesizer();
                        reader.Voice = SpeechSynthesizer.AllVoices.First(gender => gender.Gender == VoiceGender.Female);
                        SpeechSynthesisStream stream = reader.SynthesizeTextToStreamAsync(item).AsTask().Result;
                        Task t = new Task(() =>
                        {
                            this.mediaPlayer = new MediaPlayer();

                            this.mediaPlayer.Source = MediaSource.CreateFromStream(stream, stream.ContentType);

                            this.indexOfRead++;
                            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

                            mediaPlayer.Play();
                        });
                        t.Start();

                        mre.WaitOne();
                    }
                });
            }

            public void Slicer(String[] spliters = null)
            {
                if (spliters is null)
                {
                    spliters = new String[1];
                    spliters[0] = " ";
                }
                slicedText = text.Split(spliters, StringSplitOptions.RemoveEmptyEntries);
            }

            internal void Pause()
            {
                mediaPlayer.Pause();
            }

            internal void Resume()
            {
                mediaPlayer.Play();
            }

            internal void Stop()
            {
                mediaPlayer.Dispose();
            }
            #endregion

            #region Events
            private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
            {
                mre.Set();
                this.mediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
            }
            #endregion


        }
        #endregion
    }
}

