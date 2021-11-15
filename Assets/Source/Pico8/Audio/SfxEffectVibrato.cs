using UnityEngine;

namespace PicoUnity.Audio
{
    internal class SfxEffectVibrato : SfxEffect
    {
        private Oscillator lfo;

        private float maxFrequency;
        private float minFrequency;

        public SfxEffectVibrato(float minFrequency, float maxFrequency, float frequencyLFO)
        {
            lfo = new Oscillator(frequencyLFO, AudioSynth.Waveform.LFO);

            this.maxFrequency = maxFrequency;
            this.minFrequency = minFrequency;
        }

        public override void Reset()
        {
        }

        public override void Step(double deltaTime, Oscillator oscillator)
        {
            float t = (float)lfo.Step(deltaTime);
            oscillator.Frequency = Mathf.Lerp(minFrequency, maxFrequency, t);
        }
    }
}
