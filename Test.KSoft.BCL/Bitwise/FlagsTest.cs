using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Bitwise.Test
{
	[TestClass]
	public sealed class FlagsTest : BaseTestClass
	{
		[TestMethod]
		public void Test_SingleFlags_ReturnsExpectedState()
		{
			Assert.IsTrue(Flags.Test((byte)0b0011, (byte)0b0001));
			Assert.IsTrue(Flags.Test((sbyte)0b0011, (sbyte)0b0011));
			Assert.IsFalse(Flags.Test((ushort)0b0011, (ushort)0b0100));
			Assert.IsTrue(Flags.Test((short)0b0111, (short)0b0011));
			Assert.IsTrue(Flags.Test(0b1011U, 0b0011U));
			Assert.IsFalse(Flags.Test(0b1011, 0b0100));
			Assert.IsTrue(Flags.Test(0b1011UL, 0b1001UL));
			Assert.IsFalse(Flags.Test(0b1011L, 0b0100L));
		}

		[TestMethod]
		public void TestAny_SingleFlags_ReturnsExpectedState()
		{
			Assert.IsTrue(Flags.TestAny((byte)0b0010, (byte)0b0110));
			Assert.IsFalse(Flags.TestAny((ushort)0b0010, (ushort)0b0100));
			Assert.IsTrue(Flags.TestAny(0b1000U, 0b1100U));
			Assert.IsFalse(Flags.TestAny(0b1000UL, 0b0011UL));
		}

		[TestMethod]
		public void Test_ParamsFlags_ReturnsAllOrAnyState()
		{
			Assert.IsTrue(Flags.Test((byte)0b0111, (byte)0b0001, (byte)0b0010));
			Assert.IsFalse(Flags.Test((byte)0b0111, (byte)0b0001, (byte)0b1000));
			Assert.IsTrue(Flags.Test((ushort)0b0111, (ushort)0b0001, (ushort)0b0010));
			Assert.IsFalse(Flags.Test((ushort)0b0111, (ushort)0b0001, (ushort)0b1000));
			Assert.IsTrue(Flags.Test(0b0111U, 0b0001U, 0b0010U));
			Assert.IsFalse(Flags.Test(0b0111U, 0b0001U, 0b1000U));
			Assert.IsTrue(Flags.Test(0b0111UL, 0b0001UL, 0b0010UL));
			Assert.IsFalse(Flags.Test(0b0111UL, 0b0001UL, 0b1000UL));

			Assert.IsTrue(Flags.TestAny((byte)0b0100, (byte)0b0001, (byte)0b0100));
			Assert.IsFalse(Flags.TestAny((ushort)0b0100, (ushort)0b0001, (ushort)0b0010));
			Assert.IsTrue(Flags.TestAny(0b0100U, 0b0001U, 0b0100U));
			Assert.IsFalse(Flags.TestAny(0b0100UL, 0b0001UL, 0b0010UL));
		}

		[TestMethod]
		public void AddRemoveToggle_MajorWords_ReturnsAndMutatesExpectedState()
		{
			Assert.AreEqual(0b0111U, Flags.Add(0b0101U, 0b0011U));
			Assert.AreEqual(0b0100U, Flags.Remove(0b0111U, 0b0011U));
			Assert.AreEqual(0b0110U, Flags.Toggle(0b0101U, 0b0011U));

			uint u32 = 0b0101U;
			Flags.Add(ref u32, 0b0011U);
			Assert.AreEqual(0b0111U, u32);
			Flags.Remove(ref u32, 0b0011U);
			Assert.AreEqual(0b0100U, u32);
			Flags.Toggle(ref u32, 0b0110U);
			Assert.AreEqual(0b0010U, u32);

			Assert.AreEqual(0b0111UL, Flags.Add(0b0101UL, 0b0011UL));
			Assert.AreEqual(0b0100UL, Flags.Remove(0b0111UL, 0b0011UL));
			Assert.AreEqual(0b0110UL, Flags.Toggle(0b0101UL, 0b0011UL));

			ulong u64 = 0b0101UL;
			Flags.Add(ref u64, 0b0011UL);
			Assert.AreEqual(0b0111UL, u64);
			Flags.Remove(ref u64, 0b0011UL);
			Assert.AreEqual(0b0100UL, u64);
			Flags.Toggle(ref u64, 0b0110UL);
			Assert.AreEqual(0b0010UL, u64);
		}

		[TestMethod]
		public void Modify_UnsignedWords_ReturnsAndMutatesExpectedState()
		{
			Assert.AreEqual((byte)0b0111, Flags.Modify(true, (byte)0b0101, (byte)0b0011));
			Assert.AreEqual((byte)0b0100, Flags.Modify(false, (byte)0b0111, (byte)0b0011));
			Assert.AreEqual((ushort)0b0111, Flags.Modify(true, (ushort)0b0101, (ushort)0b0011));
			Assert.AreEqual((ushort)0b0100, Flags.Modify(false, (ushort)0b0111, (ushort)0b0011));
			Assert.AreEqual(0b0111U, Flags.Modify(true, 0b0101U, 0b0011U));
			Assert.AreEqual(0b0100U, Flags.Modify(false, 0b0111U, 0b0011U));
			Assert.AreEqual(0b0111UL, Flags.Modify(true, 0b0101UL, 0b0011UL));
			Assert.AreEqual(0b0100UL, Flags.Modify(false, 0b0111UL, 0b0011UL));

			byte u8 = 0b0101;
			Assert.IsTrue(Flags.Modify(true, ref u8, 0b0011));
			Assert.AreEqual((byte)0b0111, u8);
			Assert.IsFalse(Flags.Modify(false, ref u8, 0b0011));
			Assert.AreEqual((byte)0b0100, u8);

			ushort u16 = 0b0101;
			Assert.IsTrue(Flags.Modify(true, ref u16, 0b0011));
			Assert.AreEqual((ushort)0b0111, u16);
			Assert.IsFalse(Flags.Modify(false, ref u16, 0b0011));
			Assert.AreEqual((ushort)0b0100, u16);

			uint u32 = 0b0101;
			Assert.IsTrue(Flags.Modify(true, ref u32, 0b0011));
			Assert.AreEqual(0b0111U, u32);
			Assert.IsFalse(Flags.Modify(false, ref u32, 0b0011));
			Assert.AreEqual(0b0100U, u32);

			ulong u64 = 0b0101;
			Assert.IsTrue(Flags.Modify(true, ref u64, 0b0011));
			Assert.AreEqual(0b0111UL, u64);
			Assert.IsFalse(Flags.Modify(false, ref u64, 0b0011));
			Assert.AreEqual(0b0100UL, u64);
		}
	}
}
