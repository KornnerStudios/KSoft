using System.Text;

namespace KSoft.SourceGeneration.Text;

internal sealed class SourceWriter
{
	public const string IndentText = "\t";
	public const string NewLine = "\n";

	private readonly StringBuilder mBuilder = new();

	private int mIndentLevel;

	public int IndentLevel => mIndentLevel;

	public SourceWriterBlock EnterBlock(SourceWriterBlockType type = SourceWriterBlockType.NoBraces, int indentCount = 1)
	{
		ExceptionHelpers.ThrowIfNegative(indentCount, nameof(indentCount));

		if (type is SourceWriterBlockType.Braces or SourceWriterBlockType.BracesStatement)
		{
			WriteLine("{");
		}

		mIndentLevel += indentCount;

		return new SourceWriterBlock(this, type, indentCount);
	}

	public void WriteLine()
	{
		mBuilder.Append(NewLine);
	}

	public void WriteLine(string value)
	{
		ExceptionHelpers.ThrowIfNull(value, nameof(value));

		if (value.Length != 0)
		{
			AppendIndent();
		}

		mBuilder.Append(value);
		mBuilder.Append(NewLine);
	}

	public override string ToString()
	{
		return mBuilder.ToString();
	}

	internal void ExitBlock(SourceWriterBlockType type, int indentCount)
	{
		ExceptionHelpers.ThrowIfNegative(indentCount, nameof(indentCount));
		ExceptionHelpers.ThrowIfGreaterThan(indentCount, mIndentLevel, nameof(indentCount));

		mIndentLevel -= indentCount;

		if (type == SourceWriterBlockType.Braces)
		{
			WriteLine("}");
		}
		else if (type == SourceWriterBlockType.BracesStatement)
		{
			WriteLine("};");
		}
	}

	private void AppendIndent()
	{
		for (int x = 0; x < mIndentLevel; x++)
		{
			mBuilder.Append(IndentText);
		}
	}
};
