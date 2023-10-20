using GTAChaos.Effects;
using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GTAChaos.Utils
{
    public class AudioPlayer
    {
        //public AudioPlayer()
        //{
        //    InitPlayer();
        //}

        public static readonly AudioPlayer INSTANCE = new AudioPlayer();
        private List<WaveOutEvent> WavesEvents = new List<WaveOutEvent>();
        public int volume = 30;

        //void InitPlayer() { SetAudioVolume(this.volume); }
        public void SetAudioVolume(int _volume)
        {
            this.volume = _volume;
            try
            {
                foreach (var waveEvent in WavesEvents)
                {
                    if (waveEvent != null) { waveEvent.Volume = _volume / 100.0f; }
                }
            }
            catch { }
        }

        public void StopAll()
        {
            try
            {
                foreach (var waveEvent in WavesEvents)
                {
                    //if (waveEvent != null) { waveEvent.Stop(); }
                    if (waveEvent != null) { waveEvent.Dispose(); }
                }
            }
            catch { }
            WavesEvents.Clear();
        }



        public async Task PlayAudio(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("Invalid audio path.");
                return;
            }

            if (!File.Exists(path))
            {
                Console.WriteLine("Audio file does not exist: " + path);
                return;
            }

            // Play the audio asynchronously
            await PlayAudioFile(path);
        }


        private async Task PlayAudioFile(string path, bool stop_prev = false)
        {
            // Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GTAChaos.assets.audio." + this.Path + ".ogg");

            if (stop_prev)
            {
                StopAll();
                //GetWaveOutEvent().Dispose();
            }

            WaveStream waveStream = null;
            try { waveStream = new VorbisWaveReader(path); } catch { }

            if (waveStream == null)
            {
                string[] supportedFormats = new string[4] { "mp3", "wav", "aac", "m4a" };
                foreach (string supportedFormat in supportedFormats)
                {
                    try
                    {
                        waveStream = new MediaFoundationReader(path);
                        if (waveStream != null)
                            break;
                    }
                    catch
                    {
                    }
                }
            }
            // if (waveStream == null)
            // {
            //     try
            //     {
            //         waveStream = new VorbisWaveReader(manifestResourceStream, false);
            //     }
            //     catch
            //     {
            //     }
            // }
            if (waveStream == null) { return; }


            WaveOutEvent waveOutEvent = new WaveOutEvent();
            waveOutEvent.Init((IWaveProvider)waveStream);
            //waveOutEvent.PlaybackStopped += (EventHandler<StoppedEventArgs>)((sender, e) => waveOutEvent.Dispose()); // not remove in list
            waveOutEvent.PlaybackStopped += (sender, e) =>
            {
                var matchingWave = WavesEvents.FirstOrDefault(item => item == waveOutEvent);
                matchingWave.Dispose();
                WavesEvents.Remove(matchingWave);
                waveOutEvent.Dispose(); // на всякий
            };

            //waveOutEvent.PlaybackStopped += (sender, e) =>
            //{
            //    // Найдем соответствующий WaveOutEvent в списке и удалим его
            //    var matchingWave = WavesEvents.FirstOrDefault(item => item == waveOutEvent);
            //    try
            //    {
            //        matchingWave.Dispose();
            //        WavesEvents.Remove(matchingWave);
            //    }
            //    catch { }
            //    //waveOutEvent.Dispose();
            //};

            waveOutEvent.Volume = this.volume / 100.0f;
            WavesEvents.Add(waveOutEvent);
            waveOutEvent.Play();
        }
    }
}