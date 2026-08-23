using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Collections.Test;

[TestClass]
public sealed class StringSegmentTest : BaseTestClass
{
	static void AssertThrowsArgumentNull(Action action)
	{
		var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

		Assert.AreEqual("data", exception.ParamName);
	}

	static void AssertThrowsArgumentOutOfRange(Action action, string paramName)
	{
		var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

		Assert.AreEqual(paramName, exception.ParamName);
	}

	[TestMethod]
	public void Constructors_NullData_ThrowArgumentNullException()
	{
		AssertThrowsArgumentNull(() => _ = new StringSegment(null!));
		AssertThrowsArgumentNull(() => _ = new StringSegment(null!, 0, 0));
	}

	[TestMethod]
	public void RangeConstructor_NegativeOffsetOrCount_ThrowArgumentOutOfRangeException()
	{
		AssertThrowsArgumentOutOfRange(() => _ = new StringSegment("abc", -1, 0), "offset");
		AssertThrowsArgumentOutOfRange(() => _ = new StringSegment("abc", 0, -1), "count");
	}

	[TestMethod]
	public void RangeConstructor_CountAtOrPastRemainingLength_ThrowsArgumentException()
	{
		var exactEndException = Assert.ThrowsExactly<ArgumentException>(() => _ = new StringSegment("abc", 0, 3));
		var offsetPastEndException = Assert.ThrowsExactly<ArgumentException>(() => _ = new StringSegment("abc", 4, 0));

		Assert.AreEqual("count", exactEndException.ParamName);
		Assert.AreEqual("count", offsetPastEndException.ParamName);
	}

	[TestMethod]
	public void Constructors_ValidData_InitializeSegment()
	{
		var whole = new StringSegment("abc");
		var segment = new StringSegment("abc", 1, 1);

		Assert.AreEqual("abc", whole.Data);
		Assert.AreEqual(0, whole.Offset);
		Assert.AreEqual(3, whole.Count);
		Assert.AreEqual(1, segment.Offset);
		Assert.AreEqual(1, segment.Count);
		Assert.AreEqual('b', segment[0]);
	}


	[TestMethod]
	public void Equality_NullAndMatchingSegments_HaveExpectedResults()
	{
		var lhs = new StringSegment("abc", 1, 1);
		var rhs = new StringSegment("abc", 1, 1);

		Assert.IsTrue(lhs.Equals(rhs));
		Assert.IsTrue(lhs.Equals((object)rhs));
		Assert.IsFalse(lhs.Equals((object?)null));
		Assert.IsFalse(lhs.Equals("abc"));
		Assert.IsTrue(lhs == rhs);
		Assert.IsFalse(lhs != rhs);
	}

}
