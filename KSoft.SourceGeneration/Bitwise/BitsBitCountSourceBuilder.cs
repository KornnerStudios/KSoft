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
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft");
		writer.WriteLine();
		using (writer.EnterTypeDeclaration("partial class Bits"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesMajorWords)
			{
				WriteBitCountToMaskMethod(writer, typeSpec);
				writer.WriteLine();
			}
		}

		return writer.ToString();
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
