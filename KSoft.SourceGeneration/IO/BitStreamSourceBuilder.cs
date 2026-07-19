using System;
using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.IO;

internal static class BitStreamSourceBuilder
{
	public const string HintName = "KSoft.IO.BitStream.g.cs";

	public static string Build()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("#nullable disable");
		writer.WriteLine();
		writer.WriteLine("using System.Collections.Generic;");
		writer.WriteLine("#if CONTRACTS_FULL_SHIM");
		writer.WriteLine("using Contract = System.Diagnostics.ContractsShim.Contract;");
		writer.WriteLine("#else");
		writer.WriteLine("using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D");
		writer.WriteLine("#endif");
		writer.WriteLine();
		writer.WriteLine("using TWord = System.UInt32;");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft.IO");
		writer.WriteLine();
		using (writer.EnterTypeDeclaration("partial class BitStream"))
		{
			WriteCoreRegion(writer);
			writer.WriteLine();
			WriteReadRegion(writer);
			writer.WriteLine();
			WriteWriteRegion(writer);
			writer.WriteLine();
			WriteStreamValueRegion(writer);
			writer.WriteLine();
			WriteStreamFixedArrayRegion(writer);
			writer.WriteLine();
			WriteStreamArrayRegion(writer);
			writer.WriteLine();
			WriteStreamElementsRegion(writer);
		}

