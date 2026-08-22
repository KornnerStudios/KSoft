using System;
using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Bitwise;

internal static class ByteSwapSourceBuilder
{
	public const string HintName = "KSoft.Bitwise.ByteSwap.g.cs";

	private const string kBinaryPrimitivesShimComment =
		"// #VITA_SHIM: Keep KSoft API while callers migrate to BinaryPrimitives.ReverseEndianness.";

	// Mirrors KSoft.T4.Bitwise.BitwiseT4.ByteSwapableIntegers while deriving primitive metadata from NumberSpec,
	// the Roslyn-side replacement for KSoft.T4.PrimitiveDefinitions.
	private static readonly ByteSwapWordSpec[] kWordSpecs =
		[
			new(PrimitiveCatalog.NumberFor(TypeCode.UInt16)),
			new(PrimitiveCatalog.NumberFor(TypeCode.UInt32)),
			new(PrimitiveCatalog.NumberFor(TypeCode.UInt64)),
			new(PrimitiveCatalog.NumberFor(TypeCode.UInt32), 24),
			new(PrimitiveCatalog.NumberFor(TypeCode.UInt64), 40),
		];

	public static string Build()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("using System;");
		writer.WriteLine("using System.Buffers.Binary;");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft.Bitwise");
		writer.WriteLine();
		using (writer.EnterTypeDeclaration("partial class ByteSwap"))
		{
			foreach (ByteSwapWordSpec spec in kWordSpecs)
			{
				if (spec.IsUnnaturalWord)
				{
					writer.WriteLine(
						$"public const int kSizeOfU{spec.ConstantKeyword} = sizeof(byte) * {spec.SizeOfInBytes};");
					writer.WriteLine(
						$"public const int kSizeOf{spec.ConstantKeyword} = kSizeOfU{spec.ConstantKeyword};");
					writer.WriteLine();
				}

				WriteUnsignedRegion(writer, spec);
				writer.WriteLine();
				if (spec.IsUnnaturalWord)
				{
					writer.WriteLine("// #TODO: verify we don't need any sign-extension magic");
				}
				WriteSignedRegion(writer, spec);
				writer.WriteLine();
			}
		}

