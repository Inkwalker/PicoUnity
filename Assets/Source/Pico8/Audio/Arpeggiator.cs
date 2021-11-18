namespace PicoUnity.Audio
{
    internal class Arpeggiator 
    {
        public float[] Notes { get; }
        public float NoteTime { get; set; }
        public bool Active { get; set; }

        private double timer;
        private int noteIndex;

        public Arpeggiator()
        {
            Notes = new float[4];
        }

        public void SetNotes(float hz_0, float hz_1, float hz_2, float hz_3)
        {
            Notes[0] = hz_0;
            Notes[1] = hz_1;
            Notes[2] = hz_2;
            Notes[3] = hz_3;
        }

        public void Reset()
        {
            timer = 0;
            noteIndex = 0;
        }

        public float Step(double deltaTime)
        {
            timer = timer + deltaTime;

            if (timer > NoteTime)
            {
                timer = 0;
                noteIndex++;
                if (noteIndex > 3)
                    noteIndex = 0;
            }

            return Notes[noteIndex];
        }
    }
}