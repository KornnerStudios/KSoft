using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Values.Test;

[TestClass]
public sealed class KGuidTest : BaseTestClass
{

	[TestMethod]
	public void Version_GuidVersionField_ReturnsUuidVersion()
	{
		var guid = new KGuid("f81d4fae-7dec-11d0-a765-00a0c91e6bf6");

		Assert.AreEqual(UuidVersion.TimeBased, guid.Version);
	}

	[TestMethod]
	public void Version_Version7Guid_ReturnsUnixEpochTimeBased()
	{
		var guid = new KGuid(Guid.CreateVersion7());

		Assert.AreEqual(UuidVersion.UnixEpochTimeBased, guid.Version);
	}

	[TestMethod]
	public void TimeBasedProperties_TimeBasedGuid_ReturnDecodedFields()
	{
		var guid = new KGuid("f81d4fae-7dec-11d0-a765-00a0c91e6bf6");

		Assert.AreEqual(0x1D07DECF81D4FAEL, guid.Timestamp);
		Assert.AreEqual(0x2765, guid.ClockSequence);
		Assert.AreEqual(0x00A0C91E6BF6L, guid.Node);
	}

	[TestMethod]
	public void TimeBasedProperties_NonTimeBasedGuid_ThrowInvalidOperationException()
	{
		var guid = new KGuid("00112233-4455-4677-8899-aabbccddeeff");

		Assert.ThrowsExactly<InvalidOperationException>(() => _ = guid.Timestamp);
		Assert.ThrowsExactly<InvalidOperationException>(() => _ = guid.ClockSequence);
		Assert.ThrowsExactly<InvalidOperationException>(() => _ = guid.Node);
	}

	[TestMethod]
	public void Constructor_Bytes16_RoundTripsExactLayout()
	{
		var expected = Guid.Parse("00112233-4455-6677-8899-aabbccddeeff");
		byte[] bytes = expected.ToByteArray();

		var guid = new KGuid(bytes);

		Assert.AreEqual(expected, guid.ToGuid());
		CollectionAssert.AreEqual(bytes, guid.ToByteArray());
	}

	[TestMethod]
	public void Constructor_Bytes15_ThrowsArgumentException()
	{
		AssertThrowsArgument("bytes", () => _ = new KGuid(new byte[15]));
	}

	[TestMethod]
	public void Constructor_Bytes17_ThrowsArgumentException()
	{
		AssertThrowsArgument("bytes", () => _ = new KGuid(new byte[17]));
	}

	[TestMethod]
	public void Constructor_BytesMutatedAfterConstruction_DoesNotAffectGuid()
	{
		byte[] bytes = Guid.Parse("00112233-4455-6677-8899-aabbccddeeff").ToByteArray();
		var expected = (byte[])bytes.Clone();
		var guid = new KGuid(bytes);

		Array.Clear(bytes);

		CollectionAssert.AreEqual(expected, guid.ToByteArray());
	}

	[TestMethod]
	public void Constructor_ComponentTailBytes8_ConstructsExpectedGuid()
	{
		byte[] tail = [0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF];

		var guid = new KGuid(0x00112233, 0x4455, 0x6677, tail);

		Assert.AreEqual(Guid.Parse("00112233-4455-6677-8899-aabbccddeeff"), guid.ToGuid());
	}

	[TestMethod]
	public void Constructor_ComponentTailBytes7_ThrowsArgumentException()
	{
		AssertThrowsArgument("d", () => _ = new KGuid(0, 0, 0, new byte[7]));
	}

	[TestMethod]
	public void Constructor_ComponentTailBytes9_ThrowsArgumentException()
	{
		AssertThrowsArgument("d", () => _ = new KGuid(0, 0, 0, new byte[9]));
	}

	[TestMethod]
	public void Constructor_ComponentTailMutatedAfterConstruction_DoesNotAffectGuid()
	{
		byte[] tail = [0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF];
		var guid = new KGuid(0x00112233, 0x4455, 0x6677, tail);
		var expected = guid.ToGuid();

		Array.Clear(tail);

		Assert.AreEqual(expected, guid.ToGuid());
	}

	[TestMethod]
	public void ToByteBuffer_ExactSizeBuffer_WritesGuidBytes()
	{
		var guid = new KGuid("00112233-4455-6677-8899-aabbccddeeff");
		var buffer = new byte[KGuid.kSizeOf];

		guid.ToByteBuffer(buffer);

		CollectionAssert.AreEqual(guid.ToByteArray(), buffer);
	}

	[TestMethod]
	public void ToByteBuffer_OversizedBuffer_WritesPrefixAndPreservesSuffix()
	{
		var guid = new KGuid("00112233-4455-6677-8899-aabbccddeeff");
		var buffer = new byte[KGuid.kSizeOf + 8];
		var expected = new byte[buffer.Length];

		Array.Fill(buffer, (byte)0xCC);
		Array.Fill(expected, (byte)0xCC);
		Array.Copy(guid.ToByteArray(), 0, expected, 0, KGuid.kSizeOf);

		guid.ToByteBuffer(buffer);

		CollectionAssert.AreEqual(expected, buffer);
	}

	[TestMethod]
	public void ToByteBuffer_BufferTooSmall_ThrowsArgumentOutOfRangeException()
	{
		var guid = new KGuid("00112233-4455-6677-8899-aabbccddeeff");
		var buffer = new byte[KGuid.kSizeOf - 1];

		Array.Fill(buffer, (byte)0xCC);
		var expected = (byte[])buffer.Clone();

		AssertThrowsArgumentOutOfRange("buffer", () => guid.ToByteBuffer(buffer));

		// The undersized destination must be rejected before any byte is written.
		CollectionAssert.AreEqual(expected, buffer);
	}

	[TestMethod]
	public void ToByteArray_CalledTwice_ReturnsIndependentDurableArrays()
	{
		var guid = new KGuid("00112233-4455-6677-8899-aabbccddeeff");

		var first = guid.ToByteArray();
		Array.Clear(first);
		var second = guid.ToByteArray();

		CollectionAssert.AreNotEqual(first, second);
		CollectionAssert.AreEqual(Guid.Parse("00112233-4455-6677-8899-aabbccddeeff").ToByteArray(), second);
	}
}
