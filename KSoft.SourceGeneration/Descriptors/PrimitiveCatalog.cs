using System;
using System.Collections.Generic;
using System.Globalization;

namespace KSoft.SourceGeneration.Descriptors;

/// <summary>
/// Canonical primitive descriptor catalog used by KSoft source generators.
/// </summary>
/// <remarks>
/// This replaces the static descriptor surface previously provided by <c>KSoft.T4.PrimitiveDefinitions</c> and
/// <c>KSoft.T4.Bitwise.BitwiseT4</c>. Keep the order of each list aligned with the T4 source because generated
/// member order is part of the review surface for T4-to-Roslyn migration packets.
/// </remarks>
internal static class PrimitiveCatalog
{
	/// <summary>
	/// Number of bits in a byte.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.Bitwise.BitwiseT4.kBitsPerByte</c>.</remarks>
	public const int BitsPerByte = 8;

	/// <summary>
	/// Culture used for deterministic descriptor formatting.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.UtilT4.InvariantCultureInfo</c>.</remarks>
	public static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

	private static readonly PrimitiveSpec kBool = new("bool", TypeCode.Boolean, sizeof(bool));
	private static readonly PrimitiveSpec kChar = new("char", TypeCode.Char, sizeof(char));
	private static readonly PrimitiveSpec kString = new("string", TypeCode.String, -1);
	private static readonly PrimitiveSpec kKGuid = new("Values.KGuid", TypeCode.Object, 16);

	private static readonly NumberSpec kByte = new(TypeCode.Byte);
	private static readonly NumberSpec kSByte = new(TypeCode.SByte);
	private static readonly NumberSpec kUInt16 = new(TypeCode.UInt16);
	private static readonly NumberSpec kInt16 = new(TypeCode.Int16);
	private static readonly NumberSpec kUInt32 = new(TypeCode.UInt32);
	private static readonly NumberSpec kInt32 = new(TypeCode.Int32);
	private static readonly NumberSpec kUInt64 = new(TypeCode.UInt64);
	private static readonly NumberSpec kInt64 = new(TypeCode.Int64);
	private static readonly NumberSpec kSingle = new(TypeCode.Single);
	private static readonly NumberSpec kDouble = new(TypeCode.Double);

	/// <summary>
	/// All primitive type definitions that are numeric.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.PrimitiveDefinitions.Numbers</c>.</remarks>
	public static IReadOnlyList<NumberSpec> Numbers { get; } =
		[
			kByte,
			kSByte,
			kUInt16,
			kInt16,
			kUInt32,
			kInt32,
			kUInt64,
			kInt64,
			kSingle,
			kDouble,
		];

	/// <summary>
	/// All primitive type definitions, excluding <see cref="String" />.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.PrimitiveDefinitions.Primitives</c>.</remarks>
	public static IReadOnlyList<PrimitiveSpec> Primitives { get; } =
		[
			kBool,
			kChar,
			kByte.Primitive,
			kSByte.Primitive,
			kUInt16.Primitive,
			kInt16.Primitive,
			kUInt32.Primitive,
			kInt32.Primitive,
			kUInt64.Primitive,
			kInt64.Primitive,
			kSingle.Primitive,
			kDouble.Primitive,
		];

	/// <summary>
	/// Integer primitive types that can be used by the bitwise generators.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.Bitwise.BitwiseT4.BittableTypes</c>.</remarks>
	public static IReadOnlyList<NumberSpec> BittableTypes { get; } =
		[
			kByte,
			kSByte,
			kUInt16,
			kInt16,
			kUInt32,
			kInt32,
			kUInt64,
			kInt64,
		];

	/// <summary>
	/// Unsigned integer primitive types that can be used by the bitwise generators.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.Bitwise.BitwiseT4.BittableTypes_Unsigned</c>.</remarks>
	public static IReadOnlyList<NumberSpec> BittableTypesUnsigned { get; } =
		[
			kByte,
			kUInt16,
			kUInt32,
			kUInt64,
		];

	/// <summary>
	/// Major word integer primitive types used by bitstream and bit-vector generators.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.Bitwise.BitwiseT4.BittableTypes_MajorWords</c>.</remarks>
	public static IReadOnlyList<NumberSpec> BittableTypesMajorWords { get; } =
		[
			kUInt32,
			kUInt64,
		];

