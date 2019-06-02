using FixedPointy;
using System;

namespace PicoMoonSharp.Interpreter.CoreLib
{
    [MoonSharpModule]
    public class PicoMathModule
    {
        private static Random random = new Random();

        private static DynValue exec1(CallbackArguments args, string funcName, Func<Fix, Fix> func)
        {
            DynValue arg = args.AsType(0, funcName, DataType.Number, true);
            return DynValue.NewNumber(func(arg.Number));
        }

        private static DynValue exec2(CallbackArguments args, string funcName, Func<Fix, Fix, Fix> func)
        {
            DynValue arg = args.AsType(0, funcName, DataType.Number, true);
            DynValue arg2 = args.AsType(1, funcName, DataType.Number, true);
            return DynValue.NewNumber(func(arg.Number, arg2.Number));
        }

        [MoonSharpModuleMethod]
        public static DynValue abs(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "abs", (x) => FixMath.Abs(x));
        }

        [MoonSharpModuleMethod]
        public static DynValue atan2(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec2(args, "atan2", (dx, dy) =>
            {
                if (dx == 0 && dy == 0) return (Fix)0.75; //special case
                var a = FixMath.Atan2(dy, dx) / 360;
                return a > 0 ? 1 - a : -a;
            });
        }

        [MoonSharpModuleMethod]
        public static DynValue ceil(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "ceil", (x) => FixMath.Ceiling(x));
        }

        [MoonSharpModuleMethod]
        public static DynValue cos(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "cos", (x) => FixMath.Cos(x * 360));
        }

        [MoonSharpModuleMethod]
        public static DynValue flr(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "flr", (x) => FixMath.Floor(x));
        }

        [MoonSharpModuleMethod]
        public static DynValue lshr(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec2(args, "lshr", (x, y) => new Fix((int)((uint)x.Raw >> (int)y)));
        }

        [MoonSharpModuleMethod]
        public static DynValue max(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec2(args, "max", (x, y) => x > y ? x : y);
        }

        [MoonSharpModuleMethod]
        public static DynValue mid(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue x = args.AsType(0, "mid", DataType.Number, true);
            DynValue y = args.AsType(1, "mid", DataType.Number, true);
            DynValue z = args.AsType(2, "mid", DataType.Number, true);

            Func<Fix, Fix, Fix> min = (a, b) => a < b ? a : b;
            Func<Fix, Fix, Fix> max = (a, b) => a > b ? a : b;

            var mid = max(min(x.Number, y.Number), min(max(x.Number, y.Number), z.Number));

            return DynValue.NewNumber(mid);
        }

        [MoonSharpModuleMethod]
        public static DynValue min(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec2(args, "min", (x, y) => x < y ? x : y);
        }

        [MoonSharpModuleMethod]
        public static DynValue sin(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "sin", (x) => -FixMath.Sin(x * 360));
        }

        [MoonSharpModuleMethod]
        public static DynValue rnd(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue arg = args.AsType(0, "rnd", DataType.Number, true);

            double x = arg.IsNil() ? 1 : (double)arg.Number;

            return DynValue.NewNumber((Fix)(random.NextDouble() * x));
        }

        [MoonSharpModuleMethod]
        public static DynValue srand(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue arg = args.AsType(0, "srand", DataType.Number, true);

            random = new Random((int)arg.Number);

            return DynValue.Nil;
        }

        [MoonSharpModuleMethod]
        public static DynValue sqrt(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "sqrt", (x) => FixMath.Sqrt(x));
        }

        [MoonSharpModuleMethod]
        public static DynValue rotr(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec2(args, "rotr", (x, y) => 
            {
                uint num = (uint)x.Raw;
                int bits = (int)y;
                return new Fix((int)((num >> bits) | (num << (Fix.FractionalBits + Fix.IntegerBits - bits))));
            });
        }

        [MoonSharpModuleMethod]
        public static DynValue rotl(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec2(args, "rotl", (x, y) =>
            {
                uint num = (uint)x.Raw;
                int bits = (int)y;
                return new Fix((int)((num << bits) | (num >> (Fix.FractionalBits + Fix.IntegerBits - bits))));
            });
        }

        [MoonSharpModuleMethod]
        public static DynValue shr(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec2(args, "shr", (x, y) => x >> (int)y);
        }

        [MoonSharpModuleMethod]
        public static DynValue shl(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec2(args, "shl", (x, y) => x << (int)y);
        }

        [MoonSharpModuleMethod]
        public static DynValue bxor(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec2(args, "bxor", (x, y) => new Fix(x.Raw ^ y.Raw));
        }

        [MoonSharpModuleMethod]
        public static DynValue bor(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec2(args, "bor", (x, y) => new Fix(x.Raw | y.Raw));
        }

        [MoonSharpModuleMethod]
        public static DynValue bnot(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "bnot", (x) => new Fix(~x.Raw));
        }

        [MoonSharpModuleMethod]
        public static DynValue band(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec2(args, "band", (x, y) => new Fix(x.Raw & y.Raw));
        }
    }
}
