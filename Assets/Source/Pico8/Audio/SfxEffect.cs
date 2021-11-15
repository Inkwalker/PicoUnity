namespace PicoUnity.Audio
{
    internal abstract class SfxEffect
    {
        public abstract void Reset();
        public abstract void Step(double deltaTime, Oscillator oscillator);
    }
}
