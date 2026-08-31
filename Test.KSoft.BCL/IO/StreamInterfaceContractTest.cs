using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.IO.Test
{
	[TestClass]
	public sealed class StreamInterfaceContractTest : BaseTestClass
	{
		static void AssertThrowsArgumentOutOfRange(Action action, string paramName)
		{
			var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

			Assert.AreEqual(paramName, exception.ParamName);
		}
		static void AssertThrowsInvalidStreamMode(Action action)
		{
			var exception = Assert.ThrowsExactly<InvalidOperationException>(action);

			Assert.AreEqual("Stream doesn't support the requested access mode", exception.Message);
		}

		[TestMethod]
		public void VirtualBufferSettersRejectNegativeValuesTest()
		{
			using var stream = new EndianStream(new MemoryStream());

			AssertThrowsArgumentOutOfRange(() => stream.VirtualBufferStart = -1, "value");
			AssertThrowsArgumentOutOfRange(() => stream.VirtualBufferLength = -1, "value");
		}

		[TestMethod]
		public void StreamModeableSettersRejectReadWriteModeTest()
		{
			using var endianStream = new EndianStream(new MemoryStream());
			using var bitStream = new BitStream(new MemoryStream());

			AssertThrowsArgumentOutOfRange(() => endianStream.StreamMode = FileAccess.ReadWrite, "value");
			AssertThrowsArgumentOutOfRange(() => endianStream.StreamMode = (FileAccess)4, "value");
			AssertThrowsArgumentOutOfRange(() => bitStream.StreamMode = FileAccess.ReadWrite, "value");
			AssertThrowsArgumentOutOfRange(() => bitStream.StreamMode = (FileAccess)4, "value");
		}

		[TestMethod]
		public void StreamModeableSettersRejectUnsupportedPermissionsTest()
		{
			using var endianStream = new EndianStream(new MemoryStream(), FileAccess.Read);
			using var bitStream = new BitStream(new MemoryStream(), FileAccess.Read);

			AssertThrowsInvalidStreamMode(() => endianStream.StreamMode = FileAccess.Write);
			AssertThrowsInvalidStreamMode(() => bitStream.StreamMode = FileAccess.Write);
		}
	};
}
