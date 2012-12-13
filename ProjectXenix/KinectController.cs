using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;
using System.Diagnostics;
using ProjectXenix.iTunes;
using ProjectXenix.Master;

namespace ProjectXenix
{
    class KinectController
    {
        private const string anchor = "Optimus";
        private const string RecognizerId = "SR_MS_en-US_Kinect_10.0";
        private KinectSensor sensor;
        private SpeechRecognitionEngine speechEngine = null;
        private Stream stream = null;
        private Boolean inUse = false;
        // Modules
        private iTunesModule itm = null;
        private MasterModule mm = null;
        private WeatherModule wm = null;
        private DTimeModule dtm = null;
        private AIModule aim = null;

        public KinectController(AIModule aim)
        {
            this.aim = aim;
            aim.SayIt("Initializing");
            InitializeController();
        }

        private void InitializeController()
        {
            Console.WriteLine("- Kinect Controller Initializing");

            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }


            /**
            kinectSource.FeatureMode = true;
            kinectSource.AutomaticGainControl = false;
            kinectSource.SystemMode = SystemMode.OptibeamArrayAndAec;
            kinectSource.AcousticEchoSuppression = 1;
            kinectSource.NoiseSuppression = true;
            kinectSource.MicArrayMode = MicArrayMode.MicArrayAdaptiveBeam;
             **/

            RecognizerInfo recInf = SpeechRecognitionEngine.InstalledRecognizers().Where(r => r.Id == RecognizerId).FirstOrDefault();

            if (recInf == null)
            {
                throw new ApplicationException("Could not load Kinect.");
            }

            speechEngine = new SpeechRecognitionEngine(recInf.Id);

            Console.WriteLine("- Master Module Initializing");
            mm = new MasterModule(aim);
            Console.WriteLine("- iTunes Module Initializing");
            itm = new iTunesModule(aim);
            Console.WriteLine("- Weather Module Initializing");
            wm = new WeatherModule(aim);
            Console.WriteLine("- Day/Time Module Initializing");
            dtm = new DTimeModule(aim);


            speechEngine.LoadGrammar(itm.BuildGrammar(recInf, anchor));
            speechEngine.SpeechRecognized += itm.SreSpeechRecognized;
            speechEngine.LoadGrammar(mm.BuildGrammar(recInf, anchor));
            speechEngine.SpeechRecognized += mm.SreSpeechRecognized;
            speechEngine.LoadGrammar(wm.BuildGrammar(recInf, anchor));
            speechEngine.SpeechRecognized += wm.SreSpeechRecognized;
            speechEngine.LoadGrammar(dtm.BuildGrammar(recInf, anchor));
            speechEngine.SpeechRecognized += dtm.SreSpeechRecognized;

            // Generic Recognizers
            speechEngine.SpeechRecognized += SreSpeechRecognized;
            speechEngine.SpeechHypothesized += SreSpeechHypothesized;
            speechEngine.SpeechRecognitionRejected += SreSpeechRecognitionRejected;

            var audioSource = this.sensor.AudioSource;
            stream = audioSource.Start();
            speechEngine.SetInputToAudioStream(stream,
                                        new SpeechAudioFormatInfo(
                                            EncodingFormat.Pcm, 16000, 16, 1,
                                            32000, 2, null));

            speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            aim.SayIt("Ready");
            Console.WriteLine("- Loading complete.");
        }

        public void SreSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence >= 0.90)
            {
                string speechText = e.Result.Text;
                Console.WriteLine(string.Format("#{0} ({1})", e.Result.Text, e.Result.Confidence.ToString()));
            }
        }

        public void SreSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Console.WriteLine(string.Format("{0} did not understand you.  Try again!", anchor));
            //aim.SayIt("What was that?");
        }

        public void SreSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e) { }

    }
}
