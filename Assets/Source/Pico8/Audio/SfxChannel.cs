namespace PicoUnity
{
    internal class SfxChannel
    {
        private int       sfxPointer = -1;
        private int       sfxOffset;
        private float     time;
        private AudioNote note;
        private float     noteLength;

        private MemoryModule mem;

        public bool IsPlaying => sfxPointer > 0;

        public SfxChannel(MemoryModule mem)
        {
            this.mem = mem;
        }

        public void Sfx(int n, int offset = 0, int length = 0)
        {
            if (n < 0) return;

            sfxPointer = MemoryModule.ADDR_SOUND + 68 * n;
            sfxOffset = offset;
            time = 0;
            noteLength = mem.Peek(sfxPointer + 65) / 128f;
        }

        private void PullNode()
        {
            int noteAddr = sfxPointer + sfxOffset;

            byte lo = mem.Peek(noteAddr);
            byte hi = mem.Peek(noteAddr + 1);

            note = AudioNote.Decode(lo, hi);
        }

        private void StepNode()
        {
            sfxOffset += 2;
            if (sfxOffset > 64)
            {
                sfxPointer = -1;
            }
            else
            {
                time -= noteLength;
                PullNode();
            }
        }

        public float Sample(float delta)
        {
            if (sfxPointer < 0) return 0;

            time += delta;
            float t =  time - time % 1 / 22050f;
            float result = AudioSynth.PlayNote(note, t);

            if (time > noteLength) StepNode();

            return result;
        }
    }
}
