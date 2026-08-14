using System;

namespace KSoft
{
	internal sealed class EnumFlags<TEnum> : Reflection.EnumUtilBase<TEnum>
		where TEnum : struct, Enum
	{
		static EnumFlags()
		{
			Reflection.EnumUtils.AssertTypeIsFlagsEnum(kEnumType);
		}

		public static TEnum Add(TEnum value, TEnum flags)
		{
			return kUnderlyingTypeCode switch
			{
				TypeCode.Byte => Reflection.EnumValue<TEnum>.FromByte(unchecked(
					(byte)(Reflection.EnumValue<TEnum>.ToByte(value) | Reflection.EnumValue<TEnum>.ToByte(flags)))),
				TypeCode.SByte => Reflection.EnumValue<TEnum>.FromSByte(unchecked(
					(sbyte)(Reflection.EnumValue<TEnum>.ToSByte(value) | Reflection.EnumValue<TEnum>.ToSByte(flags)))),
				TypeCode.UInt16 => Reflection.EnumValue<TEnum>.FromUInt16(unchecked(
					(ushort)(Reflection.EnumValue<TEnum>.ToUInt16(value) | Reflection.EnumValue<TEnum>.ToUInt16(flags)))),
				TypeCode.Int16 => Reflection.EnumValue<TEnum>.FromInt16(unchecked(
					(short)(Reflection.EnumValue<TEnum>.ToInt16(value) | Reflection.EnumValue<TEnum>.ToInt16(flags)))),
				TypeCode.UInt32 => Reflection.EnumValue<TEnum>.FromUInt32(
					Reflection.EnumValue<TEnum>.ToUInt32(value) | Reflection.EnumValue<TEnum>.ToUInt32(flags)),
				TypeCode.Int32 => Reflection.EnumValue<TEnum>.FromInt32(
					Reflection.EnumValue<TEnum>.ToInt32(value) | Reflection.EnumValue<TEnum>.ToInt32(flags)),
				TypeCode.UInt64 => Reflection.EnumValue<TEnum>.FromUInt64(
					Reflection.EnumValue<TEnum>.ToUInt64(value) | Reflection.EnumValue<TEnum>.ToUInt64(flags)),
				TypeCode.Int64 => Reflection.EnumValue<TEnum>.FromInt64(
					Reflection.EnumValue<TEnum>.ToInt64(value) | Reflection.EnumValue<TEnum>.ToInt64(flags)),
				_ => throw new InvalidOperationException($"Unsupported enum underlying type {kUnderlyingType}."),
			};
		}
		public static void Add(ref TEnum value, TEnum flags)
		{
			value = Add(value, flags);
		}

		public static TEnum Remove(TEnum value, TEnum flags)
		{
			return kUnderlyingTypeCode switch
			{
				TypeCode.Byte => Reflection.EnumValue<TEnum>.FromByte(unchecked(
					(byte)(Reflection.EnumValue<TEnum>.ToByte(value) & ~Reflection.EnumValue<TEnum>.ToByte(flags)))),
				TypeCode.SByte => Reflection.EnumValue<TEnum>.FromSByte(unchecked(
					(sbyte)(Reflection.EnumValue<TEnum>.ToSByte(value) & ~Reflection.EnumValue<TEnum>.ToSByte(flags)))),
				TypeCode.UInt16 => Reflection.EnumValue<TEnum>.FromUInt16(unchecked(
					(ushort)(Reflection.EnumValue<TEnum>.ToUInt16(value) & ~Reflection.EnumValue<TEnum>.ToUInt16(flags)))),
				TypeCode.Int16 => Reflection.EnumValue<TEnum>.FromInt16(unchecked(
					(short)(Reflection.EnumValue<TEnum>.ToInt16(value) & ~Reflection.EnumValue<TEnum>.ToInt16(flags)))),
				TypeCode.UInt32 => Reflection.EnumValue<TEnum>.FromUInt32(
					Reflection.EnumValue<TEnum>.ToUInt32(value) & ~Reflection.EnumValue<TEnum>.ToUInt32(flags)),
				TypeCode.Int32 => Reflection.EnumValue<TEnum>.FromInt32(
					Reflection.EnumValue<TEnum>.ToInt32(value) & ~Reflection.EnumValue<TEnum>.ToInt32(flags)),
				TypeCode.UInt64 => Reflection.EnumValue<TEnum>.FromUInt64(
					Reflection.EnumValue<TEnum>.ToUInt64(value) & ~Reflection.EnumValue<TEnum>.ToUInt64(flags)),
				TypeCode.Int64 => Reflection.EnumValue<TEnum>.FromInt64(
					Reflection.EnumValue<TEnum>.ToInt64(value) & ~Reflection.EnumValue<TEnum>.ToInt64(flags)),
				_ => throw new InvalidOperationException($"Unsupported enum underlying type {kUnderlyingType}."),
			};
		}
		public static void Remove(ref TEnum value, TEnum flags)
		{
			value = Remove(value, flags);
		}

		public static TEnum Modify(bool addOrRemove, TEnum value, TEnum flags)
		{
			return addOrRemove
				? Add(value, flags)
				: Remove(value, flags);
		}
		public static void Modify(bool addOrRemove, ref TEnum value, TEnum flags)
		{
			value = Modify(addOrRemove, value, flags);
		}
	};

	/// <summary>Utility for mutating flags enum values without spelling out bitwise operations at call sites</summary>
	/// <remarks>
	/// Generic enum constraints do not make bitwise operators available on <typeparamref name="TEnum"/>. This helper keeps
	/// readable one-line flag mutation call sites while reusing <see cref="Reflection.EnumValue{TEnum}"/> conversion
	/// delegates instead of maintaining a separate expression-compiled delegate family.
	/// </remarks>
	public static class EnumFlags
	{
		#region Add
		public static TEnum Add<TEnum>(TEnum value, TEnum flags)
			where TEnum : struct, Enum
		{
			return EnumFlags<TEnum>.Add(value, flags);
		}
		public static void Add<TEnum>(ref TEnum value, TEnum flags)
			where TEnum : struct, Enum
		{
			EnumFlags<TEnum>.Add(ref value, flags);
		}
		#endregion

		#region Remove
		public static TEnum Remove<TEnum>(TEnum value, TEnum flags)
			where TEnum : struct, Enum
		{
			return EnumFlags<TEnum>.Remove(value, flags);
		}
		public static void Remove<TEnum>(ref TEnum value, TEnum flags)
			where TEnum : struct, Enum
		{
			EnumFlags<TEnum>.Remove(ref value, flags);
		}
		#endregion

		#region Modify
		public static TEnum Modify<TEnum>(bool addOrRemove, TEnum value, TEnum flags)
			where TEnum : struct, Enum
		{
			return EnumFlags<TEnum>.Modify(addOrRemove, value, flags);
		}
		public static void Modify<TEnum>(bool addOrRemove, ref TEnum value, TEnum flags)
			where TEnum : struct, Enum
		{
			EnumFlags<TEnum>.Modify(addOrRemove, ref value, flags);
		}
		#endregion
	};
}
