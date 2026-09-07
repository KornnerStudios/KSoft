using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Enum;

internal static class EnumSourceBuilder
{
	public const string EnumBitEncoderHintName = "KSoft.EnumBitEncoder.g.cs";
	public const string EnumValueHintName = "KSoft.Reflection.EnumValue.g.cs";

	public static string BuildEnumBitEncoder()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("using System;");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft");
		writer.WriteLine();
		foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesMajorWords)
		{
			WriteEnumBitEncoderClass(writer, spec);
			writer.WriteLine();
		}

		return writer.ToString();
	}

	public static string BuildEnumValue()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("using System;");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft.Reflection");
		writer.WriteLine();
		writer.WriteXmlDocSummary(
			"Utility for converting to and from a given Enum and integer types, without boxing operations but " +
			"without the safeguards of reflection");
		writer.WriteLine("/// <typeparam name=\"TEnum\"></typeparam>");
		writer.WriteLine("/// <remarks>");
		writer.WriteLine(
			"/// First use of each closed type compiles converter delegates, which allocates and takes measurable " +
			"time. Reused");
		writer.WriteLine("/// delegates avoid boxing and per-call allocations.");
		writer.WriteLine("/// 'From' methods can be unforgiving. Make sure you know what you're doing.");
		writer.WriteLine("/// </remarks>");
		writer.WriteLine("public sealed class EnumValue<TEnum> : EnumUtilBase<TEnum>");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"where TEnum : {SourceGenerationConstants.EnumConstraint}");
		}
		using (writer.EnterBlock(SourceWriterBlockType.BracesStatement))
		{
			writer.WriteLine(
				"// #VITA_MEASURE: Keep compiled no-boxing converters until BCL/Unsafe alternatives prove parity.");
			foreach (NumberSpec spec in PrimitiveCatalog.Numbers)
			{
				if (!spec.IsInteger)
				{
					continue;
				}

				writer.WriteLine(
					$"public static readonly Func<TEnum, {spec.Keyword}> To{spec.TypeCode} =   " +
					$"GenerateToMethod  <{spec.Keyword}>();");
				writer.WriteLine(
					$"public static readonly Func<{spec.Keyword}, TEnum> From{spec.TypeCode} = " +
					$"GenerateFromMethod<{spec.Keyword}>();");
				writer.WriteLine();
			}
		}

		return writer.ToString();
	}

	private static void WriteEnumBitEncoderClass(SourceWriter writer, NumberSpec spec)
	{
		WriteEnumBitEncoderClassDocs(writer);
		writer.WriteLine(
			"[System.Diagnostics.DebuggerDisplay(\"MaxValue = {MaxValueTrait}, Bitmask = {BitmaskTrait}, " +
			"BitCount = {BitCountTrait}\")]");
		writer.WriteLine(
			$"public sealed partial class EnumBitEncoder{spec.SizeOfInBits}<TEnum> : EnumBitEncoderBase, " +
			$"IEnumBitEncoder<{spec.Keyword}>");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"where TEnum : {SourceGenerationConstants.EnumConstraint}");
		}
		using (writer.EnterBlock(SourceWriterBlockType.BracesStatement))
		{
			WriteTraitFields(writer, spec);
			writer.WriteLine();
			WriteStaticInitializeRegion(writer, spec);
			writer.WriteLine();
			WriteTraitPropertiesRegion(writer, spec);
			writer.WriteLine();
			WriteDefaultBitIndexRegion(writer, spec);
			writer.WriteLine();
			WriteConstructors(writer, spec);
			writer.WriteLine();
			WriteEncodeRegion(writer, spec);
			writer.WriteLine();
			WriteDecodeRegion(writer, spec);
			writer.WriteLine();
			WriteEndianStreamingRegion(writer, spec);
		}
	}

	private static void WriteEnumBitEncoderClassDocs(SourceWriter writer)
	{
		writer.WriteXmlDocSummary("Utility class for encoding Enumerations into an integer's bits.");
		writer.WriteLine("/// <typeparam name=\"TEnum\"></typeparam>");
		writer.WriteLine("/// <remarks>");
		writer.WriteLine("/// Regular Enumerations should have a member called <b>kMax</b>. This value");
		writer.WriteLine("/// must be the highest value and shouldn't actually be used.");
		writer.WriteLine("/// If <b>kMax</b> doesn't exist, the highest value found, plus 1, is used as");
		writer.WriteLine("/// the assumed <b>kMax</b>");
		writer.WriteLine("///");
		writer.WriteLine("/// <see cref=\"FlagsAttribute\"/> Enumerations should have a member called");
		writer.WriteLine("/// <b>kAll</b>. This value must be equal to all the usable bits in the type.");
		writer.WriteLine("/// If you want to leave a certain bit or bits out of the encoder, don't include");
		writer.WriteLine("/// them in <b>kAll</b>'s value.");
		writer.WriteLine("/// If <b>kAll</b> doesn't exist, ALL members are OR'd together to create the");
		writer.WriteLine("/// assumed <b>kAll</b> value.");
		writer.WriteLine("/// </remarks>");
	}

	private static void WriteTraitFields(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteLine(
			"/// <remarks>Only made public for <see cref=\"Collections.EnumBitSet\"/>.</remarks>");
		writer.WriteLine("public static readonly bool kHasNone;");
		writer.WriteXmlDocSummary(
			"The <see cref=\"kEnumMaxMemberName\"/>\\<see cref=\"kFlagsMaxMemberName\"/>",
			"value or the member value whom this class assumed would be the max");
		writer.WriteLine($"static readonly {spec.Keyword} kMaxValue;");
		writer.WriteXmlDocSummary("Masking value that can be used to single out this enumeration's value(s)");
		writer.WriteLine($"public static readonly {spec.Keyword} kBitmask;");
		writer.WriteXmlDocSummary("How many bits the enumeration consumes");
		writer.WriteLine("public static readonly int kBitCount;");
	}

	private static void WriteStaticInitializeRegion(SourceWriter writer, NumberSpec spec)
	{
		using (writer.EnterRegion("Static Initialize"))
		{
			WriteProcessMembers(writer, spec);
			writer.WriteLine();
			WriteStaticConstructor(writer, spec);
		}
	}

	private static void WriteProcessMembers(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteLine($"static void ProcessMembers(Type t, out {spec.Keyword} maxValue, out bool hasNone)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine($"maxValue = {spec.Keyword}.MaxValue;");
			writer.WriteLine("hasNone = false;");
			writer.WriteLine("var mvalues = Reflection.EnumUtil<TEnum>.Values;");
			writer.WriteLine("var mnames = Reflection.EnumUtil<TEnum>.Names;");
			writer.WriteLine();
			WriteIsTypeSignedRegion(writer, spec);
			writer.WriteLine();
			writer.WriteLine($"{spec.Keyword} greatest = 0, temp;");
			writer.WriteLine("for (int x = 0; x < mvalues.Length; x++)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				WriteProcessMemberLoopBody(writer, spec);
			}
			writer.WriteLine();
			WriteProcessMembersFallbackMax(writer, spec);
		}
	}

	private static void WriteIsTypeSignedRegion(SourceWriter writer, NumberSpec spec)
	{
		using (writer.EnterRegion("is_type_signed"))
		{
			writer.WriteLine("static bool func_is_type_signed()");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("return Reflection.EnumUtil<TEnum>.UnderlyingTypeCode switch");
				using (writer.EnterBlock(SourceWriterBlockType.BracesStatement))
				{
					writer.WriteLine("TypeCode.SByte or TypeCode.Int16 or TypeCode.Int32");
					if (spec.SizeOfInBits >= 64)
					{
						writer.WriteLine("\tor TypeCode.Int64");
					}
					writer.WriteLine("=> true,");
					writer.WriteLine("_ => false,");
				}
			}
			writer.WriteLine("bool is_type_signed = func_is_type_signed();");
		}
	}

	private static void WriteProcessMemberLoopBody(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteLine("bool mvalue_is_none = false;");
		writer.WriteLine();
		writer.WriteLine("// Validate members when the underlying type is signed");
		writer.WriteLine("if (!Reflection.EnumUtil<TEnum>.IsFlags && is_type_signed)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine(
				$"{spec.SignedKeyword} int_value = Convert.ToInt{spec.SizeOfInBits}(mvalues.GetValue(x), " +
				"Util.InvariantCultureInfo);");
			writer.WriteLine();
			writer.WriteLine($"if (int_value < TypeExtensions.kNoneInt{spec.SizeOfInBits})");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteUnindentedLine(
					"#pragma warning disable CA2208 // Instantiate argument exceptions correctly");
				writer.WriteLine("throw new ArgumentOutOfRangeException(nameof(TEnum),");
				using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
				{
					writer.WriteLine("string.Format(Util.InvariantCultureInfo,");
					using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
					{
						writer.WriteLine(
							"\"{0}:{1} is invalid (negative, less than NONE)!\", t.FullName, mnames[x]));");
					}
				}
				writer.WriteUnindentedLine(
					"#pragma warning restore CA2208 // Instantiate argument exceptions correctly");
			}
			writer.WriteLine("else if (int_value.IsNone())");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("hasNone = mvalue_is_none = true;");
			}
		}
		writer.WriteLine();
		writer.WriteLine("ProcessMembers_DebugCheckMemberName(t, Reflection.EnumUtil<TEnum>.IsFlags, mnames[x]);");
		writer.WriteLine();
		writer.WriteLine("if (mvalue_is_none) // don't perform greatest value checking on NONE values");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("continue;");
		}
		writer.WriteLine();
		writer.WriteLine(
			$"temp = Convert.ToUInt{spec.SizeOfInBits}(mvalues.GetValue(x), Util.InvariantCultureInfo);");
		writer.WriteLine("// Base max_value off the predetermined member name first");
		writer.WriteLine("if (IsMaxMemberName(Reflection.EnumUtil<TEnum>.IsFlags, mnames[x]))");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("maxValue = greatest = temp;");
			writer.WriteLine("// we don't stop processing even after we hit the 'max' member");
			writer.WriteLine("// just to be safe that we're sanity checking all members, and in the event");
			writer.WriteLine("// the 'none' member is defined after the 'max' member");
			writer.WriteLine("//break;");
		}
		writer.WriteLine("// Record the greatest value thus far in case the above doesn't exist");
		writer.WriteLine("else");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (!Reflection.EnumUtil<TEnum>.IsFlags)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("greatest = System.Math.Max(greatest, temp);");
			}
			writer.WriteLine("else");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("greatest |= temp; // just add all the flag values together");
			}
		}
	}

	private static void WriteProcessMembersFallbackMax(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteLine("// If the Enum doesn't have a member named k*MaxMemberName, use the assumed max value");
		writer.WriteLine(
			$"if (maxValue == {spec.Keyword}.MaxValue && greatest != {spec.Keyword}.MaxValue) " +
			"// just in case k*MaxMemberName actually equaled uint.MaxValue");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("maxValue = greatest;");
			writer.WriteLine();
			writer.WriteLine(
				$"// NOTE: we add +1 because the [Bits.GetBitmaskEnum{spec.SizeOfInBits}] method assumes " +
				"the parameter");
			writer.WriteLine("// isn't a real member of the enumeration. We didn't find a k*MaxMemberName so we");
			writer.WriteLine("// fake it");
			writer.WriteLine("if (!Reflection.EnumUtil<TEnum>.IsFlags)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("maxValue += 1;");
			}
		}
	}

	private static void WriteStaticConstructor(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteLine($"static EnumBitEncoder{spec.SizeOfInBits}()");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Type t = typeof(TEnum);");
			writer.WriteLine("InitializeBase(t);");
			writer.WriteLine();
			writer.WriteLine("ProcessMembers(t, out kMaxValue, out kHasNone);");
			writer.WriteLine("if (Reflection.EnumUtil<TEnum>.IsFlags)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("kBitmask = kMaxValue;");
			}
			writer.WriteLine("else");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("kBitmask = Bits.GetBitmaskEnum(kHasNone ? kMaxValue+1 : kMaxValue);");
			}
			writer.WriteLine("kBitCount = System.Numerics.BitOperations.PopCount(kBitmask);");
		}
	}

	private static void WriteTraitPropertiesRegion(SourceWriter writer, NumberSpec spec)
	{
		using (writer.EnterRegion("IEnumBitEncoder<TUInt>"))
		{
			writer.WriteLine("public bool IsFlags => Reflection.EnumUtil<TEnum>.IsFlags;");
			writer.WriteLine("public bool HasNone => kHasNone;");
			writer.WriteLine($"public {spec.Keyword} MaxValueTrait => kMaxValue;");
			writer.WriteLine("/// <see cref=\"kBitmask\"/>");
			writer.WriteLine($"public {spec.Keyword} BitmaskTrait => kBitmask;");
			writer.WriteLine("/// <see cref=\"kBitCount\"/>");
			writer.WriteLine("public override int BitCountTrait => kBitCount;");
		}
	}

	private static void WriteDefaultBitIndexRegion(SourceWriter writer, NumberSpec spec)
	{
		using (writer.EnterRegion("DefaultBitIndex"))
		{
			writer.WriteLine("readonly int mDefaultBitIndex;");
			writer.WriteXmlDocSummary("The bit index assumed when one isn't provided");
			writer.WriteLine("public int DefaultBitIndex => mDefaultBitIndex;");
		}
	}

	private static void WriteConstructors(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteLine($"public EnumBitEncoder{spec.SizeOfInBits}() : this(0) {{}}");
		writer.WriteLine($"public EnumBitEncoder{spec.SizeOfInBits}(int defaultBitIndex)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(defaultBitIndex);");
			writer.WriteLine();
			writer.WriteLine("mDefaultBitIndex = defaultBitIndex;");
		}
	}

	private static void WriteEncodeRegion(SourceWriter writer, NumberSpec spec)
	{
		using (writer.EnterRegion("Encode"))
		{
			WriteEncodeWithDefaultBitIndex(writer, spec);
			WriteEncodeWithExplicitBitIndex(writer, spec);
			WriteEncodeWithTraits(writer, spec);
			WriteEncodeWithBitIndexRef(writer, spec);
		}
	}

	private static void WriteEncodeWithDefaultBitIndex(SourceWriter writer, NumberSpec spec)
	{
		WriteEncodeXmlDocs(writer, includeBitIndex: false, includeReturns: true);
		writer.WriteLine(
			"/// <remarks>Uses <see cref=\"DefaultBitIndex\"/> as the bit index to start encoding at</remarks>");
		writer.WritePurityAnnotation();
		writer.WriteLine($"public {spec.Keyword} BitEncode(TEnum value, {spec.Keyword} bits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("return BitEncode(value, bits, mDefaultBitIndex);");
		}
	}

	private static void WriteEncodeWithExplicitBitIndex(SourceWriter writer, NumberSpec spec)
	{
		WriteEncodeXmlDocs(writer, includeBitIndex: true, includeReturns: true);
		writer.WritePurityAnnotation();
		writer.WriteLine($"public {spec.Keyword} BitEncode(TEnum value, {spec.Keyword} bits, int bitIndex)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitIndexContracts(writer, spec);
			writer.WriteLine();
			WriteEncodeValuePreparation(writer, spec);
			writer.WriteLine();
			writer.WriteLine("if (v > kMaxValue) { throw new InvalidOperationException(\"Value exceeds the maximum encoded value.\"); }");
			WriteEncodeReturn(writer, spec);
		}
	}

	private static void WriteEncodeWithTraits(SourceWriter writer, NumberSpec spec)
	{
		WriteEncodeXmlDocs(writer, includeBitIndex: false, includeReturns: true, traits: true);
		writer.WritePurityAnnotation();
		writer.WriteLine(
			$"public {spec.Keyword} BitEncode(TEnum value, {spec.Keyword} bits, " +
			"Bitwise.BitFieldTraits traits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (traits.IsEmpty) { throw new ArgumentException(\"Traits must not be empty.\", nameof(traits)); }");
			writer.WriteLine();
			writer.WriteLine("return BitEncode(value, bits, traits.BitIndex);");
		}
	}

	private static void WriteEncodeWithBitIndexRef(SourceWriter writer, NumberSpec spec)
	{
		WriteEncodeXmlDocs(writer, includeBitIndex: true, includeReturns: false);
		writer.WriteLine("/// <remarks>");
		writer.WriteLine("/// On return <paramref name=\"bits\"/> has <paramref name=\"value\"/> encoded into it and");
		writer.WriteLine(
			"/// <paramref name=\"bitIndex\"/> is incremented by the bit count of the underlying enumeration");
		writer.WriteLine("/// </remarks>");
		writer.WritePurityAnnotation();
		writer.WriteLine($"public void BitEncode(TEnum value, ref {spec.Keyword} bits, ref int bitIndex)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitIndexContracts(writer, spec);
			writer.WriteLine(
				$"if ((bitIndex+kBitCount) >= Bits.kInt{spec.SizeOfInBits}BitCount) " +
				"{ throw new ArgumentOutOfRangeException(nameof(bitIndex)); }");
			writer.WriteLine();
			WriteEncodeValuePreparation(writer, spec);
			writer.WriteLine();
			writer.WriteLine("if (v > kMaxValue) { throw new InvalidOperationException(\"Value exceeds the maximum encoded value.\"); }");
			writer.WriteLine("bits = Reflection.EnumUtil<TEnum>.IsFlags");
			writer.WriteLine($"\t? Bits.BitEncodeFlags(v, bits, bitIndex, kBitmask)");
			writer.WriteLine($"\t: Bits.BitEncodeEnum (v, bits, bitIndex, kBitmask);");
			writer.WriteLine();
			writer.WriteLine("bitIndex += kBitCount;");
		}
	}

	private static void WriteEncodeXmlDocs(
		SourceWriter writer,
		bool includeBitIndex,
		bool includeReturns,
		bool traits = false)
	{
		writer.WriteXmlDocSummary("Bit encode an enumeration value into an unsigned integer");
		writer.WriteXmlDocParam("value", "Enumeration value to encode");
		writer.WriteXmlDocParam("bits", "Bit data as an unsigned integer");
		if (includeBitIndex)
		{
			writer.WriteXmlDocParam("bitIndex", "Index in <paramref name=\"bits\"/> to start encoding at");
		}
		else if (traits)
		{
			writer.WriteXmlDocParam("traits", "Index in <paramref name=\"bits\"/> to start encoding at");
		}
		if (includeReturns)
		{
			writer.WriteXmlDocReturns("<paramref name=\"bits\"/> with <paramref name=\"value\"/> encoded into it");
		}
	}

	private static void WriteBitIndexContracts(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(bitIndex);");
		writer.WriteLine($"ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitIndex, Bits.kInt{spec.SizeOfInBits}BitCount);");
	}

	private static void WriteEncodeValuePreparation(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteLine($"{spec.Keyword} v = Reflection.EnumValue<TEnum>.ToUInt{spec.SizeOfInBits}(value);");
		writer.WriteLine("if (kHasNone)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("v++;");
		}
	}

	private static void WriteEncodeReturn(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteLine("return Reflection.EnumUtil<TEnum>.IsFlags");
		writer.WriteLine($"\t? Bits.BitEncodeFlags(v, bits, bitIndex, kBitmask)");
		writer.WriteLine($"\t: Bits.BitEncodeEnum (v, bits, bitIndex, kBitmask);");
	}

	private static void WriteDecodeRegion(SourceWriter writer, NumberSpec spec)
	{
		using (writer.EnterRegion("Decode"))
		{
			WriteDecodeWithDefaultBitIndex(writer, spec);
			WriteDecodeWithExplicitBitIndex(writer, spec);
			WriteDecodeWithTraits(writer, spec);
			WriteDecodeWithBitIndexRef(writer, spec);
		}
	}

	private static void WriteDecodeWithDefaultBitIndex(SourceWriter writer, NumberSpec spec)
	{
		WriteDecodeXmlDocs(writer, includeBitIndex: false, firstParamHasLegacyTypo: true);
		writer.WriteLine(
			"/// <remarks>Uses <see cref=\"DefaultBitIndex\"/> as the bit index to start decoding at</remarks>");
		writer.WritePurityAnnotation();
		writer.WriteLine($"public TEnum BitDecode({spec.Keyword} bits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("return BitDecode(bits, mDefaultBitIndex);");
		}
	}

	private static void WriteDecodeWithExplicitBitIndex(SourceWriter writer, NumberSpec spec)
	{
		WriteDecodeXmlDocs(writer, includeBitIndex: true);
		writer.WritePurityAnnotation();
		writer.WriteLine($"public TEnum BitDecode({spec.Keyword} bits, int bitIndex)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitIndexContracts(writer, spec);
			writer.WriteLine();
			WriteDecodeValuePreparation(writer, spec);
			writer.WriteLine();
			writer.WriteLine(
				$"if (v > kMaxValue && (!kHasNone || v != {spec.Keyword}.MaxValue)) " +
				"{ throw new InvalidOperationException(\"Value exceeds the maximum encoded value.\"); }");
			writer.WriteLine($"return Reflection.EnumValue<TEnum>.FromUInt{spec.SizeOfInBits}(v);");
		}
	}

	private static void WriteDecodeWithTraits(SourceWriter writer, NumberSpec spec)
	{
		WriteDecodeXmlDocs(writer, includeBitIndex: false, traits: true);
		writer.WritePurityAnnotation();
		writer.WriteLine($"public TEnum BitDecode({spec.Keyword} bits, Bitwise.BitFieldTraits traits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (traits.IsEmpty) { throw new ArgumentException(\"Traits must not be empty.\", nameof(traits)); }");
			writer.WriteLine();
			writer.WriteLine("return BitDecode(bits, traits.BitIndex);");
		}
	}

	private static void WriteDecodeWithBitIndexRef(SourceWriter writer, NumberSpec spec)
	{
		WriteDecodeXmlDocs(writer, includeBitIndex: true);
		writer.WriteLine("/// <remarks>");
		writer.WriteLine(
			"/// <paramref name=\"bitIndex\"/> is incremented by the bit count of the underlying enumeration");
		writer.WriteLine("/// </remarks>");
		writer.WritePurityAnnotation();
		writer.WriteLine($"public TEnum BitDecode({spec.Keyword} bits, ref int bitIndex)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteBitIndexContracts(writer, spec);
			writer.WriteLine(
				$"if ((bitIndex+kBitCount) >= Bits.kInt{spec.SizeOfInBits}BitCount) " +
				"{ throw new ArgumentOutOfRangeException(nameof(bitIndex)); }");
			writer.WriteLine();
			WriteDecodeValuePreparation(writer, spec);
			writer.WriteLine();
			writer.WriteLine("bitIndex += kBitCount;");
			writer.WriteLine();
			writer.WriteLine(
				$"if (v > kMaxValue && (!kHasNone || v != {spec.Keyword}.MaxValue)) " +
				"{ throw new InvalidOperationException(\"Value exceeds the maximum encoded value.\"); }");
			writer.WriteLine($"return Reflection.EnumValue<TEnum>.FromUInt{spec.SizeOfInBits}(v);");
		}
	}

	private static void WriteDecodeXmlDocs(
		SourceWriter writer,
		bool includeBitIndex,
		bool firstParamHasLegacyTypo = false,
		bool traits = false)
	{
		writer.WriteXmlDocSummary("Bit decode an enumeration value from an unsigned integer");
		string bitsDocumentation = firstParamHasLegacyTypo
			? "Unsigned integer to decode from<"
			: "Unsigned integer to decode from";
		writer.WriteXmlDocParam("bits", bitsDocumentation);
		if (includeBitIndex)
		{
			writer.WriteXmlDocParam("bitIndex", "Index in <paramref name=\"bits\"/> to start decoding at");
		}
		else if (traits)
		{
			writer.WriteXmlDocParam("traits", "Index in <paramref name=\"bits\"/> to start decoding at");
		}
		writer.WriteXmlDocReturns(
			"The enumeration value as it stood before it was ever encoded into <paramref name=\"bits\"/>");
	}

	private static void WriteDecodeValuePreparation(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteLine($"{spec.Keyword} v = Bits.BitDecode(bits, bitIndex, kBitmask);");
		writer.WriteLine("if (kHasNone)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("v--;");
		}
	}

	private static void WriteEndianStreamingRegion(SourceWriter writer, NumberSpec spec)
	{
		using (writer.EnterRegion("Endian Streaming"))
		{
			WriteReadMethod(writer, spec);
			WriteWriteMethod(writer, spec);
		}
	}

	private static void WriteReadMethod(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteXmlDocSummary("Read a <typeparamref name=\"TEnum\"/> value from a stream");
		writer.WriteXmlDocParam("s", "Stream to read from");
		writer.WriteXmlDocParam("value", "Enum value read from the stream");
		writer.WriteLine("/// <remarks>");
		writer.WriteLine("/// Uses <typeparamref name=\"TEnum\"/>'s underlying <see cref=\"TypeCode\"/> to");
		writer.WriteLine("/// decide how big of a numeric type to read from the stream.");
		writer.WriteLine("/// </remarks>");
		writer.WriteLine("public static void Read(IO.EndianReader s, out TEnum value)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentNullException.ThrowIfNull(s);");
			writer.WriteLine();
			writer.WriteLine($"{spec.Keyword} stream_value = Reflection.EnumUtil<TEnum>.UnderlyingTypeCode switch");
			using (writer.EnterBlock(SourceWriterBlockType.BracesStatement))
			{
				writer.WriteLine("TypeCode.Byte or TypeCode.SByte => s.ReadByte(),");
				writer.WriteLine("TypeCode.Int16 or TypeCode.UInt16 => s.ReadUInt16(),");
				writer.WriteLine("TypeCode.Int32 or TypeCode.UInt32 => s.ReadUInt32(),");
				if (spec.SizeOfInBits >= 64)
				{
					writer.WriteLine("TypeCode.Int64 or TypeCode.UInt64 => s.ReadUInt64(),");
				}
				writer.WriteLine("_ => throw new Debug.UnreachableException(),");
			}
			writer.WriteLine();
			writer.WriteLine("value = Reflection.EnumValue<TEnum>.FromUInt64(stream_value);");
		}
	}

	private static void WriteWriteMethod(SourceWriter writer, NumberSpec spec)
	{
		writer.WriteXmlDocSummary("Write a <typeparamref name=\"TEnum\"/> value to a stream");
		writer.WriteXmlDocParam("s", "Stream to write to");
		writer.WriteXmlDocParam("value", "Value to write to the stream");
		writer.WriteLine("/// <remarks>");
		writer.WriteLine("/// Uses <typeparamref name=\"TEnum\"/>'s underlying <see cref=\"TypeCode\"/> to");
		writer.WriteLine("/// decide how big of a numeric type to write to the stream.");
		writer.WriteLine("/// </remarks>");
		writer.WriteLine("public static void Write(IO.EndianWriter s, TEnum value)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentNullException.ThrowIfNull(s);");
			writer.WriteLine();
			writer.WriteLine(
				$"{spec.Keyword} stream_value = Reflection.EnumValue<TEnum>.ToUInt{spec.SizeOfInBits}(value);");
			writer.WriteLine("switch (Reflection.EnumUtil<TEnum>.UnderlyingTypeCode)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("case TypeCode.Byte:");
				writer.WriteLine("case TypeCode.SByte: s.Write((byte)stream_value);");
				using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
				{
					writer.WriteLine("break;");
				}
				writer.WriteLine("case TypeCode.Int16:");
				writer.WriteLine("case TypeCode.UInt16: s.Write((ushort)stream_value);");
				using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
				{
					writer.WriteLine("break;");
				}
				writer.WriteLine("case TypeCode.Int32:");
				writer.WriteLine("case TypeCode.UInt32: s.Write((uint)stream_value);");
				using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
				{
					writer.WriteLine("break;");
				}
				if (spec.SizeOfInBits >= 64)
				{
					writer.WriteLine("case TypeCode.Int64:");
					writer.WriteLine("case TypeCode.UInt64: s.Write(stream_value);");
					using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
					{
						writer.WriteLine("break;");
					}
				}
				writer.WriteLine();
				writer.WriteLine("default:");
				using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
				{
					writer.WriteLine("throw new Debug.UnreachableException();");
				}
			}
		}
	}
}
