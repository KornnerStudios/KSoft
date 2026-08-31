using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.IO.Test
{
	[TestClass]
	public sealed class StreamInterfaceContractTest : BaseTestClass
	{

		[TestMethod]
		public void VirtualBufferSettersRejectNegativeValuesTest()
		{
			using var stream = new EndianStream(new MemoryStream());

			AssertThrowsArgumentOutOfRange("value", () => stream.VirtualBufferStart = -1);
			AssertThrowsArgumentOutOfRange("value", () => stream.VirtualBufferLength = -1);
		}

		[TestMethod]
		public void StreamModeableSettersRejectReadWriteModeTest()
		{
			using var endianStream = new EndianStream(new MemoryStream());
			using var bitStream = new BitStream(new MemoryStream());

			AssertThrowsArgumentOutOfRange("value", () => endianStream.StreamMode = FileAccess.ReadWrite);
			AssertThrowsArgumentOutOfRange("value", () => endianStream.StreamMode = (FileAccess)4);
			AssertThrowsArgumentOutOfRange("value", () => bitStream.StreamMode = FileAccess.ReadWrite);
			AssertThrowsArgumentOutOfRange("value", () => bitStream.StreamMode = (FileAccess)4);
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
