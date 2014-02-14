using System;

namespace KSoft.Reflection
{
	/// <summary>Utility for converting to and from a given Enum and integer types, without boxing operations but without the safeguards of reflection</summary>
	/// <typeparam name="TEnum"></typeparam>
	/// <remarks>'From' methods can be unforgiving. Make sure you know what you're doing</remarks>
	public sealed class EnumValue<TEnum> : EnumUtilBase<TEnum>
		where TEnum : struct, IComparable, IFormattable, IConvertible
	{
		public static readonly Func<TEnum, byte> ToByte =   GenerateToMethod  <byte>();
		public static readonly Func<byte, TEnum> FromByte = GenerateFromMethod<byte>();

		public static readonly Func<TEnum, sbyte> ToSByte =   GenerateToMethod  <sbyte>();
		public static readonly Func<sbyte, TEnum> FromSByte = GenerateFromMethod<sbyte>();

		public static readonly Func<TEnum, ushort> ToUInt16 =   GenerateToMethod  <ushort>();
		public static readonly Func<ushort, TEnum> FromUInt16 = GenerateFromMethod<ushort>();

		public static readonly Func<TEnum, short> ToInt16 =   GenerateToMethod  <short>();
		public static readonly Func<short, TEnum> FromInt16 = GenerateFromMethod<short>();

		public static readonly Func<TEnum, uint> ToUInt32 =   GenerateToMethod  <uint>();
		public static readonly Func<uint, TEnum> FromUInt32 = GenerateFromMethod<uint>();

		public static readonly Func<TEnum, int> ToInt32 =   GenerateToMethod  <int>();
		public static readonly Func<int, TEnum> FromInt32 = GenerateFromMethod<int>();

		public static readonly Func<TEnum, ulong> ToUInt64 =   GenerateToMethod  <ulong>();
		public static readonly Func<ulong, TEnum> FromUInt64 = GenerateFromMethod<ulong>();

		public static readonly Func<TEnum, long> ToInt64 =   GenerateToMethod  <long>();
		public static readonly Func<long, TEnum> FromInt64 = GenerateFromMethod<long>();

	};
}