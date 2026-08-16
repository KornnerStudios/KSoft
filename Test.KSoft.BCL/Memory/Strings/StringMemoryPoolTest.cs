using System;
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
}
