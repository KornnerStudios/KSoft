using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Expr = System.Linq.Expressions.Expression;
using ExprParam = System.Linq.Expressions.ParameterExpression;


namespace KSoft.Reflection
{
	// #TODO: check out http://damieng.com/blog/2010/10/17/enums-better-syntax-improved-performance-and-tryparse-in-net-3-5

	internal static class EnumUtils
	{
		/// <summary>Name of the internal member used to represent an enum's integral value</summary>
		/// <remarks>.NET uses this for the name...not sure if Mono or other implementations do as well</remarks>
		public const string kMemberName = "value__";

		#region Underlying Type support utils
		/// <summary>TypeCodes for supported underlying types</summary>
		/// <remarks>Even indices represent signed types, odd are unsigned</remarks>
		public static readonly TypeCode[] kSupportedTypeCodes = [
			TypeCode.SByte,	TypeCode.Byte,
			TypeCode.Int16,	TypeCode.UInt16,
			TypeCode.Int32, TypeCode.UInt32,
			TypeCode.Int64, TypeCode.UInt64,
		];
		/// <summary>Types for supported underlying types</summary>
		/// <remarks>Even indices represent signed types, odd are unsigned</remarks>
		public static readonly Type[] kSupportedTypes = [
			typeof(SByte), typeof(Byte),
			typeof(Int16), typeof(UInt16),
			typeof(Int32), typeof(UInt32),
			typeof(Int64), typeof(UInt64),
		];

		/// <summary>Is the TypeCode a supported underlying type?</summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public static bool TypeIsSupported(TypeCode c)
		{
			return c switch
			{
				TypeCode.SByte or TypeCode.Byte or
				TypeCode.Int16 or TypeCode.UInt16 or
				TypeCode.Int32 or TypeCode.UInt32 or
				TypeCode.Int64 or TypeCode.UInt64
				=> true,
				_ => false,
			};
		}

		/// <summary>Assert the properties of an enum type are valid for any Enum Utils code</summary>
		/// <param name="kEnumType">Enum type in question</param>
		/// <param name="kUnderlyingType">Optional. <paramref name="kEnumType"/>'s underlying type</param>
		/// <remarks>
		/// If <paramref name="kUnderlyingType"/> is null, we use the type info found in <paramref name="kEnumType"/>.
		/// </remarks>
		/// <exception cref="NotSupportedException">Thrown when the underlying type is unsupported</exception>
		public static void AssertUnderlyingTypeIsSupported(Type kEnumType, Type? kUnderlyingType)
		{
			ArgumentNullException.ThrowIfNull(kEnumType);

			if (kUnderlyingType == null)
			{
				kUnderlyingType = kEnumType.GetEnumUnderlyingType();
			}

			if (!TypeIsSupported(Type.GetTypeCode(kUnderlyingType)))
			{
				var message =
					string.Format(KSoft.Util.InvariantCultureInfo,
						"The underlying type of the type parameter {0} is {1}. " +
						"Enum Utils only supports Enums with underlying type of " +
						"SByte, Byte, Int16, UInt16, Int32, UInt32, Int64, or UInt64.",
						kEnumType, kUnderlyingType);
				throw new NotSupportedException(message);
			}
		}
		#endregion

		/// <summary>Assert the properties of a type are that of an Enum</summary>
		/// <param name="theType">Type in question</param>
		/// <exception cref="NotSupportedException">Thrown when <paramref name="theType"/> is not an Enum</exception>
		public static void AssertTypeIsEnum(Type theType)
		{
			ArgumentNullException.ThrowIfNull(theType);

			if (theType.IsEnum)
			{
				return;
			}

			var message = string.Format(KSoft.Util.InvariantCultureInfo,
				"The type parameter {0} is not an Enum. Enum Utils supports Enums only.",
				theType);
			throw new NotSupportedException(message);
		}

		public static void AssertTypeIsFlagsEnum(Type theType)
		{
			ArgumentNullException.ThrowIfNull(theType);

			AssertTypeIsEnum(theType);

			if (theType.GetCustomAttribute<FlagsAttribute>() != null)
			{
				return;
			}

			var message = string.Format(KSoft.Util.InvariantCultureInfo,
				"The Enum type parameter {0} is not annotated as being a Flags Enum (via FlagsAttribute).",
				theType);
			throw new NotSupportedException(message);
		}

		public static List<FieldInfo> GetEnumFields(Type enumType)
		{
			ArgumentNullException.ThrowIfNull(enumType);
			if (!enumType.IsEnum)
			{
				throw new ArgumentException("Type must be an enum.", nameof(enumType));
			}

			var fields = enumType.GetFields();
			var results = new List<FieldInfo>(fields.Length - 1);
			foreach (var field in fields)
			{
				if (field.Name == kMemberName)
				{
					continue;
				}

				results.Add(field);
			}

			return results;
		}
	};

