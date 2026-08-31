using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.IO.Test;

[TestClass]
public sealed class StreamPositionContextTest : BaseTestClass
{
	sealed class NonSeekableMemoryStream : MemoryStream
	{
		public override bool CanSeek => false;
	}

	[TestMethod]
	public void Constructor_InvalidStream_Throws()
	{
		AssertThrowsArgumentNull("baseStream", () => _ = new StreamPositionContext((Stream)null!));
		Assert.ThrowsExactly<InvalidOperationException>(() => _ = new StreamPositionContext(new NonSeekableMemoryStream()));
	}

	[TestMethod]
	public void Constructor_NullWrappedStreams_ThrowArgumentNullException()
	{
		AssertThrowsArgumentNull("stream", () => _ = new StreamPositionContext((BinaryReader)null!));
		AssertThrowsArgumentNull("stream", () => _ = new StreamPositionContext((BinaryWriter)null!));
		AssertThrowsArgumentNull("stream", () => _ = new StreamPositionContext((StreamReader)null!));
		AssertThrowsArgumentNull("stream", () => _ = new StreamPositionContext((StreamWriter)null!));
	}

	[TestMethod]
	public void Dispose_RestoresOriginalStreamPosition()
	{
		using var stream = new MemoryStream(new byte[8]);
		stream.Position = 3;

		using (new StreamPositionContext(stream))
		{
			stream.Position = 7;
		}

		Assert.AreEqual(3, stream.Position);
	}

	[TestMethod]
	public void DefaultDispose_DoesNotThrow()
	{
		default(StreamPositionContext).Dispose();
	}
}
