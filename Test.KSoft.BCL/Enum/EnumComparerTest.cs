using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Test
{
	[TestClass]
	public class EnumComparerTest : BaseTestClass
	{
		enum TestEnum : long
		{
			Zero,
			One,

			BigValueNeg = long.MinValue + 1,
			BigValuePos = long.MaxValue - 1,
		};

		[TestMethod]
		public void Enum_ComparerGetHashCodeTest()
		{
			var ksoft = EnumComparer.For<TestEnum>();
			var msnet = EqualityComparer<TestEnum>.Default;

			int ksoft_hashcode, msnet_hashcode;

			ksoft_hashcode = ksoft.GetHashCode(TestEnum.Zero);
			msnet_hashcode = msnet.GetHashCode(TestEnum.Zero);
			Assert.AreEqual(msnet_hashcode, ksoft_hashcode);

			ksoft_hashcode = ksoft.GetHashCode(TestEnum.One);
			msnet_hashcode = msnet.GetHashCode(TestEnum.One);
			Assert.AreEqual(msnet_hashcode, ksoft_hashcode);

			ksoft_hashcode = ksoft.GetHashCode(TestEnum.BigValueNeg);
			msnet_hashcode = msnet.GetHashCode(TestEnum.BigValueNeg);
			Assert.AreEqual(msnet_hashcode, ksoft_hashcode);

			ksoft_hashcode = ksoft.GetHashCode(TestEnum.BigValuePos);
			msnet_hashcode = msnet.GetHashCode(TestEnum.BigValuePos);
			Assert.AreEqual(msnet_hashcode, ksoft_hashcode);
		}

		[TestMethod]
		public void Enum_ComparerEqualsTest()
		{
			var ksoft = EnumComparer.For<TestEnum>();
			var msnet = EqualityComparer<TestEnum>.Default;

			bool ksoft_equals, msnet_equals;

			// BigValueNeg == BigValueNeg
			ksoft_equals = ksoft.Equals(TestEnum.BigValueNeg, TestEnum.BigValueNeg);
			msnet_equals = msnet.Equals(TestEnum.BigValueNeg, TestEnum.BigValueNeg);
			Assert.AreEqual(msnet_equals, ksoft_equals);

			// BigValuePos == BigValuePos
			ksoft_equals = ksoft.Equals(TestEnum.BigValuePos, TestEnum.BigValuePos);
			msnet_equals = msnet.Equals(TestEnum.BigValuePos, TestEnum.BigValuePos);
			Assert.AreEqual(msnet_equals, ksoft_equals);

			// BigValueNeg == BigValuePos
			ksoft_equals = ksoft.Equals(TestEnum.BigValueNeg, TestEnum.BigValuePos);
			msnet_equals = msnet.Equals(TestEnum.BigValueNeg, TestEnum.BigValuePos);
			Assert.AreEqual(msnet_equals, ksoft_equals);

			// BigValuePos == BigValueNeg
			ksoft_equals = ksoft.Equals(TestEnum.BigValuePos, TestEnum.BigValueNeg);
			msnet_equals = msnet.Equals(TestEnum.BigValuePos, TestEnum.BigValueNeg);
			Assert.AreEqual(msnet_equals, ksoft_equals);
		}

		[TestMethod]
		public void Enum_ComparerTest()
		{
			var ksoft = EnumComparer.For<TestEnum>();
			var msnet = Comparer<TestEnum>.Default;

			int ksoft_equals, msnet_equals;

			// BigValueNeg == BigValueNeg
			ksoft_equals = ksoft.Compare(TestEnum.BigValueNeg, TestEnum.BigValueNeg);
			msnet_equals = msnet.Compare(TestEnum.BigValueNeg, TestEnum.BigValueNeg);
			Assert.AreEqual(msnet_equals, ksoft_equals);

			// BigValuePos == BigValuePos
			ksoft_equals = ksoft.Compare(TestEnum.BigValuePos, TestEnum.BigValuePos);
			msnet_equals = msnet.Compare(TestEnum.BigValuePos, TestEnum.BigValuePos);
			Assert.AreEqual(msnet_equals, ksoft_equals);

			// BigValueNeg == BigValuePos
			ksoft_equals = ksoft.Compare(TestEnum.BigValueNeg, TestEnum.BigValuePos);
			msnet_equals = msnet.Compare(TestEnum.BigValueNeg, TestEnum.BigValuePos);
			Assert.AreEqual(msnet_equals, ksoft_equals);

			// BigValuePos == BigValueNeg
			ksoft_equals = ksoft.Compare(TestEnum.BigValuePos, TestEnum.BigValueNeg);
			msnet_equals = msnet.Compare(TestEnum.BigValuePos, TestEnum.BigValueNeg);
			Assert.AreEqual(msnet_equals, ksoft_equals);
		}
	};
}