	/// <summary>Base class for all Enum utilities (even if they're static utils)</summary>
	/// <typeparam name="TEnum">Enum type we're dealing with</typeparam>
	/// <remarks>Not used as a base for utils which have a non-generic core, eg. EnumBinaryStreamerBase</remarks>
	public abstract class EnumUtilBase<TEnum>
		where TEnum : struct, Enum
	{
		/// <summary>Enum type we're dealing with</summary>
		protected static readonly Type kEnumType =				typeof(TEnum);
		/// <summary>Enum (by-reference) type we're dealing with</summary>
		protected static readonly Type kEnumTypeByRef =			kEnumType.MakeByRefType();
		/// <summary><see cref="kEnumType"/>'s integer type used to represent its raw value</summary>
		protected static readonly Type kUnderlyingType =		Enum.GetUnderlyingType(kEnumType);
		protected static readonly TypeCode kUnderlyingTypeCode=	Type.GetTypeCode(kUnderlyingType);
		/// <summary>Does the underlying enumeration have a <see cref="FlagsAttribute"/>?</summary>
		protected static readonly bool kIsFlags =				kEnumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;

		// #VITA_KEEP: cache typed enum metadata for the public Names and Values reflection surface.
		protected static readonly string[] kEnumNames =			System.Enum.GetNames<TEnum>();
		protected static readonly TEnum[] kEnumValues =			System.Enum.GetValues<TEnum>();

		/// <summary>
		/// Sign extends the Enum value's hash-code if its underlying type is:
		/// 1. Signed
		/// 2. Smaller than sizeof(<see cref="System.Int32"/>)
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int GetHashCodeSignExtended(TEnum value)
		{
			int hc = value.GetHashCode();

			return kUnderlyingTypeCode switch
			{
				TypeCode.SByte => (int)((sbyte)hc),
				TypeCode.Int16 => (int)((short)hc),
				_ => hc,
			};
		}

		// NOTE: Up to inheriting class to call AssertTypeIsEnum or AssertTypeIsFlagsEnum in their static ctor

		#region EnumValue helpers that can be helpful elsewhere
		static ExprParam GenerateParamValue()
		{
			return Expr.Parameter(kEnumType, "value");	// TEnum value
		}
		internal static Func<TEnum, TInt> GenerateToMethod<TInt>()
		{
			// NOTE: Getting runtime errors in lambda.Compile() when we try to return value__ directly

			//////////////////////////////////////////////////////////////////////////
			// Define the generated method's parameters
			var param_v = GenerateParamValue();
#if false
			var param_v_member = Expr.PropertyOrField(param_v, EnumUtils.kMemberName); // value.value__
#endif

			//////////////////////////////////////////////////////////////////////////
			// [result] = (TInt)value.value__
			var TIntType = typeof(TInt);
#if false
			var value_expr = TIntType != kUnderlyingType ?
				Expr.Convert(param_v_member, TIntType) :
				param_v_member as Expr;
#else
			var value_expr = Expr.Convert(param_v, TIntType);
#endif

			//////////////////////////////////////////////////////////////////////////
			// return (TInt)value.value__
			var ret = value_expr;

			var lambda = Expr.Lambda<Func<TEnum, TInt>>(ret, param_v);
			return lambda.Compile();
		}

		internal static Func<TInt, TEnum> GenerateFromMethod<TInt>()
		{
			var TIntType = typeof(TInt);
			var param_v = Expr.Parameter(TIntType, "value");

			var ret = Expr.Convert(param_v, kEnumType);

			var lambda = Expr.Lambda<Func<TInt, TEnum>>(ret, param_v);
			return lambda.Compile();
		}
		#endregion
	};
	public sealed class EnumUtil<TEnum>: EnumUtilBase<TEnum>
		where TEnum : struct, Enum
	{
		/// <summary>Enum type we're dealing with</summary>
		public static Type EnumType					=> kEnumType;
		/// <summary>Enum (by-reference) type we're dealing with</summary>
		public static Type EnumTypeByRef			=> kEnumTypeByRef;
		/// <summary><see cref="EnumType"/>'s integer type used to represent its raw value</summary>
		public static Type UnderlyingType			=> kUnderlyingType;
		public static TypeCode UnderlyingTypeCode	=> kUnderlyingTypeCode;
		/// <summary>Does the underlying enumeration have a <see cref="FlagsAttribute"/>?</summary>
		public static bool IsFlags					=> kIsFlags;

		public static string[] Names				=> kEnumNames;
		public static TEnum[] Values				=> kEnumValues;

		static EnumUtil()
		{
			EnumUtils.AssertTypeIsEnum(kEnumType);
		}
	};
}