		return writer.ToString();
	}

	private static void WriteUnsignedRegion(SourceWriter writer, ByteSwapWordSpec spec)
	{
		using (writer.EnterRegion("U" + spec.ConstantKeyword))
		{
			WriteSwapReturnMethod(writer, spec, isSigned: false);
			WriteSwapRefMethod(writer, spec, isSigned: false);
			WriteSwapBufferMethod(writer, spec, isSigned: false);
			WriteReplaceBytesMethod(writer, spec, isSigned: false);
		}
	}

	private static void WriteSignedRegion(SourceWriter writer, ByteSwapWordSpec spec)
	{
		using (writer.EnterRegion(spec.ConstantKeyword))
		{
			WriteSwapReturnMethod(writer, spec, isSigned: true);
			WriteSwapRefMethod(writer, spec, isSigned: true);
			WriteSwapBufferMethod(writer, spec, isSigned: true);
			WriteReplaceBytesMethod(writer, spec, isSigned: true);
		}
	}

	private static void WriteSwapReturnMethod(SourceWriter writer, ByteSwapWordSpec spec, bool isSigned)
	{
		string typeName = isSigned ? spec.SignedKeyword : spec.UnsignedKeyword;
		string methodName = "Swap" + (isSigned ? spec.ConstantKeyword : "U" + spec.ConstantKeyword);
		string crefName = isSigned ? spec.SignedCode : spec.UnsignedCode;

		writer.WriteXmlDocSummary($"Swaps a <see cref=\"{crefName}\" /> and returns the result");
		writer.WriteXmlDocParam("value", "");
		writer.WriteXmlDocReturns();
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static {typeName} {methodName}(");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"{typeName} value)");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			if (spec.IsUnnaturalWord)
			{
				writer.WriteLine("// #VITA_KEEP: 24/40-bit game-format widths have no BinaryPrimitives equivalent.");
				writer.WriteLine("return");
				WriteUnnaturalSwapExpression(writer, spec);
			}
			else
			{
				writer.WriteLine(kBinaryPrimitivesShimComment);
				writer.WriteLine("return BinaryPrimitives.ReverseEndianness(value);");
			}
		}
	}

	private static void WriteSwapRefMethod(SourceWriter writer, ByteSwapWordSpec spec, bool isSigned)
	{
		string typeName = isSigned ? spec.SignedKeyword : spec.UnsignedKeyword;
		string methodName = spec.IsUnnaturalWord
			? "Swap" + (isSigned ? spec.ConstantKeyword : "U" + spec.ConstantKeyword)
			: "Swap";
		string crefName = isSigned ? spec.SignedCode : spec.UnsignedCode;

		writer.WriteXmlDocSummary($"Swaps a <see cref=\"{crefName}\" /> by reference");
		writer.WriteXmlDocParam("value", "");
		writer.WriteLine($"public static void {methodName}(");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"ref {typeName} value)");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			if (spec.IsUnnaturalWord)
			{
				writer.WriteLine("// #VITA_KEEP: 24/40-bit game-format widths have no BinaryPrimitives equivalent.");
				writer.WriteLine("value =");
				WriteUnnaturalSwapExpression(writer, spec);
			}
			else
			{
				writer.WriteLine(kBinaryPrimitivesShimComment);
				writer.WriteLine("value = BinaryPrimitives.ReverseEndianness(value);");
			}
		}
	}

	private static void WriteSwapBufferMethod(SourceWriter writer, ByteSwapWordSpec spec, bool isSigned)
	{
		string methodName = "Swap" + (isSigned ? spec.ConstantKeyword : "U" + spec.ConstantKeyword);
		string crefName = isSigned ? spec.SignedCode : spec.UnsignedCode;

		writer.WriteXmlDocSummary($"Swaps a <see cref=\"{crefName}\" /> at a position in a bye array");
		writer.WriteXmlDocParam("buffer", "source array");
		writer.WriteXmlDocParam("offset", "offset at which to perform the byte swap");
		writer.WriteXmlDocReturns($"offset + {spec.SizeOfInBytes}");
		writer.WriteLine($"public static int {methodName}(byte[] buffer, int offset)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			if (isSigned)
			{
				writer.WriteLine($"return SwapU{spec.ConstantKeyword}(buffer, offset);");
				return;
			}

			WriteBufferContracts(writer, spec);
			writer.WriteLine();
			WriteByteDeclarations(writer, spec);
			WriteBytesFromBuffer(writer, spec);
			writer.WriteLine();
			WriteBytesToBuffer(writer, spec, useSwapFormat: true);
			writer.WriteLine();
			writer.WriteLine($"return offset + {spec.SizeOfCode};");
		}
	}

	private static void WriteReplaceBytesMethod(SourceWriter writer, ByteSwapWordSpec spec, bool isSigned)
	{
		string typeName = isSigned ? spec.SignedKeyword : spec.UnsignedKeyword;
		string methodSuffix = spec.IsUnnaturalWord
			? (isSigned ? spec.ConstantKeyword : "U" + spec.ConstantKeyword)
			: "";

		writer.WriteXmlDocSummary($"Replaces {spec.SizeOfInBytes} bytes in an array with a integer value");
		writer.WriteXmlDocParam("buffer", "byte buffer");
		writer.WriteXmlDocParam("offset", "offset in <paramref name=\"buffer\"/> to put the new value");
		writer.WriteXmlDocParam("value", "value to replace the buffer's current bytes with");
		writer.WriteXmlDocReturns($"offset + {spec.SizeOfInBytes}");
		if (isSigned)
		{
			writer.WriteLine(
				"/// <remarks><paramref name=\"buffer\"/>'s endian order is assumed to be the same as the " +
				"current operating environment</remarks>");
		}
		else
		{
			writer.WriteLine("/// <remarks>");
			writer.WriteLine(
				"/// <paramref name=\"buffer\"/>'s endian order is assumed to be the same as the current " +
				"operating environment.");
			writer.WriteLine(
				"/// Uses <see cref=\"BitConverter.IsLittleEndian\" /> to determine <paramref name=\"value\"/>'s " +
				"byte ordering");
			writer.WriteLine("/// when written to the buffer");
			writer.WriteLine("/// </remarks>");
		}
		writer.WriteLine($"public static int ReplaceBytes{methodSuffix}(byte[] buffer, int offset,");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"{typeName} value)");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			if (isSigned)
			{
				string unsignedMethodSuffix = spec.IsUnnaturalWord
					? "U" + spec.ConstantKeyword
					: "";
				writer.WriteLine(
					$"return ReplaceBytes{unsignedMethodSuffix}(buffer, offset, ({spec.UnsignedKeyword})value);");
				return;
			}

			WriteBufferContracts(writer, spec);
			writer.WriteLine();
			WriteByteDeclarations(writer, spec);
			writer.WriteLine("if (BitConverter.IsLittleEndian) {");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				WriteBytesFromValue(writer, spec, littleEndian: true);
			}
			writer.WriteLine("} else {");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				WriteBytesFromValue(writer, spec, littleEndian: false);
			}
			writer.WriteLine("}");
			writer.WriteLine();
			WriteBytesToBuffer(writer, spec, useSwapFormat: false);
			writer.WriteLine();
			writer.WriteLine("return offset;");
		}
	}

	private static void WriteBufferContracts(SourceWriter writer, ByteSwapWordSpec spec)
	{
		writer.WriteLine("ArgumentNullException.ThrowIfNull(buffer);");
		writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(offset);");
		writer.WriteLine(
			$"ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, buffer.Length - {spec.SizeOfCode});");
	}

	private static void WriteByteDeclarations(SourceWriter writer, ByteSwapWordSpec spec)
	{
		string[] byteNames = ByteNames(spec);
		writer.WriteLine("byte " + string.Join(", ", byteNames) + ";");
	}

	private static void WriteBytesFromBuffer(SourceWriter writer, ByteSwapWordSpec spec)
	{
		foreach (string byteName in ByteNames(spec))
		{
			writer.WriteLine($"{byteName} = buffer[offset++];");
		}
	}

	private static void WriteBytesFromValue(SourceWriter writer, ByteSwapWordSpec spec, bool littleEndian)
	{
		string[] byteNames = ByteNames(spec);
		for (int index = 0; index < byteNames.Length; index++)
		{
			int byteIndex = littleEndian
				? index
				: byteNames.Length - index - 1;
			writer.WriteLine($"{byteNames[index]} = (byte)(value >> {byteIndex * 8,2});");
		}
	}

	private static void WriteBytesToBuffer(SourceWriter writer, ByteSwapWordSpec spec, bool useSwapFormat)
	{
		foreach (string byteName in ByteNames(spec))
		{
			string offsetExpression = useSwapFormat
				? "--offset"
				: "offset++";
			writer.WriteLine($"buffer[{offsetExpression}] = {byteName};");
		}
	}

	private static void WriteUnnaturalSwapExpression(SourceWriter writer, ByteSwapWordSpec spec)
	{
		for (int byteIndex = 0; byteIndex < spec.SizeOfInBytes; byteIndex++)
		{
			int sourceByteIndex = spec.SizeOfInBytes - byteIndex - 1;
			int shift = (sourceByteIndex - byteIndex) * 8;
			string shiftOperator = shift >= 0
				? ">>"
				: "<<";
			int shiftMagnitude = System.Math.Abs(shift);
			string suffix = byteIndex == spec.SizeOfInBytes - 1
				? ""
				: " |";
			writer.WriteLine(
				$"((value {shiftOperator} {shiftMagnitude,2}) & {ByteMask(spec, byteIndex)}){suffix}");
		}
		writer.WriteLine(";");
	}

	private static string[] ByteNames(ByteSwapWordSpec spec)
	{
		var names = new string[spec.SizeOfInBytes];
		for (int index = 0; index < names.Length; index++)
		{
			names[index] = "b" + index.ToString(PrimitiveCatalog.InvariantCulture);
		}

		return names;
	}

	private static string ByteMask(ByteSwapWordSpec spec, int byteIndex)
	{
		ulong mask = 0xFFUL << (byteIndex * 8);
		// T4 emits masks only for unnatural 24/40-bit words backed by uint/ulong, so storage width is intentional.
		return "0x" + mask.ToString(spec.WordHexFormat, PrimitiveCatalog.InvariantCulture);
	}

	private readonly struct ByteSwapWordSpec
	{
		private readonly NumberSpec mStorageSpec;
		private readonly int mSizeOfInBits;

		public ByteSwapWordSpec(NumberSpec storageSpec)
			: this(storageSpec, storageSpec.SizeOfInBits)
		{
		}

		public ByteSwapWordSpec(NumberSpec storageSpec, int sizeOfInBits)
		{
			if (!storageSpec.IsUnsigned)
			{
				throw new ArgumentException("ByteSwap words must use unsigned storage descriptors.", nameof(storageSpec));
			}

			if (sizeOfInBits <= 0 ||
				sizeOfInBits % PrimitiveCatalog.BitsPerByte != 0 ||
				sizeOfInBits > storageSpec.SizeOfInBits)
			{
				throw new ArgumentOutOfRangeException(nameof(sizeOfInBits), sizeOfInBits, "Invalid byte-swap width.");
			}

			mStorageSpec = storageSpec;
			mSizeOfInBits = sizeOfInBits;
		}

		public string UnsignedKeyword => mStorageSpec.Keyword;

		public string SignedKeyword => mStorageSpec.SignedKeyword;

		public string UnsignedCode => mStorageSpec.TypeCode.ToString();

		public string SignedCode => mStorageSpec.SignedTypeCode.ToString();

		public string ConstantKeyword => IsUnnaturalWord
			? "Int" + mSizeOfInBits.ToString(PrimitiveCatalog.InvariantCulture)
			: mStorageSpec.ConstantKeyword;

		public string SizeOfCode => IsUnnaturalWord
			? "kSizeOf" + ConstantKeyword
			: $"sizeof({UnsignedKeyword})";

		public int SizeOfInBytes => mSizeOfInBits / PrimitiveCatalog.BitsPerByte;

		public bool IsUnnaturalWord => mSizeOfInBits != mStorageSpec.SizeOfInBits;

		public string WordHexFormat => mStorageSpec.ToStringHexFormat;
	}
}
