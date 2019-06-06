using FixedPointy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PicoUnity
{
    public class MemoryModule : EmulatorModule
    {
        #region Constants
        public const int
            ADDR_SPRITE = 0x0,
            ADDR_SHARED = 0x1000,
            ADDR_MAP = 0x2000,
            ADDR_FLAGS = 0x3000,
            ADDR_MUSIC = 0x3100,
            ADDR_SOUND = 0x3200,
            ADDR_GENERAL = 0x4300,
            ADDR_CARTDATA = 0x5e00,

            ADDR_DRAW_STATE = 0x5f00,
            ADDR_PALETTE_0 = 0x5f00,
            ADDR_PALETTE_1 = 0x5f10,
            ADDR_CLIP_X0 = 0x5f20,
            ADDR_CLIP_Y0 = 0x5f21,
            ADDR_CLIP_X1 = 0x5f22,
            ADDR_CLIP_Y1 = 0x5f23,
            ADDR_PEN_COLOR = 0x5f25,
            ADDR_CURSOR_X = 0x5f26,
            ADDR_CURSOR_Y = 0x5f27,
            ADDR_CAMERA_X = 0x5f28,
            ADDR_CAMERA_Y = 0x5f2a,
            ADDR_SCREEN_EFFECT = 0x5f2c,
            ADDR_DEVKIT = 0x5f2d,
            ADDR_PALETTE_LOCK = 0x5f2e,
            ADDR_CANCEL_PAUSE = 0x5f30,
            ADDR_FILL = 0x5f31,
            ADDR_FILL_T = 0x5f33,
            ADDR_PRO_COLOR = 0x5f34,
            ADDR_LINE_X = 0x5f3c,
            ADDR_LINE_Y = 0x5f3e,

            ADDR_HARDWARE = 0x5f40,
            ADDR_GPIO = 0x5f80,
            ADDR_VRAM = 0x6000;

        public const int
            SIZE_SPRITE = 0x1000,
            SIZE_SHARED = 0x1000,
            SIZE_MAP = 0x1000,
            SIZE_FLAGS = 0x100,
            SIZE_MUSIC = 0x100,
            SIZE_SOUND = 0x1100,
            SIZE_GENERAL = 0x1b00,
            SIZE_CARTDATA = 0x100,
            SIZE_DRAW_STATE = 0x40,
            SIZE_HARDWARE = 0x40,
            SIZE_GPIO = 0x40,
            SIZE_VRAM = 0x2000,
            SIZE_TOTAL = 0x8000;
        #endregion

        readonly byte[] ram;
        readonly byte[] ramMap;

        readonly List<IMemoryListener> ramListeners;

        public MemoryModule()
        {
            ram = new byte[SIZE_TOTAL];
            ramMap = new byte[SIZE_TOTAL];

            ramListeners = new List<IMemoryListener>();
            ramListeners.Add(null); //reserve index 0
        }

        public void InitRam()
        {
            ram[ADDR_PALETTE_0] = 0x10;
            ram[ADDR_PALETTE_1] = 0;
            ram[ADDR_CLIP_X1] = 127;
            ram[ADDR_CLIP_Y1] = 127;
            ram[ADDR_PEN_COLOR] = 6;
            for (byte i = 1; i < 16; i++)
            {
                ram[ADDR_PALETTE_0 + i] = i;
                ram[ADDR_PALETTE_1 + i] = i;
            }
        }

        public void AddMemoryListener(IMemoryListener listener)
        {
            ramListeners.Add(listener);
            byte index = (byte)(ramListeners.Count - 1);
            ArraySet(ramMap, listener.Address, index, listener.Length);
        }

        public void RemoveMemoryListener(IMemoryListener listener)
        {
            if (ramListeners.Remove(listener))
            {
                ArraySet(ramMap, listener.Address, 0, listener.Length);
            }
        }

        public void CopyTo(byte[] dst, int src_addr, int dst_addr, int len)
        {
            Buffer.BlockCopy(ram, src_addr, dst, dst_addr, len);
        }

        public void CopyFrom(byte[] src, int src_addr, int dst_addr, int len)
        {
            Buffer.BlockCopy(src, src_addr, ram, dst_addr, len);

            for (int i = 1; i < ramListeners.Count; i++)
            {
                var ls = ramListeners[i];
                if (dst_addr < ls.Address + ls.Length && dst_addr + len >= ls.Address) ls.OnMemSet();
            }
        }

        private static void ArraySet(byte[] array, int start, byte val, int len)
        {
            int block = 4;
            int index = start;
            int length = Math.Min(start + block, start + len);

            while (index < length)
            {
                array[index++] = val;
            }

            length = start + len;
            while (index < length)
            {
                Buffer.BlockCopy(array, start, array, index, Math.Min(block, length - index));
                index += block;
                block *= 2;
            }
        }

        public byte PeekHalf(int offset, int element)
        {
            int addr = offset + element / 2;

            if (addr >= SIZE_TOTAL) return 0;

            byte result = Peek(addr);
            if (element % 2 == 0)
            {
                result = (byte)(result & 0x0f);
            }
            else
            {
                result = (byte)(result >> 4);
            }

            return result;
        }

        public void PokeHalf(int offset, int element, byte val)
        {
            int addr = offset + element / 2;

            byte result = Peek(addr);
            if (element % 2 == 0)
            {
                result = (byte)(result & 0xf0);
                val    = (byte)(val    & 0x0f);
            }
            else
            {
                result = (byte)(result & 0x0f);
                val    = (byte)(val << 4);
            }
            Poke(addr, (byte)(result + val));
        }

        #region Pico8 API

        public byte Peek(int addr)
        {
            if (addr < 0 || addr >= SIZE_TOTAL)
                return 0;

            return ram[addr];
        }
        public short Peek2(int addr)
        {
            if (addr < 0 || addr + 1 >= SIZE_TOTAL) return 0;
            return (short)(((sbyte)ram[addr + 1] << 8) | ram[addr]);
        }

        public Fix Peek4(int addr)
        {
            if (addr < 0 || addr + 3 >= SIZE_TOTAL) return 0;

            int raw = ((sbyte)ram[addr + 3] << 24) | (ram[addr + 2] << 16) | (ram[addr + 1] << 8) | ram[addr];

            return new Fix(raw);
        }

        public void Poke(int addr, byte val)
        {
            if (addr < 0 || addr >= SIZE_TOTAL) return;

            ram[addr] = val;

            var i = ramMap[addr];
            if (i > 0) ramListeners[i].OnPoke(addr);
        }

        public void Poke2(int addr, short val)
        {
            Poke(addr, (byte)(val & 0xff));
            Poke(addr + 1, (byte)(val >> 8));
        }

        public void Poke4(int addr, Fix val)
        {
            int raw = val.Raw;

            Poke(addr,     (byte)(raw & 0xff));
            Poke(addr + 1, (byte)((raw >> 8) & 0xff));
            Poke(addr + 2, (byte)((raw >> 16) & 0xff));
            Poke(addr + 3, (byte)((raw >> 24) & 0xff));
        }

        public void MemSet(int start, byte val, int len)
        {
            ArraySet(ram, start, val, len);

            for (int i = 1; i < ramListeners.Count; i++)
            {
                var ls = ramListeners[i];
                if (start < ls.Address + ls.Length && start + len >= ls.Address) ls.OnMemSet();
            }
        }

        public void MemCpy(int dst_addr, int src_addr, int len)
        {
            Buffer.BlockCopy(ram, src_addr, ram, dst_addr, len);

            for (int i = 1; i < ramListeners.Count; i++)
            {
                var ls = ramListeners[i];
                if (dst_addr < ls.Address + ls.Length && dst_addr + len >= ls.Address) ls.OnMemSet();
            }
        }

        #endregion

        public override ApiTable GetApiTable()
        {
            return new ApiTable()
            {
                { "peek",   (Func<int, byte>)        Peek },
                { "peek2",  (Func<int, short>)       Peek2 },
                { "peek4",  (Func<int, Fix>)         Peek4 },
                { "poke",   (Action<int, byte>)      Poke },
                { "poke2",  (Action<int, short>)     Poke2 },
                { "poke4",  (Action<int, Fix>)       Poke4 },
                { "memcpy", (Action<int, int, int>)  MemCpy },
                { "memset", (Action<int, byte, int>) MemSet },
            };
        }
    }
}
