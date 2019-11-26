using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Collections.Test
{
	[TestClass]
	public class ValueTypeComparersTest : BaseTestClass
	{
#if false // This is aspect programming. Bare Attributes can't be used like this.
		// However, unlike with KSoft.dll's types, these attributes ran automatically (must be due to VS getting the custom attributes on everything in the test assembly)
		[InitializeValueTypeComparer(typeof(TestValueType))]
		[InitializeValueTypeEqualityComparer(typeof(TestValueType))]
#endif
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
			var dot_net_version = Environment.Version;
			Assert.IsTrue(dot_net_version.Major <= 2);

			Util.ValueTypeInitializeComparer<TestValueType>();

			var comparer = Comparer<TestValueType>.Default;
			Assert.IsInstanceOfType(comparer, typeof(ValueTypeComparer<TestValueType>));
		}

		[TestMethod]
		public void Collections_ValueTypeEqualityComparerStaticCtorTest()
		{
			var dot_net_version = Environment.Version;
			Assert.IsTrue(dot_net_version.Major <= 2);

			// REMINDER: This overwrites ValueTypeEquatableComparer's changes
			Util.ValueTypeInitializeEqualityComparer<TestValueType>();

			var comparer = EqualityComparer<TestValueType>.Default;
			Assert.IsInstanceOfType(comparer, typeof(ValueTypeEqualityComparer<TestValueType>));
		}

		[TestMethod]
		public void Collections_ValueTypeEquatableComparerStaticCtorTest()
		{
			var dot_net_version = Environment.Version;
			Assert.IsTrue(dot_net_version.Major <= 2);

			// REMINDER: This overwrites ValueTypeEqualityComparer's changes
			Util.ValueTypeInitializeEquatableComparer<TestValueType>();

			var comparer = EqualityComparer<TestValueType>.Default;
			Assert.IsInstanceOfType(comparer, typeof(ValueTypeEquatableComparer<TestValueType>));
		}

#if false // This is aspect programming. Bare Attributes can't be used like this.
		[TestMethod]
		public void Collections_InitializeValueTypeComparerTest()
		{
			var comparer = Comparer<TestValueType>.Default;
			Assert.IsInstanceOfType(comparer, typeof(ValueTypeComparer<TestValueType>));
		}

		[TestMethod]
		public void Collections_InitializeValueTypeEqualityComparerTest()
		{
			var comparer = EqualityComparer<TestValueType>.Default;
			Assert.IsInstanceOfType(comparer, typeof(ValueTypeEqualityComparer<TestValueType>));
		}
#endif
	};
}