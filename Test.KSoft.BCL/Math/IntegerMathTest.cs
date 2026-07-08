using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Test;

[TestClass]
public class IntegerMathTest : BaseTestClass
{
	[TestMethod]
	public void FloorLog2MatchesBitOperationsAndKeepsZeroSentinelTest()
	{
		// FloorLog2(0) is KSoft's sentinel. Non-zero values should match the BCL floor-log primitive.
		Assert.AreEqual(-1, IntegerMath.FloorLog2((byte)0));
		Assert.AreEqual(-1, IntegerMath.FloorLog2((ushort)0));
		Assert.AreEqual(-1, IntegerMath.FloorLog2(0U));
		Assert.AreEqual(-1, IntegerMath.FloorLog2(0UL));

		AssertFloorLog2((byte)1);
		AssertFloorLog2((byte)2);
		AssertFloorLog2((byte)3);
		AssertFloorLog2(byte.MaxValue);

		AssertFloorLog2((ushort)1);
		AssertFloorLog2((ushort)2);
		AssertFloorLog2((ushort)3);
		AssertFloorLog2((ushort)0x8000);
		AssertFloorLog2(ushort.MaxValue);

		AssertFloorLog2(1U);
		AssertFloorLog2(2U);
		AssertFloorLog2(3U);
		AssertFloorLog2(0x80000000U);
		AssertFloorLog2(uint.MaxValue);

		AssertFloorLog2(1UL);
		AssertFloorLog2(2UL);
		AssertFloorLog2(3UL);
		AssertFloorLog2(1UL << 32);
		AssertFloorLog2(ulong.MaxValue);
	}

	static void AssertFloorLog2(byte value)
		=> Assert.AreEqual(BitOperations.Log2((uint)value), IntegerMath.FloorLog2(value));

	static void AssertFloorLog2(ushort value)
		=> Assert.AreEqual(BitOperations.Log2((uint)value), IntegerMath.FloorLog2(value));

	static void AssertFloorLog2(uint value)
		=> Assert.AreEqual(BitOperations.Log2(value), IntegerMath.FloorLog2(value));

	static void AssertFloorLog2(ulong value)
		=> Assert.AreEqual(BitOperations.Log2(value), IntegerMath.FloorLog2(value));
}
