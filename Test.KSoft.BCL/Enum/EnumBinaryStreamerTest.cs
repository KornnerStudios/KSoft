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