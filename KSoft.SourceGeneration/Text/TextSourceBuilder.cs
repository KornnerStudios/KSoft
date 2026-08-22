using System;
using System.Collections.Generic;
using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Text;

internal static class TextSourceBuilder
{
	// Keep the three old Text T4 rollback boundaries separate even though they share number descriptors.
	public const string NumbersToStringHintName = "KSoft.Numbers.ToString.g.cs";
	public const string NumbersParseHintName = "KSoft.Numbers.Parse.g.cs";
	public const string CharLookupTablesHintName = "KSoft.Text.CharLookupTables.g.cs";

	// Maps to KSoft.T4.NumbersT4.ParseableIntegersSmall. The Text generator owns this semantic grouping;
	// PrimitiveCatalog only supplies canonical primitive descriptors.
	private static readonly IReadOnlyList<NumberSpec> kParseableIntegersSmall =
		[
			PrimitiveCatalog.NumberFor(TypeCode.Byte),
			PrimitiveCatalog.NumberFor(TypeCode.SByte),
			PrimitiveCatalog.NumberFor(TypeCode.UInt16),
			PrimitiveCatalog.NumberFor(TypeCode.Int16),
		];

	// Maps to KSoft.T4.NumbersT4.ParseableIntegersWordAligned; keep Text-specific parse coverage here.
	private static readonly IReadOnlyList<NumberSpec> kParseableIntegersWordAligned =
		[
			PrimitiveCatalog.NumberFor(TypeCode.UInt32),
			PrimitiveCatalog.NumberFor(TypeCode.Int32),
			PrimitiveCatalog.NumberFor(TypeCode.UInt64),
			PrimitiveCatalog.NumberFor(TypeCode.Int64),
		];

	private static readonly IReadOnlyList<NumberSpec> kIntegerNumbers = PrimitiveCatalog.BittableTypes;

	public static string BuildNumbersToString()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("#nullable disable");
		writer.WriteLine();
		writer.WriteLine("using System;");
		writer.WriteLine("using System.Collections.Generic;");
		writer.WriteLine("using StringBuilder = System.Text.StringBuilder;");
		writer.WriteLine();
		writer.WriteUnindentedLine("#pragma warning disable IDE0305 // Collection initialization can be simplified");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft");
		writer.WriteLine();
		using (writer.EnterTypeDeclaration("partial class Numbers"))
		{
			foreach (NumberSpec typeSpec in kParseableIntegersWordAligned)
			{
				WriteToStringWordAlignedRegion(writer, typeSpec);
			}
			writer.WriteLine();
			foreach (NumberSpec typeSpec in kParseableIntegersSmall)
			{
				WriteToStringSmallRegion(writer, typeSpec);
			}
			writer.WriteLine();
			foreach (NumberSpec typeSpec in kParseableIntegersWordAligned)
			{
				WriteToStringListRegion(writer, typeSpec);
			}
		}

