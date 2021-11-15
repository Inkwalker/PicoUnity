namespace PicoUnity.Audio
{
    internal class Voice
    {
        public Oscillator Oscillator { get; private set; }
        public EnvelopeADSR ADSR { get; private set; }
        public bool Active => !ADSR.Stopped;
        public bool Triggered => ADSR.Triggered;
        public bool Released => ADSR.Released;

        private SfxEffect effect;

        public Voice(AudioSynth.Waveform waveform)
        {
            Oscillator = new Oscillator(440, waveform);
            ADSR = new EnvelopeADSR(0.01, 0, 1, 0.02);
        }

        public Voice(AudioSynth.Waveform waveform, double attack, double decay, float sustain, double release)
        {
            Oscillator = new Oscillator(440, waveform);
            ADSR = new EnvelopeADSR(attack, decay, sustain, release);
        }

        public void Trigger()
        {
            if (ADSR.Triggered == false)
            {
                ADSR.Trigger();
                if (effect != null) effect.Reset();
            }
        }

        public void Trigger(float frequency, float volume = 1)
        {
            Oscillator.Frequency = frequency;
            Oscillator.Volume = volume;

            Trigger();
        }

        public void SetEffect(SfxEffect effect)
        {
            this.effect = effect;
        }

        public void Release()
        {
            ADSR.Release();
        }

        public double Step(double deltaTime)
        {
            if (ADSR.Stopped) return 0;
            ADSR.Step(deltaTime);

            if (effect != null) effect.Step(deltaTime, Oscillator);

            return Oscillator.Step(deltaTime) * ADSR.Amplitude;
        }
    }
}
