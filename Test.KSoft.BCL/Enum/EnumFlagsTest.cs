using System;
using System.Diagnostics.CodeAnalysis;
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

		[Flags]
		enum FlagsEnum
		{
			Flag0 = 1<<0,
			Flag1 = 1<<1,
			Flag2 = 1<<2,
			Flag3 = 1<<3,
		};

		[Flags] enum FlagsEnumByte : byte { Flag0 = 1<<0, Flag1 = 1<<1, Flag7 = 1<<7 };
		[Flags] enum FlagsEnumSByte : sbyte { Flag0 = 1<<0, Flag1 = 1<<1, Flag6 = 1<<6 };
		[Flags] enum FlagsEnumUInt16 : ushort { Flag0 = 1<<0, Flag1 = 1<<1, Flag15 = 1<<15 };
		[Flags] enum FlagsEnumInt16 : short { Flag0 = 1<<0, Flag1 = 1<<1, Flag14 = 1<<14 };
		[Flags] enum FlagsEnumUInt32 : uint { Flag0 = 1U<<0, Flag1 = 1U<<1, Flag31 = 1U<<31 };
		[Flags] enum FlagsEnumInt32 : int { Flag0 = 1<<0, Flag1 = 1<<1, Flag30 = 1<<30 };
		[Flags] enum FlagsEnumUInt64 : ulong { Flag0 = 1UL<<0, Flag1 = 1UL<<1, Flag63 = 1UL<<63 };
		[Flags] enum FlagsEnumInt64 : long { Flag0 = 1L<<0, Flag1 = 1L<<1, Flag62 = 1L<<62 };

		[TestMethod]
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void FlagsSansAttributeTest()
		{
			try
			{
				FlagsEnumSansAttribute e = FlagsEnumSansAttribute.Flag0;
				EnumFlags.Add(ref e, FlagsEnumSansAttribute.Flag2);

				Assert.Fail("EnumFlags didn't fail on an Enum without a Flags attribute!");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType<NotSupportedException>(ex.InnerException);
			}
		}

		[TestMethod]
		public void FlagsAddTest()
		{
			const FlagsEnum kExpectedResult1 = FlagsEnum.Flag0 | FlagsEnum.Flag2;
			FlagsEnum e1 = FlagsEnum.Flag0;
			EnumFlags.Add(ref e1, FlagsEnum.Flag2);
			Assert.AreEqual(kExpectedResult1, e1);

			const FlagsEnum kExpectedResult2 = kExpectedResult1 | FlagsEnum.Flag3;
			FlagsEnum e2 = EnumFlags.Add(e1, FlagsEnum.Flag3);
			Assert.AreEqual(kExpectedResult2, e2);
		}

		[TestMethod]
		public void FlagsRemoveTest()
		{
			const FlagsEnum kExpectedResult1 = FlagsEnum.Flag0;
			FlagsEnum e1 = FlagsEnum.Flag0 | FlagsEnum.Flag2;
			EnumFlags.Remove(ref e1, FlagsEnum.Flag2);
			Assert.AreEqual(kExpectedResult1, e1);

			const FlagsEnum kExpectedResult2 = 0;
			FlagsEnum e2 = EnumFlags.Remove(e1, FlagsEnum.Flag0);
			Assert.AreEqual(kExpectedResult2, e2);
		}

		[TestMethod]
		public void FlagsModifyTest()
		{
			const FlagsEnum kExpectedResult1 = FlagsEnum.Flag0 | FlagsEnum.Flag2;
			FlagsEnum e1 = FlagsEnum.Flag0;
			EnumFlags.Modify(true, ref e1, FlagsEnum.Flag2);
			Assert.AreEqual(kExpectedResult1, e1);

			const FlagsEnum kExpectedResult2 = FlagsEnum.Flag2;
			FlagsEnum e2 = EnumFlags.Modify(false, e1, FlagsEnum.Flag0);
			Assert.AreEqual(kExpectedResult2, e2);
		}

		[TestMethod]
		public void FlagsMutatesAllUnderlyingTypesTest()
		{
			Verify(FlagsEnumByte.Flag0, FlagsEnumByte.Flag1, FlagsEnumByte.Flag7);
			Verify(FlagsEnumSByte.Flag0, FlagsEnumSByte.Flag1, FlagsEnumSByte.Flag6);
			Verify(FlagsEnumUInt16.Flag0, FlagsEnumUInt16.Flag1, FlagsEnumUInt16.Flag15);
			Verify(FlagsEnumInt16.Flag0, FlagsEnumInt16.Flag1, FlagsEnumInt16.Flag14);
			Verify(FlagsEnumUInt32.Flag0, FlagsEnumUInt32.Flag1, FlagsEnumUInt32.Flag31);
			Verify(FlagsEnumInt32.Flag0, FlagsEnumInt32.Flag1, FlagsEnumInt32.Flag30);
			Verify(FlagsEnumUInt64.Flag0, FlagsEnumUInt64.Flag1, FlagsEnumUInt64.Flag63);
			Verify(FlagsEnumInt64.Flag0, FlagsEnumInt64.Flag1, FlagsEnumInt64.Flag62);
		}

		static void Verify<TEnum>(TEnum lowFlag, TEnum middleFlag, TEnum highFlag)
			where TEnum : struct, Enum
		{
			TEnum value = lowFlag;

			EnumFlags.Add(ref value, highFlag);
			Assert.AreEqual(EnumFlags.Add(lowFlag, highFlag), value);

			EnumFlags.Modify(true, ref value, middleFlag);
			Assert.AreEqual(EnumFlags.Add(EnumFlags.Add(lowFlag, middleFlag), highFlag), value);

			EnumFlags.Remove(ref value, lowFlag);
			Assert.AreEqual(EnumFlags.Add(middleFlag, highFlag), value);

			value = EnumFlags.Modify(false, value, highFlag);
			Assert.AreEqual(middleFlag, value);
		}
	};
}
