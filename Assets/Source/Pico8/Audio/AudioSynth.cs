using UnityEngine;

namespace PicoUnity
{
    public static class AudioSynth
    {
        private static System.Random rnd = new System.Random();

        public static float PlayNote(AudioNote note, float time)
        {
            return (note.volume / 7f) * Oscillate((Waveform)note.waveform, time, note.hz);
        }

        public static float Oscillate(Waveform waveform, float time, float hz)
        {
            switch (waveform)
            {
                case Waveform.Triangle:
                    return Tri(time * hz);
                case Waveform.TiltedTriangle:
                    return TiltedTri(time * hz);
                case Waveform.Sawtooth:
                    return Saw(time * hz);
                case Waveform.Square:
                    return Square(time * hz);
                case Waveform.Pulse:
                    return Pulse(time * hz);
                case Waveform.Organ:
                    return Organ(time * hz);
                case Waveform.Noise:
                    return Noise(time * hz);
                case Waveform.Phaser:
                    return Phaser(time * hz);
            }
            return 0;
        }

        public static float Tri(float x)
        {
            return (Mathf.Abs((x % 1) * 2 - 1) * 2 - 1) * 0.5f;
        }

        public static float TiltedTri(float x)
        {
            float t = x % 1;
            return (((t < 0.875f) ? (t * 16 / 7) : ((1 - t) * 16)) - 1) * 0.5f;
        }

        public static float Saw(float x)
        {
            return (x % 1 - 0.5f) * 2 / 3f;
        }

        public static float Square(float x)
        {
            return ((x % 1 < 0.5f) ? 1 : -1) * 0.25f;
        }

        public static float Pulse(float x)
        {
            return ((x % 1 < 0.3125f) ? 1 : -1) * 0.25f;
        }

        public static float Organ(float x)
        {
            x = x * 4;
            return (Mathf.Abs((x % 2) - 1) - 0.5f + (Mathf.Abs(((x * 0.5f) % 2) - 1) - 0.5f) / 2 - 0.1f) * 0.5f;
        }

        public static float Noise(float x)
        {
            return (float)(rnd.NextDouble() * 2 - 1) * 0.5f;
        }

        public static float Phaser(float x)
        {
            x = x * 2;
            return (Mathf.Abs(((x * 127 / 128) % 2) - 1) / 2f + Mathf.Abs((x % 2) - 1) - 1) * 2 / 3f;
        }

        public enum Waveform
        {
            Triangle,
            TiltedTriangle,
            Sawtooth,
            Square,
            Pulse,
            Organ,
            Noise,
            Phaser
        }
    }
}
