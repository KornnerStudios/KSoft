using System;
using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Bitwise;

internal static class BitsBitCountSourceBuilder
{
	public const string HintName = "KSoft.Bits.BitCount.g.cs";

	public static string Build()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("using System;");
		writer.WriteLine("using System.Numerics;");
		writer.WriteLine("using Contracts = System.Diagnostics.Contracts;");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft");
		writer.WriteLine();
		using (writer.EnterTypeDeclaration("partial class Bits"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				if (typeSpec.TypeCode == TypeCode.UInt16)
				{
					// Match the active T4 output: ushort only existed in the disabled pre-BitOperations block.
					continue;
				}

				WriteBitCountMethod(writer, typeSpec);
				writer.WriteLine();
			}

			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesMajorWords)
			{
				WriteBitCountToMaskMethod(writer, typeSpec);
				writer.WriteLine();
			}
		}

		return writer.ToString();
	}

	private static void WriteBitCountMethod(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteXmlDocSummary("Count the number of 'on' bits in an unsigned integer");
		writer.WriteXmlDocParam("bits", "Integer whose bits to count");
		writer.WriteXmlDocReturns();
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static int BitCount({typeSpec.Keyword} bits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("// #VITA_SHIM: Keep KSoft API while callers migrate to BitOperations.PopCount.");
			writer.WriteLine("return BitOperations.PopCount(bits);");
		}
	}

	private static void WriteBitCountToMaskMethod(SourceWriter writer, NumberSpec typeSpec)
	{
		string methodName = $"BitCountToMask{typeSpec.SizeOfInBits.ToString(PrimitiveCatalog.InvariantCulture)}";
		string bitCountConstantName = $"k{typeSpec.TypeCode}BitCount";

		writer.WriteXmlDocSummary("Calculate the bit-mask needed for a number of bits");
		writer.WriteXmlDocParam("bitCount", "Number of bits needed for the mask");
		writer.WriteXmlDocReturns();
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static {typeSpec.Keyword} {methodName}(int bitCount)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfLessThan(bitCount, 0);");
			writer.WriteLine($"ArgumentOutOfRangeException.ThrowIfGreaterThan(bitCount, {bitCountConstantName});");
			writer.WriteLine();
			writer.WriteLine($"const {typeSpec.Keyword} kOne = 1;");
			writer.WriteLine($"return bitCount == {bitCountConstantName}");
			using (writer.EnterBlock())
			{
				writer.WriteLine($"? {typeSpec.Keyword}.MaxValue");
				writer.WriteLine(": (kOne << bitCount) - 1;");
			}
		}
	}
};
