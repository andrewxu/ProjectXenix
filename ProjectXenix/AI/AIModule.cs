using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTunesLib;
using System.Speech.Synthesis;
using System.Threading;
using System.ComponentModel;

namespace ProjectXenix
{
    class AIModule
    {
        private SpeechSynthesizer bot;
        private iTunesApp iTunes = null;
        private string lastResponse = "I didn't say anything.";

        public AIModule()
        {
            iTunes = new iTunesLib.iTunesApp();
            bot = new SpeechSynthesizer();
            bot.SelectVoice("VW Paul");
            bot.Rate = 1;
            bot.Volume = 100;
        }

        public void SayIt(string phrase)
        {
            phrase = phrase.Replace("+", "featuring");
            phrase = phrase.Replace("/", "featuring");

            lastResponse = phrase;
            Dim(phrase);
        }

        public void Dim(string phrase)
        {
            iTunes.SoundVolume = 70;
            BackgroundWorker bgWorker1 = new BackgroundWorker();
            bgWorker1.DoWork += new DoWorkEventHandler(bgWorker1_DoWork);
            bgWorker1.RunWorkerAsync(phrase);
        }

        private void bgWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string phrase = e.Argument as string;
            bot.Speak(phrase);
            do
            {
                iTunes.SoundVolume = iTunes.SoundVolume + 1;
                Thread.Sleep(50);
            } while (iTunes.SoundVolume < 100);
        }

        public void RepeatLast()
        {
            bot.SpeakAsync("I said, "+lastResponse);
        }
    }
}
