// Disable warnings about XML documentation
#pragma warning disable 1591

using FixedPointy;
using System;
using System.Collections.Generic;
using System.Text;

namespace PicoMoonSharp.Interpreter.CoreLib
{
	/// <summary>
	/// Class implementing Pico8 table functions 
	/// </summary>
	[MoonSharpModule]
	public class TableModule
	{
        [MoonSharpModuleMethod]
        public static DynValue add(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue vlist = args.AsType(0, "add", DataType.Table, false);
            DynValue val = args[1];

            if (args.Count > 2)
                throw new ScriptRuntimeException("wrong number of arguments to 'add'");

            vlist.Table.Append(val);

            return DynValue.Nil;
        }


        [MoonSharpModuleMethod]
		public static DynValue del(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue vlist = args.AsType(0, "del", DataType.Table, false);
            DynValue val = args[1];
			DynValue ret = DynValue.Nil;

			if (args.Count > 2)
				throw new ScriptRuntimeException("wrong number of arguments to 'del'");

			int len = GetTableLength(executionContext, vlist);
			Table list = vlist.Table;

            int index = -1;

            for (int i = 0; i < len; i++)
            {
                if (list.Get(i).Equals(val))
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                for (int i = index; i <= len; i++)
                {
                    list.Set(i, list.Get(i + 1));
                }
            }

			return DynValue.Nil;
		}

		private static int GetTableLength(ScriptExecutionContext executionContext, DynValue vlist)
		{
			DynValue __len = executionContext.GetMetamethod(vlist, "__len");

			if (__len != null)
			{
				DynValue lenv = executionContext.GetScript().Call(__len, vlist);

				Fix? len = lenv.CastToNumber();

				if (len == null)
					throw new ScriptRuntimeException("object length is not a number");

				return (int)len;
			}
			else
			{
				return (int)vlist.Table.Length;
			}
		}
	}
}