		return writer.ToString();
	}

	private static void WriteCoreRegion(SourceWriter writer)
	{
		foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesMajorWords)
		{
			using (writer.EnterRegion($@"Read\Write {typeSpec.Keyword} Impl"))
			{
				WriteReadWordMethod(writer, typeSpec);
				WriteWriteWordMethod(writer, typeSpec);
			}

			writer.WriteLine();
		}
	}

	private static void WriteReadRegion(SourceWriter writer)
	{
		foreach (PrimitiveSpec typeSpec in PrimitiveCatalog.BitStreamableIntegerTypes)
		{
			WriteReadScalarMethod(writer, typeSpec);
			writer.WriteLine();
		}

		foreach (PrimitiveSpec typeSpec in PrimitiveCatalog.BitStreamableIntegerTypes)
		{
			WriteReadOutMethod(writer, typeSpec);
			writer.WriteLine();
		}
	}

	private static void WriteWriteRegion(SourceWriter writer)
	{
		foreach (PrimitiveSpec typeSpec in PrimitiveCatalog.BitStreamableIntegerTypes)
		{
			WriteWriteMethod(writer, typeSpec);
			writer.WriteLine();
		}
	}

	private static void WriteStreamValueRegion(SourceWriter writer)
	{
		foreach (PrimitiveSpec typeSpec in PrimitiveCatalog.BitStreamableIntegerTypes)
		{
			WriteStreamValueMethod(writer, typeSpec);
			writer.WriteLine();
		}

		foreach (PrimitiveSpec typeSpec in PrimitiveCatalog.BitStreamableNonIntegerTypes)
		{
			WriteStreamNonIntegerValueMethod(writer, typeSpec);
			writer.WriteLine();
		}
	}

	private static void WriteStreamFixedArrayRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("StreamFixedArray"))
		{
			foreach (PrimitiveSpec typeSpec in PrimitiveCatalog.BitStreamableIntegerTypes)
			{
				WriteStreamFixedArrayMethod(writer, typeSpec);
				writer.WriteLine();
			}

			foreach (PrimitiveSpec typeSpec in PrimitiveCatalog.BitStreamableNonIntegerTypes)
			{
				WriteStreamNonIntegerFixedArrayMethod(writer, typeSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteStreamArrayRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("StreamArray"))
		{
			foreach (PrimitiveSpec typeSpec in PrimitiveCatalog.BitStreamableIntegerTypes)
			{
				WriteStreamArrayMethod(writer, typeSpec);
				writer.WriteLine();
			}

			foreach (PrimitiveSpec typeSpec in PrimitiveCatalog.BitStreamableNonIntegerTypes)
			{
				WriteStreamNonIntegerArrayMethod(writer, typeSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteStreamElementsRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("StreamList"))
		{
			foreach (PrimitiveSpec typeSpec in PrimitiveCatalog.BitStreamableIntegerTypes)
			{
				WriteStreamElementsMethod(writer, typeSpec);
				writer.WriteLine();
			}

			foreach (PrimitiveSpec typeSpec in PrimitiveCatalog.BitStreamableNonIntegerTypes)
			{
				WriteStreamNonIntegerElementsMethod(writer, typeSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteReadWordMethod(SourceWriter writer, NumberSpec typeSpec)
	{
		string keyword = typeSpec.Keyword;

		writer.WriteLine($"internal void ReadWord(out {keyword} word, int bitCount)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(bitCount <= kWordBitCount);");
			writer.WriteLine();
			writer.WriteLine("int bits_remaining = CacheBitsRemaining;");
			writer.WriteLine();
			writer.WriteLine("// if the requested bits are contained entirely in the cache...");
			writer.WriteLine("if (bitCount <= bits_remaining)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"word = ({keyword})ExtractWordFromCache(bitCount);");
				writer.WriteLine("mCacheBitIndex += bitCount;");
				writer.WriteLine();
				writer.WriteLine("// If we consumed the rest of the cache after that last extraction");
				writer.WriteLine("if (mCacheBitIndex == kWordBitCount && !IsEndOfStream)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("FillCache();");
				}
			}

			writer.WriteLine("else // else the cache only has a portion of the bits (or needs to be re-filled)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("int word_bits_remaining = bitCount;");
				writer.WriteLine();
				writer.WriteLine("// will always be negative, so abs it");
				writer.WriteLine("int msb_shift = -(bits_remaining - word_bits_remaining);");
				writer.WriteLine("// get the word bits (MSB) that are left in the cache");
				writer.WriteLine($"word = ({keyword})ExtractWordFromCache(bits_remaining);");
				writer.WriteLine("word_bits_remaining -= bits_remaining;");
				writer.WriteLine("// adjust the bits to the MSB");
				writer.WriteLine("word <<= msb_shift;");
				writer.WriteLine();
				writer.WriteLine("FillCache(); // fill the cache with the next round of bits");
				writer.WriteLine();
				writer.WriteLine("// get the 'rest' of the bits that weren't initially in our cache");
				writer.WriteLine("TWord more_bits = ExtractWordFromCache(word_bits_remaining);");
				writer.WriteLine();
				writer.WriteLine($"word |= ({keyword})more_bits;");
				writer.WriteLine("mCacheBitIndex = word_bits_remaining;");
			}
		}
	}

	private static void WriteWriteWordMethod(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteLine($"internal void WriteWord({typeSpec.Keyword} word, int bitCount)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(bitCount <= kWordBitCount);");
			writer.WriteLine();
			writer.WriteLine("int bits_remaining = CacheBitsRemaining;");
			writer.WriteLine();
			writer.WriteLine("// if the bits to write can be held entirely in the cache...");
			writer.WriteLine("if (bitCount <= bits_remaining)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("PutWordInCache((TWord)word, bitCount);");
				writer.WriteLine("mCacheBitIndex += bitCount;");
				writer.WriteLine();
				writer.WriteLine("if (mCacheBitIndex == kWordBitCount)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("FlushCache();");
				}
			}

			writer.WriteLine("else // else we have to split the cache writes between a flush");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("int word_bits_remaining = bitCount;");
				writer.WriteLine();
				writer.WriteLine("// will always be negative, so abs it");
				writer.WriteLine("int msb_shift = -(bits_remaining - word_bits_remaining);");
				writer.WriteLine("// write the upper (MSB) word bits to the remaining cache bits");
				writer.WriteLine("PutWordInCache((TWord)(word >> msb_shift), bits_remaining);");
				writer.WriteLine("word_bits_remaining -= bits_remaining;");
				writer.WriteLine();
				writer.WriteLine("// Flush determines the amount of bytes to write based on the current");
				writer.WriteLine("// bit index. This causes it to write all the bytes of the TWord");
				writer.WriteLine("mCacheBitIndex += bits_remaining;");
				writer.WriteLine("FlushCache(); // flush the MSB results and reset the cache");
				writer.WriteLine();
				writer.WriteLine("PutWordInCache((TWord)word, word_bits_remaining);");
				writer.WriteLine("mCacheBitIndex = word_bits_remaining;");
			}
		}
	}

	private static void WriteReadScalarMethod(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		WriteReadXmlDocs(writer, typeSpec, includeValueParam: false);
		writer.WriteLine($"public {typeSpec.Keyword} {ReadMethodName(typeSpec)}({BitCountParameter(typeSpec)})");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitCountContract(writer, typeSpec);
			writer.WriteLine();
			if (Uses64BitSplit(typeSpec))
			{
				WriteRead64Body(writer, typeSpec);
			}
			else
			{
				WriteRead32Body(writer, typeSpec);
			}
		}
	}

	private static void WriteReadOutMethod(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		WriteReadXmlDocs(writer, typeSpec, includeValueParam: true);
		writer.WriteLine($"public void Read(out {typeSpec.Keyword} value, {BitCountParameter(typeSpec)})");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitCountContract(writer, typeSpec);
			writer.WriteLine();
			writer.WriteLine($"value = {ReadIntegerCall(typeSpec, "bitCount")};");
		}
	}

	private static void WriteRead32Body(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteLine("ReadWord(out TWord word, bitCount);");
		WriteSignExtensionReturn(writer, typeSpec, "word");
	}

	private static void WriteRead64Body(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteLine("uint msb_word = 0;");
		writer.WriteLine("int msb_bit_count = bitCount > Bits.kInt32BitCount ? bitCount - Bits.kInt32BitCount : 0;");
		writer.WriteLine("int lsb_bit_count = bitCount > Bits.kInt32BitCount ? bitCount - msb_bit_count : bitCount;");
		writer.WriteLine();
		writer.WriteLine("if (msb_bit_count > 0)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ReadWord(out msb_word, msb_bit_count);");
		}

		writer.WriteLine();
		writer.WriteLine("ReadWord(out uint lsb_word, lsb_bit_count);");
		writer.WriteLine();
		writer.WriteLine("ulong word = (ulong)msb_word << lsb_bit_count;");
		writer.WriteLine("word |= (ulong)lsb_word;");
		WriteSignExtensionReturn(writer, typeSpec, "word");
	}

	private static void WriteSignExtensionReturn(SourceWriter writer, PrimitiveSpec typeSpec, string wordVariable)
	{
		if (IsSignedInteger(typeSpec))
		{
			writer.WriteLine($"if (signExtend && bitCount != {BitCountConstant(typeSpec)})");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"return ({typeSpec.Keyword})Bits.SignExtend( ({typeSpec.Keyword}){wordVariable}, bitCount );");
			}

			writer.WriteLine();
		}

		writer.WriteLine($"return ({typeSpec.Keyword}){wordVariable};");
	}

	private static void WriteWriteMethod(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteXmlDocSummary($"Write an <see cref=\"System.{typeSpec.TypeCode}\"/> to the stream");
		writer.WriteXmlDocParam("value", "value to write to the stream");
		writer.WriteXmlDocParam("bitCount", "Number of bits to write");
		writer.WriteLine($"public void Write({typeSpec.Keyword} value, int bitCount = {BitCountConstant(typeSpec)})");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitCountContract(writer, typeSpec);
			writer.WriteLine();
			if (Uses64BitSplit(typeSpec))
			{
				WriteWrite64Body(writer);
			}
			else
			{
				writer.WriteLine("TWord word = (TWord)value;");
				writer.WriteLine("WriteWord(word, bitCount);");
			}
		}
	}

	private static void WriteWrite64Body(SourceWriter writer)
	{
		writer.WriteLine("uint msb_word = (uint)(value >> Bits.kInt32BitCount);");
		writer.WriteLine("uint lsb_word = (uint)value;");
		writer.WriteLine("int msb_bit_count = bitCount > Bits.kInt32BitCount ? bitCount - Bits.kInt32BitCount : 0;");
		writer.WriteLine("int lsb_bit_count = bitCount > Bits.kInt32BitCount ? bitCount - msb_bit_count : bitCount;");
		writer.WriteLine();
		writer.WriteLine("if (msb_bit_count > 0)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("WriteWord(msb_word, msb_bit_count);");
		}

		writer.WriteLine();
		writer.WriteLine("WriteWord(lsb_word, lsb_bit_count);");
	}

	private static void WriteStreamValueMethod(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteXmlDocSummary($"Serialize an <see cref=\"System.{typeSpec.TypeCode}\"/> to/from the stream");
		writer.WriteXmlDocParam("value", "value to serialize");
		writer.WriteXmlDocParam("bitCount", "Number of bits to use");
		if (IsSignedInteger(typeSpec))
		{
			writer.WriteXmlDocParam("signExtend", "If true, the result will have the MSB extended");
		}

		writer.WriteXmlDocReturns("Returns this instance");
		writer.WriteLine($"public BitStream Stream(ref {typeSpec.Keyword} value, {BitCountParameter(typeSpec)})");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitCountContract(writer, typeSpec);
			writer.WriteLine();
			writer.WriteLine($"if (IsReading) {{ value = {ReadIntegerCall(typeSpec, "bitCount")}; }}");
			writer.WriteLine("else if (IsWriting) { Write(value, bitCount); }");
			writer.WriteLine();
			writer.WriteLine("return this;");
		}
	}

	private static void WriteStreamNonIntegerValueMethod(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteXmlDocSummary($"Serialize an <see cref=\"System.{typeSpec.TypeCode}\"/> to/from the stream");
		writer.WriteXmlDocParam("value", "value to serialize");
		writer.WriteXmlDocReturns("Returns this instance");
		writer.WriteLine($"public BitStream Stream(ref {typeSpec.Keyword} value)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine($"if (IsReading) {{ value = {ReadMethodName(typeSpec)}(); }}");
			writer.WriteLine("else if (IsWriting) { Write(value); }");
			writer.WriteLine();
			writer.WriteLine("return this;");
		}
	}

	private static void WriteStreamFixedArrayMethod(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteLine($"public BitStream StreamFixedArray({typeSpec.Keyword}[] array,");
		writer.WriteLine($"\tint elementBitSize = {BitCountConstant(typeSpec)}{SignedParameterSuffix(typeSpec)})");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(array != null);");
			writer.WriteLine($"Contract.Requires(elementBitSize <= {BitCountConstant(typeSpec)});");
			writer.WriteLine();
			writer.WriteLine($"for (int x = 0; x < array.Length; x++) {{ {StreamArrayElementCall(typeSpec)} }}");
			writer.WriteLine();
			writer.WriteLine("return this;");
		}
	}

	private static void WriteStreamNonIntegerFixedArrayMethod(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteLine($"public BitStream StreamFixedArray({typeSpec.Keyword}[] array)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(array != null);");
			writer.WriteLine();
			writer.WriteLine("for (int x = 0; x < array.Length; x++) { Stream(ref array[x]); }");
			writer.WriteLine();
			writer.WriteLine("return this;");
		}
	}

	private static void WriteStreamArrayMethod(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteLine($"public BitStream StreamArray(ref {typeSpec.Keyword}[] array,");
		writer.WriteLine($"\tint lengthBitSize, int elementBitSize = {BitCountConstant(typeSpec)}");
		WriteClosingParameterLine(writer, typeSpec);
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(IsReading || array != null);");
			writer.WriteLine("Contract.Requires(lengthBitSize <= Bits.kInt32BitCount);");
			writer.WriteLine($"Contract.Requires(elementBitSize <= {BitCountConstant(typeSpec)});");
			writer.WriteLine();
			WriteArrayCountReadWriteBody(writer, typeSpec);
			writer.WriteLine($"for (int x = 0; x < count; x++) {{ {StreamArrayElementCall(typeSpec)} }}");
			writer.WriteLine();
			writer.WriteLine("return this;");
		}
	}

	private static void WriteStreamNonIntegerArrayMethod(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteLine($"public BitStream StreamArray(ref {typeSpec.Keyword}[] array,");
		writer.WriteLine("\tint lengthBitSize)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(IsReading || array != null);");
			writer.WriteLine("Contract.Requires(lengthBitSize <= Bits.kInt32BitCount);");
			writer.WriteLine();
			WriteArrayCountReadWriteBody(writer, typeSpec);
			writer.WriteLine("for (int x = 0; x < count; x++) { Stream(ref array[x]); }");
			writer.WriteLine();
			writer.WriteLine("return this;");
		}
	}

	private static void WriteStreamElementsMethod(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteLine($"public BitStream StreamElements(ICollection<{typeSpec.Keyword}> list,");
		writer.WriteLine($"\tint countBitSize, int elementBitSize = {BitCountConstant(typeSpec)}");
		WriteClosingParameterLine(writer, typeSpec);
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(list != null);");
			writer.WriteLine("Contract.Requires(countBitSize <= Bits.kInt32BitCount);");
			writer.WriteLine($"Contract.Requires(elementBitSize <= {BitCountConstant(typeSpec)});");
			writer.WriteLine();
			WriteElementsReadWriteBody(writer, typeSpec);
			writer.WriteLine();
			writer.WriteLine("return this;");
		}
	}

	private static void WriteStreamNonIntegerElementsMethod(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteLine($"public BitStream StreamElements(ICollection<{typeSpec.Keyword}> list,");
		writer.WriteLine("\tint countBitSize)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(list != null);");
			writer.WriteLine("Contract.Requires(countBitSize <= Bits.kInt32BitCount);");
			writer.WriteLine();
			WriteNonIntegerElementsReadWriteBody(writer, typeSpec);
			writer.WriteLine();
			writer.WriteLine("return this;");
		}
	}

	private static void WriteArrayCountReadWriteBody(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteLine("int count = IsReading ? 0 : array.Length;");
		writer.WriteLine("Stream(ref count, lengthBitSize);");
		writer.WriteLine();
		writer.WriteLine("if (IsReading)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine($"array = new {typeSpec.Keyword}[count];");
		}

		writer.WriteLine();
	}

	private static void WriteElementsReadWriteBody(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteLine("int count = list.Count;");
		writer.WriteLine("Stream(ref count, countBitSize);");
		writer.WriteLine();
		writer.WriteLine("if (IsReading)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("for (int x = 0; x < count; x++)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"var value = {ReadIntegerCall(typeSpec, "elementBitSize")};");
				writer.WriteLine("list.Add(value);");
			}
		}

		writer.WriteLine("else if (IsWriting)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("foreach (var value in list)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("Write(value, elementBitSize);");
			}
		}
	}

	private static void WriteNonIntegerElementsReadWriteBody(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteLine("int count = list.Count;");
		writer.WriteLine("Stream(ref count, countBitSize);");
		writer.WriteLine();
		writer.WriteLine("if (IsReading)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("for (int x = 0; x < count; x++)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"var value = {ReadMethodName(typeSpec)}();");
				writer.WriteLine("list.Add(value);");
			}
		}

		writer.WriteLine("else if (IsWriting)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("foreach (var value in list)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("Write(value);");
			}
		}
	}

	private static void WriteClosingParameterLine(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		if (IsSignedInteger(typeSpec))
		{
			writer.WriteLine("\t, bool signExtend = false)");
		}
		else
		{
			writer.WriteLine("\t)");
		}
	}

	private static void WriteReadXmlDocs(SourceWriter writer, PrimitiveSpec typeSpec, bool includeValueParam)
	{
		writer.WriteXmlDocSummary($"Read an <see cref=\"System.{typeSpec.TypeCode}\"/> from the stream");
		if (includeValueParam)
		{
			writer.WriteXmlDocParam("value", "value read from the stream");
		}

		writer.WriteXmlDocParam("bitCount", "Number of bits to read");
		if (IsSignedInteger(typeSpec))
		{
			writer.WriteXmlDocParam("signExtend", "If true, the result will have the MSB extended");
		}

		if (!includeValueParam)
		{
			writer.WriteXmlDocReturns();
		}
	}

	private static void WriteBitCountContract(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteLine($"Contract.Requires(bitCount <= {BitCountConstant(typeSpec)});");
	}

	private static string BitCountConstant(PrimitiveSpec typeSpec)
		=> $"Bits.k{typeSpec.TypeCode}BitCount";

	private static string BitCountParameter(PrimitiveSpec typeSpec)
		=> $"int bitCount = {BitCountConstant(typeSpec)}{SignedParameterSuffix(typeSpec)}";

	private static string SignedParameterSuffix(PrimitiveSpec typeSpec)
		=> IsSignedInteger(typeSpec)
			? ", bool signExtend = false"
			: "";

	private static string ReadMethodName(PrimitiveSpec typeSpec)
		=> "Read" + typeSpec.TypeCode;

	private static string ReadIntegerCall(PrimitiveSpec typeSpec, string bitCountExpression)
	{
		if (IsSignedInteger(typeSpec))
		{
			return $"{ReadMethodName(typeSpec)}({bitCountExpression}, signExtend)";
		}

		return $"{ReadMethodName(typeSpec)}({bitCountExpression})";
	}

	private static string StreamArrayElementCall(PrimitiveSpec typeSpec)
	{
		if (IsSignedInteger(typeSpec))
		{
			return "Stream(ref array[x], elementBitSize, signExtend);";
		}

		return "Stream(ref array[x], elementBitSize);";
	}

	private static bool Uses64BitSplit(PrimitiveSpec typeSpec)
		=> typeSpec.TypeCode is TypeCode.UInt64 or TypeCode.Int64;

	private static bool IsSignedInteger(PrimitiveSpec typeSpec)
		=> typeSpec.TypeCode is TypeCode.SByte or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64;
};
