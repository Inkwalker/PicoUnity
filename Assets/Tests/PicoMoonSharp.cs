using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PicoMoonSharp;
using PicoMoonSharp.Interpreter;
using FixedPointy;

namespace Tests
{
    public class PicoMoonSharp
    {
        [Test]
        public void IfShorthand()
        {
            Script script = new Script();

            var str = @"
                x = 5
                if (x > 0) return true else return false 
            ";

            var result = script.DoString(str);

            Assert.AreEqual(true, result.Boolean);
        }

        [Test]
        public void UnaryAssignments()
        {
            Script script = new Script();

            // +=
            var str = @"
                x = 5
                x += 5
                return x
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)10, result.Number);

            // -=
            str = @"
                x = 5
                x -= 5
                return x
            ";

            result = script.DoString(str);

            Assert.AreEqual((Fix)0, result.Number);

            // *=
            str = @"
                x = 5
                x *= 2
                return x
            ";

            result = script.DoString(str);

            Assert.AreEqual((Fix)10, result.Number);

            // /=
            str = @"
                x = 10
                x /= 2
                return x
            ";

            result = script.DoString(str);

            Assert.AreEqual((Fix)5, result.Number);

            // %=
            str = @"
                x = 15
                x %= 10
                return x
            ";

            result = script.DoString(str);

            Assert.AreEqual((Fix)5, result.Number);
        }

        [Test]
        public void FixedMath()
        {
            Script script = new Script();

            // +=
            var str = @"
                x = 5 + 10.5
                y = 11 - -2.5
                z = 10 * 2
                a = 30 / 2
                b = 15 % 10
                c = 2 ^ 8
                return x, y, z, a, b, c
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)15.5, result.Tuple[0].Number, "x");
            Assert.AreEqual((Fix)13.5, result.Tuple[1].Number, "y");
            Assert.AreEqual((Fix)20, result.Tuple[2].Number, "z");
            Assert.AreEqual((Fix)15, result.Tuple[3].Number, "a");
            Assert.AreEqual((Fix)5, result.Tuple[4].Number, "b");
            Assert.AreEqual((Fix)256, result.Tuple[5].Number, "c");
        }

        [Test]
        public void BinaryLiterals()
        {
            Script script = new Script();

            // +=
            var str = @"
                x = 0b1111
                y = 0b1111.0111111111111111
                return x, y
            ";

            var result = script.DoString(str);

            Debug.Log(new Fix(0b00000000_00001111_01111111_11111111).ToString());
            Debug.Log(result.Tuple[1].Number.ToString());

            Assert.AreEqual((Fix)15, result.Tuple[0].Number, "x");
            Assert.Less(result.Tuple[1].Number, (Fix)15.45, "y");
            Assert.Greater(result.Tuple[1].Number, (Fix)15.55, "y");
        }

        [Test]
        public void HexLiterals()
        {
            Script script = new Script();

            // +=
            var str = @"
                x = 0xff
                y = 0xff.7fff
                return x, y
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)0xff, result.Tuple[0].Number, "x");
            Assert.Less(result.Tuple[1].Number, (Fix)255.45, "y");
            Assert.Greater(result.Tuple[1].Number, (Fix)255.55, "y");
        }
    }
}
