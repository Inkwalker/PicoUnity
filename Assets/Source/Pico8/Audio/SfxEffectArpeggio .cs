using UnityEngine;

namespace PicoUnity.Audio
{
    internal class SfxEffectArpeggio : SfxEffect
    {

        private float[] frequences;
        private double noteTime;
        private double timer;
        private int noteIndex;

        public SfxEffectArpeggio(float f1, float f2, float f3, float f4, float hz)
        {
            frequences = new float[] { f1, f2, f3, f4 };
            noteTime = 1.0 / hz;
        }

        public override void Reset()
        {
            timer = 0;
            noteIndex = 0;
        }

        public override void Step(double deltaTime, Oscillator oscillator)
        {
            timer = timer + deltaTime;

            if (timer > noteTime)
            {
                timer = 0;
                noteIndex++;
                if (noteIndex > 3)
                    noteIndex = 0;
            }

            oscillator.Frequency = frequences[noteIndex];
        }
    }
}
