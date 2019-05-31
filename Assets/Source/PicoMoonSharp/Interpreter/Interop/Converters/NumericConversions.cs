using FixedPointy;
using System;
using System.Collections.Generic;

namespace PicoMoonSharp.Interpreter.Interop.Converters
{
	/// <summary>
	/// Static functions to handle conversions of numeric types
	/// </summary>
	internal static class NumericConversions
	{
		static NumericConversions()
		{
			NumericTypesOrdered = new Type[] 
			{
                typeof(Fix),
                typeof(double),
				typeof(decimal), 
				typeof(float), 
				typeof(long), 
				typeof(int), 
				typeof(short), 
				typeof(sbyte), 
				typeof(ulong), 
				typeof(uint), 
				typeof(ushort), 
				typeof(byte), 
			};
			NumericTypes = new HashSet<Type>(NumericTypesOrdered);
		}

		/// <summary>
		/// HashSet of numeric types
		/// </summary>
		internal static readonly HashSet<Type> NumericTypes;
		/// <summary>
		/// Array of numeric types in order used for some conversions
		/// </summary>
		internal static readonly Type[] NumericTypesOrdered;

		/// <summary>
		/// Converts a double to another type
		/// </summary>
		internal static object FixToType(Type type, Fix d)
		{
			type = Nullable.GetUnderlyingType(type) ?? type;

            if (type == typeof(Fix))     return d;
			if (type == typeof(double))  return (double)d;
			if (type == typeof(sbyte))   return (sbyte)d;
			if (type == typeof(byte))    return (byte)d;
			if (type == typeof(short))   return (short)d;
			if (type == typeof(ushort))  return (ushort)d;
			if (type == typeof(int))     return (int)d;
			if (type == typeof(uint))    return (uint)d;
			if (type == typeof(long))    return (long)d;
			if (type == typeof(ulong))   return (ulong)d;
			if (type == typeof(float))   return (float)d;
			if (type == typeof(decimal)) return (decimal)d;
			return d;
		}

		/// <summary>
		/// Converts a type to double
		/// </summary>
		internal static Fix TypeToFix(Type type, object d)
		{
            if (type == typeof(Fix))     return (Fix)d;
			if (type == typeof(double))  return (Fix)(double)d;
			if (type == typeof(sbyte))   return (Fix)(sbyte)d;
			if (type == typeof(byte))    return (Fix)(byte)d;
			if (type == typeof(short))   return (Fix)(short)d;
			if (type == typeof(ushort))  return (Fix)(ushort)d;
			if (type == typeof(int))     return (Fix)(int)d;
			if (type == typeof(uint))    return (Fix)(int)(uint)d;
			if (type == typeof(long))    return (Fix)(int)d;
			if (type == typeof(ulong))   return (Fix)(int)(ulong)d;
			if (type == typeof(float))   return (Fix)(float)d;
			if (type == typeof(decimal)) return (Fix)(double)(decimal)d;
			return (Fix)d;
		}



	}
}
