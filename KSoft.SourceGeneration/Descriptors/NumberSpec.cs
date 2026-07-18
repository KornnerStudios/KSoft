using System;

namespace KSoft.SourceGeneration.Descriptors;

/// <summary>
/// Describes a numeric primitive type used by KSoft source generators.
/// </summary>
/// <remarks>
/// Maps to <c>KSoft.T4.NumberCodeDefinition</c>. This is intentionally separate from
/// <see cref="PrimitiveSpec" /> because <c>bool</c>, <c>char</c>, <c>string</c>, and <c>Values.KGuid</c> are
/// primitives but not numbers.
/// </remarks>
internal readonly struct NumberSpec : IEquatable<NumberSpec>
{
	/// <summary>
	/// Initializes a new numeric primitive descriptor.
	/// </summary>
	/// <param name="typeCode">Runtime type code represented by the descriptor.</param>
	public NumberSpec(TypeCode typeCode)
	{
		Keyword = PrimitiveCatalog.KeywordFor(typeCode);
		TypeCode = typeCode;
		SizeOfInBytes = SizeOf(typeCode);
		OperationWord = OperationWordFor(typeCode);
		Primitive = new PrimitiveSpec(Keyword, TypeCode, SizeOfInBytes, BuildSimpleDescription());
	}

	/// <summary>
	/// Gets the primitive descriptor view for this numeric descriptor.
	/// </summary>
	/// <remarks>Maps to the inheritance relationship between T4 number and primitive definitions.</remarks>
	public PrimitiveSpec Primitive { get; }

	/// <summary>
	/// Gets the C# keyword emitted for the numeric primitive type.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.PrimitiveCodeDefinition.Keyword</c>.</remarks>
	public string Keyword { get; }

	/// <summary>
	/// Gets the runtime type code represented by the descriptor.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.PrimitiveCodeDefinition.Code</c>.</remarks>
	public TypeCode TypeCode { get; }

	/// <summary>
	/// Gets the primitive size in bytes.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.PrimitiveCodeDefinition.SizeOfInBytes</c>.</remarks>
	public int SizeOfInBytes { get; }

	/// <summary>
	/// Gets the primitive size in bits.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.PrimitiveCodeDefinition.SizeOfInBits</c>.</remarks>
	public int SizeOfInBits => SizeOfInBytes * PrimitiveCatalog.BitsPerByte;

	/// <summary>
	/// Gets a short human-readable description for XML documentation emitted by generators.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.PrimitiveCodeDefinition.SimpleDesc</c>.</remarks>
	public string SimpleDescription => Primitive.SimpleDescription;

	/// <summary>
	/// Gets the integer type keyword used when generated code performs operations.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.NumberCodeDefinition.OperationWord</c>.</remarks>
	public string OperationWord { get; }

	/// <summary>
	/// Gets whether this descriptor represents an integer primitive.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.NumberCodeDefinition.IsInteger</c>.</remarks>
	public bool IsInteger => TypeCode >= TypeCode.SByte && TypeCode <= TypeCode.UInt64;

	/// <summary>
	/// Gets whether this descriptor represents a byte-sized integer primitive.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.NumberCodeDefinition.IsByte</c>.</remarks>
	public bool IsByte => TypeCode == TypeCode.Byte || TypeCode == TypeCode.SByte;

	/// <summary>
	/// Gets whether this descriptor represents a signed integer primitive.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.NumberCodeDefinition.IsSigned</c>.</remarks>
	public bool IsSigned => IsInteger && Keyword[0] != 'u' && Keyword[0] != 'b';

	/// <summary>
	/// Gets whether this descriptor represents an unsigned integer primitive.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.NumberCodeDefinition.IsUnsigned</c>.</remarks>
	public bool IsUnsigned => IsInteger && !IsSigned;

	/// <summary>
	/// Gets whether bit operators implicitly promote this integer type to a larger integer type.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.NumberCodeDefinition.BitOperatorsImplicitlyUpCast</c>.</remarks>
	public bool BitOperatorsImplicitlyUpCast => IsInteger && SizeOfInBits < 32;

	/// <summary>
	/// Gets the suffix to use on literal values of this number type.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.NumberCodeDefinition.LiteralSuffix</c>.</remarks>
	public string LiteralSuffix
	{
		get => TypeCode switch
		{
			TypeCode.UInt32	=> "U",
			TypeCode.Int64	=> "L",
			TypeCode.UInt64	=> "UL",
			TypeCode.Single	=> "f",
			_ => "",
		};
	}

	/// <summary>
	/// Gets the non-truncating hexadecimal format string for this integer type.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.NumberCodeDefinition.ToStringHexFormat</c>.</remarks>
	public string ToStringHexFormat => IsInteger
		? "X" + (SizeOfInBytes * 2).ToString(PrimitiveCatalog.InvariantCulture)
		: "";

	/// <summary>
	/// Gets the keyword used to define general constants for integer types.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.Bitwise.BitwiseT4.GetConstantKeyword</c>.</remarks>
	public string ConstantKeyword
	{
		get => TypeCode switch
		{
			TypeCode.Byte or TypeCode.SByte		=> "Byte",
			TypeCode.UInt16 or TypeCode.Int16	=> "Int16",
			TypeCode.UInt32 or TypeCode.Int32	=> "Int32",
			TypeCode.UInt64 or TypeCode.Int64	=> "Int64",
			_ => throw new InvalidOperationException(TypeCode.ToString()),
		};
	}

	/// <summary>
	/// Gets the signed <see cref="TypeCode" /> equivalent for unsigned integer descriptors.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.NumberCodeDefinition.SignedCode</c>.</remarks>
	public TypeCode SignedTypeCode
	{
		get => TypeCode switch
		{
			TypeCode.Byte	=> TypeCode.SByte,
			TypeCode.UInt16	=> TypeCode.Int16,
			TypeCode.UInt32	=> TypeCode.Int32,
			TypeCode.UInt64	=> TypeCode.Int64,
			_ => TypeCode,
		};
	}

	/// <summary>
	/// Gets the signed C# keyword equivalent for unsigned integer descriptors.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.NumberCodeDefinition.SignedKeyword</c>.</remarks>
	public string SignedKeyword => PrimitiveCatalog.KeywordFor(SignedTypeCode);

	/// <summary>
	/// Gets the shift amount to access the most significant byte.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.NumberCodeDefinition.MostSignificantByteBitShift</c>.</remarks>
	public int MostSignificantByteBitShift => SizeOfInBits - PrimitiveCatalog.BitsPerByte;

	public override bool Equals(object obj)
		=> obj is NumberSpec other && Equals(other);

	public bool Equals(NumberSpec other)
	{
		return Primitive.Equals(other.Primitive)
			&& OperationWord == other.OperationWord;
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hash = 17;
			hash = (hash * 31) + Primitive.GetHashCode();
			hash = (hash * 31) + OperationWord.GetHashCode();
			return hash;
		}
	}

	private static int SizeOf(TypeCode typeCode)
		=> typeCode switch
		{
			TypeCode.Byte   or TypeCode.SByte						=> sizeof(byte),
			TypeCode.UInt16 or TypeCode.Int16						=> sizeof(ushort),
			TypeCode.UInt32 or TypeCode.Int32 or TypeCode.Single	=> sizeof(uint),
			TypeCode.UInt64 or TypeCode.Int64 or TypeCode.Double	=> sizeof(ulong),
			_ => throw new InvalidOperationException(typeCode.ToString()),
		};

	private static string OperationWordFor(TypeCode typeCode)
	{
		return typeCode switch
		{
			TypeCode.Byte  or TypeCode.UInt16	=> "uint",
			TypeCode.SByte or TypeCode.Int16	=> "int",
			_ => PrimitiveCatalog.KeywordFor(typeCode),
		};
	}

	private string BuildSimpleDescription()
	{
		if (IsInteger)
		{
			string prefix = IsSigned
				?   "signed"
				: "unsigned";

			return string.Format(
				PrimitiveCatalog.InvariantCulture,
				"{0} {1}-bit integer",
				prefix,
				SizeOfInBits);
		}

		return string.Format(
			PrimitiveCatalog.InvariantCulture,
			"{0}-precision number",
			TypeCode.ToString().ToLower(PrimitiveCatalog.InvariantCulture));
	}
};
