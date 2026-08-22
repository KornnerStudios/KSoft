using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Test
{
	[TestClass]
	public sealed class PredicatesTest : BaseTestClass
	{
		[TestMethod]
		public void NullAcceptingPredicates_ReturnFalseForNull()
		{
			Assert.IsFalse(Predicates.IsNotNull<string>(null!));
			Assert.IsFalse(Predicates.IsNotNullOrEmpty(null!));
			Assert.IsFalse(Predicates.HasItems(null!));
			Assert.IsFalse(Predicates.HasBits(null!));
		}

		[TestMethod]
		public void CachedPredicates_ReturnExpectedValues()
		{
			Assert.IsTrue(Predicates.IsTrue(true));
			Assert.IsFalse(Predicates.IsFalse(true));
			Assert.IsTrue(Predicates.IsNotNullOrEmpty("value"));
			Assert.IsFalse(Predicates.IsNotNullOrEmpty(string.Empty));
			Assert.IsTrue(Predicates.HasItems(new List<int> { 1 }));
			Assert.IsFalse(Predicates.HasItems(new List<int>()));
		}
	}
}
