using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Bitwise.Test
{
	[TestClass]
	public class ByteSwapTest : BaseTestClass
	{
		const ulong kBeforeValue = 0x1234123412341234;
		const ulong kAfterValue = 0x3412341234123412;

		[TestMethod]
		public void ByteSwap_SwapIntegersTest()
		{
			ulong value_before = kBeforeValue;
			ulong value_after = kAfterValue;

			// UInt64
			Assert.AreEqual(value_after, ByteSwap.SwapUInt64(value_before));
			Assert.AreEqual((long)value_after, ByteSwap.SwapInt64((long)value_before));

			// UInt32
			value_before >>= 32;
			value_after >>= 32;
			Assert.AreEqual((uint)value_after, ByteSwap.SwapUInt32((uint)value_before));
			Assert.AreEqual((int)value_after,  ByteSwap.SwapInt32 ((int) value_before));

			// UInt16
			value_before >>= 16;
			value_after >>= 16;
			Assert.AreEqual((ushort)value_after, ByteSwap.SwapUInt16((ushort)value_before));
			Assert.AreEqual((short)value_after,  ByteSwap.SwapInt16 ((short) value_before));

			value_before = 0x123456;
			value_after = 0x563412;
			Assert.AreEqual((uint)value_after, ByteSwap.SwapUInt24((uint)value_before));

			value_before = 0x123456789A;
			value_after = 0x9A78563412;
			Assert.AreEqual(value_after, ByteSwap.SwapUInt40(value_before));
		}

		[TestMethod]
		public void ByteSwap_ReplaceBytesTest()
		{
			byte[] buffer = new byte[sizeof(ulong)];
			byte[] buffer_bc = null;
			ulong value = kBeforeValue;

			// UInt64
			ByteSwap.ReplaceBytes(buffer, 0, value);
			buffer_bc = BitConverter.GetBytes(value);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

			// UInt32
			value >>= 32;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytes(buffer, 0, (uint)value);
			buffer_bc = BitConverter.GetBytes((uint)value);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

			// TODO: UInt24

			// UInt16
			value >>= 16;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytes(buffer, 0, (ushort)value);
			buffer_bc = BitConverter.GetBytes((ushort)value);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));
		}
		// NOTE: ByteSwap_ReplaceBytesTest should be tested before SwapBufferTest (see OrderedTests_ByteSwap)
		[TestMethod]
		public void ByteSwap_SwapBufferTest()
		{
			byte[] buffer = new byte[sizeof(ulong)];
			byte[] buffer_bc = null;
			ulong value_before = kBeforeValue;
			ulong value_after = kAfterValue;

			// UInt64
			ByteSwap.ReplaceBytes(buffer, 0, value_before);
			ByteSwap.SwapInt64(buffer, 0);
			buffer_bc = BitConverter.GetBytes(value_after);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

			// UInt32
			value_before >>= 32;
			value_after >>= 32;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytes(buffer, 0, (uint)value_before);
			ByteSwap.SwapInt32(buffer, 0);
			buffer_bc = BitConverter.GetBytes((uint)value_after);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

			// TODO: UInt24

			// UInt16
			value_before >>= 16;
			value_after >>= 16;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytes(buffer, 0, (ushort)value_before);
			ByteSwap.SwapInt16(buffer, 0);
			buffer_bc = BitConverter.GetBytes((ushort)value_after);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));
		}
	};
}