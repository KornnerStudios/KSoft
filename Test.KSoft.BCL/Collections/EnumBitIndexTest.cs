using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Collections.Test;

[TestClass]
public sealed class EnumBitIndexTest
{
	enum SByteBits : sbyte { First, Last32 = 31, Last64 = 63 }
	enum ByteBits : byte { First, Last32 = 31, Last64 = 63 }
	enum ShortBits : short { First, Last32 = 31, Last64 = 63 }
	enum UShortBits : ushort { First, Last32 = 31, Last64 = 63 }
	enum IntBits { First, Last32 = 31, Last64 = 63 }
	enum UIntBits : uint { First, Last32 = 31, Last64 = 63 }
	enum LongBits : long { First, Last32 = 31, Last64 = 63 }
	enum ULongBits : ulong { First, Last32 = 31, Last64 = 63 }

	[TestMethod]
	[DataRow(SByteBits.First, SByteBits.Last32, SByteBits.Last64)]
	[DataRow(ByteBits.First, ByteBits.Last32, ByteBits.Last64)]
	[DataRow(ShortBits.First, ShortBits.Last32, ShortBits.Last64)]
	[DataRow(UShortBits.First, UShortBits.Last32, UShortBits.Last64)]
	[DataRow(IntBits.First, IntBits.Last32, IntBits.Last64)]
	[DataRow(UIntBits.First, UIntBits.Last32, UIntBits.Last64)]
	[DataRow(LongBits.First, LongBits.Last32, LongBits.Last64)]
	[DataRow(ULongBits.First, ULongBits.Last32, ULongBits.Last64)]
	public void Access_EveryEnumBackingWidth_PreservesFirstAndLastBits<TEnum>(TEnum first, TEnum last32, TEnum last64)
		where TEnum : struct, Enum
	{
		var vector32 = new BitVector32();
		var vector64 = new BitVector64();
		var bits = new BitSet(64);

		vector32.Set(first);
		vector32.Set(last32);
		vector64.Set(first);
		vector64.Set(last64);
		bits.Set(first);
		bits.Set(last64);

		Assert.IsTrue(vector32.Test(first));
		Assert.IsTrue(vector32.Test(last32));
		Assert.AreEqual(unchecked((int)0x80000001), vector32.Data);
		Assert.IsTrue(vector64.Test(first));
		Assert.IsTrue(vector64.Test(last64));
		Assert.AreEqual(unchecked((long)0x8000000000000001), vector64.Data);
		Assert.IsTrue(bits.Test(first));
		Assert.IsTrue(bits.Test(last64));
		Assert.AreEqual(2, bits.Cardinality);

		var typed = new BitVector64<TEnum>();
		typed.Set(first);
		typed.Set(last64);
		Assert.IsTrue(typed.Test(first));
		Assert.IsTrue(typed.Test(last64));
		Assert.AreEqual(vector64, typed.ToRaw());

		vector32.Set(last32, false);
		vector64.Set(last64, false);
		bits.Set(last64, false);
		Assert.AreEqual(1, vector32.Data);
		Assert.AreEqual(1L, vector64.Data);
		Assert.AreEqual(1, bits.Cardinality);
	}

	[TestMethod]
	[DataRow((SByteBits)(-1))]
	[DataRow((ShortBits)(-1))]
	[DataRow((IntBits)(-1))]
	[DataRow((LongBits)0x1_0000_0000L)]
	[DataRow((LongBits)(-0x1_0000_0000L))]
	[DataRow((LongBits)(-1L))]
	[DataRow((LongBits)64L)]
	[DataRow((ULongBits)0x1_0000_0000UL)]
	[DataRow((ULongBits)ulong.MaxValue)]
	[DataRow((ULongBits)64UL)]
	public void Access_InvalidEnumValue_ThrowsBeforeNarrowing<TEnum>(TEnum invalid)
		where TEnum : struct, Enum
	{
		var vector32 = new BitVector32(1);
		var vector64 = new BitVector64(1);
		var bits = new BitSet(64);
		bits[0] = true;

		AssertInvalid(() => vector32.Test(invalid));
		AssertInvalid(() => vector32.Set(invalid, false));
		AssertInvalid(() => vector64.Test(invalid));
		AssertInvalid(() => vector64.Set(invalid, false));
		AssertInvalid(() => bits.Test(invalid));
		AssertInvalid(() => bits.Set(invalid, false));

		Assert.AreEqual(1, vector32.Data);
		Assert.AreEqual(1L, vector64.Data);
		Assert.AreEqual(1, bits.Cardinality);
		Assert.IsTrue(bits[0]);
	}

	[TestMethod]
	public void Access_InRangeNumericValues_PreservesRawApiContract()
	{
		var vector32 = new BitVector32();
		var vector64 = new BitVector64();
		var bits = new BitSet(64);
		var unnamed = (LongBits)7;

		vector32.Set(unnamed);
		vector64.Set(unnamed);
		bits.Set(unnamed);

		Assert.IsTrue(vector32.Test(unnamed));
		Assert.IsTrue(vector64.Test(unnamed));
		Assert.IsTrue(bits.Test(unnamed));
		AssertInvalid(() => vector32.Test((LongBits)32));
		AssertInvalid(() => vector32.Set((LongBits)32));
	}

	static void AssertInvalid(Action action)
	{
		var error = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);
		Assert.AreEqual("bit", error.ParamName);
	}
}
