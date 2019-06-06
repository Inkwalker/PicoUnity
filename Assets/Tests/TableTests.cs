using System.Collections;
using System.Collections.Generic;
using FixedPointy;
using NUnit.Framework;
using PicoMoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class TableTests
    {
        [Test]
        public void Add()
        {
            Script script = new Script();

            var str = @"
                x = { 10 }
                add(x, 8)
                add(x, 5)
                return x[3]
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)5, result.Number);
        }

        [Test]
        public void Del()
        {
            Script script = new Script();

            var str = @"
                a={1,10,2,11,3,12}
			    for item in all(a) do
				    if (item < 10) then del(a, item) end
			    end

                return a[2]
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)11, result.Number);
        }

        [Test]
        public void Foreach()
        {
            Script script = new Script();

            var str = @"
                y = {}

                function f(i)
                    add(y, i)
                end

                x = { 10, 8, 5 }

                foreach(x, f);

                return y[2]
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)8, result.Number);
        }

        [Test]
        public void All()
        {
            Script script = new Script();

            var str = @"
                x = {}
                t = { 10, nil, 8, 5 }

                for v in all(t) do add(x, v) end

                return x[2]
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)8, result.Number);
        }

        [Test]
        public void Pairs()
        {
            Script script = new Script();

            var str = @"
                t = {3, 10, 15}
                x = 0
                for k, v in pairs(t) do
                    x += v
                end

                return x
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)28, result.Number);
        }

        [Test]
        public void Count()
        {
            Script script = new Script();

            var str = @"
                t = {3, 10, nil, 15}

                return count(t)
            ";

            var result = script.DoString(str);

            Assert.AreEqual((Fix)3, result.Number);
        }
    }
}
