using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Speech.Recognition;
using Microsoft.Kinect;
using System.Net;
using Animaonline.Weather;

namespace ProjectXenix.Master
{
    class MasterModule
    {
        private string anchor = null;
        private AIModule aim = null;

        public MasterModule(AIModule aim)
        {
            this.aim = aim;
           
        }

        #region GRAMMAR

        public Grammar BuildGrammar(RecognizerInfo recInfo, string anchor)
        {
            Choices choices = new Choices();
            this.anchor = anchor;

            // Build General Command Grammar
            choices.Add(string.Format("{0} What's your name?", anchor));
            choices.Add(string.Format("{0} Wake up", anchor));
            choices.Add(string.Format("{0} You suck", anchor));
            choices.Add(string.Format("{0} Shut Up", anchor));
            choices.Add(string.Format("{0} What was that?", anchor));
            choices.Add(string.Format("{0} What did you say?", anchor));
            choices.Add(string.Format("{0} Move upwards", anchor));
            choices.Add(string.Format("{0} Give me your status", anchor));

            var gb = new GrammarBuilder();
            gb.Culture = recInfo.Culture;
            gb.Append(choices);

            return new Grammar(gb);
        }
        #endregion

        #region RECOGNITION EVENTS
        public void SreSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Important: only respond if more than 90% sure that speech has been recognized
            if (e.Result.Confidence >= 0.90)
            {
                string speechText = e.Result.Text;

                if (speechText.Contains(string.Format("{0} What's your name?", anchor)))
                {
                    aim.SayIt(string.Format("My name is {0}", anchor));
                }
                else if (speechText.Contains(string.Format("{0} Wake up", anchor)))
                {
                    aim.SayIt("I'm awake now, what's up?");
                }
                else if (speechText.Contains(string.Format("{0} Give me your status", anchor)))
                {
                    aim.SayIt("I am online");
                }
                else if (speechText.Contains(string.Format("{0} You suck", anchor)))
                {
                    aim.SayIt("Hey at least I can calculate pie to 20 decimal places in under a minute, bet you can't do that.");
                }
                else if (speechText.Contains(string.Format("{0} What was that?", anchor)))
                {
                    aim.RepeatLast();
                }
                else if (speechText.Contains(string.Format("{0} What did you say?", anchor)))
                {
                    aim.RepeatLast();
                }
                else if (speechText.Contains(string.Format("{0} Shut Up", anchor)))
                {
                    aim.SayIt("Why don't you shut up nadia?");
                }
            }
        }
        #endregion

    }
}
