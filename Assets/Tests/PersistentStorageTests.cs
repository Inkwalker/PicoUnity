using System.Collections;
using System.Collections.Generic;
using FixedPointy;
using NUnit.Framework;
using PicoUnity;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class PersistentStorageTests
    {
        [Test]
        public void CartData()
        {
            var mem = new MemoryModule();
            mem.InitRam();

            var storage = new PersistentDataStorage(mem);
            mem.AddMemoryListener(storage);

            Fix writeValNeg = (Fix)(-354.245);
            Fix writeValPos = (Fix)325.246;

            storage.CartData("test_slot_0");
            storage.Dset(0, writeValNeg);
            storage.Dset(1, writeValPos);
            storage.Dset(63, writeValPos);

            storage.CartData("test_slot_1");
            storage.Dset(0, writeValPos);

            Assert.AreNotEqual(writeValNeg, storage.Dget(0));

            storage.CartData("test_slot_0");
            Assert.AreEqual(writeValNeg, storage.Dget(0));
            Assert.AreEqual(writeValPos, storage.Dget(1));
            Assert.AreEqual(writeValPos, storage.Dget(63));
        }
    }
}
