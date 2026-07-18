using System;

namespace KSoft.SourceGeneration.Text;

internal readonly struct SourceWriterBlock : IDisposable
{
	private readonly SourceWriter mWriter;
	private readonly SourceWriterBlockType mType;
	private readonly int mIndentCount;

	public SourceWriterBlock(SourceWriter writer, SourceWriterBlockType type, int indentCount)
	{
		ExceptionHelpers.ThrowIfNull(writer, nameof(writer));
		ExceptionHelpers.ThrowIfNegative(indentCount, nameof(indentCount));

		mWriter = writer;
		mType = type;
		mIndentCount = indentCount;
	}

	public void Dispose()
	{
		mWriter.ExitBlock(mType, mIndentCount);
	}
};
