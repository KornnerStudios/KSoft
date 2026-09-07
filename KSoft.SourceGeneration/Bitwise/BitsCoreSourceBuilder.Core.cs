using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Bitwise;

internal static partial class BitsCoreSourceBuilder
{
	public static string BuildCore()
		=> BuildFile(WriteCoreBody);

	private static void WriteCoreBody(SourceWriter writer)
	{
		WriteBitmaskLookupTableRegion(writer);
		writer.WriteLine();
		WriteBitReverseRefRegion(writer);
		writer.WriteLine();
		WriteGetMaxEnumBitsRegion(writer);
		writer.WriteLine();
		WriteGetBitmaskRegion(writer);
		writer.WriteLine();
		WriteSignExtendRegion(writer);
	}

	private static void WriteBitmaskLookupTableRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("BitmaskLookUpTable"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				WriteBitmaskLookupTableMethods(writer, typeSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteBitmaskLookupTableMethods(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteXmlDocSummary($"Generate an {typeSpec.SizeOfInBits}-bit bit count to bitmask table");
		writer.WriteXmlDocParam("wordBitSize", "Number of bits to generate a table for");
		writer.WriteXmlDocParam("lut", "Receives a non-null bitmask lookup table");
		writer.WriteLine("/// <remarks>Treat <paramref name=\"lut\"/> as <b>read-only</b></remarks>");
		writer.WriteLine($"public static void BitmaskLookUpTableGenerate(int wordBitSize, out {typeSpec.Keyword}[] lut)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (wordBitSize <= 0)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine(
					"throw new ArgumentOutOfRangeException(nameof(wordBitSize), wordBitSize, " +
					"\"Word bit size must be positive.\");");
			}
			writer.WriteLine(
				$"ArgumentOutOfRangeException.ThrowIfGreaterThan(wordBitSize, {BitCountConstantName(typeSpec)});");
			writer.WriteLine();
			writer.WriteLine(
				$"if (wordBitSize == {BitCountConstantName(typeSpec)} && {BitmaskLookupName(typeSpec)} != null)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"lut = {BitmaskLookupName(typeSpec)};");
			}
			writer.WriteLine("else");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"lut = new {typeSpec.Keyword}[BitmaskLookUpTableGetLength(wordBitSize)];");
				writer.WriteLine("for (int x = 1, shift = lut.Length-2; x < lut.Length; x++, shift--)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine($"lut[x] = ({typeSpec.Keyword})({typeSpec.Keyword}.MaxValue >> shift);");
				}
			}
		}

		writer.WriteXmlDocSummary($"Generate an {typeSpec.SizeOfInBits}-bit bit count to bitmask table");
		writer.WriteXmlDocParam("wordBitSize", "Number of bits to generate a table for");
		writer.WriteXmlDocReturns("A non-null bitmask lookup table");
		writer.WriteLine("/// <remarks>Treat <paramref name=\"lut\"/> as <b>read-only</b></remarks>");
		writer.WriteLine($"public static {typeSpec.Keyword}[] {BitmaskLookupGenerateMethodName(typeSpec)}(int wordBitSize)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (wordBitSize <= 0)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine(
					"throw new ArgumentOutOfRangeException(nameof(wordBitSize), wordBitSize, " +
					"\"Word bit size must be positive.\");");
			}
			writer.WriteLine(
				$"ArgumentOutOfRangeException.ThrowIfGreaterThan(wordBitSize, {BitCountConstantName(typeSpec)});");
			writer.WriteLine();
			writer.WriteLine($"BitmaskLookUpTableGenerate(wordBitSize, out {typeSpec.Keyword}[] lut);");
			writer.WriteLine();
			writer.WriteLine("return lut;");
		}
	}

	private static void WriteBitReverseRefRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("BitReverse (by-ref)"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				writer.WriteXmlDocSummary("Get the bit-reversed equivalent of an unsigned integer");
				writer.WriteXmlDocParam("x", "Integer to bit-reverse");
				writer.WriteLine($"public static void BitReverse(ref {typeSpec.Keyword} x)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("x = BitReverse(x);");
				}
				writer.WriteLine();
			}
		}
	}

	private static void WriteGetMaxEnumBitsRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("GetMaxEnumBits"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesMajorWords)
			{
				WriteGetMaxEnumBitsMethod(writer, typeSpec, typeSpec.Keyword, "");
				WriteGetMaxEnumBitsMethod(writer, typeSpec, typeSpec.SignedKeyword, $"({typeSpec.Keyword})");
				writer.WriteLine();
			}
		}
	}

	private static void WriteGetMaxEnumBitsMethod(
		SourceWriter writer,
		NumberSpec wordSpec,
		string argumentType,
		string castPrefix)
	{
		WriteMaxEnumXmlDocs(writer, "Number of bits needed to represent");
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static int GetMaxEnumBits({argumentType} maxValue)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (maxValue <= 1)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine(
					"throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, " +
					"kGetMaxEnumBits_MaxValueOutOfRangeMessage);");
			}
			writer.WriteLine();
			writer.WriteLine($"return Bits.IndexOfHighestBitSet({castPrefix}maxValue - 1) + 1;");
		}
	}

	private static void WriteGetBitmaskRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("GetBitmask"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesMajorWords)
			{
				WriteGetBitmaskEnumMethod(writer, typeSpec);
				WriteGetBitmaskFlagsMethod(writer, typeSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteGetBitmaskEnumMethod(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteXmlDocSummary("Calculate the masking value for an enumeration");
		writer.WriteXmlDocParam("maxValue", "An enumeration's <b>kMax</b> value");
		writer.WriteXmlDocReturns(
			"The smallest positive bit mask value for (<paramref name=\"maxValue\"/> - 1)");
		writer.WriteLine(
			"/// <remarks>A <b>kMax</b> value should be unused and the last entry of an Enumeration. " +
			"This is why 1 is subtracted from <paramref name=\"maxValue\"/>.</remarks>");
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static {typeSpec.Keyword} GetBitmaskEnum({typeSpec.Keyword} maxValue)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (maxValue <= 1)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine(
					"throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, " +
					"kGetBitmaskEnum_MaxValueOutOfRangeMessage);");
			}
			writer.WriteLine();
			writer.WriteLine("int bit_count = GetMaxEnumBits(maxValue);");
			writer.WriteLine();
			writer.WriteLine($"return {BitCountToMaskMethodName(typeSpec)}(bit_count);");
		}
	}

	private static void WriteGetBitmaskFlagsMethod(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteXmlDocSummary("Calculate the masking value for a series of flags");
		writer.WriteXmlDocParam(
			"maxValue",
			"A bit enumeration's <b>kMax</b> value. IE, the 'highest bit' plus one");
		writer.WriteXmlDocReturns(
			"The smallest bit mask value for (<paramref name=\"maxValue\"/> - 1)");
		writer.WriteLine(
			"/// <remarks>A <b>kMax</b> value should be unused and the last entry of an Enumeration. " +
			"This is why 1 is subtracted from <paramref name=\"maxValue\"/>.</remarks>");
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static {typeSpec.Keyword} GetBitmaskFlags({typeSpec.Keyword} maxValue)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (maxValue <= 0)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine(
					"throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, " +
					"kGetBitmaskFlag_MaxValueOutOfRangeMessage);");
			}
			writer.WriteLine(
				$"ArgumentOutOfRangeException.ThrowIfGreaterThan(maxValue, ({typeSpec.Keyword}){BitCountConstantName(typeSpec)});");
			writer.WriteLine();
			writer.WriteLine($"return {BitCountToMaskMethodName(typeSpec)}((int)--maxValue);");
		}
	}

	private static void WriteSignExtendRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("SignExtend"))
		{
			writer.WriteLine("// http://graphics.stanford.edu/~seander/bithacks.html#VariableSignExtend");
			writer.WriteLine();
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesMajorWords)
			{
				WriteSignExtendMethod(writer, typeSpec, clearValue: true);
				WriteSignExtendMethod(writer, typeSpec, clearValue: false);
				writer.WriteLine();
			}
		}
	}

	private static void WriteSignExtendMethod(SourceWriter writer, NumberSpec typeSpec, bool clearValue)
	{
		string methodName = clearValue
			? "SignExtend"
			: "SignExtendWithoutClear";

		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"public static {typeSpec.SignedKeyword} {methodName}({typeSpec.SignedKeyword} value, int bitCount)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (bitCount <= 0)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine(
					"throw new ArgumentOutOfRangeException(nameof(bitCount), bitCount, " +
					"\"Bit count must be positive.\");");
			}
			writer.WriteLine(
				$"ArgumentOutOfRangeException.ThrowIfGreaterThan(bitCount, {BitCountConstantName(typeSpec)});");
			if (clearValue)
			{
				writer.WriteLine($"const {typeSpec.Keyword} k_one = 1;");
				writer.WriteLine();
				writer.WriteLine(
					$"var bit_mask = ({typeSpec.SignedKeyword}){BitCountToMaskMethodName(typeSpec)}(bitCount);");
				writer.WriteLine($"var ext_mask = ({typeSpec.SignedKeyword})(k_one << (bitCount - 1));");
				writer.WriteLine();
				writer.WriteLine("// clear the bits outside of our bit count range");
				writer.WriteLine("value &= bit_mask;");
				writer.WriteLine();
				writer.WriteLine("// if the clear operation above isn't needed, we could do the following instead:");
				writer.WriteLine("// (value << ext_mask) >> ext_mask");
				writer.WriteLine("return (value ^ ext_mask) - ext_mask;");
			}
			else
			{
				writer.WriteLine();
				writer.WriteLine($"int ext_shift = {BitCountConstantName(typeSpec)} - bitCount;");
				writer.WriteLine();
				writer.WriteLine("return (value << ext_shift) >> ext_shift;");
			}
		}
	}

	private static void WriteMaxEnumXmlDocs(SourceWriter writer, string returnsPrefix)
	{
		writer.WriteXmlDocSummary("Calculate how many bits are needed to represent the provided value");
		writer.WriteXmlDocParam("maxValue", "An enumeration's <b>kMax</b> value");
		writer.WriteXmlDocReturns($"{returnsPrefix} (<paramref name=\"maxValue\"/> - 1), always positive");
		writer.WriteLine(
			"/// <remarks>A <b>kMax</b> value should be unused and the last entry of an Enumeration. " +
			"This is why 1 is subtracted from <paramref name=\"maxValue\"/>.</remarks>");
	}
}
