using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTunesLib;
using ProjectXenix.iTunes;
using Microsoft.Speech.Recognition;
using System.Speech.Synthesis;

namespace ProjectXenix
{
    class ITunesController
    {
        private iTunesApp iTunes = null;
        private List<ITunesTrack> trackList { get; set; }
        private AIModule aim = null;

        public ITunesController(AIModule aim)
        {
            try
            {
                this.iTunes = new iTunesLib.iTunesApp();
                this.aim = aim;
            }
            catch (Exception)
            {
                throw new Exception("Problem loading iTunes.");
            }
        }

        #region HELPERS
        public List<ITunesTrack> GetLibrary()
        {
            trackList = new List<ITunesTrack>();

            var musicLibrary = this.iTunes.LibraryPlaylist;

            // note: iTunes index starts at 1 not 0
            for (int i = 1; i <= musicLibrary.Tracks.Count; i++)
            {
                IITTrack track = musicLibrary.Tracks[i];
                // Only audio tracks are added
                if (track != null && track.KindAsString.Contains("audio"))
                {
                    ITunesTrack iTrack = new ITunesTrack
                    {
                        Index = i,
                        Artist = track.Artist,
                        Name = track.Name
                    };

                    trackList.Add(iTrack);
                }
            }

            return trackList;
        }

        public int GetRandomSongFromArtist(string artist)
        {
            if (string.IsNullOrEmpty(artist))
                throw new ArgumentNullException("Artist name cannot be null.");

            int result = -1;

            var allArtistSongIndices = from t in trackList
                                       where t.Artist == artist
                                       select new {t.Index, t.Name};

            Console.WriteLine(allArtistSongIndices.Count());

            //foreach (var item in allArtistSongIndices)
            //{
            //    Console.WriteLine("Index={0}, Song={1}", item.Index, item.Name);
            //}

            //Random random = new Random();
            //result = random.Next(allArtistSongIndices.Min(), allArtistSongIndices.Max());

            return result;
        }

        public void PlayArtist(string speechText)
        {
            string artistName = this.ParseArtistOrSong(speechText);

            // Pick random song to play
            int randomSongIndex = GetRandomSongFromArtist(artistName);

            if (randomSongIndex != -1)
            {
                ITunesTrack track = Play(randomSongIndex);
                aim.SayIt(string.Format("Playing {1}, by {0}", track.Artist, track.Name));
                Console.WriteLine(string.Format("Now Playing Artist: {0}, Song: {1}", track.Artist, track.Name));
            }
        }

        public void PlaySong(string speechText)
        {
            string song = this.ParseArtistOrSong(speechText);

            this.iTunes.Stop();
            ITunesTrack track = Play(song);
            
            aim.SayIt(string.Format("Playing {1}, by {0}", track.Artist, track.Name));
            Console.WriteLine(string.Format("Now Playing Artist: {0}, Song: {1}", track.Artist, track.Name));
        }

        public string ParseArtistOrSong(string speechText)
        {
            string result = string.Empty;

            // Extract artist or song name
            string[] words = speechText.Split(' ');

            string artistOrSong = string.Empty;
            for (int i = 3; i < words.Length; i++)
            {
                artistOrSong += words[i] + " ";
            }

            result = artistOrSong.Trim();

            return result;
        }
        #endregion

        #region PLAY: index, trackname
        public ITunesTrack Play(int index)
        {
            if (index <= 0)
                throw new ArgumentNullException("Index must be greater than 0");

            IITTrack track = this.iTunes.LibraryPlaylist.Tracks[index];
            return this.Play(track);
        }

        public ITunesTrack Play(string trackName)
        {
            if (string.IsNullOrEmpty(trackName))
                throw new ArgumentNullException("Track name cannot be null");

            IITTrack track = this.iTunes.LibraryPlaylist.Tracks.get_ItemByName(trackName);
            return this.Play(track);
        }

        private ITunesTrack Play(IITTrack track)
        {
            ITunesTrack result = null;

            if (track != null)
            {
                result = new ITunesTrack
                {
                    Artist = track.Artist,
                    Album = track.Album,
                    Name = track.Name,
                    Index = track.Index
                };

                track.Play();
            }

            return result;
        }
        #endregion

        #region CONTROLS: resume, pause, stop, next, previoius
        public void Resume()
        {
            this.iTunes.Play();
        }

        public void Pause()
        {
            this.iTunes.Pause();
        }

        public void Stop()
        {
            this.iTunes.Stop();
        }

        public void Next()
        {
            this.iTunes.NextTrack();
        }

        public void Previous()
        {
            this.iTunes.PreviousTrack();
        }
        #endregion

       
    }
}
