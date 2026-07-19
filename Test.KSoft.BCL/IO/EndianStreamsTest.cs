using System;
using System.Collections.Generic;
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

	[TestMethod]
	public void FixedArrayWritesUseDeclaredEndianByteOrderAndRangeTest()
	{
		CollectionAssert.AreEqual(CreateBigEndianFixedArrayRangeBytes(),
			WriteFixedArrayRange(Shell.EndianFormat.Big));
		CollectionAssert.AreEqual(CreateLittleEndianFixedArrayRangeBytes(),
			WriteFixedArrayRange(Shell.EndianFormat.Little));
	}

	[TestMethod]
	public void FixedArrayReadsUseDeclaredEndianByteOrderAndRangeTest()
	{
		AssertReadFixedArrayRange(CreateBigEndianFixedArrayRangeBytes(), Shell.EndianFormat.Big);
		AssertReadFixedArrayRange(CreateLittleEndianFixedArrayRangeBytes(), Shell.EndianFormat.Little);
	}

	[TestMethod]
	public void EndianStreamStreamsScalarsUseDeclaredEndianByteOrderTest()
	{
		CollectionAssert.AreEqual(CreateBigEndianEndianStreamScalarBytes(),
			WriteEndianStreamScalars(Shell.EndianFormat.Big));
		CollectionAssert.AreEqual(CreateLittleEndianEndianStreamScalarBytes(),
			WriteEndianStreamScalars(Shell.EndianFormat.Little));

		AssertReadEndianStreamScalars(CreateBigEndianEndianStreamScalarBytes(), Shell.EndianFormat.Big);
		AssertReadEndianStreamScalars(CreateLittleEndianEndianStreamScalarBytes(), Shell.EndianFormat.Little);
	}

	[TestMethod]
	public void EndianStreamFixedArrayUsesDeclaredEndianByteOrderAndRangeTest()
	{
		CollectionAssert.AreEqual(CreateBigEndianFixedArrayRangeBytes(),
			WriteEndianStreamFixedArrayRange(Shell.EndianFormat.Big));
		CollectionAssert.AreEqual(CreateLittleEndianFixedArrayRangeBytes(),
			WriteEndianStreamFixedArrayRange(Shell.EndianFormat.Little));

		AssertReadEndianStreamFixedArrayRange(CreateBigEndianFixedArrayRangeBytes(), Shell.EndianFormat.Big);
		AssertReadEndianStreamFixedArrayRange(CreateLittleEndianFixedArrayRangeBytes(), Shell.EndianFormat.Little);
	}

#if CONTRACTS_FULL_SHIM
	[TestMethod]
	public void FixedArrayInvalidArgumentsKeepUntypedContractShimBehaviorTest()
	{
		using var writerStream = new MemoryStream();
		using var writer = new EndianWriter(writerStream, Shell.EndianFormat.Big) { BaseStreamOwner = false };

		AssertContractShimException(() => writer.WriteFixedArray((ushort[])null, 0, 0));
		AssertContractShimException(() => writer.WriteFixedArray(new ushort[1], -1, 1));
		AssertContractShimException(() => writer.WriteFixedArray(new ushort[1], 0, -1));

		using var reader = new EndianReader(
			new MemoryStream(new byte[] { 0x12, 0x34 }),
			Shell.EndianFormat.Big);

		AssertContractShimException(() => reader.ReadFixedArray((ushort[])null, 0, 0));
		AssertContractShimException(() => reader.ReadFixedArray(new ushort[1], -1, 1));
		AssertContractShimException(() => reader.ReadFixedArray(new ushort[1], 0, -1));
	}
