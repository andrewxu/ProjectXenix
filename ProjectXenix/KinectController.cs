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

        private RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

        private void ActivateSensor()
        {
            // Get sensor
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                this.sensor = potentialSensor;
                break;
            }

            // Start it
            if (null != this.sensor)
            {
                try
                {
                    // Attempt to start it
                    this.sensor.Start();
                    this.sensor.AudioSource.AutomaticGainControlEnabled = false;
                    this.sensor.AudioSource.NoiseSuppression = true;
                    this.sensor.AudioSource.EchoCancellationMode = EchoCancellationMode.CancellationAndSuppression;

                    /**
                     * legacy modes
                    kinectSource.FeatureMode = true;
                    kinectSource.AutomaticGainControl = false;
                    kinectSource.SystemMode = SystemMode.OptibeamArrayAndAec;
                    kinectSource.AcousticEchoSuppression = 1;
                    kinectSource.NoiseSuppression = true;
                    kinectSource.MicArrayMode = MicArrayMode.MicArrayAdaptiveBeam;
                     **/
                }
                catch (IOException)
                {
                    // Failed to start: sensor is in use by another program, mark it null
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                throw new Exception("Failed to start Kinect Sensor");
            }
        }

        private void InitializeController()
        {
            Console.WriteLine("- Kinect Controller Initializing");
            try
            {
                ActivateSensor();
                RecognizerInfo recInf = GetKinectRecognizer();

                if (null != recInf)
                {
                    this.speechEngine = new SpeechRecognitionEngine(recInf.Id);
                }
                else
                {
                    throw new Exception("Unable to find recognizer");
                }

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
            catch (Exception e)
            {
                Console.WriteLine("failed...");
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
                return;
            }
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
