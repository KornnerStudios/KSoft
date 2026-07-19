using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Bitwise;

internal static partial class BitsEncodingSourceBuilder
{
	public static string BuildEncode()
	{
		var writer = new SourceWriter();

		WriteFile(writer, WriteEncodeBody);

		return writer.ToString();
	}

	private static void WriteEncodeBody(SourceWriter writer)
	{
		WriteBitEncodeEnumRegion(writer);
		writer.WriteLine();
		WriteBitEncodeFlagsRegion(writer);
		writer.WriteLine();
		WriteBitEncodeRegion(writer);
	}

	private static void WriteBitEncodeEnumRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("BitEncode (Enum)"))
		{
			foreach (NumberSpec wordSpec in PrimitiveCatalog.BittableTypesMajorWords)
			{
				WriteBitEncodeEnumValueMethod(writer, wordSpec);
				WriteBitEncodeEnumRefMethod(writer, wordSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteBitEncodeFlagsRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("BitEncode (Flags)"))
		{
			foreach (NumberSpec wordSpec in PrimitiveCatalog.BittableTypesMajorWords)
			{
				WriteBitEncodeFlagsValueMethod(writer, wordSpec);
				WriteBitEncodeFlagsRefMethod(writer, wordSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteBitEncodeRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("BitEncode"))
		{
			foreach (NumberSpec wordSpec in PrimitiveCatalog.BittableTypesMajorWords)
			{
				WriteBitEncodeValueMethod(writer, wordSpec);
				WriteBitEncodeRefMethod(writer, wordSpec);
				WriteBitEncodeTraitsMethod(writer, wordSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteBitEncodeEnumValueMethod(SourceWriter writer, NumberSpec wordSpec)
	{
		WriteBitEncodeEnumValueXmlDocs(writer, includeReturns: true);
		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"public static {wordSpec.Keyword} BitEncodeEnum({wordSpec.Keyword} value, " +
			$"{wordSpec.Keyword} bits, int bitIndex, {wordSpec.Keyword} bitMask)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitEncodeContracts(writer, wordSpec);
			writer.WriteLine();
			writer.WriteLine("return Bitwise.Flags.Add(bits & ~(bitMask << bitIndex), // clear the bit-space");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine("(value & bitMask) << bitIndex); // add [value] to the newly cleared bit-space");
			}
		}
	}

	private static void WriteBitEncodeEnumRefMethod(SourceWriter writer, NumberSpec wordSpec)
	{
		WriteBitEncodeEnumValueXmlDocs(writer, includeReturns: false);
		WriteBitEncodeIndexRemarks(writer);
		writer.WriteLine(
			$"public static void BitEncodeEnum({wordSpec.Keyword} value, ref {wordSpec.Keyword} bits, " +
			$"ref int bitIndex, {wordSpec.Keyword} bitMask)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitEncodeContracts(writer, wordSpec);
			writer.WriteLine();
			WriteBitEncodeRefCore(writer, wordSpec, "BitEncodeEnum");
		}
	}

	private static void WriteBitEncodeFlagsValueMethod(SourceWriter writer, NumberSpec wordSpec)
	{
		WriteBitEncodeFlagsValueXmlDocs(writer, includeReturns: true);
		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"public static {wordSpec.Keyword} BitEncodeFlags({wordSpec.Keyword} value, " +
			$"{wordSpec.Keyword} bits, int bitIndex, {wordSpec.Keyword} bitMask)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitEncodeContracts(writer, wordSpec);
			writer.WriteLine();
			writer.WriteLine("return Bitwise.Flags.Add(bits,");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine("(value & bitMask) << bitIndex); // add [value] to the existing bits");
			}
		}
	}

	private static void WriteBitEncodeFlagsRefMethod(SourceWriter writer, NumberSpec wordSpec)
	{
		WriteBitEncodeFlagsValueXmlDocs(writer, includeReturns: false);
		WriteBitEncodeIndexRemarks(writer);
		writer.WriteLine(
			$"public static void BitEncodeFlags({wordSpec.Keyword} value, ref {wordSpec.Keyword} bits, " +
			$"ref int bitIndex, {wordSpec.Keyword} bitMask)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitEncodeContracts(writer, wordSpec);
			writer.WriteLine();
			WriteBitEncodeRefCore(writer, wordSpec, "BitEncodeFlags");
		}
	}

	private static void WriteBitEncodeValueMethod(SourceWriter writer, NumberSpec wordSpec)
	{
		WriteBitEncodeValueXmlDocs(writer, includeBitMask: true, includeRefRemarks: false);
		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"public static {wordSpec.Keyword} BitEncode({wordSpec.Keyword} value, {wordSpec.Keyword} bits, " +
			$"int bitIndex, {wordSpec.Keyword} bitMask)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitEncodeContracts(writer, wordSpec);
			writer.WriteLine();
			writer.WriteLine("// Use the bit mask's invert so we can get all of the non-value bits");
			writer.WriteLine("return BitEncodeFlags(value, bits & (~bitMask), bitIndex, bitMask);");
		}
	}

	private static void WriteBitEncodeRefMethod(SourceWriter writer, NumberSpec wordSpec)
	{
		WriteBitEncodeValueXmlDocs(writer, includeBitMask: true, includeRefRemarks: true);
		writer.WriteLine(
			$"public static void BitEncode({wordSpec.Keyword} value, ref {wordSpec.Keyword} bits, " +
			$"ref int bitIndex, {wordSpec.Keyword} bitMask)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitEncodeContracts(writer, wordSpec);
			writer.WriteLine();
			writer.WriteLine("// Use the bit mask's invert so we can get all of the non-value bits");
			writer.WriteLine("bits &= ~bitMask;");
			writer.WriteLine("BitEncodeFlags(value, ref bits, ref bitIndex, bitMask);");
		}
	}

	private static void WriteBitEncodeTraitsMethod(SourceWriter writer, NumberSpec wordSpec)
	{
		WriteBitEncodeValueXmlDocs(writer, includeBitMask: false, includeRefRemarks: false);
		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"public static {wordSpec.Keyword} BitEncode({wordSpec.Keyword} value, {wordSpec.Keyword} bits, " +
			"Bitwise.BitFieldTraits traits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires/*<ArgumentException>*/(!traits.IsEmpty);");
			writer.WriteLine();
			writer.WriteLine($"var bitmask = traits.{BitMaskPropertyName(wordSpec)};");
			writer.WriteLine("// Use the bit mask's invert so we can get all of the non-value bits");
			writer.WriteLine("return BitEncodeFlags(value, bits & (~bitmask), traits.BitIndex, bitmask);");
		}
	}

	private static void WriteBitEncodeContracts(SourceWriter writer, NumberSpec wordSpec)
	{
		WriteBitIndexContracts(writer, wordSpec);
		WriteBitMaskContract(writer);
	}

	private static void WriteBitEncodeRefCore(SourceWriter writer, NumberSpec wordSpec, string methodName)
	{
		writer.WriteLine("int bit_count = BitCount(bitMask);");
		WriteBitCountAssert(writer, wordSpec);
		writer.WriteLine();
		writer.WriteLine($"bits = {methodName}(value, bits, bitIndex, bitMask);");
		writer.WriteLine("bitIndex += bit_count;");
	}

	private static void WriteBitEncodeEnumValueXmlDocs(SourceWriter writer, bool includeReturns)
	{
		writer.WriteXmlDocSummary("Bit encode an enumeration value into an unsigned integer");
		WriteBitEncodeCommonParams(writer, "Enumeration value to encode", includeBitMask: true);
		if (includeReturns)
		{
			writer.WriteXmlDocReturns("<paramref name=\"bits\"/> with <paramref name=\"value\"/> encoded into it");
			WriteBitEncodeClearingRemarks(writer, "so", "any possibly existing value will be zeroed before");
		}
	}

	private static void WriteBitEncodeFlagsValueXmlDocs(SourceWriter writer, bool includeReturns)
	{
		writer.WriteXmlDocSummary("Bit encode a flags value into an unsigned integer");
		WriteBitEncodeCommonParams(writer, "Flags to encode", includeBitMask: true);
		if (includeReturns)
		{
			writer.WriteXmlDocReturns("<paramref name=\"bits\"/> with <paramref name=\"value\"/> encoded into it");
			writer.WriteLine("/// <remarks>");
			writer.WriteLine(
				"/// Doesn't clear the bit-space between <paramref name=\"bitIndex\"/> + <paramref name=\"bitMask\"/>");
			writer.WriteLine(
				"/// so any possibly existing flags will be retained before and after <paramref name=\"value\"/> is added");
			writer.WriteLine("/// </remarks>");
		}
	}

	private static void WriteBitEncodeValueXmlDocs(
		SourceWriter writer,
		bool includeBitMask,
		bool includeRefRemarks)
	{
		writer.WriteXmlDocSummary(
			"Bit encode a value into an unsigned integer, removing the original data in the value's range");
		WriteBitEncodeCommonParams(writer, "Value to encode", includeBitMask);
		writer.WriteXmlDocReturns("<paramref name=\"bits\"/> with <paramref name=\"value\"/> encoded into it");
		if (!includeRefRemarks)
		{
			WriteBitEncodeClearingRemarks(writer, null, "so any existing values will be lost after");
			return;
		}

		writer.WriteLine("/// <remarks>");
		writer.WriteLine("/// Clears the bit-space between <paramref name=\"bitIndex\"/> + <paramref name=\"bitMask\"/>");
		writer.WriteLine("/// so any existing values will be lost after <paramref name=\"value\"/> is added");
		writer.WriteLine("///");
		writer.WriteLine("/// On return <paramref name=\"bitIndex\"/> is incremented by the bit count (determined");
		writer.WriteLine("/// from <paramref name=\"bitMask\"/>)");
		writer.WriteLine("/// </remarks>");
	}

	private static void WriteBitEncodeClearingRemarks(
		SourceWriter writer,
		string firstContinuationLine,
		string secondContinuationPrefix)
	{
		writer.WriteLine("/// <remarks>");
		if (firstContinuationLine is null)
		{
			writer.WriteLine("/// Clears the bit-space between <paramref name=\"bitIndex\"/> + <paramref name=\"bitMask\"/>");
		}
		else
		{
			writer.WriteLine(
				"/// Clears the bit-space between <paramref name=\"bitIndex\"/> + " +
				$"<paramref name=\"bitMask\"/> {firstContinuationLine}");
		}
		writer.WriteLine($"/// {secondContinuationPrefix} <paramref name=\"value\"/> is added");
		writer.WriteLine("/// </remarks>");
	}

	private static void WriteBitEncodeCommonParams(
		SourceWriter writer,
		string valueDescription,
		bool includeBitMask)
	{
		writer.WriteXmlDocParam("value", valueDescription);
		writer.WriteXmlDocParam("bits", "Bit data as an unsigned integer");
		if (includeBitMask)
		{
			writer.WriteXmlDocParam("bitIndex", "Index in <paramref name=\"bits\"/> to start encoding at");
			writer.WriteXmlDocParam("bitMask", "Masking value for <paramref name=\"value\"/>");
		}
		else
		{
			writer.WriteXmlDocParam("traits", "");
		}
	}

	private static void WriteBitEncodeIndexRemarks(SourceWriter writer)
	{
		writer.WriteLine("/// <remarks>");
		writer.WriteLine(
			"/// On return <paramref name=\"bits\"/> has <paramref name=\"value\"/> encoded into it and " +
			"<paramref name=\"bitIndex\"/>");
		writer.WriteLine("/// is incremented by the bit count (determined from <paramref name=\"bitMask\"/>)");
		writer.WriteLine("/// </remarks>");
	}
}
