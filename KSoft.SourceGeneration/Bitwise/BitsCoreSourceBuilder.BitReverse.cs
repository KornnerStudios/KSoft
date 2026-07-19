using System;
using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Bitwise;

internal static partial class BitsCoreSourceBuilder
{
	public static string BuildBitReverse()
		=> BuildFile(WriteBitReverseBody);

	private static void WriteBitReverseBody(SourceWriter writer)
	{
		foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
		{
			WriteBitReverseMethod(writer, typeSpec);
			writer.WriteLine();
		}
	}

	private static void WriteBitReverseMethod(SourceWriter writer, NumberSpec typeSpec)
	{
		string scratchType = typeSpec.TypeCode == TypeCode.UInt64
			? "ulong"
			: "uint";

		writer.WriteXmlDocSummary("Get the bit-reversed equivalent of an unsigned integer");
		writer.WriteXmlDocParam("bits", "Integer to bit-reverse");
		writer.WriteXmlDocReturns();
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static {typeSpec.Keyword} BitReverse({typeSpec.Keyword} bits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine($"{scratchType} x = bits;");
			foreach ((string highMask, string lowMask, int shift, string comment) in BitReverseSteps(typeSpec))
			{
				writer.WriteLine(
					$"x = ((x & {highMask}) >> {shift,2}) | " +
					$"((x & {lowMask}) << {shift,2}); // {comment}");
			}

			writer.WriteLine();
			writer.WriteLine(typeSpec.TypeCode == TypeCode.UInt32 || typeSpec.TypeCode == TypeCode.UInt64
				? "return x;"
				: $"return ({typeSpec.Keyword})x;");
		}
	}

	private static (string HighMask, string LowMask, int Shift, string Comment)[] BitReverseSteps(NumberSpec typeSpec)
	{
		(string HighMask, string LowMask, int Shift, string Comment)[] all =
			[
				("0xAAAAAAAAAAAAAAAA", "0x5555555555555555",  1, "swap odd and even bits"),
				("0xCCCCCCCCCCCCCCCC", "0x3333333333333333",  2, "swap consecutive pairs"),
				("0xF0F0F0F0F0F0F0F0", "0x0F0F0F0F0F0F0F0F",  4, "swap nibbles"),
				("0xFF00FF00FF00FF00", "0x00FF00FF00FF00FF",  8, "swap bytes"),
				("0xFFFF0000FFFF0000", "0x0000FFFF0000FFFF", 16, "swap halves"),
				("0xFFFFFFFF00000000", "0x00000000FFFFFFFF", 32, "swap words"),
			];

		int stepCount = BitShiftValue(typeSpec);
		var result = new (string, string, int, string)[stepCount];
		for (int index = 0; index < stepCount; index++)
		{
			var step = all[index];
			result[index] = (
				HexLiteral(typeSpec, step.HighMask),
				HexLiteral(typeSpec, step.LowMask),
				step.Shift,
				step.Comment);
		}

		return result;
	}
}
