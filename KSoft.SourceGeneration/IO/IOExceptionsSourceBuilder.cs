using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.IO;

internal static class IOExceptionsSourceBuilder
{
	public const string VersionMismatchHintName = "KSoft.IO.VersionMismatchException.g.cs";
	public const string SignatureMismatchHintName = "KSoft.IO.SignatureMismatchException.g.cs";

	public static string BuildVersionMismatch()
	{
		var writer = new SourceWriter();

		WriteFileHeader(writer);
		WriteVersionMismatchException(writer);
		writer.WriteLine();
		WriteVersionOutOfRangeException(writer);

		return writer.ToString();
	}

	public static string BuildSignatureMismatch()
	{
		var writer = new SourceWriter();

		WriteFileHeader(writer);
		using (writer.EnterTypeDeclaration("partial class SignatureMismatchException"))
		{
			WriteSignatureStreamConstructors(writer);
			writer.WriteLine();
			WriteSignatureEndianReaderAsserts(writer);
		}

		return writer.ToString();
	}

	private static void WriteFileHeader(SourceWriter writer)
	{
		writer.WriteGeneratedFileHeader();
		writer.WriteLine("#nullable disable");
		writer.WriteLine();
		writer.WriteLine("using System;");
		writer.WriteLine("using System.IO;");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft.IO");
		writer.WriteLine();
	}

