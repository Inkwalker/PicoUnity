// Disable warnings about XML documentation
#pragma warning disable 1591

using System.Collections.Generic;

namespace PicoMoonSharp.Interpreter.CoreLib
{
    /// <summary>
    /// Class implementing Pico8 coroutine functions 
    /// </summary>
    [MoonSharpModule]
    public class CoroutineModule
	{
		[MoonSharpModuleMethod]
		public static DynValue cocreate(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			if (args[0].Type != DataType.Function && args[0].Type != DataType.ClrFunction)
				args.AsType(0, "cocreate", DataType.Function); // this throws

			return executionContext.GetScript().CreateCoroutine(args[0]);
		}

		[MoonSharpModuleMethod]
		public static DynValue coresume(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue handle = args.AsType(0, "coresume", DataType.Thread);

			try
			{
				DynValue ret = handle.Coroutine.Resume(args.GetArray(1));

				List<DynValue> retval = new List<DynValue>();
				retval.Add(DynValue.True);

				if (ret.Type == DataType.Tuple)
				{
					for (int i = 0; i < ret.Tuple.Length; i++)
					{
						var v = ret.Tuple[i];

						if ((i == ret.Tuple.Length - 1) && (v.Type == DataType.Tuple))
						{
							retval.AddRange(v.Tuple);
						}
						else
						{
							retval.Add(v);
						}
					}
				}
				else
				{
					retval.Add(ret);
				}

				return DynValue.NewTuple(retval.ToArray());
			}
			catch (ScriptRuntimeException ex)
			{
				return DynValue.NewTuple(
					DynValue.False,
					DynValue.NewString(ex.Message));
			}
		}

		[MoonSharpModuleMethod]
		public static DynValue yield(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return DynValue.NewYieldReq(args.GetArray());
		}


		[MoonSharpModuleMethod]
		public static DynValue costatus(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue handle = args.AsType(0, "costatus", DataType.Thread);
			Coroutine running = executionContext.GetCallingCoroutine();
			CoroutineState cs = handle.Coroutine.State;

			switch (cs)
			{
				case CoroutineState.Main:
				case CoroutineState.Running:
                    return DynValue.NewString("running");
				case CoroutineState.NotStarted:
				case CoroutineState.Suspended:
				case CoroutineState.ForceSuspended:
					return DynValue.NewString("suspended");
				case CoroutineState.Dead:
					return DynValue.NewString("dead");
				default:
					throw new InternalErrorException("Unexpected coroutine state {0}", cs);
			}
	
		}


	}
}
