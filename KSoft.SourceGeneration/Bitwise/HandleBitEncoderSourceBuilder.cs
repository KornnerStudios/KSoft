using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Bitwise;

internal static class HandleBitEncoderSourceBuilder
{
	public const string HintName = "KSoft.Bitwise.HandleBitEncoder.g.cs";

	public static string Build()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("#nullable disable");
		writer.WriteLine();
		writer.WriteLine("using System;");
		writer.WriteLine("using System.Diagnostics.CodeAnalysis;");
		writer.WriteContractShimAliasUsing();
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft.Bitwise");
		writer.WriteLine();
		using (writer.EnterTypeDeclaration("partial struct HandleBitEncoder"))
		{
			// Keep constructor/accessor generation tied to the same major-word descriptors as enum bit encoders.
			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesMajorWords)
			{
				WriteConstructor(writer, spec);
			}
			writer.WriteLine();

			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesMajorWords)
			{
				WriteGetHandleMethod(writer, spec);
			}
			writer.WriteLine();

			WriteEncodeRegion(writer);
			writer.WriteLine();
			WriteDecodeRegion(writer);
		}

		return writer.ToString();
	}

	private static void WriteConstructor(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteLine($"public HandleBitEncoder({spec.Keyword} initialBits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("mBits.u64 = 0;");
			writer.WriteLine("mBits.u32 = 0;");
			writer.WriteLine("mBitIndex = 0;");
			writer.WriteLine();
			writer.WriteLine($"mBits.u{spec.SizeOfInBits} = initialBits;");
		}
	}

	private static void WriteGetHandleMethod(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteXmlDocSummary($"Get the {spec.SizeOfInBits}-bit handle value");
		writer.WriteXmlDocReturns();
		writer.WriteLine($"public readonly {spec.Keyword} GetHandle{spec.SizeOfInBits}()");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"=> mBits.u{spec.SizeOfInBits};");
		}
	}

	private static void WriteEncodeRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Encode"))
		{
			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesMajorWords)
			{
				WriteEncodeEnumMethod(writer, spec);
			}
			writer.WriteLine();

			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesMajorWords)
			{
				WriteEncodeBitMaskMethod(writer, spec);
				writer.WriteLine();
				WriteEncodeNoneableBitMaskMethod(writer, spec);
				writer.WriteLine();
				WriteEncodeTraitsMethod(writer, spec);
				WriteEncodeNoneableTraitsMethod(writer, spec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteDecodeRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Decode"))
		{
			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesMajorWords)
			{
				WriteDecodeEnumMethod(writer, spec);
			}
			writer.WriteLine();

			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesMajorWords)
			{
				WriteDecodeBitMaskMethod(writer, spec);
				WriteDecodeNoneableBitMaskMethod(writer, spec);
				writer.WriteLine();
				WriteDecodeTraitsMethod(writer, spec);
				WriteDecodeNoneableTraitsMethod(writer, spec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteEncodeEnumMethod(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteXmlDocSummary("Encode an enumeration value using an enumeration encoder object");
		writer.WriteLine("/// <typeparam name=\"TEnum\">Enumeration type to encode</typeparam>");
		writer.WriteXmlDocParam("value", "Enumeration value to encode");
		writer.WriteXmlDocParam("encoder", "Encoder for <typeparamref name=\"TEnum\"/> objects");
		writer.WriteLine(
			$"public void Encode{spec.SizeOfInBits}<TEnum>(TEnum value, " +
			$"EnumBitEncoder{spec.SizeOfInBits}<TEnum> encoder)");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"where TEnum : {SourceGenerationConstants.EnumConstraint}");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires<ArgumentNullException>(encoder != null);");
			writer.WriteLine();
			writer.WriteLine("encoder.BitEncode(value, ref mBits.u64, ref mBitIndex);");
		}
	}

	private static void WriteEncodeBitMaskMethod(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteXmlDocSummary("Bit encode a value into this handle");
		writer.WriteXmlDocParam("value", "Value to encode");
		writer.WriteXmlDocParam("bitMask", "Masking value for <paramref name=\"value\"/>");
		writer.WriteLine($"public void Encode{spec.SizeOfInBits}({spec.Keyword} value, {spec.Keyword} bitMask)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires<ArgumentException>(bitMask != 0);");
			writer.WriteLine();
			writer.WriteLine("Bits.BitEncodeEnum(value, ref mBits.u64, ref mBitIndex, bitMask);");
		}
	}

	private static void WriteEncodeNoneableBitMaskMethod(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteXmlDocSummary("Bit encode a none-able value into this handle");
		writer.WriteXmlDocParam("value", "Value to encode");
		writer.WriteXmlDocParam("bitMask", "Masking value for <paramref name=\"value\"/>");
		writer.WriteLine(
			$"public void EncodeNoneable{spec.SizeOfInBits}({spec.SignedKeyword} value, {spec.Keyword} bitMask)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires<ArgumentException>(bitMask != 0);");
			writer.WriteLine("Contract.Requires<ArgumentOutOfRangeException>(value.IsNoneOrPositive());");
			writer.WriteLine();
			writer.WriteLine("Bits.BitEncodeEnum((ulong)(value+1), ref mBits.u64, ref mBitIndex, bitMask);");
		}
	}

	private static void WriteEncodeTraitsMethod(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteXmlDocSummary("Bit encode a value into this handle");
		writer.WriteXmlDocParam("value", "Value to encode");
		writer.WriteXmlDocParam("traits", "");
		writer.WriteLine(
			$"public void Encode{spec.SizeOfInBits}({spec.Keyword} value, Bitwise.BitFieldTraits traits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires<ArgumentException>(!traits.IsEmpty);");
			writer.WriteLine();
			writer.WriteLine(
				$"Bits.BitEncodeEnum(value, ref mBits.u64, ref mBitIndex, traits.Bitmask{spec.SizeOfInBits});");
		}
	}

	private static void WriteEncodeNoneableTraitsMethod(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteXmlDocSummary("Bit encode a none-able value into this handle");
		writer.WriteXmlDocParam("value", "Value to encode");
		writer.WriteXmlDocParam("traits", "");
		writer.WriteLine(
			$"public void EncodeNoneable{spec.SizeOfInBits}({spec.SignedKeyword} value, " +
			"Bitwise.BitFieldTraits traits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires<ArgumentException>(!traits.IsEmpty);");
			writer.WriteLine("Contract.Requires<ArgumentOutOfRangeException>(value.IsNoneOrPositive());");
			writer.WriteLine();
			writer.WriteLine(
				"Bits.BitEncodeEnum((ulong)(value+1), ref mBits.u64, ref mBitIndex, " +
				$"traits.Bitmask{spec.SizeOfInBits});");
		}
	}

	private static void WriteDecodeEnumMethod(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteXmlDocSummary("Decode an enumeration value using an enumeration encoder object");
		writer.WriteLine("/// <typeparam name=\"TEnum\">Enumeration type to decode</typeparam>");
		writer.WriteXmlDocParam("value", "Enumeration value decoded from this handle");
		writer.WriteXmlDocParam("decoder", "Encoder for <typeparamref name=\"TEnum\"/> objects");
		writer.WriteLine(
			$"public void Decode{spec.SizeOfInBits}<TEnum>(out TEnum value, " +
			$"EnumBitEncoder{spec.SizeOfInBits}<TEnum> decoder)");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"where TEnum : {SourceGenerationConstants.EnumConstraint}");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires<ArgumentNullException>(decoder != null);");
			writer.WriteLine();
			writer.WriteLine("value = decoder.BitDecode(mBits.u64, ref mBitIndex);");
		}
	}

	private static void WriteDecodeBitMaskMethod(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteXmlDocSummary("Bit decode a value from this handle");
		writer.WriteXmlDocParam("value", "Value decoded from this handle");
		writer.WriteXmlDocParam("bitMask", "Masking value for <paramref name=\"value\"/>");
		writer.WriteLine($"public void Decode{spec.SizeOfInBits}(out {spec.Keyword} value, {spec.Keyword} bitMask)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires<ArgumentException>(bitMask != 0);");
			writer.WriteLine();
			writer.WriteLine($"value = ({spec.Keyword})Bits.BitDecode(mBits.u64, ref mBitIndex, bitMask);");
		}
	}

	private static void WriteDecodeNoneableBitMaskMethod(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteXmlDocSummary("Bit decode a value from this handle");
		writer.WriteXmlDocParam("value", "Value decoded from this handle");
		writer.WriteXmlDocParam("bitMask", "Masking value for <paramref name=\"value\"/>");
		writer.WriteLine(
			$"public void DecodeNoneable{spec.SizeOfInBits}(out {spec.SignedKeyword} value, {spec.Keyword} bitMask)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires<ArgumentException>(bitMask != 0);");
			writer.WriteLine();
			writer.WriteLine(
				$"value = ({spec.SignedKeyword})Bits.BitDecodeNoneable(mBits.u64, ref mBitIndex, bitMask);");
		}
	}

	private static void WriteDecodeTraitsMethod(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteXmlDocSummary("Bit decode a value from this handle");
		writer.WriteXmlDocParam("value", "Value decoded from this handle");
		writer.WriteXmlDocParam("traits", "");
		writer.WriteLine(
			$"public void Decode{spec.SizeOfInBits}(out {spec.Keyword} value, Bitwise.BitFieldTraits traits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires<ArgumentException>(!traits.IsEmpty);");
			writer.WriteLine();
			writer.WriteLine(
				$"value = ({spec.Keyword})Bits.BitDecode(mBits.u64, ref mBitIndex, " +
				$"traits.Bitmask{spec.SizeOfInBits});");
		}
	}

	private static void WriteDecodeNoneableTraitsMethod(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteXmlDocSummary("Bit decode a value from this handle");
		writer.WriteXmlDocParam("value", "Value decoded from this handle");
		writer.WriteXmlDocParam("traits", "");
		writer.WriteLine(
			$"public void DecodeNoneable{spec.SizeOfInBits}(out {spec.SignedKeyword} value, " +
			"Bitwise.BitFieldTraits traits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires<ArgumentException>(!traits.IsEmpty);");
			writer.WriteLine();
			writer.WriteLine(
				$"value = ({spec.SignedKeyword})Bits.BitDecodeNoneable(mBits.u64, ref mBitIndex, " +
				$"traits.Bitmask{spec.SizeOfInBits});");
		}
	}
}
