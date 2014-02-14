using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Test
{
	[TestClass]
	public class EnumFlagsTest : BaseTestClass
	{
		enum FlagsEnumSansAttribute
		{
			Flag0 = 1<<0,
			Flag1 = 1<<1,
			Flag2 = 1<<2,
			Flag3 = 1<<3,
		};

		[TestMethod]
		public void Enum_FlagsSansAttributeTest()
		{
			try {
				FlagsEnumSansAttribute e = FlagsEnumSansAttribute.Flag0;
				// The below statement should throw a TypeInitializationException
				EnumFlags.Add(ref e, FlagsEnumSansAttribute.Flag2);

				Assert.Fail("EnumFlags didn't fail on an Enum without a Flags attribute!");
			} catch (System.Exception ex) {
				Assert.IsInstanceOfType(ex.InnerException, typeof(System.NotSupportedException));
			}
		}

		[System.Flags]
		enum FlagsEnum
		{
			Flag0 = 1<<0,
			Flag1 = 1<<1,
			Flag2 = 1<<2,
			Flag3 = 1<<3,
		};

		[TestMethod]
		public void Enum_FlagsAddTest()
		{
			//////////////////////////////////////////////////////////////////////////
			// Test by-ref
			const FlagsEnum kExpectedResult1 = FlagsEnum.Flag0 | FlagsEnum.Flag2;
			FlagsEnum e1 = FlagsEnum.Flag0;
			EnumFlags.Add(ref e1, FlagsEnum.Flag2);
			Assert.AreEqual(kExpectedResult1, e1);
			//////////////////////////////////////////////////////////////////////////
			// Test by-value
			const FlagsEnum kExpectedResult2 = kExpectedResult1 | FlagsEnum.Flag3;
			FlagsEnum e2 = EnumFlags.Add(e1, FlagsEnum.Flag3);
			Assert.AreEqual(kExpectedResult2, e2);
		}
		[TestMethod]
		public void Enum_FlagsRemoveTest()
		{
			//////////////////////////////////////////////////////////////////////////
			// Test by-ref
			const FlagsEnum kExpectedResult1 = FlagsEnum.Flag0;
			FlagsEnum e1 = FlagsEnum.Flag0 | FlagsEnum.Flag2;
			EnumFlags.Remove(ref e1, FlagsEnum.Flag2);
			Assert.AreEqual(kExpectedResult1, e1);
			//////////////////////////////////////////////////////////////////////////
			// Test by-value
			const FlagsEnum kExpectedResult2 = 0;
			FlagsEnum e2 = EnumFlags.Remove(e1, FlagsEnum.Flag0);
			Assert.AreEqual(kExpectedResult2, e2);
		}
		[TestMethod]
		public void Enum_FlagsModifyTest()
		{
			//////////////////////////////////////////////////////////////////////////
			// Test by-ref
			const FlagsEnum kExpectedResult1 = FlagsEnum.Flag0 | FlagsEnum.Flag2;
			FlagsEnum e1 = FlagsEnum.Flag0;
			EnumFlags.Modify(true, ref e1, FlagsEnum.Flag2);
			Assert.AreEqual(kExpectedResult1, e1);
			//////////////////////////////////////////////////////////////////////////
			// Test by-value
			const FlagsEnum kExpectedResult2 = FlagsEnum.Flag2;
			FlagsEnum e2 = EnumFlags.Modify(false, e1, FlagsEnum.Flag0);
			Assert.AreEqual(kExpectedResult2, e2);
		}
		[TestMethod]
		public void Enum_FlagsTest()
		{
			FlagsEnum e = FlagsEnum.Flag0 | FlagsEnum.Flag2;
			Assert.IsTrue(EnumFlags.Test(e, FlagsEnum.Flag2));
			Assert.IsTrue(EnumFlags.Test(e, FlagsEnum.Flag0 | FlagsEnum.Flag2));
		}
	};
}