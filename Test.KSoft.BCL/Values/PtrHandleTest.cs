using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Values.Test;

[TestClass]
public sealed class PtrHandleTest : BaseTestClass
{
	[TestMethod]
	public void Add_SameSizeHandles_ReturnsMatchingSizeSum()
	{
		var left32 = new PtrHandle(0x1000U);
		var right32 = new PtrHandle(0x20U);
		var result32 = PtrHandle.Add(left32, right32);

		Assert.IsFalse(result32.Is64bit);
		Assert.AreEqual(0x1020U, result32.u32);

		var left64 = new PtrHandle(0x1_0000_0000UL);
		var right64 = new PtrHandle(0x20UL);
		var result64 = left64 + right64;

		Assert.IsTrue(result64.Is64bit);
		Assert.AreEqual(0x1_0000_0020UL, result64.u64);
	}

	[TestMethod]
	public void Subtract_SameSizeHandles_ReturnsMatchingSizeDifference()
	{
		var left32 = new PtrHandle(0x1020U);
		var right32 = new PtrHandle(0x20U);
		var result32 = PtrHandle.Subtract(left32, right32);

		Assert.IsFalse(result32.Is64bit);
		Assert.AreEqual(0x1000U, result32.u32);

		var left64 = new PtrHandle(0x1_0000_0020UL);
		var right64 = new PtrHandle(0x20UL);
		var result64 = left64 - right64;

		Assert.IsTrue(result64.Is64bit);
		Assert.AreEqual(0x1_0000_0000UL, result64.u64);
	}

	[TestMethod]
	public void AddOrSubtract_DifferentSizeHandles_ThrowsInvalidOperationException()
	{
		var handle32 = new PtrHandle(0x1000U);
		var handle64 = new PtrHandle(0x1000UL);

		Assert.ThrowsExactly<InvalidOperationException>(() => PtrHandle.Add(handle32, handle64));
		Assert.ThrowsExactly<InvalidOperationException>(() => _ = handle32 + handle64);
		Assert.ThrowsExactly<InvalidOperationException>(() => PtrHandle.Subtract(handle32, handle64));
		Assert.ThrowsExactly<InvalidOperationException>(() => _ = handle32 - handle64);
	}

	[TestMethod]
	public void AddOrSubtract_ScalarOffset_PreservesHandleSize()
	{
		var handle32 = new PtrHandle(0x1000U);
		var result32 = handle32 + 0x20U;

		Assert.IsFalse(result32.Is64bit);
		Assert.AreEqual(0x1020U, result32.u32);

		var handle64 = new PtrHandle(0x1_0000_0020UL);
		var result64 = handle64 - 0x20U;

		Assert.IsTrue(result64.Is64bit);
		Assert.AreEqual(0x1_0000_0000UL, result64.u64);
	}
}
