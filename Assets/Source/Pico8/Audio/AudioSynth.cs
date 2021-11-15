using System;
using UnityEngine;

namespace PicoUnity
{
    public static class AudioSynth
    {
        private static System.Random rnd = new System.Random();
        private static AudioModuleSettings settings = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            settings = Resources.Load<AudioModuleSettings>("audio_settings");
        }

        public static double Oscillate(Waveform waveform, float phase)
        {
            return settings.Evaluate(waveform, phase);
        }

        public static double Oscillate(Waveform waveform, double time, float hz)
        {
            return settings.Evaluate(waveform, time * hz);

            //Old procedural waveforms
            //switch (waveform)
            //{
            //    case Waveform.Triangle:
            //        return Tri(time * hz);
            //    case Waveform.TiltedTriangle:
            //        return TiltedTri(time * hz);
            //    case Waveform.Sawtooth:
            //        return Saw(time * hz);
            //    case Waveform.Square:
            //        return Square(time * hz);
            //    case Waveform.Pulse:
            //        return Pulse(time * hz);
            //    case Waveform.Organ:
            //        return Organ(time * hz);
            //    case Waveform.Noise:
            //        return Noise(time * hz);
            //    case Waveform.Phaser:
            //        return Phaser(time * hz);
            //}
            //return 0;
        }

        #region Waveform functions
        public static double Tri(double x)
        {
            return (Math.Abs(((x + 0.25) % 1) * 2 - 1) * 2 - 1) * 0.5d;
        }

        public static double LFO(double x)
        {
            return Math.Abs((x % 1) * 2 - 1);
        }

        public static double TiltedTri(double x)
        {
            double t = (x + 0.45) % 1;
            return (((t < 0.875d) ? (t * 16 / 7) : ((1 - t) * 16)) - 1) * 0.5d;
        }

        public static double Saw(double x)
        {
            return ((x + 0.5) % 1 - 0.5d) * 2 / 3d;
        }

        public static double Square(double x)
        {
            return ((x % 1 < 0.5d) ? 1 : -1) * 0.25d;
        }

        public static double Pulse(double x)
        {
            return ((x % 1 < 0.3125d) ? 1 : -1) * 0.25d;
        }

        public static double Organ(double x)
        {
            x = x * 4 + 1.8;
            return (Math.Abs((x % 2) - 1) - 0.5d + (Math.Abs(((x * 0.5d) % 2) - 1) - 0.5d) / 2 - 0.1d) * 0.5d;
        }

        public static double Noise(double x)
        {
            return (rnd.NextDouble() * 2 - 1) * 0.5d;
        }

        public static double Phaser(double x)
        {
            x = x * 2 + 0.35;
            return (Math.Abs(((x * 127 / 128) % 2) - 1) / 2d + Math.Abs((x % 2) - 1) - 1) * 2 / 3d;
        }
        #endregion

        public enum Waveform
        {
            Triangle,
            TiltedTriangle,
            Sawtooth,
            Square,
            Pulse,
            Organ,
            Noise,
            Phaser,
            LFO
        }
    }
}
