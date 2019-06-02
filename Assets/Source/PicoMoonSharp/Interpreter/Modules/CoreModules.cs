using System;

namespace PicoMoonSharp.Interpreter
{
	/// <summary>
	/// Enumeration (combinable as flags) of all the standard library modules
	/// </summary>
	[Flags]
	public enum CoreModules
	{
		/// <summary>
		/// Value used to specify no modules to be loaded (equals 0).
		/// </summary>
		None = 0,

		/// <summary>
		/// The basic methods. Includes "assert", "collectgarbage", "error", "print", "select", "type", "tonumber" and "tostring".
		/// </summary>
		Basic = 0x40,
		/// <summary>
		/// The global constants: "_G", "_VERSION" and "_MOONSHARP".
		/// </summary>
		GlobalConsts = 0x1,
		/// <summary>
		/// The table iterators: "next", "ipairs" and "pairs".
		/// </summary>
		TableIterators = 0x2,
		/// <summary>
		/// The metatable methods : "setmetatable", "getmetatable", "rawset", "rawget", "rawequal" and "rawlen".
		/// </summary>
		Metatables = 0x4,
		/// <summary>
		/// The load methods: "load", "loadsafe", "loadfile", "loadfilesafe", "dofile" and "require"
		/// </summary>
		LoadMethods = 0x10,
		/// <summary>
		/// The table package 
		/// </summary>
		Table = 0x20,
		/// <summary>
		/// The error handling methods: "pcall" and "xpcall"
		/// </summary>
		ErrorHandling = 0x80,
		/// <summary>
		/// The math package
		/// </summary>
		PicoMath = 0x100,
		/// <summary>
		/// The coroutine package
		/// </summary>
		Coroutine = 0x200,
		/// <summary>
		/// The "debug" package (it has limited support)
		/// </summary>
		Debug = 0x4000,
		/// <summary>
		/// The "dynamic" package (introduced by MoonSharp).
		/// </summary>
		Dynamic = 0x8000,


		/// <summary>
		/// A sort of "hard" sandbox preset, including string, math, table, bit32 packages, constants and table iterators.
		/// </summary>
		Preset_HardSandbox = GlobalConsts | TableIterators | Table | Basic | PicoMath,
		/// <summary>
		/// A softer sandbox preset, adding metatables support, error handling, coroutine, time functions, json parsing and dynamic evaluations.
		/// </summary>
		Preset_SoftSandbox = Preset_HardSandbox | Metatables | ErrorHandling | Coroutine | Dynamic,
		/// <summary>
		/// The default preset. Includes everything except "debug" as now.
		/// Beware that using this preset allows scripts unlimited access to the system.
		/// </summary>
		Preset_Default = Preset_SoftSandbox | LoadMethods,
		/// <summary>
		/// The complete package.
		/// Beware that using this preset allows scripts unlimited access to the system.
		/// </summary>
		Preset_Complete = Preset_Default | Debug,

	}

	internal static class CoreModules_ExtensionMethods
	{
		public static bool Has(this CoreModules val, CoreModules flag)
		{
			return (val & flag) == flag;
		}
	}


}
