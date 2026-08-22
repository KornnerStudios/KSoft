using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Bitwise;

internal static partial class BitsEncodingSourceBuilder
{
	public static string BuildNoneableEncoding()
	{
		var writer = new SourceWriter();

		WriteFile(writer, WriteNoneableEncodingBody);

		return writer.ToString();
	}

	private static void WriteNoneableEncodingBody(SourceWriter writer)
	{
		foreach (NumberSpec wordSpec in PrimitiveCatalog.BittableTypesMajorWords)
		{
			WriteNoneableEncodingTraitsMethod(writer, wordSpec);
			writer.WriteLine();
		}
	}

	private static void WriteNoneableEncodingTraitsMethod(SourceWriter writer, NumberSpec wordSpec)
	{
		writer.WriteXmlDocSummary(
			"Calculate the traits needed for representing a bit-encoded value which can also equal NONE (-1)");
		writer.WriteXmlDocParam("maxValue", "An enumeration's <b>kMax</b> value");
		writer.WriteXmlDocParam(
			"bitCount",
			"Receives the positive bit count needed to represent NONE to (<paramref name=\"maxValue\"/> - 1)");
		writer.WriteXmlDocParam(
			"traceVerboseChecks",
			"Should verbose checks be performed and traced? No side effects outside of DEBUG");
		writer.WriteLine("#if DEBUG");
		writer.WriteXmlDocParam(
			"sourceFile",
			"Source file path of this method's caller. DEBUG only, don't manually specify");
		writer.WriteXmlDocParam(
			"sourceLineNum",
			"Source file line of this method's caller. DEBUG only, don't manually specify");
		writer.WriteLine("#endif");
		writer.WriteLine(
			"/// <remarks>A <b>kMax</b> value should be unused and the last entry of an Enumeration. " +
			"This is why 1 is subtracted from <paramref name=\"maxValue\"/>.</remarks>");
		writer.WriteXmlDocReturns("A positive bitmask for the encoded value range");
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static {wordSpec.Keyword} GetNoneableEncodingTraits({wordSpec.SignedKeyword} maxValue");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine(", out int bitCount");
			writer.WriteLine(", bool traceVerboseChecks = true // only used in DEBUG");
			writer.WriteLine("#if DEBUG");
			writer.WriteLine(", [System.Runtime.CompilerServices.CallerFilePath] string sourceFile = \"\"");
			writer.WriteLine(", [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNum = -1");
			writer.WriteLine("#endif");
			writer.WriteLine(")");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (maxValue <= 0)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine(
					"throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, " +
					"\"Maximum value must be positive.\");");
			}
			writer.WriteLine(
				$"ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(maxValue, {wordSpec.SignedKeyword}.MaxValue);");
			writer.WriteLine();
			writer.WriteLine(
				"// Add one to the max value, as NONE encoding adds one to the value when encoding " +
				"(then subtracts on decode)");
			writer.WriteLine("bitCount = GetMaxEnumBits(maxValue+1);");
			writer.WriteLine($"var bitmask = {BitCountToMaskMethodName(wordSpec)}(bitCount);");
			writer.WriteLine("#if DEBUG");
			writer.WriteLine("if (traceVerboseChecks)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine(
					"// GetMaxEnumBits asserts maxValue > 1, as why would you normally want to bit encode an enum " +
					"with only 1 possible value?");
				writer.WriteLine(
					"// hard set naked bit count to 1-bit to get around this case " +
					"(where NONE and 0 are valid values)");
				writer.WriteLine("int naked_bit_count = maxValue > 1");
				using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
				{
					writer.WriteLine("? GetMaxEnumBits(maxValue)");
					writer.WriteLine(": 1;");
				}
				writer.WriteLine();
				writer.WriteLine("if (bitCount > naked_bit_count)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("Debug.Trace.KSoft.TraceInformation(");
					using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
					{
						writer.WriteLine("\"Noneable encoding at {0},#{1} appears inefficient. \" +");
						writer.WriteLine("\"#{2} requires {3}-bits, but encoding overflows to {4}-bits\",");
						writer.WriteLine("sourceFile, sourceLineNum, maxValue, naked_bit_count, bitCount);");
					}
				}
			}
			writer.WriteLine("#endif");
			writer.WriteLine();
			writer.WriteLine("return bitmask;");
		}
	}
}
