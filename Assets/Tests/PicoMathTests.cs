using System.Collections;
using System.Collections.Generic;
using FixedPointy;
using NUnit.Framework;
using PicoMoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class PicoMathTests
    {
        [Test]
        public void Max()
        {
            Script script = new Script();

            var str = @"
                x = max(-10)
                y = max(10, -50)
                z = max(nil, 5)
                return x, y, z
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)0,  result.Tuple[0].Number);
            Assert.AreEqual((Fix)10, result.Tuple[1].Number);
            Assert.AreEqual((Fix)5,  result.Tuple[2].Number);
        }

        [Test]
        public void Min()
        {
            Script script = new Script();

            var str = @"
                x = min(-10)
                y = min(-10, 50)
                z = min(nil, 5)
                return x, y, z
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)(-10), result.Tuple[0].Number);
            Assert.AreEqual((Fix)(-10), result.Tuple[1].Number);
            Assert.AreEqual((Fix)0,     result.Tuple[2].Number);
        }

        [Test]
        public void Mid()
        {
            Script script = new Script();

            var str = @"
                x = mid(-10, nil, 10)
                y = mid(10, 5, -50)
                z = mid(0, 0.1, 1)
                return x, y, z
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)0, result.Tuple[0].Number);
            Assert.AreEqual((Fix)5, result.Tuple[1].Number);
            Assert.AreEqual((Fix)0.1, result.Tuple[2].Number);
        }


        [Test]
        public void Atan2()
        {
            Script script = new Script();

            var str = @"
                x = atan2(1, 1)
                y = atan2(-1, -1)
                z = atan2(0, 1)
                a = atan2(1, 0)
                b = atan2(0, 0)
                c = atan2(99, 99)
                return x, y, z, a, b, c
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)0.875, result.Tuple[0].Number);
            Assert.AreEqual((Fix)0.375, result.Tuple[1].Number);
            Assert.AreEqual((Fix)0.75, result.Tuple[2].Number);
            Assert.AreEqual((Fix)0, result.Tuple[3].Number);
            Assert.AreEqual((Fix)0.75, result.Tuple[4].Number);
            Assert.AreEqual((Fix)0.875, result.Tuple[5].Number);
        }

        [Test]
        public void Sin()
        {
            Script script = new Script();

            var str = @"
                x = sin(0)
                y = sin(0.5)
                z = sin(0.75)
                return x, y, z
            ";

            var result = script.DoString(str);

            Assert.IsTrue(Approximately(0, result.Tuple[0].Number));
            Assert.IsTrue(Approximately(0, result.Tuple[1].Number));
            Assert.IsTrue(Approximately(1, result.Tuple[2].Number));
        }

        [Test]
        public void Cos()
        {
            Script script = new Script();

            var str = @"
                x = cos(0)
                y = cos(0.5)
                z = cos(0.75)
                return x, y, z
            ";

            var result = script.DoString(str);

            Assert.IsTrue(Approximately(1, result.Tuple[0].Number));
            Assert.IsTrue(Approximately(-1, result.Tuple[1].Number));
            Assert.IsTrue(Approximately(0, result.Tuple[2].Number));
        }

        [Test]
        public void Rnd()
        {
            Script script = new Script();

            var str = @"
                x = rnd(1000)
                y = rnd(1000)
                return x, y
            ";

            var result = script.DoString(str);

            Assert.AreNotEqual(result.Tuple[0].Number, result.Tuple[1].Number);
        }

        [Test]
        public void Srand()
        {
            Script script = new Script();

            var str = @"
                srand(500);
                x = rnd(1000)
                srand(500);
                y = rnd(1000)
                return x, y
            ";

            var result = script.DoString(str);

            Assert.AreEqual(result.Tuple[0].Number, result.Tuple[1].Number);
        }

        [Test]
        public void Shr()
        {
            Script script = new Script();

            var str = @"
                x = shr(1, 3)
                y = shr(-1, 3)
                return x, y
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)0.125, result.Tuple[0].Number);
            Assert.AreEqual((Fix)(-0.125), result.Tuple[1].Number);
        }


        [Test]
        public void Shl()
        {
            Script script = new Script();

            var str = @"
                x = shl(1, 3)
                y = shl(0.125, 3)
                return x, y
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)8, result.Tuple[0].Number);
            Assert.AreEqual((Fix)1, result.Tuple[1].Number);
        }

        [Test]
        public void Bxor()
        {
            Script script = new Script();

            var str = @"
                x = bxor(0x5, 0x9)
                return x
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)0xc, result.Number);
        }

        [Test]
        public void Bor()
        {
            Script script = new Script();

            var str = @"
                x = bor(0x5, 0x9)
                return x
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)0xd, result.Number);
        }

        [Test]
        public void Bnot()
        {
            Script script = new Script();

            var str = @"
                x = bnot(0xb)
                return x
            ";

            var result = script.DoString(str);

            Assert.IsTrue(Approximately(-11, result.Number));
        }

        [Test]
        public void Band()
        {
            Script script = new Script();

            var str = @"
                x = band(0x7, 0xd)
                return x
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)5, result.Number);
        }

        [Test]
        public void Abs()
        {
            Script script = new Script();

            var str = @"
                x = abs(-5)
                return x
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)5, result.Number);
        }

        [Test]
        public void Flr()
        {
            Script script = new Script();

            var str = @"
                x = flr(5.9)
                y = flr(-5.2)
                return x, y
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)5, result.Tuple[0].Number);
            Assert.AreEqual((Fix)(-6), result.Tuple[1].Number);
        }

        [Test]
        public void Ceil()
        {
            Script script = new Script();

            var str = @"
                x = ceil(5.9)
                y = ceil(-5.2)
                return x, y
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)6, result.Tuple[0].Number);
            Assert.AreEqual((Fix)(-5), result.Tuple[1].Number);
        }

        [Test]
        public void Sqrt()
        {
            Script script = new Script();

            var str = @"
                x = sqrt(144)
                return x
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)12, result.Number);
        }

        [Test]
        public void Rotl()
        {
            Script script = new Script();

            var str = @"
                x = rotl(0.125, 3)
                return x
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)1, result.Number);
        }

        [Test]
        public void Rotr()
        {
            Script script = new Script();

            var str = @"
                x = rotr(-4096, 12)
                return x
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)15, result.Number);
        }

        [Test]
        public void Lshr()
        {
            Script script = new Script();

            var str = @"
                x = lshr(-1, 3)
                return x
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)8191.875, result.Number);
        }

        private bool Approximately(Fix a, Fix b)
        {
            return FixMath.Abs(b - a) < (Fix)0.001;
        }
    }
}
