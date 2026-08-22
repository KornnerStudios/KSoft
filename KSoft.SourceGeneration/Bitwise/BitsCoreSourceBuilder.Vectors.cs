using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Bitwise;

internal static partial class BitsCoreSourceBuilder
{
	public static string BuildVectors()
		=> BuildFile(WriteVectorsBody);

	private static void WriteVectorsBody(SourceWriter writer)
	{
		WriteVectorLengthRegion(writer);
		writer.WriteLine();
		WriteVectorElementBitMaskRegion(writer);
		writer.WriteLine();
		WriteVectorElementSectionBitMaskRegion(writer);
		writer.WriteLine();
		WriteVectorElementFromBufferRegion(writer);
		writer.WriteLine();
		WriteVectorIndexRegion(writer);
		writer.WriteLine();
		WriteVectorBitIndexRegion(writer);
		writer.WriteLine();
		WriteVectorBitCursorRegion(writer);
	}

	private static void WriteVectorLengthRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Bit Vector length calculations"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				writer.WriteXmlDocSummary(
					$"Calculates how many <see cref=\"{VectorSystemTypeName(typeSpec)}\"/>s are needed " +
					"to hold a bit vector of a certain length");
				writer.WriteXmlDocParam("bitsCount", "Number of bits to be hosted in the vector");
				writer.WriteXmlDocReturns("Number of vector elements required, never negative");
				writer.WritePurityAnnotation();
				writer.WriteLine($"public static int VectorLengthIn{VectorWordName(typeSpec)}(int bitsCount)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(bitsCount);");
					writer.WriteLine();
					writer.WriteLine(
						$"return (bitsCount + ({GeneralBitCountConstantName(typeSpec)}-1)) " +
						$">> {GeneralBitShiftConstantName(typeSpec)};");
				}
				writer.WriteLine();
			}
		}
	}

	private static void WriteVectorElementBitMaskRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Bit Vector element bitmask (kVectorWordFormat dependent)"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				WriteVectorElementBitMaskMethods(writer, typeSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteVectorElementBitMaskMethods(SourceWriter writer, NumberSpec typeSpec)
	{
		string wordName = VectorWordName(typeSpec);
		string systemTypeName = VectorSystemTypeName(typeSpec);

		WriteVectorElementMaskXmlDocs(writer, "specific bit", "bitIndex", systemTypeName, includeByteOrder: false);
		writer.WritePurityAnnotation();
		writer.WriteLine($"/*public*/ static {typeSpec.Keyword} VectorElementBitMaskIn{wordName}LE(int bitIndex)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(bitIndex);");
			writer.WriteLine($"const {typeSpec.Keyword} k_one = 1;");
			writer.WriteLine();
			writer.WriteLine(
				$"return ({typeSpec.Keyword})(k_one << (bitIndex % {GeneralBitCountConstantName(typeSpec)}));");
		}

		WriteVectorElementMaskXmlDocs(writer, "specific bit", "bitIndex", systemTypeName, includeByteOrder: false);
		writer.WritePurityAnnotation();
		writer.WriteLine($"/*public*/ static {typeSpec.Keyword} VectorElementBitMaskIn{wordName}BE(int bitIndex)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(bitIndex);");
			writer.WriteLine($"const {typeSpec.Keyword} k_one = 1;");
			writer.WriteLine(
				$"const {typeSpec.Keyword} k_most_significant_bit = k_one << " +
				$"({GeneralBitCountConstantName(typeSpec)} - 1);");
			writer.WriteLine();
			writer.WriteLine(
				$"return ({typeSpec.Keyword})(k_most_significant_bit >> " +
				$"(bitIndex % {GeneralBitCountConstantName(typeSpec)}));");
		}

		WriteVectorElementMaskXmlDocs(writer, "specific bit", "bitIndex", systemTypeName, includeByteOrder: true);
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static {typeSpec.Keyword} VectorElementBitMaskIn{wordName}(int bitIndex,");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine("Shell.EndianFormat byteOrder = kVectorWordFormat)");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(bitIndex);");
			writer.WriteLine();
			writer.WriteLine("return byteOrder == Shell.EndianFormat.Big");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine($"? VectorElementBitMaskIn{wordName}BE(bitIndex)");
				writer.WriteLine($": VectorElementBitMaskIn{wordName}LE(bitIndex);");
			}
		}

		WriteGetVectorElementMaskProc(writer, typeSpec, "BitMask", "specific bit", "bitIndex");
	}

	private static void WriteVectorElementSectionBitMaskRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Bit Vector element section bitmask (kVectorWordFormat dependent)"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				WriteVectorElementSectionBitMaskMethods(writer, typeSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteVectorElementSectionBitMaskMethods(SourceWriter writer, NumberSpec typeSpec)
	{
		string wordName = VectorWordName(typeSpec);
		string systemTypeName = VectorSystemTypeName(typeSpec);

		WriteVectorElementMaskXmlDocs(writer, "section of bits", "startBitIndex", systemTypeName, includeByteOrder: false);
		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"/*public*/ static {typeSpec.Keyword} VectorElementSectionBitMaskIn{wordName}LE(int startBitIndex)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(startBitIndex);");
			writer.WriteLine();
			writer.WriteLine($"return ({typeSpec.Keyword})({typeSpec.Keyword}.MaxValue << startBitIndex);");
		}

		WriteVectorElementMaskXmlDocs(writer, "section of bits", "startBitIndex", systemTypeName, includeByteOrder: false);
		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"/*public*/ static {typeSpec.Keyword} VectorElementSectionBitMaskIn{wordName}BE(int startBitIndex)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(startBitIndex);");
			writer.WriteLine();
			writer.WriteLine($"return ({typeSpec.Keyword})({typeSpec.Keyword}.MaxValue >> startBitIndex);");
		}

		WriteVectorElementMaskXmlDocs(writer, "section of bits", "startBitIndex", systemTypeName, includeByteOrder: true);
		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"public static {typeSpec.Keyword} VectorElementSectionBitMaskIn{wordName}(int startBitIndex,");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine("Shell.EndianFormat byteOrder = kVectorWordFormat)");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(startBitIndex);");
			writer.WriteLine();
			writer.WriteLine("return byteOrder == Shell.EndianFormat.Big");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine($"? VectorElementSectionBitMaskIn{wordName}BE(startBitIndex)");
				writer.WriteLine($": VectorElementSectionBitMaskIn{wordName}LE(startBitIndex);");
			}
		}

		WriteGetVectorElementMaskProc(writer, typeSpec, "SectionBitMask", "section of bits", "startBitIndex");
	}

	private static void WriteGetVectorElementMaskProc(
		SourceWriter writer,
		NumberSpec typeSpec,
		string methodPart,
		string subject,
		string parameterName)
	{
		string wordName = VectorWordName(typeSpec);
		string systemTypeName = VectorSystemTypeName(typeSpec);
		string methodName = $"VectorElement{methodPart}In{wordName}";

		writer.WriteXmlDocSummary(
			$"Get the procedure for building a mask of a {subject} in a vector, " +
			$"relative to the vector's element size (<see cref=\"{systemTypeName}\"/>)");
		writer.WriteXmlDocParam("proc", "Receives a non-null mask builder procedure");
		writer.WriteXmlDocParam("byteOrder", "Order in which bits are enumerated (first to last)");
		writer.WriteLine(
			$"public static void GetVectorElement{methodPart}InT(out VectorElementBitMask<{typeSpec.Keyword}> proc,");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine("Shell.EndianFormat byteOrder = kVectorWordFormat)");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine();
			writer.WriteLine("proc = byteOrder == Shell.EndianFormat.Big");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine($"? (VectorElementBitMask<{typeSpec.Keyword}>){methodName}BE");
				writer.WriteLine($": (VectorElementBitMask<{typeSpec.Keyword}>){methodName}LE;");
			}
		}
	}

	private static void WriteVectorElementFromBufferRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Bit Vector element from byte[]"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				WriteVectorElementFromBufferMethods(writer, typeSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteVectorElementFromBufferMethods(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteXmlDocSummary("Read one vector element from a byte buffer");
		writer.WriteXmlDocParam("buffer", "Byte buffer to read from");
		writer.WriteXmlDocParam("index", "Offset in <paramref name=\"buffer\"/> to start reading at");
		writer.WriteXmlDocParam("element", "Element to receive the buffer value");
		writer.WriteLine(
			$"public static void VectorElementFromBufferInT(byte[] buffer, int index, ref {typeSpec.Keyword} element)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentNullException.ThrowIfNull(buffer);");
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(index);");
			writer.WriteLine($"if (index + sizeof({typeSpec.Keyword}) > buffer.Length)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine(
					"throw new ArgumentOutOfRangeException(nameof(index), index, " +
					"\"Element exceeds the buffer length.\");");
			}
			writer.WriteLine();
			writer.WriteLine(typeSpec.TypeCode == System.TypeCode.Byte
				? "element = buffer[index];"
				: $"element = BitConverter.To{TypeCodeName(typeSpec)}(buffer, index);");
		}

		writer.WriteXmlDocSummary("Get the procedure for reading one vector element from a byte buffer");
		writer.WriteXmlDocParam("proc", "Receives a non-null buffer reader procedure");
		writer.WriteLine(
			$"public static void GetVectorElementFromBufferInT(out VectorElementFromBuffer<{typeSpec.Keyword}> proc)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine();
			writer.WriteLine("proc = VectorElementFromBufferInT;");
		}
	}

	private static void WriteVectorIndexRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Bit Vector bitIndex to vector_index"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				string wordName = VectorWordName(typeSpec);
				string systemTypeName = VectorSystemTypeName(typeSpec);

				writer.WriteXmlDocSummary(
					$"Get the vector index of a bit index, for a vector represented in <see cref=\"{systemTypeName}\"/>s");
				writer.WriteXmlDocParam("bitIndex", "Index of the bit which we want the vector index of");
				writer.WriteXmlDocReturns($"The index of a <see cref=\"{systemTypeName}\"/> which holds the bit in question");
				writer.WritePurityAnnotation();
				writer.WriteLine($"public static int VectorIndexIn{wordName}(int bitIndex)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(bitIndex);");
					writer.WriteLine();
					writer.WriteLine($"return bitIndex >> {GeneralBitShiftConstantName(typeSpec)};");
				}
				writer.WriteLine();
			}
		}
	}

	private static void WriteVectorBitIndexRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Bit Vector cursor to bitIndex"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				string wordName = VectorWordName(typeSpec);
				string systemTypeName = VectorSystemTypeName(typeSpec);

				writer.WriteXmlDocSummary(
					$"Calculates the bit position of a vector cursor based on <see cref=\"{systemTypeName}\"/> elements");
				writer.WriteXmlDocParam("index", "Element index of the cursor");
				writer.WriteXmlDocParam("bitOffset", "Element bit offset of the current");
				writer.WriteXmlDocReturns();
				writer.WritePurityAnnotation();
				writer.WriteLine($"public static int VectorBitIndexIn{wordName}(int index, int bitOffset)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(index);");
					writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(bitOffset);");
					writer.WriteLine();
					writer.WriteLine($"return (index << {GeneralBitShiftConstantName(typeSpec)}) + bitOffset;");
				}
				writer.WriteLine();
			}
		}
	}

	private static void WriteVectorBitCursorRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Bit Vector cursor from bitIndex"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				string wordName = VectorWordName(typeSpec);
				string systemTypeName = VectorSystemTypeName(typeSpec);

				writer.WriteXmlDocSummary(
					$"Calculates the vector cursor based on a bit index in a <see cref=\"{systemTypeName}\"/> vector");
				writer.WriteXmlDocParam("bitIndex", "Index to translate into a cursor");
				writer.WriteXmlDocParam("index", "Element index of the cursor");
				writer.WriteXmlDocParam("bitOffset", "Element bit offset of the current");
				writer.WriteLine(
					$"public static void VectorBitCursorIn{wordName}(int bitIndex, out int index, out int bitOffset)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(bitIndex);");
					writer.WriteLine();
					writer.WriteLine($"index = VectorIndexIn{wordName}(bitIndex);");
					writer.WriteLine($"bitOffset = bitIndex & {GeneralBitModConstantName(typeSpec)};");
				}
				writer.WriteLine();
			}
		}
	}

	private static void WriteVectorElementMaskXmlDocs(
		SourceWriter writer,
		string subject,
		string parameterName,
		string systemTypeName,
		bool includeByteOrder)
	{
		writer.WriteXmlDocSummary(
			$"Get the mask for a {subject} in a vector, relative to the vector's element size " +
			$"(<see cref=\"{systemTypeName}\"/>)");
		writer.WriteXmlDocParam(parameterName, parameterName == "bitIndex"
			? "Bit index to get the mask for"
			: "Bit index to begin the mask at");
		if (includeByteOrder)
		{
			writer.WriteXmlDocParam("byteOrder", "Order in which bits are enumerated (first to last)");
		}
		writer.WriteXmlDocReturns();
	}
}