		return writer.ToString();
	}

	public static string BuildNumbersParse()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("#nullable disable");
		writer.WriteLine();
		writer.WriteLine("using System;");
		writer.WriteLine("using System.Collections.Generic;");
		writer.WriteLine("using System.Linq;");
		writer.WriteLine("using System.Threading.Tasks;");
		writer.WriteLine();
		writer.WriteUnindentedLine("#pragma warning disable IDE0301 // Collection initialization can be simplified");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft");
		writer.WriteLine();
		using (writer.EnterTypeDeclaration("partial class Numbers"))
		{
			foreach (NumberSpec typeSpec in kParseableIntegersWordAligned)
			{
				WriteTryParseWordAlignedRegion(writer, typeSpec);
			}
			writer.WriteLine();
			foreach (NumberSpec typeSpec in kParseableIntegersSmall)
			{
				WriteTryParseSmallRegion(writer, typeSpec);
			}
			writer.WriteLine();
			foreach (NumberSpec typeSpec in kIntegerNumbers)
			{
				WriteParseStringRegion(writer, typeSpec);
			}
			writer.WriteLine();
			foreach (NumberSpec typeSpec in kIntegerNumbers)
			{
				WriteTryParseListRegion(writer, typeSpec);
			}
		}

		return writer.ToString();
	}

	public static string BuildCharLookupTables()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("#nullable disable");
		writer.WriteLine();
		writer.WriteUnindentedLine("#pragma warning disable IDE0300 // Collection initialization can be simplified");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft.Text");
		writer.WriteLine();
		using (writer.EnterTypeDeclaration("partial class Util"))
		{
			WriteCharToByteLookupTable(writer, 36, hexOnly: false, base36CaseFold: true);
			writer.WriteLine();
			WriteCharToByteLookupTable(writer, 62, hexOnly: false, base36CaseFold: false);
			writer.WriteLine();
			WriteCharIsDigitLookupTable(writer, 62, hexOnly: false);
			writer.WriteLine();
			WriteCharToByteLookupTable(writer, 16, hexOnly: true, base36CaseFold: true);
			writer.WriteLine();
			WriteCharIsDigitLookupTable(writer, 16, hexOnly: true);
		}

		return writer.ToString();
	}

	private static void WriteToStringWordAlignedRegion(SourceWriter writer, NumberSpec typeSpec)
	{
		string code = typeSpec.TypeCode.ToString();
		string keyword = typeSpec.Keyword;

		using (writer.EnterRegion(code))
		{
			writer.WriteLine(
				$"static void ToStringBuilder(StringBuilder sb, {keyword} value, int radix, int startIndex, string digits)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				WriteToStringSignGuard(writer, typeSpec, "sb", "startIndex");
				writer.WriteLine($"var radix_in_word = ({keyword})radix;");
				WriteToStringDigitLoop(writer, "sb.Insert(startIndex, digits[digit_index]);");
			}
			writer.WriteLine("// List<> has a(n in-place) Reverse method. StringBuilder doesn't. That's why.");
			writer.WriteLine(
				"// We use additional memory (List<>.ToArrray allocates a new array) but have less computational complexity");
			writer.WriteLine($"static void ToStringBuilder(List<char> sb, {keyword} value, int radix, string digits)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("int start_index = sb.Count;");
				writer.WriteLine();
				WriteToStringListSignGuard(writer, typeSpec);
				writer.WriteLine($"var radix_in_word = ({keyword})radix;");
				WriteToStringDigitLoop(writer, "sb.Add(digits[digit_index]);");
				if (typeSpec.IsSigned)
				{
					writer.WriteLine();
					writer.WriteLine("if (is_signed)");
					using (writer.EnterBlock(SourceWriterBlockType.Braces))
					{
						writer.WriteLine("sb.Add('-');");
					}
				}
				writer.WriteLine();
				writer.WriteLine("sb.Reverse(start_index, sb.Count-start_index);");
			}
			writer.WriteLine($"static string ToStringImpl({keyword} value, int radix, string digits)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("var sb = new List<char>();");
				writer.WriteLine("ToStringBuilder(sb, value, radix, digits);");
				writer.WriteLine();
				writer.WriteLine("return new string(sb.ToArray());");
			}
			WriteToStringPublicOverloads(writer, typeSpec);
		}
	}

	private static void WriteToStringSmallRegion(SourceWriter writer, NumberSpec typeSpec)
	{
		using (writer.EnterRegion(typeSpec.TypeCode.ToString()))
		{
			WriteToStringPublicOverloads(writer, typeSpec);
		}
	}

	private static void WriteToStringPublicOverloads(SourceWriter writer, NumberSpec typeSpec)
	{
		string keyword = typeSpec.Keyword;

		writer.WriteLine(
			$"public static string ToString({keyword} value, int radix = kBase10, string digits = kBase64Digits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentException.ThrowIfNullOrEmpty(digits);");
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfLessThan(radix, 2);");
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfGreaterThan(radix, digits.Length);");
			writer.WriteLine();
			writer.WriteLine("return ToStringImpl(value, radix, digits);");
		}
		writer.WriteLine(
			$"public static string ToString({keyword} value, NumeralBase radix = NumeralBase.Decimal, " +
			"string digits = kBase64Digits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentException.ThrowIfNullOrEmpty(digits);");
			writer.WriteLine(
				"if (!IsValidLookupTable(radix, digits)) { throw new ArgumentException(\"Invalid lookup table.\", nameof(digits)); }");
			writer.WriteLine();
			writer.WriteLine("return ToStringImpl(value, (int)radix, digits);");
		}
		if (typeSpec.SizeOfInBits >= 32)
		{
			writer.WriteLine(
				$"public static StringBuilder ToStringBuilder(StringBuilder sb, {keyword} value, " +
				"NumeralBase radix = NumeralBase.Decimal, int startIndex = -1, string digits = kBase64Digits)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("ArgumentNullException.ThrowIfNull(sb);");
				writer.WriteLine("ArgumentException.ThrowIfNullOrEmpty(digits);");
				writer.WriteLine(
					"if (!IsValidLookupTable(radix, digits)) { throw new ArgumentException(\"Invalid lookup table.\", nameof(digits)); }");
				writer.WriteLine(
					"if (!startIndex.IsNoneOrPositive()) { throw new ArgumentOutOfRangeException(nameof(startIndex)); }");
				writer.WriteLine("if (startIndex.IsNone())");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("startIndex = sb.Length;");
				}
				writer.WriteLine();
				writer.WriteLine("ToStringBuilder(sb, value, (int)radix, startIndex, digits);");
				writer.WriteLine("return sb;");
			}
			writer.WriteLine(
				$"public static List<char> ToStringBuilder(List<char> sb, {keyword} value, " +
				"NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("ArgumentNullException.ThrowIfNull(sb);");
				writer.WriteLine("ArgumentException.ThrowIfNullOrEmpty(digits);");
				writer.WriteLine(
					"if (!IsValidLookupTable(radix, digits)) { throw new ArgumentException(\"Invalid lookup table.\", nameof(digits)); }");
				writer.WriteLine();
				writer.WriteLine("ToStringBuilder(sb, value, (int)radix, digits);");
				writer.WriteLine("return sb;");
			}
		}
	}

	private static void WriteToStringSignGuard(
		SourceWriter writer,
		NumberSpec typeSpec,
		string builderName,
		string startIndexName)
	{
		if (!typeSpec.IsSigned)
		{
			return;
		}

		writer.WriteLine("// Sign support only exist for decimal and lower bases");
		writer.WriteLine("if (radix <= kBase10 && value < 0)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine($"{builderName}.Append('-');");
			writer.WriteLine($"++{startIndexName};");
			writer.WriteLine("value = -value; // change the value to positive");
		}
		writer.WriteLine("else if (radix > kBase10 && value < 0)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteSignSupportThrow(writer);
		}
		writer.WriteLine();
	}

	private static void WriteToStringListSignGuard(SourceWriter writer, NumberSpec typeSpec)
	{
		if (!typeSpec.IsSigned)
		{
			return;
		}

		writer.WriteLine("bool is_signed = false;");
		writer.WriteLine("// Sign support only exist for decimal and lower bases");
		writer.WriteLine("if (radix <= kBase10 && value < 0)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("is_signed = true;");
			writer.WriteLine("value = -value; // change the value to positive");
		}
		writer.WriteLine("else if (radix > kBase10 && value < 0)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteSignSupportThrow(writer);
		}
		writer.WriteLine();
	}

	private static void WriteSignSupportThrow(SourceWriter writer)
	{
		writer.WriteLine(
			"throw new ArgumentOutOfRangeException(nameof(value), value, " +
			"\"Sign support only exist for decimal and lower bases\");");
	}

	private static void WriteToStringDigitLoop(SourceWriter writer, string appendLine)
	{
		writer.WriteLine("do {");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine("int digit_index = (int)(value % radix_in_word);");
			writer.WriteLine(appendLine);
			writer.WriteLine("value /= radix_in_word;");
		}
		writer.WriteLine("} while (value > 0);");
	}

	private static void WriteToStringListRegion(SourceWriter writer, NumberSpec typeSpec)
	{
		string code = typeSpec.TypeCode.ToString();
		string keyword = typeSpec.Keyword;

		using (writer.EnterRegion($"ToStringList {code}"))
		{
			writer.WriteLine($"public static string ToStringList(StringListDesc desc, IEnumerable<{keyword}> values,");
			writer.WriteLine($"\tPredicate<IEnumerable<{keyword}>> writeTerminator = null)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("if (desc.RequiresTerminator) { ArgumentNullException.ThrowIfNull(writeTerminator); }");
				writer.WriteLine();
				writer.WriteLine("var chars = new List<char>();");
				writer.WriteLine();
				writer.WriteLine("bool needs_separator = false;");
				writer.WriteLine("int radix = (int)desc.Radix;");
				writer.WriteLine("if (values != null)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("foreach (var value in values)");
					using (writer.EnterBlock(SourceWriterBlockType.Braces))
					{
						writer.WriteLine("if (needs_separator)");
						using (writer.EnterBlock(SourceWriterBlockType.Braces))
						{
							writer.WriteLine("chars.Add(desc.Separator);");
						}
						writer.WriteLine("else");
						using (writer.EnterBlock(SourceWriterBlockType.Braces))
						{
							writer.WriteLine("needs_separator = true;");
						}
						writer.WriteLine();
						writer.WriteLine("ToStringBuilder(chars, value, radix, desc.Digits);");
					}
					writer.WriteLine();
					writer.WriteLine("if (writeTerminator != null && writeTerminator(values))");
					using (writer.EnterBlock(SourceWriterBlockType.Braces))
					{
						writer.WriteLine("chars.Add(desc.Terminator);");
					}
				}
				writer.WriteLine();
				writer.WriteLine("return new string(chars.ToArray());");
			}
		}
	}

	private static void WriteTryParseWordAlignedRegion(SourceWriter writer, NumberSpec typeSpec)
	{
		string code = typeSpec.TypeCode.ToString();
		string keyword = typeSpec.Keyword;

		using (writer.EnterRegion(code))
		{
			writer.WriteLine(
				$"static bool TryParseImpl(string s, ref {keyword} result, int radix, int startIndex, int length, string digits)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				WriteTryParseImplStart(writer);
				WriteTryParseSignRead(writer, typeSpec);
				writer.WriteLine($"for (var radix_in_word = ({keyword})radix; pos < end && !char.IsWhiteSpace(s[pos]); ++pos)");
				WriteTryParseDigitLoopBody(writer, keyword);
				WriteTryParseSignApply(writer, typeSpec);
				writer.WriteLine();
				writer.WriteLine("return success;");
			}
			WriteTryParsePublicOverloads(writer, typeSpec, hasDoubleSpaceReturn: false);
		}
	}

	private static void WriteTryParseSmallRegion(SourceWriter writer, NumberSpec typeSpec)
	{
		string code = typeSpec.TypeCode.ToString();
		string keyword = typeSpec.Keyword;
		string operationWord = typeSpec.OperationWord;

		using (writer.EnterRegion(code))
		{
			writer.WriteLine(
				$"static bool TryParseImpl(string s, ref {keyword} result, int radix, int startIndex, int length, string digits)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"{operationWord} word = 0;");
				writer.WriteLine("bool success = false;");
				writer.WriteLine("if (TryParseImpl(s, ref word, radix, startIndex, length, digits) &&");
				writer.WriteLine($"\tword >= {keyword}.MinValue && word <= {keyword}.MaxValue)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine($"result = ({keyword})word;");
					writer.WriteLine("success = true;");
				}
				writer.WriteLine();
				writer.WriteLine("return success;");
			}
			WriteTryParsePublicOverloads(writer, typeSpec, hasDoubleSpaceReturn: true);
		}
	}

	private static void WriteTryParseImplStart(SourceWriter writer)
	{
		writer.WriteLine("int pos = startIndex;");
		writer.WriteLine("int end = startIndex+length;");
		writer.WriteLine("bool success = true;");
		writer.WriteLine();
		writer.WriteLine("if (radix == 16)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if ((pos+2)<end && s[pos+0]=='0' && s[pos+1]=='x')");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("pos += 2;");
			}
		}
		writer.WriteLine();
		writer.WriteLine("// Skip any starting whitespace, avoids s.Trim() allocations");
		writer.WriteLine("while (pos < end && char.IsWhiteSpace(s[pos]))");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("++pos;");
		}
		writer.WriteLine();
		writer.WriteLine();
	}

	private static void WriteTryParseSignRead(SourceWriter writer, NumberSpec typeSpec)
	{
		if (!typeSpec.IsSigned)
		{
			writer.WriteLine();
			return;
		}

		writer.WriteLine("bool negate = false;");
		writer.WriteLine("// Sign support only exist for decimal and lower bases");
		writer.WriteLine("if (radix <= kBase10 && pos < end)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("char sign = s[pos];");
			writer.WriteLine();
			writer.WriteLine("negate = sign == '-';");
			writer.WriteLine("// Skip the sign character");
			writer.WriteLine("if (negate || sign == '+')");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("++pos;");
			}
		}
		writer.WriteLine();
	}

	private static void WriteTryParseDigitLoopBody(SourceWriter writer, string keyword)
	{
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("char digit = s[pos];");
			writer.WriteLine();
			writer.WriteLine();
			writer.WriteLine("int x = digits.IndexOf(digit);");
			writer.WriteLine("if (x >= 0 && x < radix)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("result *= radix_in_word;");
				writer.WriteLine($"result += ({keyword})x;");
			}
			writer.WriteLine("else // Character wasn't found in the look-up table, it is invalid");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("success = false;");
				writer.WriteLine("break;");
			}
		}
	}

	private static void WriteTryParseSignApply(SourceWriter writer, NumberSpec typeSpec)
	{
		if (!typeSpec.IsSigned)
		{
			writer.WriteLine();
			return;
		}

		writer.WriteLine();
		writer.WriteLine("// Negate the result if anything was processed");
		writer.WriteLine("if (negate)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("result = -result;");
		}
	}

	private static void WriteTryParsePublicOverloads(
		SourceWriter writer,
		NumberSpec typeSpec,
		bool hasDoubleSpaceReturn)
	{
		string keyword = typeSpec.Keyword;
		string returnPrefix = hasDoubleSpaceReturn ? "return  s" : "return s";

		writer.WriteLine(
			$"public static bool TryParse(string s, out {keyword} result, int radix = kBase10, " +
			"int startIndex = 0, string digits = kBase64Digits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentException.ThrowIfNullOrEmpty(digits);");
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfLessThan(radix, 2);");
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfGreaterThan(radix, digits.Length);");
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(startIndex);");
			writer.WriteLine("result = 0;");
			writer.WriteLine();
			writer.WriteLine($"{returnPrefix} != null && startIndex < s.Length &&");
			writer.WriteLine("\tTryParseImpl(s, ref result, radix, startIndex, s.Length, digits);");
		}
		writer.WriteLine(
			$"public static bool TryParse(string s, out {keyword} result, NumeralBase radix = NumeralBase.Decimal, " +
			"int startIndex = 0, string digits = kBase64Digits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentException.ThrowIfNullOrEmpty(digits);");
			writer.WriteLine(
				"if (!IsValidLookupTable(radix, digits)) { throw new ArgumentException(\"Invalid lookup table.\", nameof(digits)); }");
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(startIndex);");
			writer.WriteLine("result = 0;");
			writer.WriteLine();
			writer.WriteLine($"{returnPrefix} != null && startIndex < s.Length &&");
			writer.WriteLine("\tTryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);");
		}
		writer.WriteLine(
			$"public static bool TryParseRange(string s, out {keyword} result, int startIndex, int length, " +
			"NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentException.ThrowIfNullOrEmpty(digits);");
			writer.WriteLine(
				"if (!IsValidLookupTable(radix, digits)) { throw new ArgumentException(\"Invalid lookup table.\", nameof(digits)); }");
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(startIndex);");
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(length);");
			writer.WriteLine("result = 0;");
			writer.WriteLine();
			writer.WriteLine("return s != null && startIndex+length <= s.Length &&");
			writer.WriteLine("\tTryParseImpl(s, ref result, (int)radix, startIndex, length, digits);");
		}
	}

	private static void WriteParseStringRegion(SourceWriter writer, NumberSpec typeSpec)
	{
		string keyword = typeSpec.Keyword;
		string code = typeSpec.TypeCode.ToString();

		using (writer.EnterRegion($"ParseString {code}"))
		{
			writer.WriteLine(
				$"static bool ParseStringImpl(string s, ref {keyword} value, bool noThrow, int radix, int startIndex");
			writer.WriteLine("\t, Text.IHandleTextParseError parseErrorHandler)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("var result = string.IsNullOrEmpty(s)");
				writer.WriteLine("\t? ParseErrorType.NoInput");
				writer.WriteLine("\t: ParseErrorType.None;");
				writer.WriteLine();
				writer.WriteLine("if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("result = ParseErrorType.InvalidStartIndex;");
				}
				writer.WriteLine();
				writer.WriteLine("if (result == ParseErrorType.None)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("result = TryParse(s, out value, radix, startIndex, kBase64Digits)");
					writer.WriteLine("\t? ParseErrorType.None");
					writer.WriteLine("\t: ParseErrorType.InvalidValue;");
				}
				writer.WriteLine();
				writer.WriteLine("return HandleParseError(result, noThrow, s, startIndex, parseErrorHandler);");
			}
			writer.WriteLine($"public static bool ParseString(string s, ref {keyword} result, bool noThrow");
			writer.WriteLine(
				"\t, Text.IHandleTextParseError parseErrorHandler = null, " +
				"NumeralBase radix = NumeralBase.Decimal, int startIndex = 0)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine(
					"if (!IsValidLookupTable(radix, kBase64Digits)) { throw new ArgumentException(\"Invalid lookup table.\", nameof(radix)); }");
				writer.WriteLine();
				writer.WriteLine("return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, parseErrorHandler);");
			}
		}
	}

	private static void WriteTryParseListRegion(SourceWriter writer, NumberSpec typeSpec)
	{
		string code = typeSpec.TypeCode.ToString();
		string keyword = typeSpec.Keyword;
		string nullableKeyword = keyword + "?";
		string baseName = $"TryParse{code}ListBase";
		string asyncName = $"TryParse{code}ListAsync";
		string syncName = $"TryParse{code}List";

		using (writer.EnterRegion($"TryParse list {code}"))
		{
			writer.WriteLine($"abstract class {baseName}<TListItem>");
			writer.WriteLine("\t: TryParseNumberListBase<");
			writer.WriteLine($"\t\t\t{keyword},");
			writer.WriteLine("\t\t\tTListItem");
			writer.WriteLine("\t\t>");
			using (writer.EnterBlock(SourceWriterBlockType.BracesStatement))
			{
				writer.WriteLine($"protected {baseName}(StringListDesc desc, string values)");
				writer.WriteLine("\t: base(desc, values)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
				}
				writer.WriteLine();
				writer.WriteLine(
					$"protected override IEnumerable<{nullableKeyword}> EmptyResult => " +
					$"Array.Empty<{nullableKeyword}>();");
				writer.WriteLine();
				writer.WriteLine($"protected {nullableKeyword} ProcessItem(int start, int length)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine($"var result = ({keyword})0;");
					writer.WriteLine(
						"bool success = TryParseImpl(mValues, ref result, (int)mDesc.Radix, start, length, mDesc.Digits);");
					writer.WriteLine("return success");
					writer.WriteLine("\t? result");
					writer.WriteLine($"\t: ({nullableKeyword})null;");
				}
			}
			writer.WriteLine();
			WriteTryParseListAsync(writer, typeSpec, baseName, asyncName, nullableKeyword);
			writer.WriteLine();
			WriteTryParseListSync(writer, typeSpec, baseName, syncName, nullableKeyword);
		}
	}

	private static void WriteTryParseListAsync(
		SourceWriter writer,
		NumberSpec typeSpec,
		string baseName,
		string asyncName,
		string nullableKeyword)
	{
		string code = typeSpec.TypeCode.ToString();

		writer.WriteLine($"sealed class {asyncName}");
		writer.WriteLine($"\t: {baseName}< Task<{nullableKeyword}> >");
		using (writer.EnterBlock(SourceWriterBlockType.BracesStatement))
		{
			writer.WriteLine($"public {asyncName}(StringListDesc desc, string values)");
			writer.WriteLine("\t: base(desc, values)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
			}
			writer.WriteLine();
			writer.WriteLine($"static {nullableKeyword} ProcessItemAsync(object state)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"var args = (Tuple<{asyncName}, int, int>)state;");
				writer.WriteLine("var me = args.Item1;");
				writer.WriteLine("return me.ProcessItem(args.Item2, args.Item3);");
			}
			writer.WriteLine("protected override Task<" + nullableKeyword + "> CreateItem(int start, int length)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"return Task<{nullableKeyword}>.Factory.StartNew(");
				writer.WriteLine("\t\tProcessItemAsync,");
				writer.WriteLine($"\t\tnew Tuple<{asyncName}, int, int>(this, start, length)");
				writer.WriteLine("\t);");
			}
			writer.WriteLine();
			writer.WriteLine($"protected override IEnumerable<{nullableKeyword}> CreateResult()");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("return");
				writer.WriteLine("\tfrom task in mList");
				writer.WriteLine("\tselect task.Result;");
			}
		}
		writer.WriteLine(
			$"public static IEnumerable<{nullableKeyword}> TryParse{code}Async(StringListDesc desc, " +
			"string values)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine($"return new {asyncName}(desc, values).TryParse();");
		}
	}

	private static void WriteTryParseListSync(
		SourceWriter writer,
		NumberSpec typeSpec,
		string baseName,
		string syncName,
		string nullableKeyword)
	{
		string code = typeSpec.TypeCode.ToString();

		writer.WriteLine($"sealed class {syncName}");
		writer.WriteLine($"\t: {baseName}< {nullableKeyword} >");
		using (writer.EnterBlock(SourceWriterBlockType.BracesStatement))
		{
			writer.WriteLine($"public {syncName}(StringListDesc desc, string values)");
			writer.WriteLine("\t: base(desc, values)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
			}
			writer.WriteLine();
			writer.WriteLine($"protected override {nullableKeyword} CreateItem(int start, int length)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("return base.ProcessItem(start, length);");
			}
			writer.WriteLine();
			writer.WriteLine($"protected override IEnumerable<{nullableKeyword}> CreateResult()");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("return mList;");
			}
		}
		writer.WriteLine($"public static IEnumerable<{nullableKeyword}> TryParse{code}(StringListDesc desc, string values)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine($"return new {syncName}(desc, values).TryParse();");
		}
	}

	private static void WriteCharToByteLookupTable(SourceWriter writer, int radix, bool hexOnly, bool base36CaseFold)
	{
		writer.WriteXmlDocSummary("Latin-1 lookup table for converting char to a digit");
		writer.WriteLine($"/// <remarks>Supports up to base {radix}</remarks>");
		writer.WriteLine($"static readonly byte[] kCharToByteLookup{radix} = {{");
		WriteCharLookupRows(writer, isDigitTable: false, hexOnly, base36CaseFold);
		writer.WriteLine("};");
	}

	private static void WriteCharIsDigitLookupTable(SourceWriter writer, int radix, bool hexOnly)
	{
		writer.WriteXmlDocSummary("Latin-1 lookup table for testing if char is a digit");
		writer.WriteLine($"/// <remarks>Supports up to base {radix}</remarks>");
		writer.WriteLine($"static readonly bool[] kCharIsDigitLookup{radix} = {{");
		WriteCharLookupRows(writer, isDigitTable: true, hexOnly, base36CaseFold: false);
		writer.WriteLine("};");
	}

	private static void WriteCharLookupRows(
		SourceWriter writer,
		bool isDigitTable,
		bool hexOnly,
		bool base36CaseFold)
	{
		WriteCharLookupColumnHeader(writer, isDigitTable, 0, 16);

		int column = 0;
		int row = 0;
		int digitIndex = 0;
		var rowText = new System.Text.StringBuilder();
		for (int x = byte.MinValue; x <= byte.MaxValue; x++)
		{
			if (column == 0)
			{
				WriteCharLookupSpecialHeader(writer, isDigitTable, row);
				rowText.Clear();
				rowText.Append('\t');
			}

			if (CharLookupUsesCharByte(x, hexOnly))
			{
				if (base36CaseFold && (char)x == 'a')
				{
					digitIndex = ('9' - '0') + 1;
				}
				rowText.Append(CharLookupElementText(isDigitTable, digitIndex++));
			}
			else
			{
				rowText.Append(CharLookupElementText(isDigitTable, -1));
			}

			if (column++ == 15)
			{
				rowText.Append("// ");
				rowText.Append(row.ToString("X", PrimitiveCatalog.InvariantCulture));
				writer.WriteLine(rowText.ToString());
				column = 0;
				row++;
			}
		}
	}

	private static void WriteCharLookupSpecialHeader(SourceWriter writer, bool isDigitTable, int row)
	{
		if (row == 3)
		{
			WriteCharLookupColumnHeader(writer, isDigitTable, 0, 10);
		}
		else if (row == 4)
		{
			WriteCharLookupColumnHeader(writer, isDigitTable, 'A', 'O' + 1);
		}
		else if (row == 5)
		{
			WriteCharLookupColumnHeader(writer, isDigitTable, 'P', 'Z' + 1);
		}
		else if (row == 6)
		{
			WriteCharLookupColumnHeader(writer, isDigitTable, 'a', 'o' + 1);
		}
		else if (row == 7)
		{
			WriteCharLookupColumnHeader(writer, isDigitTable, 'p', 'z' + 1);
		}
	}

	private static void WriteCharLookupColumnHeader(SourceWriter writer, bool isDigitTable, int start, int end)
	{
		string tab = isDigitTable ? "\t\t" : "\t";
		var text = new System.Text.StringBuilder("//\t");
		for (int x = start; x < end; x++)
		{
			if (start == 0 && end == 16)
			{
				text.Append(x.ToString("X", PrimitiveCatalog.InvariantCulture));
			}
			else
			{
				text.Append(x < 10 ? x.ToString(PrimitiveCatalog.InvariantCulture) : ((char)x).ToString());
			}
			text.Append(tab);
		}
		writer.WriteUnindentedLine(text.ToString());
	}

	private static string CharLookupElementText(bool isDigitTable, int digitIndex)
	{
		if (isDigitTable)
		{
			return digitIndex >= 0 ? "true,\t" : "false,\t";
		}

		if (digitIndex < 16)
		{
			if (digitIndex == -1)
			{
				digitIndex = 0;
			}
			return "0x" + digitIndex.ToString("X", PrimitiveCatalog.InvariantCulture) + ",";
		}

		return digitIndex.ToString(PrimitiveCatalog.InvariantCulture) + ", ";
	}

	private static bool CharLookupUsesCharByte(int charByte, bool hexOnly)
	{
		return hexOnly
			? charByte >= '0' && charByte <= '9'
				|| charByte >= 'A' && charByte <= 'F'
				|| charByte >= 'a' && charByte <= 'f'
			: charByte >= '0' && charByte <= '9'
				|| charByte >= 'A' && charByte <= 'Z'
				|| charByte >= 'a' && charByte <= 'z';
	}
};
