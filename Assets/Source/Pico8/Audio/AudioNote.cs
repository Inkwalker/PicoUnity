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

        public static AudioNote Decode(byte lowByte, byte highByte)
        {
            AudioNote note;
            note.pitch = (byte)(lowByte & 0x3f);
            note.volume = (byte)((highByte >> 2) & 0x7);
            note.effect = (byte)((highByte >> 5) & 0x7);
            note.waveform = (byte)((lowByte >> 6) | ((highByte & 0x1) << 2));
            note.isCustom = (highByte >> 7) > 0;

            note.hz = 440 * Mathf.Pow(2, (note.pitch - 33) / 12f);

            return note;
        }
    }
}
