namespace PicoUnity
{
    public interface IMemoryListener
    {
        int Address { get; }
        int Length { get; }

        void OnPoke(int addr);
        void OnMemSet();
    }
}
