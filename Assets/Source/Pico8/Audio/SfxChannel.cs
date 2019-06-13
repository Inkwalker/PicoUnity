using System.Collections.Generic;

namespace PicoUnity
{
    internal class SfxChannel
    {
        private int       sfxPointer = -1;
        private int       sfxOffset;
        private double    time;
        private float     noteLength;

        private MemoryModule mem;

        private Oscillator oscillator;

        public bool IsPlaying => sfxPointer > 0;

        public SfxChannel(MemoryModule mem)
        {
            this.mem = mem;

            oscillator = new Oscillator(new AudioNote() { hz = 440 });
        }

        public void Sfx(int n, int offset = 0, int length = 0)
        {
            if (n < 0) return;

            oscillator.Stop();

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

            var note = AudioNote.Decode(lo, hi);

            oscillator.Reset(note);
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

        public float Sample(double delta)
        {
            if (sfxPointer < 0) return 0;

            time = (time + delta) % 10;

            if (time > noteLength)
            {
                oscillator.Stop();
            }
            if (oscillator.Stopped)
            {
                StepNode();
            }

            return oscillator.Sample(delta);
        }
    }
}
