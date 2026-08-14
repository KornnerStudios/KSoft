using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.IO.Test
{
	using TypeCodeStreamer8 = EnumBitStreamer<System.TypeCode, byte>;
	using TypeCodeStreamer32 = EnumBitStreamer<System.TypeCode, int>;
	using TypeCodeStreamer64 = EnumBitStreamer<System.TypeCode, long>;
	using UInt32EnumStreamer = EnumBitStreamer<EnumBinaryStreamerTest.UInt32Enum>;
	using NoneSentinelEncodedEnumStreamer = EnumBitStreamerWithOptions<EnumBitStreamerTest.NoneSentinelEncodedEnum,
		EnumBitStreamerTest.EnumBinaryStreamerBase1Options>;

	[TestClass]
	public class EnumBitStreamerTest : BaseTestClass
	{
		internal enum ByteEnum : byte
		{
			Value = 0xAB,
		};

		internal enum SByteEnum : sbyte
		{
			Value = -2,
		};

		internal enum UInt16Enum : ushort
		{
			Value = 0xABCD,
		};

		internal enum Int16Enum : short
		{
			Value = -1234,
		};

		internal enum Int32Enum : int
		{
			Value = -123456789,
		};

		internal enum UInt64Enum : ulong
		{
			Value = 0x0123456789ABCDEFUL,
		};

		internal enum Int64Enum : long
		{
			Value = -0x0102030405060708L,
		};

		internal enum FourBitFlags : uint
		{
			Value = 0b1101,
		};

		internal enum SignedThreeBitEnum : int
		{
			NegativeThree = -3,
			PositiveFive = 5,
		};

		[SuppressMessage("Microsoft.Design", "CA1812:AvoidUninstantiatedInternalClasses")]
		internal class SignExtendOptions : EnumBitStreamerOptions
		{
			public override bool SignExtend => true;
		};

		static void AssertUnderlyingBitRoundTrip<TEnum>(TEnum value, int bitCount)
			where TEnum : struct, Enum
		{
			using (var ms = new System.IO.MemoryStream())
			using (var s = new IO.BitStream(ms))
			{
				s.StreamMode = System.IO.FileAccess.Write;
				EnumBitStreamer<TEnum>.Write(s, value, bitCount);
				s.Flush();
				s.SeekToStart();

				s.StreamMode = System.IO.FileAccess.Read;
				EnumBitStreamer<TEnum>.Read(s, out var actualValue, bitCount);

				Assert.AreEqual(value, actualValue);
			}
		}

		static void AssertBitConversionRoundTrip<TEnum, TStreamType>(TEnum value, int bitCount)
			where TEnum : struct, Enum
			where TStreamType : struct
		{
			using (var ms = new System.IO.MemoryStream())
			using (var s = new IO.BitStream(ms))
			{
				s.StreamMode = System.IO.FileAccess.Write;
				EnumBitStreamer<TEnum, TStreamType>.Write(s, value, bitCount);
				s.Flush();
				s.SeekToStart();

				s.StreamMode = System.IO.FileAccess.Read;
				EnumBitStreamer<TEnum, TStreamType>.Read(s, out var actualValue, bitCount);

				Assert.AreEqual(value, actualValue);
			}
		}

		[TestMethod]
		public void Enum_BitStreamerUnderlyingTypesRoundTripTest()
		{
			AssertUnderlyingBitRoundTrip(ByteEnum.Value, 8);
			AssertUnderlyingBitRoundTrip(SByteEnum.Value, 8);
			AssertUnderlyingBitRoundTrip(UInt16Enum.Value, 16);
			AssertUnderlyingBitRoundTrip(Int16Enum.Value, 16);
			AssertUnderlyingBitRoundTrip(EnumBinaryStreamerTest.UInt32Enum.DeadBeef, 32);
			AssertUnderlyingBitRoundTrip(Int32Enum.Value, 32);
			AssertUnderlyingBitRoundTrip(UInt64Enum.Value, 64);
			AssertUnderlyingBitRoundTrip(Int64Enum.Value, 64);
		}

		[TestMethod]
		public void Enum_BitStreamerSignedUnsignedConversionsRoundTripTest()
		{
			AssertBitConversionRoundTrip<SByteEnum, byte>(SByteEnum.Value, 8);
			AssertBitConversionRoundTrip<EnumBinaryStreamerTest.UInt32Enum, sbyte>(
				EnumBinaryStreamerTest.UInt32Enum.AllBitsExceptOne, 8);
		}

		[TestMethod]
		public void Enum_BitStreamerBitSwapWritesSwappedBitsTest()
		{
			byte[] bytes;
			using (var ms = new System.IO.MemoryStream())
			using (var s = new IO.BitStream(ms))
			{
				s.StreamMode = System.IO.FileAccess.Write;
				EnumBitStreamer<FourBitFlags, uint, EnumBitStreamerOptions.ShouldBitSwap>.Write(s, FourBitFlags.Value, 4);
				s.Flush();

				bytes = ms.ToArray();
			}

			using (var ms = new System.IO.MemoryStream(bytes))
			using (var s = new IO.BitStream(ms, System.IO.FileAccess.Read))
			{
				s.StreamMode = System.IO.FileAccess.Read;
				Assert.AreEqual(0b1011U, s.ReadUInt32(4));
			}

			using (var ms = new System.IO.MemoryStream(bytes))
			using (var s = new IO.BitStream(ms, System.IO.FileAccess.Read))
			{
				s.StreamMode = System.IO.FileAccess.Read;
				EnumBitStreamer<FourBitFlags, uint, EnumBitStreamerOptions.ShouldBitSwap>.Read(s, out var value, 4);
				Assert.AreEqual(FourBitFlags.Value, value);
			}
		}

		[TestMethod]
		public void Enum_BitStreamerSignExtendOptionsPreserveNegativeValuesTest()
		{
			byte[] bytes;
			using (var ms = new System.IO.MemoryStream())
			using (var s = new IO.BitStream(ms))
			{
				s.StreamMode = System.IO.FileAccess.Write;
				EnumBitStreamer<SignedThreeBitEnum, int>.Write(s, SignedThreeBitEnum.NegativeThree, 3);
				s.Flush();

				bytes = ms.ToArray();
			}

			using (var ms = new System.IO.MemoryStream(bytes))
			using (var s = new IO.BitStream(ms, System.IO.FileAccess.Read))
			{
				s.StreamMode = System.IO.FileAccess.Read;
				EnumBitStreamer<SignedThreeBitEnum, int>.Read(s, out var withoutSignExtend, 3);
				Assert.AreEqual(SignedThreeBitEnum.PositiveFive, withoutSignExtend);
			}

			using (var ms = new System.IO.MemoryStream(bytes))
			using (var s = new IO.BitStream(ms, System.IO.FileAccess.Read))
			{
				s.StreamMode = System.IO.FileAccess.Read;
				EnumBitStreamer<SignedThreeBitEnum, int, SignExtendOptions>.Read(s, out var withSignExtend, 3);
				Assert.AreEqual(SignedThreeBitEnum.NegativeThree, withSignExtend);
			}
		}

		/// <summary>Test with conditions that don't require a conversion</summary>
		[TestMethod]
		public void Enum_BitStreamerTest()
		{
			const int k_bit_count = 32;

			using (var ms = new System.IO.MemoryStream())
			using (var s = new IO.BitStream(ms))
			{
				const System.TypeCode kExpectedValue = System.TypeCode.String;
				var value = kExpectedValue;

				s.StreamMode = System.IO.FileAccess.Write;
				TypeCodeStreamer32.Write(s, value, k_bit_count);
				s.Flush(); s.SeekToStart();
				s.StreamMode = System.IO.FileAccess.Read;
				TypeCodeStreamer32.Read(s, out value, k_bit_count);

				Assert.AreEqual(kExpectedValue, value);

				//////////////////////////////////////////////////////////////////////////
				// Test the instance interface
				var streamer_instance = TypeCodeStreamer32.Instance;
				s.SeekToStart();

				s.StreamMode = System.IO.FileAccess.Write;
				streamer_instance.Write(s, value, k_bit_count);
				s.Flush(); s.SeekToStart();
				s.StreamMode = System.IO.FileAccess.Read;
				streamer_instance.Read(s, out value, k_bit_count);

				Assert.AreEqual(kExpectedValue, value);
			}
		}

		/// <summary>Test with conditions that require a down-cast conversion</summary>
		[TestMethod]
		public void Enum_BitStreamerDownCastTest()
		{
			const int k_bit_count = 8;

			using (var ms = new System.IO.MemoryStream())
			using (var s = new IO.BitStream(ms))
			{
				const System.TypeCode kExpectedValue = System.TypeCode.String;
				var value = kExpectedValue;

				s.StreamMode = System.IO.FileAccess.Write;
				TypeCodeStreamer8.Write(s, value, k_bit_count);
				s.Flush(); s.SeekToStart();
				s.StreamMode = System.IO.FileAccess.Read;
				TypeCodeStreamer8.Read(s, out value, k_bit_count);

				Assert.AreEqual(kExpectedValue, value);

				//////////////////////////////////////////////////////////////////////////
				// Test the instance interface
				var streamer_instance = TypeCodeStreamer8.Instance;
				s.SeekToStart();

				s.StreamMode = System.IO.FileAccess.Write;
				streamer_instance.Write(s, value, k_bit_count);
				s.Flush(); s.SeekToStart();
				s.StreamMode = System.IO.FileAccess.Read;
				streamer_instance.Read(s, out value, k_bit_count);

				Assert.AreEqual(kExpectedValue, value);
			}
		}

		/// <summary>Test with conditions that require an up-cast conversion</summary>
		[TestMethod]
		public void Enum_BitStreamerUpCastTest()
		{
			const int k_bit_count = 64;

			using (var ms = new System.IO.MemoryStream())
			using (var s = new IO.BitStream(ms))
			{
				const System.TypeCode kExpectedValue = System.TypeCode.String;
				var value = kExpectedValue;

				s.StreamMode = System.IO.FileAccess.Write;
				TypeCodeStreamer64.Write(s, value, k_bit_count);
				s.Flush(); s.SeekToStart();
				s.StreamMode = System.IO.FileAccess.Read;
				TypeCodeStreamer64.Read(s, out value, k_bit_count);

				Assert.AreEqual(kExpectedValue, value);

				//////////////////////////////////////////////////////////////////////////
				// Test the instance interface
				var streamer_instance = TypeCodeStreamer64.Instance;
				s.SeekToStart();

				s.StreamMode = System.IO.FileAccess.Write;
				streamer_instance.Write(s, value, k_bit_count);
				s.Flush(); s.SeekToStart();
				s.StreamMode = System.IO.FileAccess.Read;
				streamer_instance.Read(s, out value, k_bit_count);

				Assert.AreEqual(kExpectedValue, value);
			}
		}

		/// <summary>Test the ability to implicitly use the underlying type</summary>
		[TestMethod]
		public void Enum_BitStreamerUsingUnderlyingTypeTest()
		{
			const int k_bit_count = 32;

			using (var ms = new System.IO.MemoryStream())
			using (var s = new IO.BitStream(ms))
			{
				const EnumBinaryStreamerTest.UInt32Enum kExpectedValue = EnumBinaryStreamerTest.UInt32Enum.DeadBeef;
				var value = kExpectedValue;

				s.StreamMode = System.IO.FileAccess.Write;
				UInt32EnumStreamer.Write(s, value, k_bit_count);
				s.Flush(); s.SeekToStart();
				s.StreamMode = System.IO.FileAccess.Read;
				UInt32EnumStreamer.Read(s, out value, k_bit_count);

				Assert.AreEqual(kExpectedValue, value);

				//////////////////////////////////////////////////////////////////////////
				// Test the instance interface
				var streamer_instance = UInt32EnumStreamer.Instance;
				s.SeekToStart();

				s.StreamMode = System.IO.FileAccess.Write;
				streamer_instance.Write(s, value, k_bit_count);
				s.Flush(); s.SeekToStart();
				s.StreamMode = System.IO.FileAccess.Read;
				streamer_instance.Read(s, out value, k_bit_count);

				Assert.AreEqual(kExpectedValue, value);
			}
		}

		internal enum NoneSentinelEncodedEnum : int
		{
			None = -1,
			One,
			Two,
			Three,
		};
		[SuppressMessage("Microsoft.Design", "CA1812:AvoidUninstantiatedInternalClasses")]
		internal class EnumBinaryStreamerBase1Options : EnumBitStreamerOptions
		{
			public override bool UseNoneSentinelEncoding { get { return true; } }
		};

		[TestMethod]
		public void EnumBitStreamer_NoneSentinelWritesZeroForNoneTest()
		{
			const int k_bit_count = 32;

			using (var ms = new System.IO.MemoryStream())
			using (var s = new IO.BitStream(ms))
			{
				s.StreamMode = System.IO.FileAccess.Write;
				NoneSentinelEncodedEnumStreamer.Write(s, NoneSentinelEncodedEnum.None, k_bit_count);
				s.Flush();
				s.SeekToStart();

				s.StreamMode = System.IO.FileAccess.Read;
				Assert.AreEqual(0, s.ReadInt32(k_bit_count));
			}
		}

		/// <summary>Test the ability to implicitly use the underlying type</summary>
		[TestMethod]
		public void EnumBitStreamer_TestNoneSentinelEncoding()
		{
			const int k_bit_count = 32;

			using (var ms = new System.IO.MemoryStream())
			using (var s = new IO.BitStream(ms))
			{
				const NoneSentinelEncodedEnum kExpectedValue = NoneSentinelEncodedEnum.Three;
				var value = kExpectedValue;

				s.StreamMode = System.IO.FileAccess.Write;
				NoneSentinelEncodedEnumStreamer.Stream(s, ref value, k_bit_count);
				s.Flush(); s.SeekToStart();
				s.StreamMode = System.IO.FileAccess.Read;
				NoneSentinelEncodedEnumStreamer.Stream(s, ref value, k_bit_count);

				Assert.AreEqual(kExpectedValue, value);

				//////////////////////////////////////////////////////////////////////////
				// Test the instance interface
				var streamer_instance = NoneSentinelEncodedEnumStreamer.Instance;
				s.SeekToStart();

				s.StreamMode = System.IO.FileAccess.Write;
				streamer_instance.Stream(s, ref value, k_bit_count);
				s.Flush(); s.SeekToStart();
				s.StreamMode = System.IO.FileAccess.Read;
				streamer_instance.Stream(s, ref value, k_bit_count);

				Assert.AreEqual(kExpectedValue, value);
			}
		}
	};
}
