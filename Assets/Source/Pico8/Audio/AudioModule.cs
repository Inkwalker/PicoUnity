using System;
using System.Collections.Generic;


namespace PicoUnity
{
    public class AudioModule : EmulatorModule
    {
        private SfxChannel[] channels;
        private float sampleDelta;
        private float gain = 0.5f;

        public AudioModule(MemoryModule memory, int sampleRate)
        {
            channels = new SfxChannel[4];

            for (int i = 0; i < channels.Length; i++)
            {
                channels[i] = new SfxChannel(memory);
            }

            sampleDelta = 1f / sampleRate;
        }

        public void Music(int n, int fade_ms = 0, int channelmask = 0)
        {
        }

        public void Sfx(int n, int channel = -1, int offset = 0, int length = 0)
        {
            if (n < 0) return;

            SfxChannel sfxChannel = null;

            if (channel < 0)
            {
                foreach (var item in channels)
                {
                    if (item.IsPlaying == false)
                    {
                        sfxChannel = item;
                        break;
                    }
                }
            }
            else
            {
                sfxChannel = channels[channel];
            }

            if (sfxChannel != null)
                sfxChannel.Sfx(n, offset, length);
        }

        public override ApiTable GetApiTable()
        {
            return new ApiTable()
            {
                {"music", (Action<int, int, int>)    Music },
                {"sfx", (Action<int, int, int, int>) Sfx }
            };
        }

        public void FillBuffer(float[] data, int channels)
        {
            int samples = data.Length;

            for (var i = 0; i < samples; i = i + channels)
            {
                float sample = 0;
                for (int c = 0; c < this.channels.Length; c++)
                {
                    sample += gain * this.channels[c].Sample(sampleDelta);
                }

                data[i] = sample;

                if (channels == 2) data[i + 1] = data[i];
            }
        }
    }
}
