using System;
using KSoft.Shell;
using KSoft.Values;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Memory.Test;

[TestClass]
public sealed class VirtualAddressTranslationStackTest : BaseTestClass
{
	static void AssertThrowsArgumentOutOfRange(string parameterName, Action action)
	{
		var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

		Assert.AreEqual(parameterName, exception.ParamName);
	}

	[TestMethod]
	public void Constructor_InvalidProcessorSize_ThrowsArgumentOutOfRangeException()
	{
		AssertThrowsArgumentOutOfRange("ptrSize", () =>
			new VirtualAddressTranslationStack(ProcessorSize.AnyCPU));
	}

	[TestMethod]
	public void Constructor_ProcessorSize_SelectsNullAddressSize()
	{
		var stack32 = new VirtualAddressTranslationStack(ProcessorSize.x32);
		var stack64 = new VirtualAddressTranslationStack(ProcessorSize.x64);

		Assert.AreEqual(ProcessorSize.x32, stack32.CurrentAddress.Size);
		Assert.IsTrue(stack32.CurrentAddress.IsNull);
		Assert.AreEqual(ProcessorSize.x64, stack64.CurrentAddress.Size);
		Assert.IsTrue(stack64.CurrentAddress.IsNull);
	}

	[TestMethod]
	public void PushAndPopPhysicalAddress_UpdatesCurrentAddress()
	{
		var stack = new VirtualAddressTranslationStack(ProcessorSize.x32, capacity: 0);
		var firstAddress = new PtrHandle(ProcessorSize.x32, 0x1000);
		var secondAddress = new PtrHandle(ProcessorSize.x32, 0x2000);

		stack.PushPhysicalAddress(firstAddress);
		stack.PushPhysicalAddress(secondAddress);

		Assert.AreEqual(secondAddress, stack.CurrentAddress);
		Assert.AreEqual(secondAddress, stack.PopPhysicalAddress());
		Assert.AreEqual(firstAddress, stack.CurrentAddress);
		Assert.AreEqual(firstAddress, stack.PopPhysicalAddress());
		Assert.IsTrue(stack.CurrentAddress.IsNull);
		Assert.AreEqual(ProcessorSize.x32, stack.CurrentAddress.Size);
	}

	[TestMethod]
	public void PushPhysicalAddressOffset_AddsOffsetToCurrentAddress()
	{
		var stack = new VirtualAddressTranslationStack(ProcessorSize.x64);

		stack.PushPhysicalAddress(new PtrHandle(ProcessorSize.x64, 0x1000));
		stack.PushPhysicalAddressOffset(new PtrHandle(ProcessorSize.x64, 0x20));

		Assert.AreEqual(new PtrHandle(ProcessorSize.x64, 0x1020), stack.CurrentAddress);
	}
}