#endif

	[TestMethod]
	public void FixedArrayOutOfRangePreservesPartialSideEffectsTest()
	{
		using var writerStream = new MemoryStream();
		using (var writer = new EndianWriter(writerStream, Shell.EndianFormat.Big) { BaseStreamOwner = false })
		{
			Assert.Throws<IndexOutOfRangeException>(()
				=> writer.WriteFixedArray(new ushort[] { 0x1111, 0x2233 }, 1, 2));
		}

		CollectionAssert.AreEqual(new byte[] { 0x22, 0x33 }, writerStream.ToArray());

		using var readStream = new MemoryStream(new byte[] { 0x11, 0x22, 0x33, 0x44 });
		using var reader = new EndianReader(readStream, Shell.EndianFormat.Big);
		var values = new ushort[2];

		Assert.Throws<IndexOutOfRangeException>(() => reader.ReadFixedArray(values, 1, 2));
		Assert.AreEqual(4L, readStream.Position);
		Assert.AreEqual((ushort)0x1122, values[1]);
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

	static byte[] WriteFixedArrayRange(Shell.EndianFormat byteOrder)
	{
		using var ms = new MemoryStream();
		using (var writer = new EndianWriter(ms, byteOrder) { BaseStreamOwner = false })
		{
			WriteFixedArrayRangeValues(writer);
		}

		return ms.ToArray();
	}

	static byte[] WriteEndianStreamScalars(Shell.EndianFormat byteOrder)
	{
		using var ms = new MemoryStream();
		using (var writer = new EndianWriter(ms, byteOrder) { BaseStreamOwner = false })
		using (var stream = EndianStream.UsingWriter(writer))
		{
			StreamScalarValues(stream);
		}

		return ms.ToArray();
	}

	static byte[] WriteEndianStreamFixedArrayRange(Shell.EndianFormat byteOrder)
	{
		using var ms = new MemoryStream();
		using (var writer = new EndianWriter(ms, byteOrder) { BaseStreamOwner = false })
		using (var stream = EndianStream.UsingWriter(writer))
		{
			StreamFixedArrayRangeValues(stream);
		}

		return ms.ToArray();
	}

	static void WriteFixedArrayRangeValues(EndianWriter writer)
	{
		writer.WriteFixedArray(new byte[] { 0xA0, 0x11, 0x22, 0xA3 }, 1, 2);
		writer.WriteFixedArray(new sbyte[] { -1, 0x11, 0x22, -4 }, 1, 2);
		writer.WriteFixedArray(new ushort[] { 0, 0x1234, 0x5678, 0 }, 1, 2);
		writer.WriteFixedArray(new short[] { 0, unchecked((short)0x89AB), 0x1234, 0 }, 1, 2);
		writer.WriteFixedArray(new uint[] { 0, 0x89ABCDEFU, 0x01234567U, 0 }, 1, 2);
		writer.WriteFixedArray(new int[] { 0, unchecked((int)0x89ABCDEF), 0x01234567, 0 }, 1, 2);
		writer.WriteFixedArray(new ulong[] { 0, 0x0123456789ABCDEFUL, 0xFEDCBA9876543210UL, 0 }, 1, 2);
		writer.WriteFixedArray(new long[] {
			0,
			0x0123456789ABCDEFL,
			unchecked((long)0xFEDCBA9876543210UL),
			0,
		}, 1, 2);
		writer.WriteFixedArray(new float[] {
			0,
			BitConverter.Int32BitsToSingle(0x3F800000),
			BitConverter.Int32BitsToSingle(unchecked((int)0xFFC00001)),
			0,
		}, 1, 2);
		writer.WriteFixedArray(new double[] {
			0,
			BitConverter.Int64BitsToDouble(0x3FF0000000000000L),
			BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8000000000001UL)),
			0,
		}, 1, 2);
	}

	static void StreamScalarValues(EndianStream stream)
	{
		byte byteValue = 0x7A;
		sbyte sbyteValue = unchecked((sbyte)0x85);
		ushort ushortValue = 0x1234;
		short shortValue = unchecked((short)0x89AB);
		uint uintValue = 0x89ABCDEFU;
		int intValue = unchecked((int)0x89ABCDEF);
		ulong ulongValue = 0x0123456789ABCDEFUL;
		long longValue = unchecked((long)0xFEDCBA9876543210UL);
		float floatValue = BitConverter.Int32BitsToSingle(unchecked((int)0xFFC00001));
		double doubleValue = BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8000000000001UL));

		stream.Stream(ref byteValue);
		stream.Stream(ref sbyteValue);
		stream.Stream(ref ushortValue);
		stream.Stream(ref shortValue);
		stream.Stream(ref uintValue);
		stream.Stream(ref intValue);
		stream.Stream(ref ulongValue);
		stream.Stream(ref longValue);
		stream.Stream(ref floatValue);
		stream.Stream(ref doubleValue);
	}

	static void StreamFixedArrayRangeValues(EndianStream stream)
	{
		stream.StreamFixedArray(new byte[] { 0xA0, 0x11, 0x22, 0xA3 }, 1, 2);
		stream.StreamFixedArray(new sbyte[] { -1, 0x11, 0x22, -4 }, 1, 2);
		stream.StreamFixedArray(new ushort[] { 0, 0x1234, 0x5678, 0 }, 1, 2);
		stream.StreamFixedArray(new short[] { 0, unchecked((short)0x89AB), 0x1234, 0 }, 1, 2);
		stream.StreamFixedArray(new uint[] { 0, 0x89ABCDEFU, 0x01234567U, 0 }, 1, 2);
		stream.StreamFixedArray(new int[] { 0, unchecked((int)0x89ABCDEF), 0x01234567, 0 }, 1, 2);
		stream.StreamFixedArray(new ulong[] { 0, 0x0123456789ABCDEFUL, 0xFEDCBA9876543210UL, 0 }, 1, 2);
		stream.StreamFixedArray(new long[] {
			0,
			0x0123456789ABCDEFL,
			unchecked((long)0xFEDCBA9876543210UL),
			0,
		}, 1, 2);
		stream.StreamFixedArray(new float[] {
			0,
			BitConverter.Int32BitsToSingle(0x3F800000),
			BitConverter.Int32BitsToSingle(unchecked((int)0xFFC00001)),
			0,
		}, 1, 2);
		stream.StreamFixedArray(new double[] {
			0,
			BitConverter.Int64BitsToDouble(0x3FF0000000000000L),
			BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8000000000001UL)),
			0,
		}, 1, 2);
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

	static void AssertReadFixedArrayRange(byte[] bytes, Shell.EndianFormat byteOrder)
	{
		using var reader = new EndianReader(new MemoryStream(bytes), byteOrder);

		AssertReadFixedArrayRangeValues(reader);
	}

	static void AssertReadEndianStreamScalars(byte[] bytes, Shell.EndianFormat byteOrder)
	{
		using var reader = new EndianReader(new MemoryStream(bytes), byteOrder);
		using var stream = EndianStream.UsingReader(reader);

		byte byteValue = 0;
		sbyte sbyteValue = 0;
		ushort ushortValue = 0;
		short shortValue = 0;
		uint uintValue = 0;
		int intValue = 0;
		ulong ulongValue = 0;
		long longValue = 0;
		float floatValue = 0;
		double doubleValue = 0;

		stream.Stream(ref byteValue);
		stream.Stream(ref sbyteValue);
		stream.Stream(ref ushortValue);
		stream.Stream(ref shortValue);
		stream.Stream(ref uintValue);
		stream.Stream(ref intValue);
		stream.Stream(ref ulongValue);
		stream.Stream(ref longValue);
		stream.Stream(ref floatValue);
		stream.Stream(ref doubleValue);

		Assert.AreEqual((byte)0x7A, byteValue);
		Assert.AreEqual(unchecked((sbyte)0x85), sbyteValue);
		Assert.AreEqual((ushort)0x1234, ushortValue);
		Assert.AreEqual(unchecked((short)0x89AB), shortValue);
		Assert.AreEqual(0x89ABCDEFU, uintValue);
		Assert.AreEqual(unchecked((int)0x89ABCDEF), intValue);
		Assert.AreEqual(0x0123456789ABCDEFUL, ulongValue);
		Assert.AreEqual(unchecked((long)0xFEDCBA9876543210UL), longValue);
		Assert.AreEqual(unchecked((int)0xFFC00001), BitConverter.SingleToInt32Bits(floatValue));
		Assert.AreEqual(unchecked((long)0xFFF8000000000001UL), BitConverter.DoubleToInt64Bits(doubleValue));
	}

	static void AssertReadEndianStreamFixedArrayRange(byte[] bytes, Shell.EndianFormat byteOrder)
	{
		using var reader = new EndianReader(new MemoryStream(bytes), byteOrder);
		using var stream = EndianStream.UsingReader(reader);

		AssertReadStreamFixedArrayRangeValues(stream);
	}

	static void AssertReadFixedArrayRangeValues(EndianReader reader)
	{
		var byteValues = new byte[] { 0xA0, 0, 0, 0xA3 };
		var sbyteValues = new sbyte[] { -1, 0, 0, -4 };
		var ushortValues = new ushort[] { 0xAAAA, 0, 0, 0xBBBB };
		var shortValues = new short[] { -1, 0, 0, -2 };
		var uintValues = new uint[] { uint.MaxValue, 0, 0, uint.MaxValue };
		var intValues = new int[] { -1, 0, 0, -2 };
		var ulongValues = new ulong[] { ulong.MaxValue, 0, 0, ulong.MaxValue };
		var longValues = new long[] { -1, 0, 0, -2 };
		var floatValues = new float[] { -1, 0, 0, -2 };
		var doubleValues = new double[] { -1, 0, 0, -2 };

		reader.ReadFixedArray(byteValues, 1, 2);
		reader.ReadFixedArray(sbyteValues, 1, 2);
		reader.ReadFixedArray(ushortValues, 1, 2);
		reader.ReadFixedArray(shortValues, 1, 2);
		reader.ReadFixedArray(uintValues, 1, 2);
		reader.ReadFixedArray(intValues, 1, 2);
		reader.ReadFixedArray(ulongValues, 1, 2);
		reader.ReadFixedArray(longValues, 1, 2);
		reader.ReadFixedArray(floatValues, 1, 2);
		reader.ReadFixedArray(doubleValues, 1, 2);

		AssertFixedArrayRangeValues(
			byteValues,
			sbyteValues,
			ushortValues,
			shortValues,
			uintValues,
			intValues,
			ulongValues,
			longValues,
			floatValues,
			doubleValues);
	}

	static void AssertReadStreamFixedArrayRangeValues(EndianStream stream)
	{
		var byteValues = new byte[] { 0xA0, 0, 0, 0xA3 };
		var sbyteValues = new sbyte[] { -1, 0, 0, -4 };
		var ushortValues = new ushort[] { 0xAAAA, 0, 0, 0xBBBB };
		var shortValues = new short[] { -1, 0, 0, -2 };
		var uintValues = new uint[] { uint.MaxValue, 0, 0, uint.MaxValue };
		var intValues = new int[] { -1, 0, 0, -2 };
		var ulongValues = new ulong[] { ulong.MaxValue, 0, 0, ulong.MaxValue };
		var longValues = new long[] { -1, 0, 0, -2 };
		var floatValues = new float[] { -1, 0, 0, -2 };
		var doubleValues = new double[] { -1, 0, 0, -2 };

		stream.StreamFixedArray(byteValues, 1, 2);
		stream.StreamFixedArray(sbyteValues, 1, 2);
		stream.StreamFixedArray(ushortValues, 1, 2);
		stream.StreamFixedArray(shortValues, 1, 2);
		stream.StreamFixedArray(uintValues, 1, 2);
		stream.StreamFixedArray(intValues, 1, 2);
		stream.StreamFixedArray(ulongValues, 1, 2);
		stream.StreamFixedArray(longValues, 1, 2);
		stream.StreamFixedArray(floatValues, 1, 2);
		stream.StreamFixedArray(doubleValues, 1, 2);

		AssertFixedArrayRangeValues(
			byteValues,
			sbyteValues,
			ushortValues,
			shortValues,
			uintValues,
			intValues,
			ulongValues,
			longValues,
			floatValues,
			doubleValues);
	}

	static void AssertFixedArrayRangeValues(
		byte[] byteValues,
		sbyte[] sbyteValues,
		ushort[] ushortValues,
		short[] shortValues,
		uint[] uintValues,
		int[] intValues,
		ulong[] ulongValues,
		long[] longValues,
		float[] floatValues,
		double[] doubleValues)
	{
		CollectionAssert.AreEqual(new byte[] { 0xA0, 0x11, 0x22, 0xA3 }, byteValues);
		CollectionAssert.AreEqual(new sbyte[] { -1, 0x11, 0x22, -4 }, sbyteValues);
		CollectionAssert.AreEqual(new ushort[] { 0xAAAA, 0x1234, 0x5678, 0xBBBB }, ushortValues);
		CollectionAssert.AreEqual(new short[] { -1, unchecked((short)0x89AB), 0x1234, -2 }, shortValues);
		CollectionAssert.AreEqual(new uint[] { uint.MaxValue, 0x89ABCDEFU, 0x01234567U, uint.MaxValue }, uintValues);
		CollectionAssert.AreEqual(new int[] { -1, unchecked((int)0x89ABCDEF), 0x01234567, -2 }, intValues);
		CollectionAssert.AreEqual(new ulong[] {
			ulong.MaxValue,
			0x0123456789ABCDEFUL,
			0xFEDCBA9876543210UL,
			ulong.MaxValue,
		}, ulongValues);
		CollectionAssert.AreEqual(new long[] {
			-1,
			0x0123456789ABCDEFL,
			unchecked((long)0xFEDCBA9876543210UL),
			-2,
		}, longValues);
		Assert.AreEqual(0x3F800000, BitConverter.SingleToInt32Bits(floatValues[1]));
		Assert.AreEqual(unchecked((int)0xFFC00001), BitConverter.SingleToInt32Bits(floatValues[2]));
		Assert.AreEqual(0x3FF0000000000000L, BitConverter.DoubleToInt64Bits(doubleValues[1]));
		Assert.AreEqual(unchecked((long)0xFFF8000000000001UL), BitConverter.DoubleToInt64Bits(doubleValues[2]));
	}

