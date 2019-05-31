using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using PicoUnity;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class MemoryModuleTests
    {
        [Test]
        public void PeekPoke()
        {
            var mem = new MemoryModule();
            mem.InitRam();

            byte writeVal = (byte)Random.Range(1, 255);

            mem.Poke(MemoryModule.ADDR_GENERAL, writeVal);
            byte readVal = mem.Peek(MemoryModule.ADDR_GENERAL);

            Assert.AreEqual(writeVal, readVal);
        }

        [Test]
        public void Peek2Poke2()
        {
            var mem = new MemoryModule();
            mem.InitRam();

            short writeValNeg = -32765;
            short writeValPos = 32765;

            mem.Poke2(MemoryModule.ADDR_GENERAL, writeValNeg);
            mem.Poke2(MemoryModule.ADDR_GENERAL + 2, writeValPos);

            short readValNeg = mem.Peek2(MemoryModule.ADDR_GENERAL);
            short readValPos = mem.Peek2(MemoryModule.ADDR_GENERAL + 2);

            Assert.AreEqual(writeValNeg, readValNeg);
            Assert.AreEqual(writeValPos, readValPos);
        }

        [Test]
        public void Peek4Poke4()
        {
            var mem = new MemoryModule();
            mem.InitRam();

            int writeValNeg = -214748364;
            int writeValPos = 214748364;

            mem.Poke4(MemoryModule.ADDR_GENERAL, writeValNeg);
            mem.Poke4(MemoryModule.ADDR_GENERAL + 4, writeValPos);

            int readValNeg = mem.Peek4(MemoryModule.ADDR_GENERAL);
            int readValPos = mem.Peek4(MemoryModule.ADDR_GENERAL + 4);

            Assert.AreEqual(writeValNeg, readValNeg);
            Assert.AreEqual(writeValPos, readValPos);
        }

        [Test]
        public void MemSet()
        {
            var mem = new MemoryModule();
            mem.InitRam();

            mem.MemSet(MemoryModule.ADDR_GENERAL, 128, 64);

            for (int i = 0; i < 64; i++)
            {
                byte val = mem.Peek(MemoryModule.ADDR_GENERAL + i);

                Assert.AreEqual(128, val);
            }
        }

        [Test]
        public void MemCpy()
        {
            var mem = new MemoryModule();
            mem.InitRam();

            mem.MemSet(MemoryModule.ADDR_GENERAL, 128, 64);
            mem.MemCpy(MemoryModule.ADDR_GENERAL + 64, MemoryModule.ADDR_GENERAL, 64);

            for (int i = 0; i < 64; i++)
            {
                byte val = mem.Peek(MemoryModule.ADDR_GENERAL + 64 + i);

                Assert.AreEqual(128, val);
            }
        }
    }
}
