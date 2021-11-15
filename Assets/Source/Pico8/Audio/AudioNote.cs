using UnityEngine;

namespace PicoUnity
{
    public struct AudioNote
    {
        public byte effect;
        public byte volume;
        public byte waveform;
        public byte pitch;
        public bool isCustom;

        public float hz;

        public float Volume01 => volume / 7f;
        public static AudioNote Default
        {
            get
            {
                return new AudioNote() { pitch = 24, hz = PitchToHz(24), volume = 5 };
            }
        }

        public static AudioNote Decode(byte lowByte, byte highByte)
        {
            AudioNote note;
            note.pitch = (byte)(lowByte & 0x3f);
            note.volume = (byte)((highByte >> 1) & 0x7);
            note.effect = (byte)((highByte >> 4) & 0x7);
            note.waveform = (byte)((lowByte >> 6) | ((highByte & 0x1) << 2));
            note.isCustom = (highByte >> 7) > 0;

            note.hz = PitchToHz(note.pitch);

            return note;
        }

        public static float PitchToHz(float pitch)
        {
            return 440 * Mathf.Pow(2, (pitch - 33) / 12f);
        }
    }
}
