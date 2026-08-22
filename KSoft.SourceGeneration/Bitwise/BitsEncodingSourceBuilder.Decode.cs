using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Bitwise;

internal static partial class BitsEncodingSourceBuilder
{
	public static string BuildDecode()
	{
		var writer = new SourceWriter();

		WriteFile(writer, WriteDecodeBody);

		return writer.ToString();
	}

	private static void WriteDecodeBody(SourceWriter writer)
	{
		foreach (NumberSpec wordSpec in PrimitiveCatalog.BittableTypesMajorWords)
		{
			WriteBitDecodeRegion(writer, wordSpec);
		}

		writer.WriteLine();
		WriteBitFieldExtractRegion(writer);
	}

	private static void WriteBitDecodeRegion(SourceWriter writer, NumberSpec wordSpec)
	{
		using (writer.EnterRegion($"BitDecode {wordSpec.SizeOfInBits}"))
		{
			WriteBitDecodeValueMethod(writer, wordSpec);
			WriteBitDecodeNoneableMethod(writer, wordSpec);
			WriteBitDecodeTraitsMethod(writer, wordSpec);
			WriteBitDecodeNoneableTraitsMethod(writer, wordSpec);
			writer.WriteLine();
			WriteBitDecodeRefMethod(writer, wordSpec);
			WriteBitDecodeNoneableRefMethod(writer, wordSpec);
		}
	}

