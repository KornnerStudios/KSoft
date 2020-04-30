using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Bitwise.Test
{
	[TestClass]
	public class ByteSwapTest : BaseTestClass
	{
		const ulong kBeforeValue = 0x1234123412341234;
		const ulong kAfterValue = 0x3412341234123412;
		const ulong kSkipValue = 0x012345678ABCDEF0;

		const ulong kBeforeValueUInt40 = 0x123456789A;
		const ulong kAfterValueUInt40 = 0x9A78563412;

		const ulong kBeforeValueUInt24 = 0x123456;
		const ulong kAfterValueUInt24 = 0x563412;

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

			// UInt40
			value_before = kBeforeValueUInt40;
			value_after = kAfterValueUInt40;
			Assert.AreEqual(value_after, ByteSwap.SwapUInt40(value_before));
			Assert.AreEqual((long)value_after, ByteSwap.SwapInt40((long)value_before));

			// UInt24
			value_before = kBeforeValueUInt24;
			value_after = kAfterValueUInt24;
			Assert.AreEqual((uint)value_after, ByteSwap.SwapUInt24((uint)value_before));
			Assert.AreEqual((int)value_after,  ByteSwap.SwapInt24 ((int) value_before));
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

			// UInt40
			value = kBeforeValueUInt40;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytesUInt40(buffer, 0, value);
			buffer_bc = BitConverter.GetBytes(value);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

			// UInt32
			value >>= 32;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytes(buffer, 0, (uint)value);
			buffer_bc = BitConverter.GetBytes((uint)value);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

			// UInt24
			value = kBeforeValueUInt24;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytesUInt24(buffer, 0, (uint)value);
			buffer_bc = BitConverter.GetBytes((uint)value);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

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

			// UInt16
			value_before >>= 16;
			value_after >>= 16;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytes(buffer, 0, (ushort)value_before);
			ByteSwap.SwapInt16(buffer, 0);
			buffer_bc = BitConverter.GetBytes((ushort)value_after);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

			// UInt40
			value_before = kBeforeValueUInt40;
			value_after = kAfterValueUInt40;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytesUInt40(buffer, 0, value_before);
			ByteSwap.SwapInt40(buffer, 0);
			buffer_bc = BitConverter.GetBytes(value_after);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

			// UInt24
			value_before = kBeforeValueUInt24;
			value_after = kAfterValueUInt24;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytesUInt24(buffer, 0, (uint)value_before);
			ByteSwap.SwapInt24(buffer, 0);
			buffer_bc = BitConverter.GetBytes((uint)value_after);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));
		}

		// #NOTE Assumes ByteSwap.ReplaceBytes isn't broken
		[TestMethod]
		public void ByteSwap_SwapDataIntegersTest()
		{
			var buffer = new byte[sizeof(ulong) + sizeof(uint) + sizeof(ushort)];
			int buffer_index;

			buffer_index = 0;
			{
				ReplaceBytes(Bits.kInt64BitCount, kBeforeValue, buffer, ref buffer_index);
				ReplaceBytes(Bits.kInt32BitCount, kBeforeValue, buffer, ref buffer_index);
				ReplaceBytes(Bits.kInt16BitCount, kBeforeValue, buffer, ref buffer_index);
			}
			Assert.AreEqual(buffer.Length, buffer_index);

			buffer_index = 0;
			{
				buffer_index = ByteSwap.SwapData(ByteSwap.kInt64Definition, buffer, buffer_index);
				buffer_index = ByteSwap.SwapData(ByteSwap.kInt32Definition, buffer, buffer_index);
				buffer_index = ByteSwap.SwapData(ByteSwap.kInt16Definition, buffer, buffer_index);
			}
			Assert.AreEqual(buffer.Length, buffer_index);

			buffer_index = 0;
			{
				AssertBytesAreEqual(Bits.kInt64BitCount, kAfterValue, buffer, ref buffer_index);
				AssertBytesAreEqual(Bits.kInt32BitCount, kAfterValue, buffer, ref buffer_index);
				AssertBytesAreEqual(Bits.kInt16BitCount, kAfterValue, buffer, ref buffer_index);
			}
			Assert.AreEqual(buffer.Length, buffer_index);
		}

		[TestMethod]
		public void ByteSwap_SwapDataNestedArraysTest()
		{
			var bs_codes = new short[]
			{
				(int)BsCode.ArrayStart, 1,
					(int)BsCode.ArrayStart, 2,
						(int)BsCode.Int16,
					(int)BsCode.ArrayEnd,
					(int)BsCode.Int32,
					(int)sizeof(ulong),
					(int)BsCode.Int32,
				(int)BsCode.ArrayEnd,
			};
			int structure_count = 2;
			int structure_size =
				(sizeof(ushort) * 2) +
				sizeof(uint) +
				sizeof(ulong) +
				sizeof(uint);
			var bs_definiton = new ByteSwap.BsDefinition("UnitTest", structure_size, bs_codes);

			var buffer = new byte[structure_size * structure_count];
			int buffer_index;

			buffer_index = 0;
			for (int x = 0; x < structure_count; x++)
			{
				ReplaceBytes(Bits.kInt16BitCount, kBeforeValue, buffer, ref buffer_index);
				ReplaceBytes(Bits.kInt16BitCount, kBeforeValue, buffer, ref buffer_index);

				ReplaceBytes(Bits.kInt32BitCount, kBeforeValue, buffer, ref buffer_index);

				ReplaceBytes(Bits.kInt64BitCount, kSkipValue, buffer, ref buffer_index);

				ReplaceBytes(Bits.kInt32BitCount, kBeforeValue, buffer, ref buffer_index);
			}
			Assert.AreEqual(buffer.Length, buffer_index);

			buffer_index = 0;
			{
				buffer_index += ByteSwap.SwapData(bs_definiton, buffer, buffer_index, structure_count);
			}
			Assert.AreEqual(buffer.Length, buffer_index);

			buffer_index = 0;
			for (int x = 0; x < structure_count; x++)
			{
				AssertBytesAreEqual(Bits.kInt16BitCount, kAfterValue, buffer, ref buffer_index);
				AssertBytesAreEqual(Bits.kInt16BitCount, kAfterValue, buffer, ref buffer_index);

				AssertBytesAreEqual(Bits.kInt32BitCount, kAfterValue, buffer, ref buffer_index);

				AssertBytesAreEqual(Bits.kInt64BitCount, kSkipValue, buffer, ref buffer_index);

				AssertBytesAreEqual(Bits.kInt32BitCount, kAfterValue, buffer, ref buffer_index);
			}
			Assert.AreEqual(buffer.Length, buffer_index);
		}

		private static void ReplaceBytes(int bitCount, ulong value, byte[] buffer, ref int bufferIndex)
		{
			switch (bitCount)
			{
				case Bits.kInt16BitCount:
					bufferIndex = ByteSwap.ReplaceBytes(buffer, bufferIndex, unchecked((ushort)value));
					break;

				case Bits.kInt32BitCount:
					bufferIndex = ByteSwap.ReplaceBytes(buffer, bufferIndex, unchecked((uint)value));
					break;

				case Bits.kInt64BitCount:
					bufferIndex = ByteSwap.ReplaceBytes(buffer, bufferIndex, unchecked((ulong)value));
					break;
			}
		}

		private void AssertBytesAreEqual(int bitCount, ulong expectedValue, byte[] buffer, ref int bufferIndex)
		{
			var invariant_culture_info = KSoft.Util.InvariantCultureInfo;

			switch (bitCount)
			{
				case Bits.kInt16BitCount:
					Assert.AreEqual(
						unchecked((ushort)expectedValue).ToString("X4", invariant_culture_info),
						BitConverter.ToUInt16(buffer, bufferIndex).ToString("X4", invariant_culture_info));
					bufferIndex += sizeof(ushort);
					break;

				case Bits.kInt32BitCount:
					Assert.AreEqual(
						unchecked((uint)expectedValue).ToString("X8", invariant_culture_info),
						BitConverter.ToUInt32(buffer, bufferIndex).ToString("X8", invariant_culture_info));
					bufferIndex += sizeof(uint);
					break;

				case Bits.kInt64BitCount:
					Assert.AreEqual(
						unchecked((ulong)expectedValue).ToString("X16", invariant_culture_info),
						BitConverter.ToUInt64(buffer, bufferIndex).ToString("X16", invariant_culture_info));
					bufferIndex += sizeof(ulong);
					break;
			}
		}
	};
}
