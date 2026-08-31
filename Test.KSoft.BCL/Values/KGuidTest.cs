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
	public void ToByteBuffer_NullBuffer_ThrowsArgumentNullException()
	{
		var guid = new KGuid("00112233-4455-6677-8899-aabbccddeeff");

		AssertThrowsArgumentNull("buffer", () => guid.ToByteBuffer(null!));
	}

	[TestMethod]
	public void ToByteBuffer_InvalidIndex_ThrowsArgumentOutOfRangeException()
	{
		var guid = new KGuid("00112233-4455-6677-8899-aabbccddeeff");

		AssertThrowsArgumentOutOfRange("index", () => guid.ToByteBuffer(new byte[KGuid.kSizeOf], -1));
		AssertThrowsArgumentOutOfRange("index", () => guid.ToByteBuffer(new byte[KGuid.kSizeOf], 1));
	}

	[TestMethod]
	public void ToByteBuffer_WithIndex_WritesGuidBytesAtOffset()
	{
		var guid = new KGuid("00112233-4455-6677-8899-aabbccddeeff");
		var buffer = new byte[KGuid.kSizeOf + 8];
		var expected = new byte[buffer.Length];

		Array.Fill(buffer, (byte)0xCC);
		Array.Fill(expected, (byte)0xCC);
		Array.Copy(guid.ToByteArray(), 0, expected, 4, KGuid.kSizeOf);

		guid.ToByteBuffer(buffer, 4);

		CollectionAssert.AreEqual(expected, buffer);
	}
}
