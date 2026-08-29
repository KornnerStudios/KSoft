using System;
using System.Collections.Generic;
using System.IO;
using KSoft.Memory.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Memory.Strings.Test;

[TestClass]
public sealed class StringMemoryPoolTest : BaseTestClass
{
	static StringMemoryPool CreatePool()
	{
		var settings = new StringMemoryPoolSettings(StringStorage.CStringAscii, false, Shell.ProcessorSize.x32);

		return new StringMemoryPool(settings);
	}

	static void AssertThrowsArgumentNull(Action action, string paramName)
	{
		var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

		Assert.AreEqual(paramName, exception.ParamName);
	}

	[TestMethod]
	public void StreamMethods_NullStreams_ThrowArgumentNullException()
	{
		var pool = CreatePool();

		AssertThrowsArgumentNull(() => pool.ReadHeader(null!), "s");
		AssertThrowsArgumentNull(() => pool.WriteHeader(null!), "s");
		AssertThrowsArgumentNull(() => pool.ReadStringCharacterLengths(null!), "s");
		AssertThrowsArgumentNull(() => pool.WriteStringCharacterLengths(null!), "s");
		AssertThrowsArgumentNull(() => pool.WriteStringByteLengths(null!), "s");
		AssertThrowsArgumentNull(() => pool.ReadReferences(null!), "s");
		AssertThrowsArgumentNull(() => pool.WriteReferences(null!), "s");
		AssertThrowsArgumentNull(() => pool.ReadStrings(null!), "s");
		AssertThrowsArgumentNull(() => pool.WriteStrings(null!), "s");
		AssertThrowsArgumentNull(() => pool.Read(null!), "s");
		AssertThrowsArgumentNull(() => pool.Write(null!), "s");
	}

	[TestMethod]
	public void GetAddress_ReturnsStoredReferenceAndInvalidForMissingValues()
	{
		var settings = new StringMemoryPoolSettings(
			StringStorage.CStringAscii, false, new KSoft.Values.PtrHandle(0x1000U));
		var pool = new StringMemoryPool(settings);

		var address = pool.Add("value");

		Assert.AreEqual(address, pool.GetAddress("value"));
		Assert.AreEqual(StringMemoryPool.kInvalidReference, pool.GetAddress("missing"));
	}

	[TestMethod]
	public void GetAddress_DuplicatePoolReturnsFirstReferenceAndInvalidForMissingValues()
	{
		var pool = CreatePool();
		pool.Settings.AllowDuplicates = true;

		var firstAddress = pool.Add("value");
		_ = pool.Add("value");

		Assert.AreEqual(firstAddress, pool.GetAddress("value"));
		Assert.AreEqual(StringMemoryPool.kInvalidReference, pool.GetAddress("missing"));
	}

	[TestMethod]
	public void WriteAndRead_RoundTripsAsciiStringsAndReferences()
	{
		var settings = new StringMemoryPoolSettings(
			StringStorage.CStringAscii, false, new KSoft.Values.PtrHandle(0x4000U));
		var pool = new StringMemoryPool(settings);
		var firstAddress = pool.Add("first");
		var secondAddress = pool.Add("second");

		using var stream = new MemoryStream();
		using (var writer = new KSoft.IO.EndianWriter(stream) { BaseStreamOwner = false })
		{
			pool.Write(writer);
		}

		stream.Position = 0;
		var roundTripped = new StringMemoryPool(settings);
		using (var reader = new KSoft.IO.EndianReader(stream) { BaseStreamOwner = false })
		{
			roundTripped.Read(reader);
		}

		Assert.AreEqual(2, roundTripped.Count);
		CollectionAssert.AreEqual(new[] { "first", "second" }, new List<string>(roundTripped));
		Assert.IsTrue(roundTripped.Contains("first"));
		Assert.IsTrue(roundTripped.Contains("second"));
		Assert.AreEqual(firstAddress, roundTripped.GetAddress("first"));
		Assert.AreEqual(secondAddress, roundTripped.GetAddress("second"));
		Assert.AreEqual("first", roundTripped.Get(firstAddress));
		Assert.AreEqual("second", roundTripped.Get(secondAddress));
	}

	[TestMethod]
	public void WriteAndRead_RestoresExplicitNullReference()
	{
		var settings = new StringMemoryPoolSettings(
			StringStorage.CStringAscii, false, new KSoft.Values.PtrHandle(0x4000U));
		var pool = new StringMemoryPool(settings);
		var nullAddress = pool.Add("");
		_ = pool.Add("value");

		using var stream = new MemoryStream();
		using (var writer = new KSoft.IO.EndianWriter(stream) { BaseStreamOwner = false })
		{
			pool.Write(writer);
		}

		stream.Position = 0;
		var roundTripped = new StringMemoryPool(settings);
		using (var reader = new KSoft.IO.EndianReader(stream) { BaseStreamOwner = false })
		{
			roundTripped.Read(reader);
		}

		Assert.AreEqual(nullAddress, roundTripped.GetNull());
		Assert.AreEqual(nullAddress, roundTripped.GetAddress(""));
		Assert.AreEqual(nullAddress, roundTripped.Add(""));
		Assert.AreEqual(2, roundTripped.Count);
	}
}
