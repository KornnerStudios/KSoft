using System;
using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Bitwise;

internal static partial class BitsCoreSourceBuilder
{
	public const string BitReverseHintName = "KSoft.Bits.BitReverse.g.cs";
	public const string BitSwapHintName = "KSoft.Bits.BitSwap.g.cs";
	public const string ConstantsHintName = "KSoft.Bits.Constants.g.cs";
	public const string CoreHintName = "KSoft.Bits.Core.g.cs";
	public const string VectorsHintName = "KSoft.Bits.Vectors.g.cs";

	private static string BuildFile(
		Action<SourceWriter> writeBody)
	{
		ExceptionHelpers.ThrowIfNull(writeBody, nameof(writeBody));

		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("#nullable disable");
		writer.WriteLine();
		writer.WriteLine("using System;");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft");
		writer.WriteLine();
		using (writer.EnterTypeDeclaration("partial class Bits"))
		{
			writeBody(writer);
		}

		return writer.ToString();
	}

	private static string TypeCodeName(PrimitiveSpec typeSpec)
		=> typeSpec.TypeCode.ToString();

	private static string TypeCodeName(NumberSpec typeSpec)
		=> typeSpec.TypeCode.ToString();

	private static string SignedTypeCodeName(NumberSpec typeSpec)
		=> typeSpec.SignedTypeCode.ToString();

	private static string SizeOfExpression(PrimitiveSpec typeSpec)
		=> $"sizeof({typeSpec.Keyword})";

	private static string SizeOfExpression(NumberSpec typeSpec)
		=> $"sizeof({typeSpec.Keyword})";

	private static string BitCountConstantName(PrimitiveSpec typeSpec)
		=> $"k{TypeCodeName(typeSpec)}BitCount";

	private static string BitCountConstantName(NumberSpec typeSpec)
		=> $"k{TypeCodeName(typeSpec)}BitCount";

	private static string BitShiftConstantName(NumberSpec typeSpec)
		=> $"k{TypeCodeName(typeSpec)}BitShift";

	private static string BitModConstantName(NumberSpec typeSpec)
		=> $"k{TypeCodeName(typeSpec)}BitMod";

	private static string GeneralBitCountConstantName(NumberSpec typeSpec)
		=> $"k{typeSpec.ConstantKeyword}BitCount";

	private static string GeneralBitShiftConstantName(NumberSpec typeSpec)
		=> $"k{typeSpec.ConstantKeyword}BitShift";

	private static string GeneralBitModConstantName(NumberSpec typeSpec)
		=> $"k{typeSpec.ConstantKeyword}BitMod";

	private static string BitmaskLookupName(NumberSpec typeSpec)
		=> $"kBitmaskLookup{typeSpec.SizeOfInBits.ToString(PrimitiveCatalog.InvariantCulture)}";

	private static string BitmaskLookupGenerateMethodName(NumberSpec typeSpec)
		=> $"Bitmask{TypeCodeName(typeSpec)}LookUpTableGenerate";

	private static string BitCountToMaskMethodName(NumberSpec typeSpec)
		=> $"BitCountToMask{typeSpec.SizeOfInBits.ToString(PrimitiveCatalog.InvariantCulture)}";

	private static string VectorWordName(NumberSpec typeSpec)
		=> typeSpec.ConstantKeyword == "Byte"
			? "Bytes"
			: typeSpec.ConstantKeyword;

	private static string VectorSystemTypeName(NumberSpec typeSpec)
		=> "System." + (typeSpec.ConstantKeyword == "Byte"
			? "Byte"
			: typeSpec.ConstantKeyword);

	private static string HexLiteral(NumberSpec typeSpec, string value)
		=> typeSpec.TypeCode == TypeCode.UInt64
			? value
			: value.Substring(0, typeSpec.SizeOfInBytes * 2 + 2);

	private static int BitShiftValue(NumberSpec typeSpec)
		=> typeSpec.SizeOfInBits switch
		{
			8	=> 3,
			16	=> 4,
			32	=> 5,
			64	=> 6,
			_ => throw new InvalidOperationException(typeSpec.TypeCode.ToString()),
		};
}
