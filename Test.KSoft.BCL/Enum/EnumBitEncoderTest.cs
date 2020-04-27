using System;
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

		#region 32-bit tests
		void Test32Helper<TEnum, TEnumInformal>(TEnum value)
			where TEnum : struct, IComparable, IFormattable, IConvertible
			where TEnumInformal : struct, IComparable, IFormattable, IConvertible
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
				Assert.AreNotEqual(0, bits);
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
				Assert.AreNotEqual(0, bits);
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
			where TEnum : struct, IComparable, IFormattable, IConvertible
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
		public void Enum_BitEncoder32Test()
		{
			Test32Helper<EnumTest, EnumTestInformal>(EnumTest.Member3);
			Test32Helper<FlagsTest, FlagsTestInformal>(FlagsTest.Member3 | FlagsTest.Member4);
			TestNone32Helper(EnumTestWithNone.Member3, EnumTestWithNone.None);
		}
		#endregion

		#region 64-bit tests
		static void Test64Helper<TEnum, TEnumInformal>(TEnum value)
			where TEnum : struct, IComparable, IFormattable, IConvertible
			where TEnumInformal : struct, IComparable, IFormattable, IConvertible
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
				Assert.AreNotEqual(0, bits);
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
				Assert.AreNotEqual(0, bits);
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
			where TEnum : struct, IComparable, IFormattable, IConvertible
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
		public void Enum_BitEncoder64Test()
		{
			Test64Helper<EnumTest, EnumTestInformal>(EnumTest.Member3);
			Test64Helper<FlagsTest, FlagsTestInformal>(FlagsTest.Member3 | FlagsTest.Member4);
			TestNone64Helper(EnumTestWithNone.Member3, EnumTestWithNone.None);
		}
		#endregion

		[TestMethod]
		public void Enum_BitEncoderHashCode32Test()
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
		public void Enum_HandleBitEncoder32Test()
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
	};
}
