using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Collections.Test
{
	[TestClass]
	public class ValueTypeComparersTest : BaseTestClass
	{
		struct TestValueType :
			System.Collections.IComparer, IComparer<TestValueType>,
			IEquatable<TestValueType>, IEqualityComparer<TestValueType>
		{
			#region IComparer<TestValueType> Members
			int System.Collections.IComparer.Compare(object x, object y)
			{
				return 0;
			}
			int IComparer<TestValueType>.Compare(TestValueType x, TestValueType y)
			{
				return 0;
			}
			#endregion

			#region IEquatable<TestValueType> Members
			bool IEquatable<TestValueType>.Equals(TestValueType other)
			{
				return true;
			}
			#endregion

			#region IEqualityComparer<TestValueType> Members
			public bool Equals(TestValueType x, TestValueType y)
			{
				return true;
			}

			public int GetHashCode(TestValueType obj)
			{
				return 0;
			}
			#endregion
		};

		[TestMethod]
		public void Collections_ValueTypeComparerStaticCtorTest()
		{
			var comparer = ValueTypeComparer<TestValueType>.Default;
			Assert.IsNotNull(comparer);
			Assert.IsInstanceOfType(comparer, typeof(ValueTypeComparer<TestValueType>));
			comparer = Comparer<TestValueType>.Default;
			Assert.IsInstanceOfType(comparer, typeof(ValueTypeComparer<TestValueType>));
		}

		[TestMethod]
		public void Collections_ValueTypeEqualityComparerStaticCtorTest()
		{
			var comparer = ValueTypeEqualityComparer<TestValueType>.Default;
			Assert.IsNotNull(comparer);
			Assert.IsInstanceOfType(comparer, typeof(ValueTypeEqualityComparer<TestValueType>));
			comparer = EqualityComparer<TestValueType>.Default;
			Assert.IsInstanceOfType(comparer, typeof(ValueTypeEqualityComparer<TestValueType>));
		}

		[TestMethod]
		public void Collections_ValueTypeEquatableComparerStaticCtorTest()
		{
			var comparer = ValueTypeEquatableComparer<TestValueType>.Default;
			Assert.IsNotNull(comparer);
			Assert.IsInstanceOfType(comparer, typeof(ValueTypeEquatableComparer<TestValueType>));
			comparer = EqualityComparer<TestValueType>.Default;
			Assert.IsInstanceOfType(comparer, typeof(ValueTypeEquatableComparer<TestValueType>));
		}
	};
}