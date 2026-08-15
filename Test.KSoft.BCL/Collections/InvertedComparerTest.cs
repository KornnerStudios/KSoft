using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Collections.Test;

[TestClass]
public sealed class InvertedComparerTest : BaseTestClass
{
	static void AssertThrowsArgumentNull(string parameterName, Action action)
	{
		var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

		Assert.AreEqual(parameterName, exception.ParamName);
	}

	[TestMethod]
	public void InvertedComparer_NullComparer_ThrowsArgumentNullException()
	{
		AssertThrowsArgumentNull("comparer", () => new InvertedComparer(null));
		AssertThrowsArgumentNull("comparer", () => new InvertedComparer<int>(null));
	}

	[TestMethod]
	public void InvertedComparer_Compare_InvertsNonGenericComparer()
	{
		var comparer = new InvertedComparer(Comparer.DefaultInvariant);

		Assert.IsGreaterThan(0, comparer.Compare("a", "b"));
		Assert.IsLessThan(0, comparer.Compare("b", "a"));
		Assert.AreEqual(0, comparer.Compare("a", "a"));
	}

	[TestMethod]
	public void InvertedComparer_Compare_InvertsGenericComparer()
	{
		var comparer = new InvertedComparer<int>(Comparer<int>.Default);

		Assert.IsGreaterThan(0, comparer.Compare(1, 2));
		Assert.IsLessThan(0, comparer.Compare(2, 1));
		Assert.AreEqual(0, comparer.Compare(1, 1));
	}
}
