using System;

namespace PicoUnity
{
    public class AudioModule : EmulatorModule
    {
        private MusicPlayer musicPlayer;
        private SfxChannelManager channelManager;
        private double sampleDelta;
        private float gain = 0.5f;
        private bool reducedSampleRate;

        public AudioModule(MemoryModule memory, int sampleRate)
        {
            channelManager = new SfxChannelManager(memory, 4);

            musicPlayer = new MusicPlayer(memory, channelManager);

            sampleDelta = 1d / sampleRate;

            reducedSampleRate = sampleRate > 44000; //If we run at 44kHz then halve the sampling rate to match original 22kHz
        }

        public void Music(int n, int fade_ms = 0, int channelmask = 0)
        {
            if (n >= 0 && n < 64)
                musicPlayer.Start(n);
            else
                musicPlayer.Stop();
        }

        public void Sfx(int n, int channel = -1, int offset = 0, int length = 0)
        {
            SfxChannel sfxChannel = null;

            if (channel < 0)
            {
                sfxChannel = channelManager.GetFreeChannel();
            }
            else
            {
                sfxChannel = channelManager.GetChannel(channel);
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

            double delta = reducedSampleRate ? sampleDelta * 2 : sampleDelta;

            for (var i = 0; i < samples; i = i + channels)
            {
                float sample = 0;
                for (int c = 0; c < channelManager.Count; c++)
                {
                    sample += gain * channelManager.GetChannel(c).Sample(delta);
                }

                for (int j = 0; j < channels; j++)
                {
                    data[i + j] = sample;

                    if (reducedSampleRate)
                        data[i + channels + j] = sample;
                }

                if (reducedSampleRate) i += channels;

                musicPlayer.Update();
            }
        }
    }
}
