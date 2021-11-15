
namespace PicoUnity.Audio
{
    internal class Oscillator
    {
        public float Frequency { get; set; }
        public float Volume { get; set; }
        public AudioSynth.Waveform Waveform { get; set; }

        private float phase;

        public Oscillator(float frequency, AudioSynth.Waveform waveform)
        {
            Volume = 1;

            Frequency = frequency;
            Waveform = waveform;
        }

        public void Reset()
        {
            phase = 0;
        }

        public double Step(double deltaTime)
        {
            var sample = AudioSynth.Oscillate(Waveform, phase) * Volume;
            phase = (phase + (float)(deltaTime * Frequency)) % 1;
            return sample;
        }
    }
}
