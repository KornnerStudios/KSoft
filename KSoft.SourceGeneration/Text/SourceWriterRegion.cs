using System;

namespace KSoft.SourceGeneration.Text;

internal readonly struct SourceWriterRegion : IDisposable
{
	private readonly SourceWriter mWriter;

	public SourceWriterRegion(SourceWriter writer)
	{
		ExceptionHelpers.ThrowIfNull(writer, nameof(writer));

		mWriter = writer;
	}

	public void Dispose()
	{
		mWriter.ExitRegion();
	}
};
