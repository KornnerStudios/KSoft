using System;
using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Bitwise;

internal static partial class BitsCoreSourceBuilder
{
	public static string BuildBitSwap()
		=> BuildFile(WriteBitSwapBody);

	private static void WriteBitSwapBody(SourceWriter writer)
	{
		foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
		{
			WriteBitSwapMethod(writer, typeSpec);
			writer.WriteLine();
		}
	}

	private static void WriteBitSwapMethod(SourceWriter writer, NumberSpec typeSpec)
	{
		string scratchType = typeSpec.TypeCode == TypeCode.UInt64
			? "ulong"
			: "uint";
		string oneLiteral = typeSpec.TypeCode == TypeCode.UInt64
			? "1UL"
			: "1U";

		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"public static {typeSpec.Keyword} BitSwap({typeSpec.Keyword} value, " +
			$"int startBitIndex = {GeneralBitCountConstantName(typeSpec)}-1)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(startBitIndex > 0, kBitSwap_StartBitIndexNotGreaterThanZero);");
			writer.WriteLine($"Contract.Requires(startBitIndex < {GeneralBitCountConstantName(typeSpec)});");
			writer.WriteLine();
			writer.WriteLine("if (value != 0 && value != " + typeSpec.Keyword + ".MaxValue)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"{scratchType} bits = 0;");
				writer.WriteLine($"const {scratchType} k_one = {oneLiteral};");
				writer.WriteLine();
				writer.WriteLine("int bits_shift = 0;");
				writer.WriteLine("int value_shift = startBitIndex;");
				writer.WriteLine("for (var value_mask = k_one << startBitIndex;");
				using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
				{
					writer.WriteLine("value_shift >= 0;");
					writer.WriteLine("value_mask >>= 1, value_shift--, bits_shift++)");
				}
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("bits |= ((value & value_mask) >> value_shift) << bits_shift;");
				}
				writer.WriteLine();
				writer.WriteLine($"value = ({typeSpec.Keyword})bits;");
			}
			writer.WriteLine("return value;");
		}
	}
}
