using UnityEngine;

namespace PicoUnity.Audio
{
    internal class SfxEffectLerp : SfxEffect
    {
        private float startFrequency;
        private float endFrequency;

        private float startVolume;
        private float endVolume;

        private double timer;
        private float time;

        public SfxEffectLerp(float fromFreq, float toFreq, float fromVol, float toVol, float time)
        {
            startFrequency = fromFreq;
            endFrequency = toFreq;
            startVolume = fromVol;
            endVolume = toVol;
            this.time = time;

            timer = 0;
        }

        public override void Reset()
        {
            timer = 0;
        }

        public override void Step(double deltaTime, Oscillator oscillator)
        {
            if (timer > time) return;

            float t = (float)(timer / time);

            var frequency = Mathf.Lerp(startFrequency, endFrequency, t);
            var volume = Mathf.Lerp(startVolume, endVolume, t);

            timer += deltaTime;

            oscillator.Frequency = frequency;
            oscillator.Volume = volume;
        }
    }
}
