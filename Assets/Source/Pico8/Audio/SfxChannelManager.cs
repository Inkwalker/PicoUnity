namespace PicoUnity
{
    internal class SfxChannelManager
    {
        private SfxChannel[] channels;

        public int Count => channels.Length;

        public SfxChannelManager(MemoryModule memory, int channels)
        {
            this.channels = new SfxChannel[channels];

            for (int i = 0; i < this.channels.Length; i++)
            {
                this.channels[i] = new SfxChannel(memory);
            }
        }

        public SfxChannel GetChannel(int n)
        {
            return channels[n];
        }

        public SfxChannel GetFreeChannel()
        {
            foreach (var channel in channels)
            {
                if (channel.IsPlaying == false)
                {
                    return channel;
                }
            }

            return null;
        }
    }
}