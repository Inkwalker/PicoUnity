using FixedPointy;
using System;
using UnityEngine;

namespace PicoUnity
{
    public class PersistentDataStorage : EmulatorModule, IMemoryListener, IDisposable
    {
        byte[] buffer;
        string slotID;
        bool dirty;
        MemoryModule mem;

        public bool Ready { get { return buffer != null; } }

        public int Address { get; private set; }
        public int Length { get; private set; }

        public PersistentDataStorage(MemoryModule mem)
        {
            Address = MemoryModule.ADDR_CARTDATA;
            Length = MemoryModule.SIZE_CARTDATA;

            this.mem = mem;
        }

        public void Flush()
        {
            if (Ready && dirty)
            {
                var slotData = Convert.ToBase64String(buffer);
                PlayerPrefs.SetString(slotID, slotData);
                PlayerPrefs.Save();
            }
        }

        public void Dispose()
        {
            Flush();
        }

        public void OnPoke(int addr)
        {
            buffer[addr - Address] = mem.Peek(addr);
            dirty = true;
        }

        public void OnMemSet()
        {
            mem.CopyTo(buffer, Address, 0, Length);
            dirty = true;
        }

        #region PICO8 API
        public bool CartData(string id)
        {
            if (Ready) Flush();

            slotID = id;

            bool hasKey = PlayerPrefs.HasKey(slotID);

            if (hasKey)
            {
                var slotData = PlayerPrefs.GetString(slotID);
                var data = Convert.FromBase64String(slotData);

                if (data.Length != Length)
                {
                    buffer = new byte[Length];
                    Buffer.BlockCopy(data, 0, buffer, 0, Mathf.Min(Length, data.Length));
                }
                else
                {
                    buffer = data;
                }
            }
            else
            {
                buffer = new byte[Length];
            }

            mem.CopyFrom(buffer, 0, Address, Length);

            return hasKey;
        }

        public Fix Dget(int? index)
        {
            int i = index.HasValue ? index.Value : 0;

            if (i < 0 || i >= Length / 4) return 0;

            return mem.Peek4(Address + i * 4);
        }

        public void Dset(int? index, Fix? val)
        {
            int i = index.HasValue ? index.Value : 0;
            val = val.HasValue ? val : 0;

            if (i < 0 || i >= Length / 4) return;

            mem.Poke4(Address + i * 4, val.Value);
        }
        #endregion

        public override ApiTable GetApiTable()
        {
            return new ApiTable()
            {
                {"cartdata", (Func<string, bool>) CartData },
                {"dget", (Func<int?, Fix>) Dget },
                {"dset", (Action<int?, Fix?>) Dset }
            };
        }

        public override void OnFrameEnd(float dt)
        {
            Flush();
        }
    }
}
