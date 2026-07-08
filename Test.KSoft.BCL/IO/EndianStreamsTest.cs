using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.IO.Test;

[TestClass]
public class EndianStreamsTest : BaseTestClass
{
	[TestMethod]
	public void PrimitiveWritesUseDeclaredEndianByteOrderTest()
	{
		CollectionAssert.AreEqual(CreateBigEndianPrimitiveBytes(), WritePrimitives(Shell.EndianFormat.Big));
		CollectionAssert.AreEqual(CreateLittleEndianPrimitiveBytes(), WritePrimitives(Shell.EndianFormat.Little));
	}

	[TestMethod]
	public void PrimitiveReadsUseDeclaredEndianByteOrderTest()
	{
		AssertReadPrimitives(CreateBigEndianPrimitiveBytes(), Shell.EndianFormat.Big);
		AssertReadPrimitives(CreateLittleEndianPrimitiveBytes(), Shell.EndianFormat.Little);
	}

	[TestMethod]
	public void PrimitiveReadsKeepEndOfStreamBehaviorTest()
	{
		using var reader = new EndianReader(new MemoryStream(new byte[] { 0x12, 0x34, 0x56 }), Shell.EndianFormat.Big);

		Assert.Throws<EndOfStreamException>(() => reader.ReadUInt32());
		Assert.AreEqual(3L, reader.BaseStream.Position);
	}

	[TestMethod]
	public void PrimitiveReadsAndWritesHonorEndianSwitchTest()
	{
		using var ms = new MemoryStream();
		using (var writer = new EndianWriter(ms, Shell.EndianFormat.Big) { BaseStreamOwner = false })
		{
			writer.Write(0x12345678U);

			using (writer.BeginEndianSwitch())
			{
				writer.Write(0x12345678U);
			}
		}

		CollectionAssert.AreEqual(new byte[] { 0x12, 0x34, 0x56, 0x78, 0x78, 0x56, 0x34, 0x12 }, ms.ToArray());

		ms.Position = 0;
		using var reader = new EndianReader(ms, Shell.EndianFormat.Big);

		Assert.AreEqual(0x12345678U, reader.ReadUInt32());

		using (reader.BeginEndianSwitch())
		{
			Assert.AreEqual(0x12345678U, reader.ReadUInt32());
		}
	}

	[TestMethod]
	public void PrimitiveReadsAndWritesTreatUnknownByteOrderAsBigEndianTest()
	{
		const Shell.EndianFormat unknownByteOrder = (Shell.EndianFormat)2;

		using var ms = new MemoryStream();
		using (var writer = new EndianWriter(ms, unknownByteOrder) { BaseStreamOwner = false })
		{
			// Tag helpers treat non-Little values as Big. Natural-width primitives should stay consistent.
			writer.Write((ushort)0x1234);
		}

		CollectionAssert.AreEqual(new byte[] { 0x12, 0x34 }, ms.ToArray());

		ms.Position = 0;
		using var reader = new EndianReader(ms, unknownByteOrder);
		Assert.AreEqual((ushort)0x1234, reader.ReadUInt16());
	}

	static byte[] WritePrimitives(Shell.EndianFormat byteOrder)
	{
		using var ms = new MemoryStream();
		using (var writer = new EndianWriter(ms, byteOrder) { BaseStreamOwner = false })
		{
			WritePrimitiveValues(writer);
		}

		return ms.ToArray();
	}

	static void WritePrimitiveValues(EndianWriter writer)
	{
		// Use signed values and NaN payloads so byte-exact tests catch bit-preservation regressions, not just equality.
		writer.Write((ushort)0x1234);
		writer.Write(unchecked((short)0x89AB));
		writer.Write(0x89ABCDEFU);
		writer.Write(unchecked((int)0x89ABCDEF));
		writer.Write(0x0123456789ABCDEFUL);
		writer.Write(unchecked((long)0xFEDCBA9876543210UL));
		writer.Write(BitConverter.Int32BitsToSingle(unchecked((int)0xFFC00001)));
		writer.Write(BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8000000000001UL)));
	}

	static void AssertReadPrimitives(byte[] bytes, Shell.EndianFormat byteOrder)
	{
		using var reader = new EndianReader(new MemoryStream(bytes), byteOrder);

		Assert.AreEqual((ushort)0x1234, reader.ReadUInt16());
		Assert.AreEqual(unchecked((short)0x89AB), reader.ReadInt16());
		Assert.AreEqual(0x89ABCDEFU, reader.ReadUInt32());
		Assert.AreEqual(unchecked((int)0x89ABCDEF), reader.ReadInt32());
		Assert.AreEqual(0x0123456789ABCDEFUL, reader.ReadUInt64());
		Assert.AreEqual(unchecked((long)0xFEDCBA9876543210UL), reader.ReadInt64());
		Assert.AreEqual(unchecked((int)0xFFC00001), BitConverter.SingleToInt32Bits(reader.ReadSingle()));
		Assert.AreEqual(unchecked((long)0xFFF8000000000001UL), BitConverter.DoubleToInt64Bits(reader.ReadDouble()));
	}

	static byte[] CreateBigEndianPrimitiveBytes() => new byte[] {
		0x12, 0x34,
		0x89, 0xAB,
		0x89, 0xAB, 0xCD, 0xEF,
		0x89, 0xAB, 0xCD, 0xEF,
		0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
		0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10,
		0xFF, 0xC0, 0x00, 0x01,
		0xFF, 0xF8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
	};

	static byte[] CreateLittleEndianPrimitiveBytes() => new byte[] {
		0x34, 0x12,
		0xAB, 0x89,
		0xEF, 0xCD, 0xAB, 0x89,
		0xEF, 0xCD, 0xAB, 0x89,
		0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01,
		0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE,
		0x01, 0x00, 0xC0, 0xFF,
		0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF8, 0xFF,
	};
}
