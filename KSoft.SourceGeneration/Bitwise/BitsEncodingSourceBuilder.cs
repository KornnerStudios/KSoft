using System;
using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Bitwise;

internal static partial class BitsEncodingSourceBuilder
{
	public const string DecodeHintName = "KSoft.Bits.Decode.g.cs";
	public const string EncodeHintName = "KSoft.Bits.Encode.g.cs";
	public const string NoneableEncodingHintName = "KSoft.Bits.NoneableEncoding.g.cs";

	private static void WriteFile(SourceWriter writer, Action<SourceWriter> writeBody)
	{
		ExceptionHelpers.ThrowIfNull(writer, nameof(writer));
		ExceptionHelpers.ThrowIfNull(writeBody, nameof(writeBody));

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
	}

	private static void WriteBitIndexContracts(SourceWriter writer, NumberSpec wordSpec)
	{
		string constantKeyword = wordSpec.ConstantKeyword;

		writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(bitIndex);");
		writer.WriteLine($"ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitIndex, k{constantKeyword}BitCount);");
	}

	private static void WriteBitMaskContract(SourceWriter writer)
		=> writer.WriteLine("if (bitMask == 0) { throw new ArgumentException(\"Bit mask must be non-zero.\", nameof(bitMask)); }");

	private static void WriteBitCountAssert(SourceWriter writer, NumberSpec wordSpec)
		=> writer.WriteLine(
			$"if ((bitIndex + bit_count) > Bits.k{wordSpec.ConstantKeyword}BitCount) " +
			"{ throw new InvalidOperationException(\"Bit range exceeds the target word size.\"); }");

	private static void WriteUnsignedDecodeXmlDocs(SourceWriter writer, bool includeBitMask)
	{
		writer.WriteXmlDocSummary("Bit decode an enumeration or flags from an unsigned integer");
		writer.WriteXmlDocParam("bits", "Unsigned integer to decode from");
		if (includeBitMask)
		{
			writer.WriteXmlDocParam("bitIndex", "Index in <paramref name=\"bits\"/> to start decoding at");
			writer.WriteXmlDocParam("bitMask", "Masking value for the enumeration\\flags type");
		}
		else
		{
			writer.WriteXmlDocParam("traits", "");
		}
		writer.WriteXmlDocReturns(
			"The enumeration\\flags value as it stood before it was ever encoded into <paramref name=\"bits\"/>");
	}

	private static void WriteNoneableDecodeXmlDocs(SourceWriter writer, bool includeBitMask)
	{
		writer.WriteXmlDocSummary("Bit decode a none-able value from an unsigned integer");
		writer.WriteXmlDocParam("bits", "Unsigned integer to decode from");
		if (includeBitMask)
		{
			writer.WriteXmlDocParam("bitIndex", "Index in <paramref name=\"bits\"/> to start decoding at");
			writer.WriteXmlDocParam("bitMask", "Masking value for the enumeration\\flags type");
		}
		else
		{
			writer.WriteXmlDocParam("traits", "");
		}
		writer.WriteXmlDocReturns(
			"The enumeration\\flags value as it stood before it was ever encoded into <paramref name=\"bits\"/>");
	}

	private static void WriteDecodeRefRemarks(SourceWriter writer)
	{
		writer.WriteLine(
			"/// <remarks>On return <paramref name=\"bitIndex\"/> is incremented by the bit count " +
			"(determined from <paramref name=\"bitMask\"/>)</remarks>");
	}

	private static string BitMaskPropertyName(NumberSpec wordSpec)
		=> "Bitmask" + wordSpec.SizeOfInBits.ToString(PrimitiveCatalog.InvariantCulture);

	private static string BitCountToMaskMethodName(NumberSpec wordSpec)
		=> "BitCountToMask" + wordSpec.SizeOfInBits.ToString(PrimitiveCatalog.InvariantCulture);
}