#if CONTRACTS_FULL_SHIM
	static void AssertContractShimException(Action action)
	{
		var exception = Assert.Throws<Exception>(action);

		Assert.AreEqual("System.Diagnostics.ContractsShim.ContractShimException", exception.GetType().FullName);
	}
#endif

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

	static byte[] CreateBigEndianEndianStreamScalarBytes() => PrependByteScalars(CreateBigEndianPrimitiveBytes());

	static byte[] CreateLittleEndianEndianStreamScalarBytes() => PrependByteScalars(CreateLittleEndianPrimitiveBytes());

	static byte[] PrependByteScalars(byte[] primitiveBytes)
	{
		var bytes = new List<byte> { 0x7A, 0x85 };

		bytes.AddRange(primitiveBytes);
		return bytes.ToArray();
	}

	static byte[] CreateBigEndianFixedArrayRangeBytes() => new byte[] {
		0x11, 0x22,
		0x11, 0x22,
		0x12, 0x34, 0x56, 0x78,
		0x89, 0xAB, 0x12, 0x34,
		0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67,
		0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67,
		0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
		0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10,
		0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
		0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10,
		0x3F, 0x80, 0x00, 0x00,
		0xFF, 0xC0, 0x00, 0x01,
		0x3F, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0xFF, 0xF8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
	};

	static byte[] CreateLittleEndianFixedArrayRangeBytes() => new byte[] {
		0x11, 0x22,
		0x11, 0x22,
		0x34, 0x12, 0x78, 0x56,
		0xAB, 0x89, 0x34, 0x12,
		0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01,
		0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01,
		0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01,
		0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE,
		0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01,
		0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE,
		0x00, 0x00, 0x80, 0x3F,
		0x01, 0x00, 0xC0, 0xFF,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F,
		0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF8, 0xFF,
	};
}
