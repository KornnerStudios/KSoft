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
