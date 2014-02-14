using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Reflection.Test
{
	[TestClass]
	public class EnumValueTest : BaseTestClass
	{
		#region Test method creation
		enum TestEnumInt8 : sbyte { };
		enum TestEnumUInt8 : byte { };

		enum TestEnumInt16 : short { };
		enum TestEnumUInt16 : ushort { };

		enum TestEnumInt32 : int { };
		enum TestEnumUInt32 : uint { };

		enum TestEnumInt64 : long { };
		enum TestEnumUIn64 : ulong { };

		[TestMethod]
		public void Enum_ValueMethodCreationTest()
		{
			EnumValue<TestEnumInt8>.FromSByte.ToString();
			EnumValue<TestEnumUInt8>.FromSByte.ToString();

			EnumValue<TestEnumInt16>.FromSByte.ToString();
			EnumValue<TestEnumUInt16>.FromSByte.ToString();

			EnumValue<TestEnumInt32>.FromSByte.ToString();
			EnumValue<TestEnumUInt32>.FromSByte.ToString();

			EnumValue<TestEnumInt64>.FromSByte.ToString();
			EnumValue<TestEnumUIn64>.FromSByte.ToString();
		}
		#endregion

		const int kNegativeValue = -8;
		const int kPositiveValue = 8;

		enum TestEnum : long
		{
			None,

			NegativeValue = kNegativeValue,
			PositiveValue = kPositiveValue,
		};

		// TODO: T4?
		[TestMethod]
		public void Enum_ValueMethodsTest()
		{
			#region Int8
			// Signed
			Assert.AreEqual((sbyte)kNegativeValue, EnumValue<TestEnum>.ToSByte(TestEnum.NegativeValue));
			Assert.AreEqual((sbyte)kPositiveValue, EnumValue<TestEnum>.ToSByte(TestEnum.PositiveValue));
			// Unsigned
			Assert.AreEqual(unchecked( (byte)kNegativeValue ), EnumValue<TestEnum>.ToByte(TestEnum.NegativeValue));
			Assert.AreEqual((byte)kPositiveValue, EnumValue<TestEnum>.ToByte(TestEnum.PositiveValue));

			// Signed
			Assert.AreEqual(TestEnum.NegativeValue, EnumValue<TestEnum>.FromSByte((sbyte)kNegativeValue));
			Assert.AreEqual(TestEnum.PositiveValue, EnumValue<TestEnum>.FromSByte((sbyte)kPositiveValue));
			// Unsigned
			//Assert.AreEqual(TestEnum.NegativeValue, EnumValue<TestEnum>.FromByte(unchecked( (byte)kNegativeValue )));
			Assert.AreEqual(TestEnum.PositiveValue, EnumValue<TestEnum>.FromByte((byte)kPositiveValue));
			#endregion

			#region Int16
			// Signed
			Assert.AreEqual((Int16)kNegativeValue, EnumValue<TestEnum>.ToInt16(TestEnum.NegativeValue));
			Assert.AreEqual((Int16)kPositiveValue, EnumValue<TestEnum>.ToInt16(TestEnum.PositiveValue));
			// Unsigned
			Assert.AreEqual(unchecked( (UInt16)kNegativeValue ), EnumValue<TestEnum>.ToUInt16(TestEnum.NegativeValue));
			Assert.AreEqual((UInt16)kPositiveValue, EnumValue<TestEnum>.ToUInt16(TestEnum.PositiveValue));

			// Signed
			Assert.AreEqual(TestEnum.NegativeValue, EnumValue<TestEnum>.FromInt16((Int16)kNegativeValue));
			Assert.AreEqual(TestEnum.PositiveValue, EnumValue<TestEnum>.FromInt16((Int16)kPositiveValue));
			// Unsigned
			//Assert.AreEqual(TestEnum.NegativeValue, EnumValue<TestEnum>.FromUInt16(unchecked( (UInt16)kNegativeValue )));
			Assert.AreEqual(TestEnum.PositiveValue, EnumValue<TestEnum>.FromUInt16((UInt16)kPositiveValue));
			#endregion

			#region Int32
			// Signed
			Assert.AreEqual((Int32)kNegativeValue, EnumValue<TestEnum>.ToInt32(TestEnum.NegativeValue));
			Assert.AreEqual((Int32)kPositiveValue, EnumValue<TestEnum>.ToInt32(TestEnum.PositiveValue));
			// Unsigned
			Assert.AreEqual(unchecked( (UInt32)kNegativeValue ), EnumValue<TestEnum>.ToUInt32(TestEnum.NegativeValue));
			Assert.AreEqual((UInt32)kPositiveValue, EnumValue<TestEnum>.ToUInt32(TestEnum.PositiveValue));

			// Signed
			Assert.AreEqual(TestEnum.NegativeValue, EnumValue<TestEnum>.FromInt32((Int32)kNegativeValue));
			Assert.AreEqual(TestEnum.PositiveValue, EnumValue<TestEnum>.FromInt32((Int32)kPositiveValue));
			// Unsigned
			//Assert.AreEqual(TestEnum.NegativeValue, EnumValue<TestEnum>.FromUInt32(unchecked( (UInt32)kNegativeValue )));
			Assert.AreEqual(TestEnum.PositiveValue, EnumValue<TestEnum>.FromUInt32((UInt32)kPositiveValue));
			#endregion

			#region Int64
			// Signed
			Assert.AreEqual((Int64)kNegativeValue, EnumValue<TestEnum>.ToInt64(TestEnum.NegativeValue));
			Assert.AreEqual((Int64)kPositiveValue, EnumValue<TestEnum>.ToInt64(TestEnum.PositiveValue));
			// Unsigned
			Assert.AreEqual(unchecked((UInt64)kNegativeValue), EnumValue<TestEnum>.ToUInt64(TestEnum.NegativeValue));
			Assert.AreEqual((UInt64)kPositiveValue, EnumValue<TestEnum>.ToUInt64(TestEnum.PositiveValue));

			// Signed
			Assert.AreEqual(TestEnum.NegativeValue, EnumValue<TestEnum>.FromInt64((Int64)kNegativeValue));
			Assert.AreEqual(TestEnum.PositiveValue, EnumValue<TestEnum>.FromInt64((Int64)kPositiveValue));
			// Unsigned
			//Assert.AreEqual(TestEnum.NegativeValue, EnumValue<TestEnum>.FromUInt64(unchecked( (UInt64)kNegativeValue )));
			Assert.AreEqual(TestEnum.PositiveValue, EnumValue<TestEnum>.FromUInt64((UInt64)kPositiveValue));
			#endregion
		}
	};
}