using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Speech.Recognition;
using System.Net;
using Animaonline.Weather;

namespace ProjectXenix.Master
{
    class DTimeModule
    {
        private string anchor = null;
        private AIModule aim = null;

        public DTimeModule(AIModule aim)
        {
            this.aim = aim;
        }

        #region GRAMMAR

        public Grammar BuildGrammar(RecognizerInfo recInfo, string anchor)
        {
            Choices choices = new Choices();
            this.anchor = anchor;

            // Build General Command Grammar
            choices.Add(string.Format("{0} What time is it?", anchor));

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

                if (speechText.Contains(string.Format("{0} What time is it?", anchor)))
                {
                    string dt = DateTime.Now.ToString("hh:mm" + " | ");
                    int am = DateTime.Now.Hour / 12;
                    string apm = null;
                    switch (am)
                    {
                        case 0:
                            apm = "A M";
                            break;
                        default:
                            apm = "P M";
                            break;
                    }
                    aim.SayIt(string.Format("The time is {0} {1}", dt, apm));
                }
            }
        }
        #endregion

    }
}
