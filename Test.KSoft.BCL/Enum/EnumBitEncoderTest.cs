using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Test
{
	/// <summary>This is a test class for EnumBitEncoder32\64 and is intended to contain all EnumBitEncoder32\64 Unit Tests</summary>
	[TestClass]
	public class EnumBitEncoderTest : BaseTestClass
	{
		#region Enumerations
		enum EnumTest
		{
			Member0,
			Member1,
			Member2,
			Member3,
			Member4,

			kMax,
		};
		enum EnumTestInformal // for testing enums which don't employ the kMax member
		{
			Member0,
			Member1,
			Member2,
			Member3,
			Member4,
		};
		enum EnumTestWithNone
		{
			None = -1,

			Member0,
			Member1,
			Member2,
			Member3,
			Member4,

			kNumberOf,
		};
		enum EnumByte : byte
		{
			Value = 0xAB,
		};
		enum EnumSByte : sbyte
		{
			Value = 0x7E,
		};
		enum EnumUInt16 : ushort
		{
			Value = 0x1234,
		};
		enum EnumInt16 : short
		{
			Value = 0x1234,
		};
		enum EnumUInt32 : uint
		{
			Value = 0x89ABCDEF,
		};
		enum EnumInt32 : int
		{
			Value = 0x12345678,
		};
		enum EnumUInt64 : ulong
		{
			Value = 0x0123456789ABCDEFUL,
		};
		enum EnumInt64 : long
		{
			Value = 0x1122334455667788L,
		};

		[System.Flags]
		enum FlagsTest
		{
			Member0 = 1 << 0,
			Member1 = 1 << 1,
			Member2 = 1 << 2,
			Member3 = 1 << 3,
			Member4 = 1 << 4,
			Member5 = 1 << 5,

			kAll = Member0 | Member1 | Member2 | Member3 | Member4 | Member5
		};
		[System.Flags]
		enum FlagsTestInformal // for testing enums which don't employ the kAll member
		{
			Member0 = 1 << 0,
			Member1 = 1 << 1,
			Member2 = 1 << 2,
			Member3 = 1 << 3,
			Member4 = 1 << 4,
			Member5 = 1 << 5,
		};
		#endregion

		static void AssertArgumentNull(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		static void AssertArgumentOutOfRange(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		static void AssertArgument(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		#region 32-bit tests
		[SuppressMessage("Microsoft.Design", "CA1806:DoNotIgnoreMethodResults",
			Justification = "Pretty sure this is a CA bug",
			Scope = "method", Target = "BitEncode")]
		void Test32Helper<TEnum, TEnumInformal>(TEnum value)
			where TEnum : struct, Enum
			where TEnumInformal : struct, Enum
		{
			var ebe_formal = new EnumBitEncoder32<TEnum>();
			var ebe_informal = new EnumBitEncoder32<TEnumInformal>();
			// Test that informal calculations correlate with that taken from
			// formal enum types (which use a konstant member)
			Assert.AreEqual(ebe_formal.BitCountTrait, ebe_informal.BitCountTrait);
			Assert.AreEqual(ebe_formal.BitmaskTrait, ebe_informal.BitmaskTrait);

			Assert.IsFalse(ebe_formal.HasNone);
			Assert.IsFalse(ebe_informal.HasNone);

			uint bits;
			int bit_index;

			{ // Test by-value encoding
				bits = 0; bit_index = 0;
				bits = ebe_formal.BitEncode(value, bits, bit_index);
				Assert.AreEqual(value, ebe_formal.BitDecode(bits, bit_index));
			}
			{ // Test by-reference encoding
				bits = 0; bit_index = 0;
				ebe_formal.BitEncode(value, ref bits, ref bit_index);
				// Test if anything was actually encoded
				Assert.AreNotEqual(0u, bits);
				// Test for the proper increment of the cursor
				Assert.AreEqual(EnumBitEncoder32<TEnum>.kBitCount, bit_index);

				bit_index = 0;
				// Test for the proper decoding of the enum value
				Assert.AreEqual(value, ebe_formal.BitDecode(bits, ref bit_index));
				// Test for the proper increment of the cursor
				Assert.AreEqual(EnumBitEncoder32<TEnum>.kBitCount, bit_index);
			}

			const int bit_index_offset = 5;
			{ // Test indexed by-value encoding
				bits = 0; bit_index = bit_index_offset;
				bits = ebe_formal.BitEncode(value, bits, bit_index);
				Assert.AreEqual(value, ebe_formal.BitDecode(bits, bit_index));
			}
			{ // Test indexed by-reference encoding
				bits = 0; bit_index = bit_index_offset;
				ebe_formal.BitEncode(value, ref bits, ref bit_index);
				// Test if anything was actually encoded
				Assert.AreNotEqual(0u, bits);
				// Test for the proper increment of the cursor
				Assert.AreEqual(bit_index_offset + EnumBitEncoder32<TEnum>.kBitCount, bit_index);

				bit_index = bit_index_offset;
				// Test for the proper decoding of the enum value
				Assert.AreEqual(value, ebe_formal.BitDecode(bits, ref bit_index));
				// Test for the proper increment of the cursor
				Assert.AreEqual(bit_index_offset + EnumBitEncoder32<TEnum>.kBitCount, bit_index);
			}
		}
		void TestNone32Helper<TEnum>(TEnum value, TEnum noneValue)
			where TEnum : struct, Enum
		{
			var ebe = new EnumBitEncoder32<TEnum>();

			Assert.IsTrue(ebe.HasNone);

			uint bits;
			int bit_index;

			{ // Test by-value encoding
				bits = 0; bit_index = 0;
				bits = ebe.BitEncode(value, bits, bit_index);
				Assert.AreEqual(value, ebe.BitDecode(bits, bit_index));
			}

			const int bit_index_offset = 5;
			{ // Test indexed by-value encoding
				bits = 0; bit_index = bit_index_offset;
				bits = ebe.BitEncode(value, bits, bit_index);
				Assert.AreEqual(value, ebe.BitDecode(bits, bit_index));
			}

			{ // Test by-value encoding
				bits = 0; bit_index = 0;
				bits = ebe.BitEncode(noneValue, bits, bit_index);
				Assert.AreEqual(noneValue, ebe.BitDecode(bits, bit_index));
			}
		}

		[TestMethod]
		public void BitEncoder32Test()
		{
			Test32Helper<EnumTest, EnumTestInformal>(EnumTest.Member3);
			Test32Helper<FlagsTest, FlagsTestInformal>(FlagsTest.Member3 | FlagsTest.Member4);
			TestNone32Helper(EnumTestWithNone.Member3, EnumTestWithNone.None);
		}

		[TestMethod]
		public void BitEncoder32InvalidBitIndexThrowsExpectedExceptions()
		{
			var encoder = new EnumBitEncoder32<EnumTest>();
			int overflowIndex = Bits.kInt64BitCount - EnumBitEncoder32<EnumTest>.kBitCount;

			AssertArgumentOutOfRange("bitIndex", () =>
			{
				ulong bits = 0;
				int bitIndex = -1;
				encoder.BitEncode(EnumTest.Member0, ref bits, ref bitIndex);
			});
			AssertArgumentOutOfRange("bitIndex", () =>
			{
				ulong bits = 0;
				int bitIndex = Bits.kInt64BitCount;
				encoder.BitEncode(EnumTest.Member0, ref bits, ref bitIndex);
			});
			AssertArgumentOutOfRange("bitIndex", () =>
			{
				ulong bits = 0;
				int bitIndex = overflowIndex;
				encoder.BitEncode(EnumTest.Member0, ref bits, ref bitIndex);
			});
			AssertArgumentOutOfRange("bitIndex", () =>
			{
				int bitIndex = -1;
				_ = encoder.BitDecode(0UL, ref bitIndex);
			});
			AssertArgumentOutOfRange("bitIndex", () =>
			{
				int bitIndex = Bits.kInt64BitCount;
				_ = encoder.BitDecode(0UL, ref bitIndex);
			});
			AssertArgumentOutOfRange("bitIndex", () =>
			{
				int bitIndex = overflowIndex;
				_ = encoder.BitDecode(0UL, ref bitIndex);
			});
		}
		#endregion

		#region 64-bit tests
		[SuppressMessage("Microsoft.Design", "CA1806:DoNotIgnoreMethodResults",
			Justification = "Pretty sure this is a CA bug",
			Scope = "method", Target = "BitEncode")]
		static void Test64Helper<TEnum, TEnumInformal>(TEnum value)
			where TEnum : struct, Enum
			where TEnumInformal : struct, Enum
		{
			var ebe_formal = new EnumBitEncoder64<TEnum>();
			var ebe_informal = new EnumBitEncoder64<TEnumInformal>();
			// Test that informal calculations correlate with that taken from
			// formal enum types (which use a konstant member)
			Assert.AreEqual(ebe_formal.BitCountTrait, ebe_informal.BitCountTrait);
			Assert.AreEqual(ebe_formal.BitmaskTrait, ebe_informal.BitmaskTrait);

			Assert.IsFalse(ebe_formal.HasNone);
			Assert.IsFalse(ebe_informal.HasNone);

			ulong bits;
			int bit_index;

			{ // Test by-value encoding
				bits = 0; bit_index = 0;
				bits = ebe_formal.BitEncode(value, bits, bit_index);
				Assert.AreEqual(value, ebe_formal.BitDecode(bits, bit_index));
			}
			{ // Test by-reference encoding
				bits = 0; bit_index = 0;
				ebe_formal.BitEncode(value, ref bits, ref bit_index);
				// Test if anything was actually encoded
				Assert.AreNotEqual(0u, bits);
				// Test for the proper increment of the cursor
				Assert.AreEqual(EnumBitEncoder64<TEnum>.kBitCount, bit_index);

				bit_index = 0;
				// Test for the proper decoding of the enum value
				Assert.AreEqual(value, ebe_formal.BitDecode(bits, ref bit_index));
				// Test for the proper increment of the cursor
				Assert.AreEqual(EnumBitEncoder64<TEnum>.kBitCount, bit_index);
			}

			const int bit_index_offset = 5;
			{ // Test indexed by-value encoding
				bits = 0; bit_index = bit_index_offset;
				bits = ebe_formal.BitEncode(value, bits, bit_index);
				Assert.AreEqual(value, ebe_formal.BitDecode(bits, bit_index));
			}
			{ // Test indexed by-reference encoding
				bits = 0; bit_index = bit_index_offset;
				ebe_formal.BitEncode(value, ref bits, ref bit_index);
				// Test if anything was actually encoded
				Assert.AreNotEqual(0u, bits);
				// Test for the proper increment of the cursor
				Assert.AreEqual(bit_index_offset + EnumBitEncoder64<TEnum>.kBitCount, bit_index);

				bit_index = bit_index_offset;
				// Test for the proper decoding of the enum value
				Assert.AreEqual(value, ebe_formal.BitDecode(bits, ref bit_index));
				// Test for the proper increment of the cursor
				Assert.AreEqual(bit_index_offset + EnumBitEncoder64<TEnum>.kBitCount, bit_index);
			}
		}
		void TestNone64Helper<TEnum>(TEnum value, TEnum noneValue)
			where TEnum : struct, Enum
		{
			var ebe = new EnumBitEncoder64<TEnum>();

			Assert.IsTrue(ebe.HasNone);

			ulong bits;
			int bit_index;

			{ // Test by-value encoding
				bits = 0; bit_index = 0;
				bits = ebe.BitEncode(value, bits, bit_index);
				Assert.AreEqual(value, ebe.BitDecode(bits, bit_index));
			}

			const int bit_index_offset = 5;
			{ // Test indexed by-value encoding
				bits = 0; bit_index = bit_index_offset;
				bits = ebe.BitEncode(value, bits, bit_index);
				Assert.AreEqual(value, ebe.BitDecode(bits, bit_index));
			}

			{ // Test by-value encoding
				bits = 0; bit_index = 0;
				bits = ebe.BitEncode(noneValue, bits, bit_index);
				Assert.AreEqual(noneValue, ebe.BitDecode(bits, bit_index));
			}
		}

		[TestMethod]
		public void BitEncoder64Test()
		{
			Test64Helper<EnumTest, EnumTestInformal>(EnumTest.Member3);
			Test64Helper<FlagsTest, FlagsTestInformal>(FlagsTest.Member3 | FlagsTest.Member4);
			TestNone64Helper(EnumTestWithNone.Member3, EnumTestWithNone.None);
		}
		#endregion

		[TestMethod]
		public void BitEncoder32EndianStreamingUsesUnderlyingTypeTest()
		{
			using var ms = new MemoryStream();
			using (var writer = new IO.EndianWriter(ms, Shell.EndianFormat.Big) { BaseStreamOwner = false })
			{
				EnumBitEncoder32<EnumByte>.Write(writer, EnumByte.Value);
				EnumBitEncoder32<EnumSByte>.Write(writer, EnumSByte.Value);
				EnumBitEncoder32<EnumUInt16>.Write(writer, EnumUInt16.Value);
				EnumBitEncoder32<EnumInt16>.Write(writer, EnumInt16.Value);
				EnumBitEncoder32<EnumUInt32>.Write(writer, EnumUInt32.Value);
				EnumBitEncoder32<EnumInt32>.Write(writer, EnumInt32.Value);
			}

			CollectionAssert.AreEqual(
				new byte[] {
					0xAB,
					0x7E,
					0x12, 0x34,
					0x12, 0x34,
					0x89, 0xAB, 0xCD, 0xEF,
					0x12, 0x34, 0x56, 0x78,
				},
				ms.ToArray());

			ms.Position = 0;
			using var reader = new IO.EndianReader(ms, Shell.EndianFormat.Big);

			EnumBitEncoder32<EnumByte>.Read(reader, out EnumByte byteValue);
			EnumBitEncoder32<EnumSByte>.Read(reader, out EnumSByte sbyteValue);
			EnumBitEncoder32<EnumUInt16>.Read(reader, out EnumUInt16 ushortValue);
			EnumBitEncoder32<EnumInt16>.Read(reader, out EnumInt16 shortValue);
			EnumBitEncoder32<EnumUInt32>.Read(reader, out EnumUInt32 uintValue);
			EnumBitEncoder32<EnumInt32>.Read(reader, out EnumInt32 intValue);

			Assert.AreEqual(EnumByte.Value, byteValue);
			Assert.AreEqual(EnumSByte.Value, sbyteValue);
			Assert.AreEqual(EnumUInt16.Value, ushortValue);
			Assert.AreEqual(EnumInt16.Value, shortValue);
			Assert.AreEqual(EnumUInt32.Value, uintValue);
			Assert.AreEqual(EnumInt32.Value, intValue);
		}

		[TestMethod]
		public void BitEncoder64EndianStreamingUsesUnderlyingTypeTest()
		{
			using var ms = new MemoryStream();
			using (var writer = new IO.EndianWriter(ms, Shell.EndianFormat.Little) { BaseStreamOwner = false })
			{
				EnumBitEncoder64<EnumUInt64>.Write(writer, EnumUInt64.Value);
				EnumBitEncoder64<EnumInt64>.Write(writer, EnumInt64.Value);
			}

			CollectionAssert.AreEqual(
				new byte[] {
					0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01,
					0x88, 0x77, 0x66, 0x55, 0x44, 0x33, 0x22, 0x11,
				},
				ms.ToArray());

			ms.Position = 0;
			using var reader = new IO.EndianReader(ms, Shell.EndianFormat.Little);

			EnumBitEncoder64<EnumUInt64>.Read(reader, out EnumUInt64 ulongValue);
			EnumBitEncoder64<EnumInt64>.Read(reader, out EnumInt64 longValue);

			Assert.AreEqual(EnumUInt64.Value, ulongValue);
			Assert.AreEqual(EnumInt64.Value, longValue);
		}

		[TestMethod]
		[SuppressMessage("Microsoft.Design", "CA1806:DoNotIgnoreMethodResults",
			Justification ="Pretty sure this is a CA bug",
			Scope = "method", Target = "BitEncode")]
		public void BitEncoderHashCode32Test()
		{
			var ss = new Memory.Strings.StringStorage(
				Memory.Strings.StringStorageWidthType.UTF32,
				Memory.Strings.StringStorageType.CharArray,
				Shell.EndianFormat.Big, 16);
			uint bits = 0;
			int bit_index = 0;

			TypeExtensions.BitEncoders.StringStorageWidthType.BitEncode(ss.WidthType, ref bits, ref bit_index);
			TypeExtensions.BitEncoders.StringStorageType.BitEncode(ss.Type, ref bits, ref bit_index);
			TypeExtensions.BitEncoders.EndianFormat.BitEncode(ss.ByteOrder, ref bits, ref bit_index);
			Bits.BitEncodeEnum((uint)ss.FixedLength, ref bits, ref bit_index, 0x7FFF);

			bits = (uint)ss.GetHashCode();
			bit_index = 0;
			var widthType = TypeExtensions.BitEncoders.StringStorageWidthType.BitDecode(bits, ref bit_index);
			var type = TypeExtensions.BitEncoders.StringStorageType.BitDecode(bits, ref bit_index);
			var byteOrder = TypeExtensions.BitEncoders.EndianFormat.BitDecode(bits, ref bit_index);
			var fixedLength = (short)Bits.BitDecode(bits, ref bit_index, 0x7FFF);

			Assert.AreEqual(ss.WidthType, widthType);
			Assert.AreEqual(ss.Type, type);
			Assert.AreEqual(ss.ByteOrder, byteOrder);
			Assert.AreEqual(ss.FixedLength, fixedLength);
		}

		[TestMethod]
		public void HandleBitEncoder32Test()
		{
			var ss = new Memory.Strings.StringStorage(
				Memory.Strings.StringStorageWidthType.UTF32,
				Memory.Strings.StringStorageType.CharArray,
				Shell.EndianFormat.Big, 16);
			var encoder = new Bitwise.HandleBitEncoder();

			encoder.Encode32(ss.WidthType, TypeExtensions.BitEncoders.StringStorageWidthType);
			encoder.Encode32(ss.Type, TypeExtensions.BitEncoders.StringStorageType);
			encoder.Encode32(ss.ByteOrder, TypeExtensions.BitEncoders.EndianFormat);
			encoder.Encode32((uint)ss.FixedLength, 0x7FFF);

			var decoder = new Bitwise.HandleBitEncoder(encoder.GetHandle32());
			decoder.Decode32(out Memory.Strings.StringStorageWidthType widthType, TypeExtensions.BitEncoders.StringStorageWidthType);
			decoder.Decode32(out Memory.Strings.StringStorageType type, TypeExtensions.BitEncoders.StringStorageType);
			decoder.Decode32(out Shell.EndianFormat byteOrder, TypeExtensions.BitEncoders.EndianFormat);
			decoder.Decode32(out uint fixedLength, 0x7FFF);

			Assert.AreEqual(ss.WidthType, widthType);
			Assert.AreEqual(ss.Type, type);
			Assert.AreEqual(ss.ByteOrder, byteOrder);
			Assert.AreEqual(ss.FixedLength, (short)fixedLength);
		}

		[TestMethod]
		public void HandleBitEncoder64Test()
		{
			var enumEncoder = new EnumBitEncoder64<EnumTest>();
			var payloadTraits = new Bitwise.BitFieldTraits(20);
			var encoder = new Bitwise.HandleBitEncoder();

			encoder.Encode64(EnumTest.Member3, enumEncoder);
			encoder.Encode64(0x12345UL, payloadTraits);

			var decoder = new Bitwise.HandleBitEncoder(encoder.GetHandle64());
			decoder.Decode64(out EnumTest value, enumEncoder);
			decoder.Decode64(out ulong payload, payloadTraits);

			Assert.AreEqual(EnumTest.Member3, value);
			Assert.AreEqual(0x12345UL, payload);
		}


		[TestMethod]
		public void HandleBitEncoderEqualityTest()
		{
			var lhs = new Bitwise.HandleBitEncoder();
			var rhs = new Bitwise.HandleBitEncoder();
			lhs.Encode64(0x12345UL, 0xFFFFFUL);
			rhs.Encode64(0x12345UL, 0xFFFFFUL);

			Assert.IsTrue(lhs.Equals(rhs));
			Assert.IsTrue(lhs.Equals((object)rhs));
			Assert.IsFalse(lhs.Equals((object?)null));
			Assert.IsFalse(lhs.Equals("HandleBitEncoder"));
			Assert.IsTrue(lhs == rhs);
			Assert.IsFalse(lhs != rhs);
		}


		[TestMethod]
		public void HandleBitEncoderNoneableTest()
		{
			var encoder = new Bitwise.HandleBitEncoder();

			encoder.EncodeNoneable32(-1, 0x3);
			encoder.EncodeNoneable32(2, new Bitwise.BitFieldTraits(2));
			encoder.EncodeNoneable64(-1, 0x3);
			encoder.EncodeNoneable64(2, new Bitwise.BitFieldTraits(2));

			var decoder = new Bitwise.HandleBitEncoder(encoder.GetHandle64());
			decoder.DecodeNoneable32(out int none32, 0x3);
			decoder.DecodeNoneable32(out int value32, new Bitwise.BitFieldTraits(2));
			decoder.DecodeNoneable64(out long none64, 0x3);
			decoder.DecodeNoneable64(out long value64, new Bitwise.BitFieldTraits(2));

			Assert.AreEqual(-1, none32);
			Assert.AreEqual(2, value32);
			Assert.AreEqual(-1, none64);
			Assert.AreEqual(2, value64);
		}

		[TestMethod]
		public void HandleBitEncoderInvalidArgumentsThrowExpectedExceptions()
		{
			var handle = new Bitwise.HandleBitEncoder();
			var emptyTraits = default(Bitwise.BitFieldTraits);

			AssertArgumentNull("encoder",
				() => handle.Encode32(EnumTest.Member0, (EnumBitEncoder32<EnumTest>)null!));
			AssertArgumentNull("encoder",
				() => handle.Encode64(EnumTest.Member0, (EnumBitEncoder64<EnumTest>)null!));
			AssertArgumentNull("decoder",
				() => handle.Decode32(out EnumTest _, (EnumBitEncoder32<EnumTest>)null!));
			AssertArgumentNull("decoder",
				() => handle.Decode64(out EnumTest _, (EnumBitEncoder64<EnumTest>)null!));

			AssertArgumentOutOfRange("bitMask", () => handle.Encode32(1U, 0U));
			AssertArgumentOutOfRange("bitMask", () => handle.EncodeNoneable32(-1, 0U));
			AssertArgumentOutOfRange("bitMask", () => handle.Decode32(out uint _, 0U));
			AssertArgumentOutOfRange("bitMask", () => handle.DecodeNoneable32(out int _, 0U));
			AssertArgumentOutOfRange("bitMask", () => handle.Encode64(1UL, 0UL));
			AssertArgumentOutOfRange("bitMask", () => handle.EncodeNoneable64(-1L, 0UL));
			AssertArgumentOutOfRange("bitMask", () => handle.Decode64(out ulong _, 0UL));
			AssertArgumentOutOfRange("bitMask", () => handle.DecodeNoneable64(out long _, 0UL));

			AssertArgumentOutOfRange("value", () => handle.EncodeNoneable32(-2, 1U));
			AssertArgumentOutOfRange("value", () => handle.EncodeNoneable64(-2L, 1UL));

			AssertArgument("traits", () => handle.Encode32(1U, emptyTraits));
			AssertArgument("traits", () => handle.EncodeNoneable32(-1, emptyTraits));
			AssertArgument("traits", () => handle.Decode32(out uint _, emptyTraits));
			AssertArgument("traits", () => handle.DecodeNoneable32(out int _, emptyTraits));
			AssertArgument("traits", () => handle.Encode64(1UL, emptyTraits));
			AssertArgument("traits", () => handle.EncodeNoneable64(-1L, emptyTraits));
			AssertArgument("traits", () => handle.Decode64(out ulong _, emptyTraits));
			AssertArgument("traits", () => handle.DecodeNoneable64(out long _, emptyTraits));
		}
	};
}
