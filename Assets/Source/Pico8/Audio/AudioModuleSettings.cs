using UnityEngine;

namespace PicoUnity
{
    [CreateAssetMenu(menuName = "PicoUnity/Audio Module Settings")]
    public class AudioModuleSettings : ScriptableObject
    {
        [SerializeField] AnimationCurve triangleWave       = new AnimationCurve();
        [SerializeField] AnimationCurve tiltedTriangleWave = new AnimationCurve();
        [SerializeField] AnimationCurve sawtoothWave       = new AnimationCurve();
        [SerializeField] AnimationCurve squareWave         = new AnimationCurve();
        [SerializeField] AnimationCurve pulseWave          = new AnimationCurve();
        [SerializeField] AnimationCurve organWave          = new AnimationCurve();
        [SerializeField] AnimationCurve phaserWave         = new AnimationCurve();
        [SerializeField] AnimationCurve phaserModulation   = new AnimationCurve();
        [SerializeField] AnimationCurve lfoWave            = new AnimationCurve();

        public double Evaluate(AudioSynth.Waveform waveform, double x)
        {
            float t = (float)x;

            switch (waveform)
            {
                case AudioSynth.Waveform.Triangle:       return triangleWave.Evaluate(t);
                case AudioSynth.Waveform.TiltedTriangle: return tiltedTriangleWave.Evaluate(t);
                case AudioSynth.Waveform.Sawtooth:       return sawtoothWave.Evaluate(t);
                case AudioSynth.Waveform.Square:         return squareWave.Evaluate(t);
                case AudioSynth.Waveform.Pulse:          return pulseWave.Evaluate(t);
                case AudioSynth.Waveform.Organ:          return organWave.Evaluate(t);
                case AudioSynth.Waveform.Phaser:         return phaserWave.Evaluate(t) * phaserModulation.Evaluate(t / 128f);

                case AudioSynth.Waveform.Noise:          return AudioSynth.Noise(x);
                case AudioSynth.Waveform.LFO:            return lfoWave.Evaluate(t);
            }

            return 0;
        }
    }
}
