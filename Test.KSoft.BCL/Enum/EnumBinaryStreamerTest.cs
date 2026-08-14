using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.IO.Test
{
	using TypeCodeStreamer8 = EnumBinaryStreamer<System.TypeCode, byte>;
	using TypeCodeStreamer32 = EnumBinaryStreamer<System.TypeCode, int>;
	using TypeCodeStreamer64 = EnumBinaryStreamer<System.TypeCode, long>;
	using UInt32EnumStreamer = EnumBinaryStreamer<EnumBinaryStreamerTest.UInt32Enum>;

	[TestClass]
	public class EnumBinaryStreamerTest : BaseTestClass
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

		static void AssertUnderlyingBinaryRoundTrip<TEnum>(TEnum value, byte[] expectedBytes)
			where TEnum : struct, Enum
		{
			using (var ms = new System.IO.MemoryStream())
			using (var br = new System.IO.BinaryReader(ms))
			using (var bw = new System.IO.BinaryWriter(ms))
			{
				EnumBinaryStreamer<TEnum>.Write(bw, value);
				CollectionAssert.AreEqual(expectedBytes, ms.ToArray());

				ms.Position = 0;
				EnumBinaryStreamer<TEnum>.Read(br, out var actualValue);

				Assert.AreEqual(value, actualValue);
			}
		}

		static void AssertBinaryConversionRoundTrip<TEnum, TStreamType>(TEnum value, byte[] expectedBytes)
			where TEnum : struct, Enum
			where TStreamType : struct
		{
			using (var ms = new System.IO.MemoryStream())
			using (var br = new System.IO.BinaryReader(ms))
			using (var bw = new System.IO.BinaryWriter(ms))
			{
				EnumBinaryStreamer<TEnum, TStreamType>.Write(bw, value);
				CollectionAssert.AreEqual(expectedBytes, ms.ToArray());

				ms.Position = 0;
				EnumBinaryStreamer<TEnum, TStreamType>.Read(br, out var actualValue);

				Assert.AreEqual(value, actualValue);
			}
		}

		[TestMethod]
		public void Enum_BinaryStreamerUnderlyingTypesWriteExpectedBytesTest()
		{
			AssertUnderlyingBinaryRoundTrip(ByteEnum.Value, new byte[] { 0xAB });
			AssertUnderlyingBinaryRoundTrip(SByteEnum.Value, new byte[] { 0xFE });
			AssertUnderlyingBinaryRoundTrip(UInt16Enum.Value, BitConverter.GetBytes((ushort)0xABCD));
			AssertUnderlyingBinaryRoundTrip(Int16Enum.Value, BitConverter.GetBytes((short)-1234));
			AssertUnderlyingBinaryRoundTrip(UInt32Enum.DeadBeef, BitConverter.GetBytes(0xDEADBEEFU));
			AssertUnderlyingBinaryRoundTrip(Int32Enum.Value, BitConverter.GetBytes(-123456789));
			AssertUnderlyingBinaryRoundTrip(UInt64Enum.Value, BitConverter.GetBytes(0x0123456789ABCDEFUL));
			AssertUnderlyingBinaryRoundTrip(Int64Enum.Value, BitConverter.GetBytes(-0x0102030405060708L));
		}

		[TestMethod]
		public void Enum_BinaryStreamerSignedUnsignedConversionsRoundTripTest()
		{
			AssertBinaryConversionRoundTrip<SByteEnum, byte>(SByteEnum.Value, new byte[] { 0xFE });
			AssertBinaryConversionRoundTrip<UInt32Enum, sbyte>(UInt32Enum.AllBitsExceptOne, new byte[] { 0xFE });
		}

		/// <summary>Test with conditions that don't require a conversion</summary>
		[TestMethod]
		public void Enum_BinaryStreamerTest()
		{
			using (var ms = new System.IO.MemoryStream())
			using (var br = new IO.EndianReader(ms))
			using (var bw = new IO.EndianWriter(ms))
			{
				const System.TypeCode kExpectedValue = System.TypeCode.String;
				var value = kExpectedValue;

				TypeCodeStreamer32.Write(bw, value);
				ms.Position = 0;
				TypeCodeStreamer32.Read(br, out value);

				Assert.IsTrue(value == kExpectedValue);

				//////////////////////////////////////////////////////////////////////////
				// Test the instance interface
				var streamer_instance = TypeCodeStreamer32.Instance;
				ms.Position = 0;

				streamer_instance.Write(bw, value);
				ms.Position = 0;
				streamer_instance.Read(br, out value);

				Assert.IsTrue(value == kExpectedValue);
			}
		}

		/// <summary>Test with conditions that require a down-cast conversion</summary>
		[TestMethod]
		public void Enum_BinaryStreamerDownCastTest()
		{
			using (var ms = new System.IO.MemoryStream())
			using (var br = new IO.EndianReader(ms))
			using (var bw = new IO.EndianWriter(ms))
			{
				const System.TypeCode kExpectedValue = System.TypeCode.String;
				var value = kExpectedValue;

				TypeCodeStreamer8.Write(bw, value);
				ms.Position = 0;
				TypeCodeStreamer8.Read(br, out value);

				Assert.IsTrue(value == kExpectedValue);

				//////////////////////////////////////////////////////////////////////////
				// Test the instance interface
				var streamer_instance = TypeCodeStreamer8.Instance;
				ms.Position = 0;

				streamer_instance.Write(bw, value);
				ms.Position = 0;
				streamer_instance.Read(br, out value);

				Assert.IsTrue(value == kExpectedValue);
			}
		}

		/// <summary>Test with conditions that require an up-cast conversion</summary>
		[TestMethod]
		public void Enum_BinaryStreamerUpCastTest()
		{
			using (var ms = new System.IO.MemoryStream())
			using (var br = new IO.EndianReader(ms))
			using (var bw = new IO.EndianWriter(ms))
			{
				const System.TypeCode kExpectedValue = System.TypeCode.String;
				var value = kExpectedValue;

				TypeCodeStreamer64.Write(bw, value);
				ms.Position = 0;
				TypeCodeStreamer64.Read(br, out value);

				Assert.IsTrue(value == kExpectedValue);

				//////////////////////////////////////////////////////////////////////////
				// Test the instance interface
				var streamer_instance = TypeCodeStreamer64.Instance;
				ms.Position = 0;

				streamer_instance.Write(bw, value);
				ms.Position = 0;
				streamer_instance.Read(br, out value);

				Assert.IsTrue(value == kExpectedValue);
			}
		}

		internal enum UInt32Enum : uint
		{
			None,
			DeadBeef = 0xDEADBEEF,
			AllBitsExceptOne = 0xFFFFFFFE,
		};
		/// <summary>Test the ability to implicitly use the underlying type</summary>
		[TestMethod]
		public void Enum_BinaryStreamerUsingUnderlyingTypeTest()
		{
			using (var ms = new System.IO.MemoryStream())
			using (var br = new IO.EndianReader(ms))
			using (var bw = new IO.EndianWriter(ms))
			{
				const UInt32Enum kExpectedValue = UInt32Enum.DeadBeef;
				var value = kExpectedValue;

				UInt32EnumStreamer.Write(bw, value);
				ms.Position = 0;
				UInt32EnumStreamer.Read(br, out value);

				Assert.IsTrue(value == kExpectedValue);

				//////////////////////////////////////////////////////////////////////////
				// Test the instance interface
				var streamer_instance = UInt32EnumStreamer.Instance;
				ms.Position = 0;

				streamer_instance.Write(bw, value);
				ms.Position = 0;
				streamer_instance.Read(br, out value);

				Assert.IsTrue(value == kExpectedValue);
			}
		}
	};
}