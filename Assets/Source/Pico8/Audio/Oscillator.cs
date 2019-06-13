using System;
using System.Collections.Generic;

namespace PicoUnity
{
    internal class Oscillator
    {       
        private AudioNote note;

        private bool   stopSignal;
        private double time;

        public bool Stopped { get; private set; }

        public Oscillator(AudioNote note)
        {
            Reset(note);
        }

        public void Reset(AudioNote note)
        {
            this.note = note;
            Stopped = false;
            time = 0;
            stopSignal = false;
        }

        public float Sample(double dt)
        {
            if (Stopped) return 0;

            float sample = 0;
            if (stopSignal)
            {
                double osct = 1.0 / note.hz;
                double rot = time % osct / osct;
                double delta_rot = dt % osct / osct;

                Stopped = rot + delta_rot > 1;
            }

            if (!Stopped)
                sample = (float)AudioSynth.PlayNote(note, time);

            time = (time + dt) % 1;

            return sample;
        }

        public void Stop()
        {
            stopSignal = true;
        }
    }
}
