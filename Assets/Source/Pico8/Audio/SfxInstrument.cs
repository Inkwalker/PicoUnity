using System.Collections.Generic;
using PicoUnity.Audio;
using UnityEngine;

namespace PicoUnity
{
    internal class SfxInstrument
    {
        private const float SUSTAIN_FREQUENCY_DELTA = 440; 

        public AudioNote Note { get; private set; }
        public AudioSynth.Waveform Waveform { get; set; }

        public bool Active => voices.Count > 0;
        public bool Released
        {
            get
            {
                foreach (var voice in voices)
                {
                    if (voice.Triggered) return false;
                }

                return true;
            }
        }


        private double time;
        private List<Voice> voices;


        public SfxInstrument()
        {
            Waveform = AudioSynth.Waveform.Triangle;

            voices = new List<Voice>();
        }

        public void Play(AudioNote note, SfxEffect effect = null)
        {
            Note = note;
            Waveform = (AudioSynth.Waveform)note.waveform;

            Voice noteConsumedByVoice = null;
            foreach (var item in voices)
            {
                if (item.Triggered)
                {
                    if (item.Oscillator.Frequency > note.hz - SUSTAIN_FREQUENCY_DELTA && 
                        item.Oscillator.Frequency < note.hz + SUSTAIN_FREQUENCY_DELTA)
                    {
                        item.Oscillator.Frequency = note.hz;
                        item.Oscillator.Volume = note.Volume01;
                        item.Oscillator.Waveform = Waveform;

                        item.SetEffect(effect);

                        noteConsumedByVoice = item;
                    }
                }
            }

            foreach (var item in voices)
            {
                if (item.Triggered && item != noteConsumedByVoice) item.Release();
            }

            if (noteConsumedByVoice == null)
            {
                var voice = new Voice(Waveform);
                voice.SetEffect(effect);
                voice.Trigger(note.hz, note.Volume01);
                voices.Add(voice);
            }
        }

        public float Sample(double dt)
        {
            if (!Active) return 0;

            float sample = 0;

            for (int i = voices.Count - 1; i >= 0; i--)
            {
                sample += (float)voices[i].Step(dt);

                if (voices[i].Active == false)
                    voices.RemoveAt(i);
            }

            time = time + dt;

            return sample;
        }

        public void Stop()
        {
            foreach (var adsr in voices)
            {
                adsr.Release();
            }
        }
    }
}
