using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace KSoft.Collections;

/// <summary>Cached metadata for declared enum bit indices, independent of enum-value encoding and presentation.</summary>
/// <typeparam name="TBits">The enum whose ordinary nonnegative values are bit positions.</typeparam>
/// <remarks>Usable indices exclude the exclusive-bound members <c>kNumberOf</c> and <c>kMax</c>; other negative members are excluded sentinels. Aliases share an index and use its first ordinary declared name. <see cref="FlagsAttribute"/> is invalid here. <see cref="EnumBitEncoderDisableAttribute"/> and presentation attributes do not disable index membership.</remarks>
public static class EnumBitTraits<TBits> where TBits : struct, Enum
{
	static readonly Metadata kData = CreateMetadata();

	/// <summary>Gets the exclusive bit-index extent, not the member count or a fixed vector's width.</summary>
	/// <remarks>Uses a declared exclusive bound or the highest usable index plus one; an empty domain without a positive bound has length zero.</remarks>
	/// <exception cref="ArgumentException">The enum domain has invalid bounds, is marked with <see cref="FlagsAttribute"/>, or requires an extent not representable by <see cref="int"/>.</exception>
	public static int Length { get { Validate(); return kData.Length; } }
	internal static int DeclaredCount { get { Validate(); return kData.Indices.Length; } }
	internal static ReadOnlySpan<int> Indices { get { Validate(); return kData.Indices; } }

	/// <summary>Validates a usable declared member and returns its numeric index.</summary>
	/// <param name="bit">The member to address.</param>
	/// <returns>The member's nonnegative bit index.</returns>
	/// <remarks>Validates the full underlying numeric value before narrowing. This does not check the capacity of a particular 32/64-bit vector.</remarks>
	/// <exception cref="ArgumentException">The enum domain is invalid.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="bit"/> is negative, undefined, or an exclusive-bound member.</exception>
	public static int ToIndex(TBits bit) => ToIndex(bit, nameof(bit));

	internal static int ToIndex(TBits bit, string paramName)
	{
		Validate();
		return ConvertIndex(bit, paramName);
	}

	internal static int ToIndex(TBits bit, int width, string paramName)
	{
		ValidateWidth(width);
		return ConvertIndex(bit, paramName);
	}

	static int ConvertIndex(TBits bit, string paramName)
	{
		int index = EnumBitIndex.ToIndex(bit, kData.Length, paramName);
		if (!IsDeclaredIndexInRange(index))
		{
			throw new ArgumentOutOfRangeException(paramName, bit, "The index is not a declared bit member.");
		}
		return index;
	}

	/// <summary>Reports whether a numeric index denotes a usable declared member.</summary>
	/// <param name="index">The numeric index to query.</param>
	/// <returns><see langword="true"/> for a usable declared index; otherwise <see langword="false"/>.</returns>
	/// <remarks>This is neither a UI-visibility predicate nor a capacity check for a particular vector.</remarks>
	/// <exception cref="ArgumentException">The enum domain is invalid.</exception>
	public static bool IsDefinedIndex(int index)
	{
		Validate();
		if ((uint)index >= (uint)kData.Length)
		{
			return false;
		}
		return IsDeclaredIndexInRange(index);
	}

	static bool IsDeclaredIndexInRange(int index)
	{
		if (kData.Indices.Length == kData.Length)
		{
			return true;
		}
		return kData.Length <= 64
			? (kData.Mask & (1UL << index)) != 0
			: Array.BinarySearch(kData.Indices, index) >= 0;
	}

	internal static void ValidateIndex(int index, string paramName)
	{
		if (!IsDefinedIndex(index))
		{
			throw new ArgumentOutOfRangeException(paramName, index, "The index is not a declared bit member.");
		}
	}

	internal static void ValidateWidth(int width)
	{
		Validate();
		if (kData.Length > width)
		{
			ThrowWidthMismatch(width);
		}
	}

	static void ThrowWidthMismatch(int width) =>
		throw new ArgumentException(string.Create(Util.InvariantCultureInfo,
			$"Enum {typeof(TBits)} requires {kData.Length} bits, exceeding the vector width {width}."));

	internal static Type GetEnumType(int width)
	{
		ValidateWidth(width);
		return typeof(TBits);
	}