	private static void WriteBitDecodeValueMethod(SourceWriter writer, NumberSpec wordSpec)
	{
		WriteUnsignedDecodeXmlDocs(writer, includeBitMask: true);
		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"public static {wordSpec.Keyword} BitDecode({wordSpec.Keyword} bits, " +
			$"int bitIndex, {wordSpec.Keyword} bitMask)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitIndexContracts(writer, wordSpec);
			WriteBitMaskContract(writer);
			writer.WriteLine();
			writer.WriteLine("return (bits >> bitIndex) & bitMask;");
		}
	}

	private static void WriteBitDecodeNoneableMethod(SourceWriter writer, NumberSpec wordSpec)
	{
		WriteNoneableDecodeXmlDocs(writer, includeBitMask: true);
		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"public static {wordSpec.SignedKeyword} BitDecodeNoneable({wordSpec.Keyword} bits, " +
			$"int bitIndex, {wordSpec.Keyword} bitMask)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitIndexContracts(writer, wordSpec);
			WriteBitMaskContract(writer);
			writer.WriteLine();
			writer.WriteLine($"return ({wordSpec.SignedKeyword})BitDecode(bits, bitIndex, bitMask) - 1;");
		}
	}

	private static void WriteBitDecodeTraitsMethod(SourceWriter writer, NumberSpec wordSpec)
	{
		WriteUnsignedDecodeXmlDocs(writer, includeBitMask: false);
		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"public static {wordSpec.Keyword} BitDecode({wordSpec.Keyword} bits, Bitwise.BitFieldTraits traits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (traits.IsEmpty) { throw new ArgumentException(\"Traits must not be empty.\", nameof(traits)); }");
			writer.WriteLine();
			writer.WriteLine($"return (bits >> traits.BitIndex) & traits.{BitMaskPropertyName(wordSpec)};");
		}
	}

	private static void WriteBitDecodeNoneableTraitsMethod(SourceWriter writer, NumberSpec wordSpec)
	{
		WriteNoneableDecodeXmlDocs(writer, includeBitMask: false);
		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"public static {wordSpec.SignedKeyword} BitDecodeNoneable({wordSpec.Keyword} bits, " +
			"Bitwise.BitFieldTraits traits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (traits.IsEmpty) { throw new ArgumentException(\"Traits must not be empty.\", nameof(traits)); }");
			writer.WriteLine();
			writer.WriteLine(
				$"return ({wordSpec.SignedKeyword})BitDecode(bits, traits.BitIndex, " +
				$"traits.{BitMaskPropertyName(wordSpec)}) - 1;");
		}
	}

	private static void WriteBitDecodeRefMethod(SourceWriter writer, NumberSpec wordSpec)
	{
		WriteUnsignedDecodeXmlDocs(writer, includeBitMask: true);
		WriteDecodeRefRemarks(writer);
		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"public static {wordSpec.Keyword} BitDecode({wordSpec.Keyword} bits, " +
			$"ref int bitIndex, {wordSpec.Keyword} bitMask)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitIndexContracts(writer, wordSpec);
			WriteBitMaskContract(writer);
			writer.WriteLine();
			writer.WriteLine("int bit_count = BitCount(bitMask);");
			WriteBitCountAssert(writer, wordSpec);
			writer.WriteLine();
			writer.WriteLine("var value = (bits >> bitIndex) & bitMask;");
			writer.WriteLine("bitIndex += bit_count;");
			writer.WriteLine();
			writer.WriteLine("return value;");
		}
	}

	private static void WriteBitDecodeNoneableRefMethod(SourceWriter writer, NumberSpec wordSpec)
	{
		WriteNoneableDecodeXmlDocs(writer, includeBitMask: true);
		WriteDecodeRefRemarks(writer);
		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"public static {wordSpec.SignedKeyword} BitDecodeNoneable({wordSpec.Keyword} bits, " +
			$"ref int bitIndex, {wordSpec.Keyword} bitMask)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitIndexContracts(writer, wordSpec);
			WriteBitMaskContract(writer);
			writer.WriteLine();
			writer.WriteLine($"return ({wordSpec.SignedKeyword})BitDecode(bits, ref bitIndex, bitMask) - 1;");
		}
	}

	private static void WriteBitFieldExtractRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("BitFieldExtract"))
		{
			foreach (NumberSpec wordSpec in PrimitiveCatalog.BittableTypesMajorWords)
			{
				WriteBitFieldExtractRangeMethod(writer, wordSpec);
				WriteBitFieldExtractValueMethod(writer, wordSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteBitFieldExtractRangeMethod(SourceWriter writer, NumberSpec wordSpec)
	{
		writer.WriteXmlDocSummary("Extract a range of bits from an unsigned integer");
		writer.WriteXmlDocParam("bits", "Unsigned integer to extract from");
		writer.WriteXmlDocParam("bitIndexLow", "Index in <paramref name=\"bits\"/> to start extracting from");
		writer.WriteXmlDocParam("bitIndexHigh", "Index in <paramref name=\"bits\"/> to stop extracting at");
		writer.WriteXmlDocReturns("Returns bits <paramref name=\"bitIndexLow\"/> to <paramref name=\"bitIndexHigh\"/>");
		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"public static {wordSpec.Keyword} BitFieldExtractRange({wordSpec.Keyword} bits, " +
			"int bitIndexLow, int bitIndexHigh)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(bitIndexLow);");
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(bitIndexHigh);");
			writer.WriteLine(
				$"ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitIndexLow, k{wordSpec.ConstantKeyword}BitCount);");
			writer.WriteLine(
				$"ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitIndexHigh, k{wordSpec.ConstantKeyword}BitCount);");
			writer.WriteLine();
			writer.WriteLine("var shifted = bits >> bitIndexLow; // Shift the bit field to start at the 0th bit");
			writer.WriteLine(
				$"var mask = {BitCountToMaskMethodName(wordSpec)}((bitIndexHigh-bitIndexLow) + 1); " +
				"// Generate a mask of the bit range");
			writer.WriteLine();
			writer.WriteLine("return shifted & mask;");
		}
	}

	private static void WriteBitFieldExtractValueMethod(SourceWriter writer, NumberSpec wordSpec)
	{
		writer.WriteXmlDocSummary("Extract a value represented in a bit-field");
		writer.WriteXmlDocParam("bits", "Unsigned integer to extract from");
		writer.WriteXmlDocParam("bitIndex", "Index in <paramref name=\"bits\"/> to start extracting from");
		writer.WriteXmlDocParam("bitCount", "Number of bits representing the value to extract");
		writer.WriteXmlDocReturns("");
		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"public static {wordSpec.Keyword} BitFieldExtractValue({wordSpec.Keyword} bits, " +
			"int bitIndex, int bitCount)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(bitIndex);");
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(bitCount);");
			writer.WriteLine(
				$"ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitIndex, k{wordSpec.ConstantKeyword}BitCount);");
			writer.WriteLine(
				$"ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitIndex + (bitCount-1), " +
				$"k{wordSpec.ConstantKeyword}BitCount);");
			writer.WriteLine();
			writer.WriteLine("return BitFieldExtractRange(bits, bitIndex, bitIndex + (bitCount-1));");
		}
	}
}
