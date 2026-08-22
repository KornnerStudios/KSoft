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

	static void AssertThrowsArgumentNull(Action action, string paramName)
	{
		var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

		Assert.AreEqual(paramName, exception.ParamName);
	}

	[TestMethod]
	public void Constructor_InvalidStream_Throws()
	{
		AssertThrowsArgumentNull(() => _ = new StreamPositionContext((Stream)null!), "baseStream");
		Assert.ThrowsExactly<InvalidOperationException>(() => _ = new StreamPositionContext(new NonSeekableMemoryStream()));
	}

	[TestMethod]
	public void Constructor_NullWrappedStreams_ThrowArgumentNullException()
	{
		AssertThrowsArgumentNull(() => _ = new StreamPositionContext((BinaryReader)null!), "stream");
		AssertThrowsArgumentNull(() => _ = new StreamPositionContext((BinaryWriter)null!), "stream");
		AssertThrowsArgumentNull(() => _ = new StreamPositionContext((StreamReader)null!), "stream");
		AssertThrowsArgumentNull(() => _ = new StreamPositionContext((StreamWriter)null!), "stream");
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