	internal static ulong GetMask(int width)
	{
		ValidateWidth(width);
		return kData.Mask;
	}

	internal static TBits FromIndex(int index)
	{
		ValidateIndex(index, nameof(index));
		return kData.Values[Array.BinarySearch(kData.Indices, index)];
	}

	internal static string GetMemberName(int index)
	{
		ValidateIndex(index, nameof(index));
		return kData.Names[Array.BinarySearch(kData.Indices, index)];
	}

	internal static string FormatVector(ulong word, int width, string separator, bool stateFilter)
	{
		ArgumentNullException.ThrowIfNull(separator);
		ulong remaining = (stateFilter ? word : ~word) & GetMask(width);
		if (remaining == 0)
		{
			return string.Empty;
		}
		var result = new StringBuilder();
		while (remaining != 0)
		{
			int index = System.Numerics.BitOperations.TrailingZeroCount(remaining);
			if (result.Length != 0)
			{
				result.Append(separator);
			}
			result.Append(kData.Names[Array.BinarySearch(kData.Indices, index)]);
			remaining &= remaining - 1;
		}
		return result.ToString();
	}

	static void Validate()
	{
		if (kData.Error != null)
		{
			throw new ArgumentException(kData.Error);
		}
	}

	static Metadata CreateMetadata()
	{
		var type = typeof(TBits);
		if (type.IsDefined(typeof(FlagsAttribute), false))
		{
			return new Metadata("Bit-index collections do not accept [Flags] enums.");
		}
		bool isSigned = Type.GetTypeCode(Enum.GetUnderlyingType(type)) is
			TypeCode.SByte or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64;
		var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
		Array.Sort(fields, static (a, b) => a.MetadataToken.CompareTo(b.MetadataToken));
		var members = new SortedDictionary<int, (string Name, TBits Value)>();
		ulong extent = 0;
		ulong? explicitBound = null;
		foreach (var field in fields)
		{
			var constant = field.GetRawConstantValue()!;
			bool isBound = field.Name is EnumBitEncoderBase.kEnumNumberOfMemberName or EnumBitEncoderBase.kEnumMaxMemberName;
			if (isSigned && Convert.ToInt64(constant, Util.InvariantCultureInfo) < 0)
			{
				if (isBound)
				{
					return new Metadata("An exclusive bit-index bound cannot be negative.");
				}
				continue;
			}
			ulong index = Convert.ToUInt64(constant, Util.InvariantCultureInfo);
			if (index > int.MaxValue)
			{
				return new Metadata("The enum's bit-index extent exceeds the maximum collection length.");
			}
			if (isBound)
			{
				if (explicitBound.HasValue && explicitBound.Value != index)
				{
					return new Metadata("The enum has conflicting exclusive bit-index bounds.");
				}
				explicitBound = index;
			}
			else
			{
				extent = System.Math.Max(extent, index + 1);
				members.TryAdd((int)index, (field.Name, (TBits)field.GetValue(null)!));
			}
		}
		if (explicitBound.HasValue && extent > explicitBound.Value)
		{
			return new Metadata("The enum's exclusive bound does not cover all declared bit indices.");
		}
		extent = explicitBound ?? extent;
		if (extent > int.MaxValue)
		{
			return new Metadata("The enum's bit-index extent exceeds the maximum collection length.");
		}
		var data = new Metadata((int)extent, members.Count);
		int cursor = 0;
		foreach (var member in members)
		{
			data.Indices[cursor] = member.Key;
			data.Names[cursor] = member.Value.Name;
			data.Values[cursor] = member.Value.Value;
			if (member.Key < 64)
			{
				data.Mask |= 1UL << member.Key;
			}
			cursor++;
		}
		return data;
	}

	sealed class Metadata
	{
		internal readonly int Length;
		internal readonly int[] Indices;
		internal readonly string[] Names;
		internal readonly TBits[] Values;
		internal readonly string? Error;
		internal ulong Mask;

		internal Metadata(int length, int count)
		{
			Length = length;
			Indices = new int[count];
			Names = new string[count];
			Values = new TBits[count];
		}

		internal Metadata(string error) : this(0, 0) { Error = error; }
	}
}
