namespace PicoUnity
{ 
    //TODO: channel reservation
    //TODO: restore music after it was overridden by a sfx
    internal class MusicPlayer
    {
        private MemoryModule mem;
        private SfxChannelManager channelManager;

        private bool[] playingOnChanels;
        private int frame_n;

        public bool Playing { get; private set; }

        public MusicPlayer(MemoryModule mem, SfxChannelManager channelManager)
        {
            this.mem = mem;
            this.channelManager = channelManager;

            playingOnChanels = new bool[channelManager.Count];
        }

        public void Start(int n)
        {
            Playing = true;
            frame_n = n;
            PullFrame();
        }

        public void Stop()
        {
            Playing = false;
            for (int i = 0; i < playingOnChanels.Length; i++)
            {
                if (playingOnChanels[i])
                    channelManager.GetChannel(i).Sfx(-1);

                playingOnChanels[i] = false;
            }
        }

        public void Update()
        {
            if (!Playing) return;

            for (int i = 0; i < playingOnChanels.Length; i++)
            {
                if (playingOnChanels[i])
                {
                    if (channelManager.GetChannel(i).IsPlaying) return;

                    if (IsStopFrame(frame_n))
                    {
                        Stop();
                        return;
                    }
                    else 
                    {
                        if (IsLoopEndFrame(frame_n))
                            frame_n = FindLoopStart(frame_n);
                        else
                        {
                            frame_n++;
                            if (frame_n > 63)
                            {
                                Stop();
                                return;
                            }
                        }

                        PullFrame();
                        return;
                    } 
                }
            }
        }

        private void PullFrame()
        {
            int framePointer = MemoryModule.ADDR_MUSIC + frame_n * 4;

            int ch_0 = mem.Peek(framePointer);
            int ch_1 = mem.Peek(framePointer + 1);
            int ch_2 = mem.Peek(framePointer + 2);
            int ch_3 = mem.Peek(framePointer + 3);

            bool ch_0_en = (ch_0 & 0x40) == 0; //bit 6
            bool ch_1_en = (ch_1 & 0x40) == 0; //bit 6
            bool ch_2_en = (ch_2 & 0x40) == 0; //bit 6
            bool ch_3_en = (ch_3 & 0x40) == 0; //bit 6

            int ch_0_sfx = ch_0 & 0x3F; //bits 0 - 5
            int ch_1_sfx = ch_1 & 0x3F; //bits 0 - 5
            int ch_2_sfx = ch_2 & 0x3F; //bits 0 - 5
            int ch_3_sfx = ch_3 & 0x3F; //bits 0 - 5

            if (ch_0_en) channelManager.GetChannel(0).Sfx(ch_0_sfx);
            if (ch_1_en) channelManager.GetChannel(1).Sfx(ch_1_sfx);
            if (ch_2_en) channelManager.GetChannel(2).Sfx(ch_2_sfx);
            if (ch_3_en) channelManager.GetChannel(3).Sfx(ch_3_sfx);

            playingOnChanels[0] = ch_0_en;
            playingOnChanels[1] = ch_1_en;
            playingOnChanels[2] = ch_2_en;
            playingOnChanels[3] = ch_3_en;
        }

        private int FindLoopStart(int loopEnd_n)
        {
            for (int i = loopEnd_n; i >= 0; i--)
            {
                if (IsLoopStartFrame(i)) return i;
            }

            return 0;
        }

        private bool IsLoopStartFrame(int frame)
        {
            int pointer = GetPointer(frame);

            int ch_0 = mem.Peek(pointer);
            bool loop_start = (ch_0 & 0x80) > 0; //bit 7

            return loop_start;
        }

        private bool IsLoopEndFrame(int frame)
        {
            int pointer = GetPointer(frame);

            int ch_1 = mem.Peek(pointer + 1);
            bool loop_end = (ch_1 & 0x80) > 0; //bit 7

            return loop_end;
        }

        private bool IsStopFrame(int frame)
        {
            int pointer = GetPointer(frame);

            int ch_2 = mem.Peek(pointer + 2);
            bool stop = (ch_2 & 0x80) > 0;
            return stop;
        }

        private int GetPointer(int frame)
        {
            return MemoryModule.ADDR_MUSIC + frame * 4;
        }
    }
}