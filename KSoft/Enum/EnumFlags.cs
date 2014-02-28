using System;
using System.Collections.Generic;
using System.Reflection;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using Expr = System.Linq.Expressions.Expression;
using ExprParam = System.Linq.Expressions.ParameterExpression;

namespace KSoft
{
	using EnumUtils = Reflection.EnumUtils;

	internal sealed partial class EnumFlags<TEnum> : Reflection.EnumUtilBase<TEnum>
		where TEnum : struct, IComparable, IFormattable, IConvertible
	{
		#region Signatures
		delegate TEnum ModifyDelegate(TEnum value, TEnum flags);
		delegate void ModifyByRefDelegate(ref TEnum value, TEnum flags);

		delegate TEnum ModifyCondDelegate(bool addOrRemove, TEnum value, TEnum flags);
		delegate void ModifyByRefCondDelegate(bool addOrRemove, ref TEnum value, TEnum flags);

		delegate bool ReadDelegate(TEnum value, TEnum flags);
		#endregion

		/// <summary>Initializes the <see cref="EnumFlags{TEnum}"/> class by generating the needed methods</summary>
		static EnumFlags()
		{
			EnumUtils.AssertTypeIsFlagsEnum(kEnumType);
		}

		#region Method generators
		static ExprParam GenerateParamValue(bool byRef)
		{
			return Expr.Parameter(byRef
				? kEnumTypeByRef
				: kEnumType, "value");							// [ref] TEnum value
		}
		static ExprParam GenerateParamFlags()
		{
			return Expr.Parameter(kEnumType, "flags");			// TEnum flags
		}
		static ExprParam GenerateParamAddOrRemove()
		{
			return Expr.Parameter(typeof(bool), "addOrRemove");	// bool addOrRemove
		}

		// Note: Both AndAssign and OrAssign were added in .NET 4 (their sans Assign counterparts are 3.5)
		// However, they're not supported by the Portable Class Library
		// More so, neither AndAssign or OrAssign work as expected with Enum.value__. IE, value__ isn't updated

		// The binary operator Or is not defined for the types 'TEnum' and 'TEnum'.
		#endregion

		// By-Val	By-Ref	Cmp
		// 1		1		R
		// 1		1		R
		// 1		2		R
		// 
		
		#region Static interface
		public static TEnum Add(TEnum value, TEnum flags)		{ return	V1.kAddFlags(value, flags); }
		public static void Add(ref TEnum value, TEnum flags)	{			V1.kAddFlagsByRef(ref value, flags); }

		public static TEnum Remove(TEnum value, TEnum flags)	{ return	V1.kRemoveFlags(value, flags); }
		public static void Remove(ref TEnum value, TEnum flags)	{			V1.kRemoveFlagsByRef(ref value, flags); }

		/// <summary>Adds or removes the given flags from the provided value, returning the result</summary>
		/// <param name="addOrRemove">ie, "true or false"</param>
		/// <param name="value"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static TEnum Modify(bool addOrRemove, TEnum value, TEnum flags)		{ return	V2.kModifyFlags(addOrRemove, value, flags); }
		/// <summary>Adds or removes the given flags from the provided value</summary>
		/// <param name="addOrRemove">ie, "true or false"</param>
		/// <param name="value"></param>
		/// <param name="flags"></param>
		public static void Modify(bool addOrRemove, ref TEnum value, TEnum flags)	{			V1.kModifyFlagsByRef(addOrRemove, ref value, flags); }

		public static bool Test(TEnum value, TEnum flags)		{ return	V1.kTestFlags(value, flags); }
		#endregion
	};

	public static class EnumFlags
	{
		#region Add
		public static TEnum Add<TEnum>(TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return EnumFlags<TEnum>.Add(value, flags);
		}
		public static void Add<TEnum>(ref TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			EnumFlags<TEnum>.Add(ref value, flags);
		}
		#endregion

		#region Remove
		public static TEnum Remove<TEnum>(TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return EnumFlags<TEnum>.Remove(value, flags);
		}
		public static void Remove<TEnum>(ref TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			EnumFlags<TEnum>.Remove(ref value, flags);
		}
		#endregion

		#region Modify
		public static TEnum Modify<TEnum>(bool addOrRemove, TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return EnumFlags<TEnum>.Modify(addOrRemove, value, flags);
		}
		public static void Modify<TEnum>(bool addOrRemove, ref TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			EnumFlags<TEnum>.Modify(addOrRemove, ref value, flags);
		}
		#endregion

		public static bool Test<TEnum>(TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return EnumFlags<TEnum>.Test(value, flags);
		}
	};
}