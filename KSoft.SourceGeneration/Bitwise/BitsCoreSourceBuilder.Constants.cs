using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Bitwise;

internal static partial class BitsCoreSourceBuilder
{
	public static string BuildConstants()
		=> BuildFile(WriteConstantsBody, includeContractsAlias: false);

	private static void WriteConstantsBody(SourceWriter writer)
	{
		WriteBitCountConstantsRegion(writer);
		writer.WriteLine();
		WriteBitShiftConstantsRegion(writer);
		writer.WriteLine();
		WriteBitModConstantsRegion(writer);
		writer.WriteLine();
		WriteBitmaskLookupRegion(writer);
		writer.WriteLine();
		WriteGetBitConstantsMethod(writer);
	}

	private static void WriteBitCountConstantsRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("kBitCount"))
		{
			foreach (PrimitiveSpec typeSpec in PrimitiveCatalog.Primitives)
			{
				if (typeSpec.TypeCode == System.TypeCode.Boolean)
				{
					continue;
				}

				writer.WriteXmlDocSummary($"Number of bits in a <see cref=\"System.{TypeCodeName(typeSpec)}\"/>");
				writer.WriteLine($"public const int {BitCountConstantName(typeSpec)} = {SizeOfExpression(typeSpec)} * 8;");
				writer.WriteLine();
			}
		}
	}

	private static void WriteBitShiftConstantsRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("kBitShift"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.Numbers)
			{
				writer.WriteXmlDocSummary(
					"Bit shift value for getting the bit count of a an " +
					$"<see cref=\"System.{TypeCodeName(typeSpec)}\"/> element");
				writer.WriteLine($"public const int {BitShiftConstantName(typeSpec)} =\t{BitShiftValue(typeSpec)};");
				writer.WriteLine();
			}
		}
	}

	private static void WriteBitModConstantsRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("kBitMod"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.Numbers)
			{
				if (!typeSpec.IsInteger)
				{
					continue;
				}

				writer.WriteXmlDocSummary(
					"Bitwise AND value for emulating modulus operations on " +
					$"<see cref=\"System.{TypeCodeName(typeSpec)}\"/> elements");
				writer.WriteLine($"public const int {BitModConstantName(typeSpec)} = {typeSpec.SizeOfInBits - 1};");
				writer.WriteLine();
			}
		}
	}

	private static void WriteBitmaskLookupRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("kBitmaskLookup"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				writer.WriteXmlDocSummary(
					$"Bit count to bit-mask look up table for {typeSpec.SizeOfInBits}-bit words");
				writer.WriteLine(
					$"public static readonly {typeSpec.Keyword}[] {BitmaskLookupName(typeSpec)} = " +
					$"{BitmaskLookupGenerateMethodName(typeSpec)}({BitCountConstantName(typeSpec)});");
				writer.WriteLine();
			}
		}
	}

	private static void WriteGetBitConstantsMethod(SourceWriter writer)
	{
		writer.WriteLine("public static bool GetBitConstants(Type integerType,");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine("out int byteCount, out int bitCount, out int bitShift, out int bitMod)");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires/*<ArgumentNullException>*/(integerType != null);");
			writer.WriteLine();
			writer.WriteLine("byteCount = bitCount = bitShift = bitMod = TypeExtensions.kNoneInt32;");
			writer.WriteLine();
			writer.WriteLine("switch (Type.GetTypeCode(integerType))");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
				{
					writer.WriteLine($"case TypeCode.{TypeCodeName(typeSpec)}:");
					writer.WriteLine($"case TypeCode.{SignedTypeCodeName(typeSpec)}:");
					using (writer.EnterBlock())
					{
						writer.WriteLine($"byteCount = {SizeOfExpression(typeSpec)};");
						writer.WriteLine($"bitCount = {BitCountConstantName(typeSpec)};");
						writer.WriteLine($"bitShift = {BitShiftConstantName(typeSpec)};");
						writer.WriteLine($"bitMod = {BitModConstantName(typeSpec)};");
						writer.WriteLine("break;");
					}
					writer.WriteLine();
				}

				writer.WriteLine("default: return false;");
			}
			writer.WriteLine();
			writer.WriteLine("return true;");
		}
	}
}