	/// <summary>
	/// Integer-like primitive types used by the BitStream generated read, write, and serialize surfaces.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.Bitwise.BitwiseT4.BitStreambleIntegerTypes</c>.</remarks>
	public static IReadOnlyList<PrimitiveSpec> BitStreamableIntegerTypes { get; } =
		[
			kChar,
			kByte.Primitive,
			kSByte.Primitive,
			kUInt16.Primitive,
			kInt16.Primitive,
			kUInt32.Primitive,
			kInt32.Primitive,
			kUInt64.Primitive,
			kInt64.Primitive,
		];

	/// <summary>
	/// Non-integer primitive types used by the BitStream generated serialize surfaces.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.Bitwise.BitwiseT4.BitStreambleNonIntegerTypes</c>.</remarks>
	public static IReadOnlyList<PrimitiveSpec> BitStreamableNonIntegerTypes { get; } =
		[
			kBool,
			kSingle.Primitive,
			kDouble.Primitive,
		];

	/// <summary>
	/// 32-bit signed and unsigned integer primitive types used by bitwise generators.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.Bitwise.BitwiseT4.BittableTypesInt32</c>.</remarks>
	public static IReadOnlyList<NumberSpec> BittableTypesInt32 { get; } =
		[
			kUInt32,
			kInt32,
		];

	/// <summary>
	/// 32-bit and 64-bit signed and unsigned integer primitive types used by bitwise generators.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.Bitwise.BitwiseT4.BittableTypesInt32And64</c>.</remarks>
	public static IReadOnlyList<NumberSpec> BittableTypesInt32And64 { get; } =
		[
			kUInt32,
			kInt32,
			kUInt64,
			kInt64,
		];

	/// <summary>
	/// Boolean primitive descriptor.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.PrimitiveDefinitions.kBool</c>.</remarks>
	public static PrimitiveSpec Bool => kBool;

	/// <summary>
	/// Character primitive descriptor.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.PrimitiveDefinitions.kChar</c>.</remarks>
	public static PrimitiveSpec Char => kChar;

	/// <summary>
	/// String primitive descriptor.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.PrimitiveDefinitions.kString</c>.</remarks>
	public static PrimitiveSpec String => kString;

	/// <summary>
	/// KSoft GUID primitive descriptor.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.PrimitiveDefinitions.kKGuid</c>.</remarks>
	public static PrimitiveSpec KGuid => kKGuid;

	/// <summary>
	/// Gets the C# keyword for a supported primitive <see cref="TypeCode" />.
	/// </summary>
	/// <param name="typeCode">Type code to map.</param>
	/// <returns>The C# keyword used by generated code.</returns>
	/// <remarks>Maps to <c>KSoft.T4.NumberCodeDefinition.TypeCodeToKeyword</c>.</remarks>
	public static string KeywordFor(TypeCode typeCode)
		=> typeCode switch
		{
			TypeCode.Byte	=>  "byte",
			TypeCode.SByte	=> "sbyte",
			TypeCode.UInt16	=> "ushort",
			TypeCode.Int16	=>  "short",
			TypeCode.UInt32	=> "uint",
			TypeCode.Int32	=>  "int",
			TypeCode.UInt64	=> "ulong",
			TypeCode.Int64	=>  "long",
			TypeCode.Single	=> "float",
			TypeCode.Double	=> "double",
			_ => throw new ArgumentException(typeCode.ToString(), nameof(typeCode)),
		};

	/// <summary>
	/// Gets the canonical numeric descriptor for a supported primitive <see cref="TypeCode" />.
	/// </summary>
	/// <param name="typeCode">Type code to map.</param>
	/// <returns>The shared numeric descriptor used by generated code.</returns>
	/// <remarks>Maps to the <c>KSoft.T4.PrimitiveDefinitions.k*</c> numeric descriptor fields.</remarks>
	public static NumberSpec NumberFor(TypeCode typeCode)
		=> typeCode switch
		{
			TypeCode.Byte	=> kByte,
			TypeCode.SByte	=> kSByte,
			TypeCode.UInt16	=> kUInt16,
			TypeCode.Int16	=> kInt16,
			TypeCode.UInt32	=> kUInt32,
			TypeCode.Int32	=> kInt32,
			TypeCode.UInt64	=> kUInt64,
			TypeCode.Int64	=> kInt64,
			TypeCode.Single	=> kSingle,
			TypeCode.Double	=> kDouble,
			_ => throw new ArgumentException(typeCode.ToString(), nameof(typeCode)),
		};
};