	private static void WriteVersionMismatchException(SourceWriter writer)
	{
		using (writer.EnterTypeDeclaration("partial class VersionMismatchException"))
		{
			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesInt32)
			{
				WriteVersionMismatchDescriptionConstructors(writer, spec);
			}
			writer.WriteLine();
			WriteVersionMismatchStreamConstructors(writer);
			writer.WriteLine();
			WriteVersionMismatchEndianReaderAsserts(writer);
		}
	}

	private static void WriteVersionOutOfRangeException(SourceWriter writer)
	{
		using (writer.EnterTypeDeclaration("partial class VersionOutOfRangeException"))
		{
			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesInt32)
			{
				WriteVersionOutOfRangeDescriptionConstructor(writer, spec);
			}
			writer.WriteLine();
			WriteVersionOutOfRangeStreamConstructors(writer);
			writer.WriteLine();
			WriteVersionOutOfRangeEndianReaderAsserts(writer);
		}
	}

	private static void WriteVersionMismatchDescriptionConstructors(SourceWriter writer, NumberSpec spec)
	{
		using (writer.EnterRegion($"{spec.TypeCode} ctors"))
		{
			writer.WriteLine(
				$"public VersionMismatchException(string dataDescription, {spec.Keyword} found)");
			writer.WriteLine(
				"\t: base(string.Format(Util.InvariantCultureInfo, \"Invalid '{0}' version '{1}'!\", " +
				"dataDescription, found))");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("ArgumentException.ThrowIfNullOrEmpty(dataDescription);");
			}
			writer.WriteLine(
				$"public VersionMismatchException(string dataDescription, {spec.Keyword} expected, " +
				$"{spec.Keyword} found)");
			writer.WriteLine(
				"\t: base(string.Format(Util.InvariantCultureInfo, kDescFormat, dataDescription, expected, " +
				"found, VersionCompareDesc(expected, found)))");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("ArgumentException.ThrowIfNullOrEmpty(dataDescription);");
			}
		}
	}

	private static void WriteVersionMismatchStreamConstructors(SourceWriter writer)
	{
		using (writer.EnterRegion("Stream ctors"))
		{
			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				writer.WriteLine(
					$"public VersionMismatchException(Stream s, {spec.Keyword} expected, {spec.Keyword} found) :");
				writer.WriteLine(
					$"\tthis(s.Position - {spec.SizeOfInBytes}, VersionCompareDesc(expected, found), " +
					FormatToStringCall(spec, "expected") + ", " + FormatToStringCall(spec, "found") + ")");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("ArgumentNullException.ThrowIfNull(s);");
				}
				writer.WriteLine();
			}
		}
	}

	private static void WriteVersionMismatchEndianReaderAsserts(SourceWriter writer)
	{
		using (writer.EnterRegion("EndianReader util"))
		{
			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				writer.WriteLine($"public static void Assert(IO.EndianReader s, {spec.Keyword} expected)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					WriteEndianReaderAssertBody(writer, spec, "VersionMismatchException", hasRange: false);
				}
				writer.WriteLine();
			}
		}
	}

	private static void WriteVersionOutOfRangeDescriptionConstructor(SourceWriter writer, NumberSpec spec)
	{
		using (writer.EnterRegion($"{spec.TypeCode} ctors"))
		{
			writer.WriteLine("public VersionOutOfRangeException(string dataDescription");
			writer.WriteLine($"\t, {spec.Keyword} expectedMin");
			writer.WriteLine($"\t, {spec.Keyword} expectedMax");
			writer.WriteLine($"\t, {spec.Keyword} found)");
			writer.WriteLine(
				"\t: base(string.Format(Util.InvariantCultureInfo, kDescFormat, dataDescription, expectedMin, " +
				"expectedMax, found, VersionCompareDesc(expectedMin, expectedMax, found)))");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("ArgumentException.ThrowIfNullOrEmpty(dataDescription);");
			}
		}
	}

	private static void WriteVersionOutOfRangeStreamConstructors(SourceWriter writer)
	{
		using (writer.EnterRegion("Stream ctors"))
		{
			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				writer.WriteLine("public VersionOutOfRangeException(Stream s");
				writer.WriteLine($"\t, {spec.Keyword} expectedMin");
				writer.WriteLine($"\t, {spec.Keyword} expectedMax");
				writer.WriteLine($"\t, {spec.Keyword} found)");
				writer.WriteLine(
					$"\t: this(s.Position - {spec.SizeOfInBytes}, VersionCompareDesc(expectedMin, expectedMax, " +
					"found), " + FormatToStringCall(spec, "expectedMin") + ", " +
					FormatToStringCall(spec, "expectedMax") + ", " + FormatToStringCall(spec, "found") + ")");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("ArgumentNullException.ThrowIfNull(s);");
				}
				writer.WriteLine();
			}
		}
	}

	private static void WriteVersionOutOfRangeEndianReaderAsserts(SourceWriter writer)
	{
		using (writer.EnterRegion("EndianReader util"))
		{
			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				writer.WriteLine($"public static {spec.Keyword} Assert(IO.EndianReader s");
				writer.WriteLine($"\t, {spec.Keyword} expectedMin");
				writer.WriteLine($"\t, {spec.Keyword} expectedMax)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					WriteEndianReaderAssertBody(writer, spec, "VersionOutOfRangeException", hasRange: true);
				}
				writer.WriteLine();
			}
		}
	}

	private static void WriteSignatureStreamConstructors(SourceWriter writer)
	{
		using (writer.EnterRegion("Stream ctors"))
		{
			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				string expectedText = FormatToStringCall(spec, "expected");
				string foundText = FormatToStringCall(spec, "found");

				writer.WriteLine(
					$"public SignatureMismatchException(Stream s, {spec.Keyword} expected, {spec.Keyword} found) :");
				writer.WriteLine(
					$"\tthis(s.Position - {spec.SizeOfInBytes},");
				writer.WriteLine($"\t\t{expectedText},");
				writer.WriteLine($"\t\t{foundText})");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("ArgumentNullException.ThrowIfNull(s);");
				}
				writer.WriteLine();
			}
		}
	}

	private static void WriteSignatureEndianReaderAsserts(SourceWriter writer)
	{
		using (writer.EnterRegion("EndianReader util"))
		{
			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				writer.WriteLine($"public static void Assert(IO.EndianReader s, {spec.Keyword} expected)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					WriteEndianReaderAssertBody(writer, spec, "SignatureMismatchException", hasRange: false);
				}
				writer.WriteLine();
			}
		}
	}

	private static void WriteEndianReaderAssertBody(
		SourceWriter writer,
		NumberSpec spec,
		string exceptionType,
		bool hasRange)
	{
		writer.WriteLine("ArgumentNullException.ThrowIfNull(s);");
		writer.WriteLine();
		writer.WriteLine($"var version = s.Read{spec.TypeCode}();");
		if (hasRange)
		{
			writer.WriteLine("if (version < expectedMin || version > expectedMax)");
		}
		else
		{
			writer.WriteLine("if (version != expected)");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			string arguments = hasRange
				? "expectedMin, expectedMax, version"
				: "expected, version";
			writer.WriteLine($"throw new {exceptionType}(s.BaseStream, {arguments});");
		}
		if (hasRange)
		{
			writer.WriteLine();
			writer.WriteLine("return version;");
		}
	}

	private static string FormatToStringCall(NumberSpec spec, string valueExpression)
		=> $"{valueExpression}.ToString(\"{spec.ToStringHexFormat}\", Util.InvariantCultureInfo)";
}
