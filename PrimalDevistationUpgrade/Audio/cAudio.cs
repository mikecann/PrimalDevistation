using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

namespace PrimalDevistation.Audio
{
    public class cAudio
    {
        private static AudioEngine _engine;
        private static WaveBank _waveBank;
        private static SoundBank _soundBank;
      
        public cAudio()
        {   
            _engine = new AudioEngine("Resources/Audio/audio.xgs");
            _waveBank = new WaveBank(_engine, "Resources/Audio/Wave Bank.xwb");
            _soundBank = new SoundBank(_engine, "Resources/Audio/Sound Bank.xsb");
        }

        public Cue play(string name)
        {          
            try
            {
                Cue c = _soundBank.GetCue(name);
                c.Play();
                return c;
            }
            catch (Exception e)
            {
                string trace = e.StackTrace;
            }
            return null;
        }

        public void stop(Cue cue)
        {
            
            cue.Stop(AudioStopOptions.Immediate);
        }

        public void Update(GameTime gametime)
        {
            _engine.Update();
        }

        /// <summary>
        /// Shuts down the sound code tidily
        /// </summary>
        public void shutdown()
        {
            _soundBank.Dispose();
            _waveBank.Dispose();
            _engine.Dispose();
        }

    }
}
