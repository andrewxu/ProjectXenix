using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Speech.Recognition;
using System.Net;
using Animaonline.Weather;

namespace ProjectXenix.Master
{
    class WeatherModule
    {
        private string anchor = null;
        private AIModule aim = null;

        public WeatherModule(AIModule aim)
        {
            this.aim = aim;
        }

        #region GRAMMAR

        public Grammar BuildGrammar(RecognizerInfo recInfo, string anchor)
        {
            Choices choices = new Choices();
            this.anchor = anchor;

            // Build General Command Grammar
            choices.Add(string.Format("{0} What's the weather right now?", anchor));
            choices.Add(string.Format("{0} What's the weather for tomorrow?", anchor));
            choices.Add(string.Format("{0} What was that?", anchor));

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

                if (speechText.Contains(string.Format("{0} What's the weather right now?", anchor)))
                {
                    aim.SayIt("let me check");
                    weather("Current");
                }
                else if (speechText.Contains(string.Format("{0} What's the weather for tomorrow?", anchor)))
                {
                    aim.SayIt("let me check");
                    weather("Tomorrow");
                }  
            }
        }

        #endregion

        public void weather(string type)
        {
            Animaonline.Weather.WeatherData.GoogleWeatherData wD = Animaonline.Weather.GoogleWeatherAPI.GetWeather(Animaonline.Globals.LanguageCode.en_US, "Calgary, AB");
            switch (type)
            {
                case "Current":
                    aim.SayIt(string.Format("It is currently {0} with a temperature of {1} degrees", wD.CurrentConditions.Condition, Math.Round(wD.CurrentConditions.Temperature.Celsius)));
                    break;
                case "Tomorrow":
                    aim.SayIt(string.Format("Tomorrow is {3}, and, it will be {0}, with a high of {1} degrees, and a low of {2} degrees", wD.ForecastConditions[1].Condition, Math.Round(wD.ForecastConditions[1].High.Celsius), Math.Round(wD.ForecastConditions[1].Low.Celsius), ShortToLongDay(wD.ForecastConditions[1].Day)));
                    break;
            }
            //foreach (Animaonline.Weather.WeatherData.ForecastCondition f in wD.ForecastConditions){
            //    Console.WriteLine(string.Format("CITY: {3} DAY:{0}, HIGH:{1}, LOW:{2}, CONDITION:{3}",f.Day,f.High.Celsius,f.Low.Celsius,f.Condition,wD.ForecastInformation.City));
                
            //}
            //Console.WriteLine(string.Format("Condition:{0} Temperature:{1}°F {2}°C",wD.CurrentConditions.Condition , wD.CurrentConditions.Temperature.Celsius, wD.CurrentConditions.Temperature.Celsius));
        }

        private string ShortToLongDay(string day)
        {
            switch (day.ToUpper())
            {
                case "MON":
                    return "Monday";
                case "TUE":
                    return "Tuesday";
                case "WED":
                    return "Wednesday";
                case "THU":
                    return "Thursday";
                case "FRI":
                    return "Friday";
                case "SAT":
                    return "Saturday";
                default:
                    return "Sunday";
            }
        }

    }
}
