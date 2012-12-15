using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Speech.Recognition;

namespace ProjectXenix.iTunes
{
    class iTunesModule
    {
        private string anchor = null;
        private ITunesController itc = null;

        public iTunesModule(AIModule aim)
        {
            itc = new ITunesController(aim);
        }

        #region GRAMMAR

        public Grammar BuildGrammar(RecognizerInfo recInfo, string anchor)
        {
            
            Choices choices = new Choices();
            List<ITunesTrack> trackList = itc.GetLibrary();
            this.anchor = anchor;

            // Build Artist Grammar
            List<string> artistList = trackList.Select(song => song.Artist).Distinct().ToList();
            foreach (var artist in artistList)
            {
                if (artist != null)
                {
                    choices.Add(string.Format("{0} play artist {1}", anchor, artist));
                }
            }

            // Build Song Grammar
            List<string> songList = trackList.Select(song => song.Name).Distinct().ToList();
            foreach (var song in songList)
            {
                if (song != null)
                {
                    var treatedSongName = song.Replace("\"", "");
                    choices.Add(string.Format("{0} play song {1}", anchor, treatedSongName));
                }
            }

            // Build General Command Grammar
            choices.Add(string.Format("{0} play", anchor));
            choices.Add(string.Format("{0} next", anchor));
            choices.Add(string.Format("{0} previous", anchor));
            choices.Add(string.Format("{0} pause", anchor));
            choices.Add(string.Format("{0} stop", anchor));

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

                if (speechText.Contains(string.Format("{0} play artist", anchor)))
                {
                    itc.PlayArtist(speechText);
                }
                else if (speechText.Contains(string.Format("{0} play song", anchor)))
                {
                    itc.PlaySong(speechText);
                }
                else if (speechText == string.Format("{0} play", anchor))
                {
                    itc.Resume();
                    Console.WriteLine("Resumed song playback.");
                }
                else if (speechText == string.Format("{0} next", anchor))
                {
                    itc.Next();
                    Console.WriteLine("Next Song");
                }
                else if (speechText == string.Format("{0} previous", anchor))
                {
                    itc.Previous();
                    Console.WriteLine("Previous Song");
                }
                else if (speechText == string.Format("{0} pause", anchor))
                {
                    itc.Pause();
                    Console.WriteLine("Paused song.");
                }
                else if (speechText == string.Format("{0} stop", anchor))
                {
                    itc.Stop();
                    Console.WriteLine("Stopped song.");
                }
            }
        }
        #endregion
    }
}
