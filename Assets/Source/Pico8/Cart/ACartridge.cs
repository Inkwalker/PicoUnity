using UnityEngine;

namespace PicoUnity
{
    public abstract class ACartridge : ScriptableObject
    {
        public const int ROM_SIZE = 0x8000;
        public const int META_SIZE = 0x5;
        public const int CART_SIZE = ROM_SIZE + META_SIZE;

        public abstract byte[] Rom { get; }
        public abstract string Lua { get; }
    }
}
