using System;
using KSoft.SourceGeneration;

namespace KSoft.SourceGeneration.Descriptors;

/// <summary>
/// Describes a primitive type used by KSoft source generators.
/// </summary>
/// <remarks>
/// Maps to <c>KSoft.T4.PrimitiveCodeDefinition</c>. Keep numeric-only metadata on <see cref="NumberSpec" /> so
/// descriptors for <c>bool</c>, <c>char</c>, <c>string</c>, and <c>Values.KGuid</c> cannot expose number APIs.
/// </remarks>
internal readonly struct PrimitiveSpec : IEquatable<PrimitiveSpec>
{
	/// <summary>
	/// Initializes a new primitive descriptor.
	/// </summary>
	/// <param name="keyword">C# keyword emitted for the primitive type.</param>
	/// <param name="typeCode">Runtime type code represented by the descriptor.</param>
	/// <param name="sizeOfInBytes">Primitive size in bytes.</param>
	/// <param name="simpleDescription">Short human-readable description for generated XML documentation.</param>
	public PrimitiveSpec(
		string keyword,
		TypeCode typeCode,
		int sizeOfInBytes,
		string simpleDescription = "NO DESC")
	{
		ExceptionHelpers.ThrowIfNullOrEmpty(keyword, nameof(keyword));
		ExceptionHelpers.ThrowIfNullOrEmpty(simpleDescription, nameof(simpleDescription));

		Keyword = keyword;
		TypeCode = typeCode;
		SizeOfInBytes = sizeOfInBytes;
		SimpleDescription = simpleDescription;
	}

	/// <summary>
	/// Gets the C# keyword emitted for the primitive type.
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
	public string SimpleDescription { get; }

	/// <summary>
	/// Gets whether this primitive descriptor represents an integer primitive.
	/// </summary>
	/// <remarks>
	/// Maps to <c>KSoft.T4.PrimitiveCodeDefinition.IsInteger</c>. Numeric primitive descriptors are exposed through
	/// <see cref="NumberSpec.Primitive" />, matching the old runtime type relationship.
	/// </remarks>
	public bool IsInteger => TypeCode >= TypeCode.SByte && TypeCode <= TypeCode.UInt64;

	public override bool Equals(object obj)
		=> obj is PrimitiveSpec other && Equals(other);

	public bool Equals(PrimitiveSpec other)
	{
		return Keyword == other.Keyword
			&& TypeCode == other.TypeCode
			&& SizeOfInBytes == other.SizeOfInBytes
			&& SimpleDescription == other.SimpleDescription;
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hash = 17;
			hash = (hash * 31) + Keyword.GetHashCode();
			hash = (hash * 31) + TypeCode.GetHashCode();
			hash = (hash * 31) + SizeOfInBytes.GetHashCode();
			hash = (hash * 31) + SimpleDescription.GetHashCode();
			return hash;
		}
	}
};
