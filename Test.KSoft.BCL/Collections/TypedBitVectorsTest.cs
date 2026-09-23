using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Collections.Test;

[TestClass]
public sealed class TypedBitVectorsTest
{
	enum Bits32 { None = -1, First, Alias = First, Last = 31, kNumberOf }
	enum Bits64 : ulong { First, Last = 63, kNumberOf }
	enum SmallBits : long { None = -1, First, Third = 2, kNumberOf }
	enum LargeBits { First, High = 64, kNumberOf }
	enum EmptyBits { }
	[Flags]
	enum FlagMasks { First = 1, Second = 2 }
	enum InvalidBounds { First, Last = 2, kNumberOf = 2 }
	[EnumBitEncoderDisable]
	enum UnencodedBits { None = -1, First, Second, kNumberOf }

	sealed class PropertyOwner
	{
		public BitVector32<SmallBits> Bits { get; set; }
	}

	[TestMethod]
	public void IndexDomains_EncoderDisabledEnum_RemainsUsable()
	{
		var vector32 = new BitVector32<UnencodedBits>().With(UnencodedBits.First);
		var vector64 = new BitVector64<UnencodedBits>().With(UnencodedBits.Second);
		var set = new EnumBitSet<UnencodedBits>(UnencodedBits.None);
		set.Set(UnencodedBits.First, true);

		Assert.AreEqual(2, EnumBitTraits<UnencodedBits>.Length);
		Assert.IsTrue(vector32.Test(UnencodedBits.First));
		Assert.IsTrue(vector64.Test(UnencodedBits.Second));
		Assert.IsTrue(set.Test(UnencodedBits.First));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => set.Get(UnencodedBits.None));
	}

	[TestMethod]
	[DataRow(32)]
	[DataRow(64)]
	public void Adapter_PhysicalReadsAndDeclaredWrites_HaveDifferentIndexContracts(int width)
	{
		IEnumBitVector vector = width == 32
			? BitVector32<SmallBits>.FromRaw(new BitVector32(1 << 7))
			: BitVector64<SmallBits>.FromRaw(new BitVector64(1L << 7));

		Assert.AreEqual(width, vector.Length);
		Assert.IsTrue(vector.GetBit(7));
		Assert.IsFalse(vector.IsDefinedIndex(7));
		Assert.AreEqual(nameof(SmallBits.Third), vector.GetBitName(2));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => vector.GetBitName(7));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => vector.WithBit(7, false));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => vector.GetBit(-1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => vector.GetBit(width));
	}

	[TestMethod]
	public void Mutators_PropertyValuesAndChainedReturns_RequireCopyAssignment()
	{
		var owner = new PropertyOwner();
		owner.Bits.Set(SmallBits.First);
		Assert.IsTrue(owner.Bits.IsAllClear);
		owner.Bits = owner.Bits.With(SmallBits.First);
		owner.Bits.Toggle(SmallBits.First);
		owner.Bits.Clear();
		owner.Bits.SetAll(true);
		Assert.IsTrue(owner.Bits.Test(SmallBits.First));
		Assert.AreEqual(1, owner.Bits.Cardinality);
		var copy = owner.Bits;
		copy.Clear();
		owner.Bits = copy;
		Assert.IsTrue(owner.Bits.IsAllClear);

		var local = new BitVector32<SmallBits>();
		local.Set(SmallBits.First).Set(SmallBits.Third);
		Assert.IsTrue(local.Test(SmallBits.First));
		Assert.IsFalse(local.Test(SmallBits.Third));
		local = local.Set(SmallBits.First).Set(SmallBits.Third);
		Assert.IsTrue(local.Test(SmallBits.Third));
	}

	[TestMethod]
	public void Storage_FixedVectors_HaveOneWordAndIndependentCopies()
	{
		Assert.AreEqual(4, Unsafe.SizeOf<BitVector32<Bits32>>());
		Assert.AreEqual(8, Unsafe.SizeOf<BitVector64<Bits64>>());
		Assert.IsFalse(RuntimeHelpers.IsReferenceOrContainsReferences<BitVector32<Bits32>>());
		Assert.IsFalse(RuntimeHelpers.IsReferenceOrContainsReferences<BitVector64<Bits64>>());
		BitVector32<Bits32> first = default;
		first.Set(Bits32.First);
		var copy = first.With(Bits32.Last);
		Assert.IsFalse(first.Test(Bits32.Last));
		Assert.IsTrue(copy.Test(Bits32.Last));
		Assert.AreEqual(unchecked((int)0x80000001), copy.ToRaw().Data);
		Assert.AreEqual(1, first.Cardinality);
		first[Bits32.First] = false;
		Assert.IsTrue(copy[Bits32.First]);
		Assert.IsTrue(first.IsAllClear);
	}

	[TestMethod]
	public void Operations_64BitAndBitwise_PreserveWholeWord()
	{
		var first = new BitVector64<Bits64>().Set(Bits64.First);
		var last = new BitVector64<Bits64>().Set(Bits64.Last);
		var both = first | last;
		Assert.AreEqual(unchecked((long)0x8000000000000001), both.ToRaw().Data);
		Assert.AreEqual(first, both.AndNot(last));
		Assert.AreEqual(last, both ^ first);
		Assert.AreEqual(first, both & first);
		Assert.AreEqual(both, first.Or(last));
		Assert.AreEqual(~both.ToRaw(), (~both).ToRaw());
		Assert.AreEqual(both, BitVector64<Bits64>.FromRaw(both.ToRaw()));
		Assert.AreEqual(both.GetHashCode(), BitVector64<Bits64>.FromRaw(both.ToRaw()).GetHashCode());
		both.Toggle(Bits64.Last);
		Assert.AreEqual(first, both);
		both.SetAll(true);
		Assert.AreEqual(64, both.Cardinality);
		both.Clear();
		Assert.AreEqual(default, both);
	}

	[TestMethod]
	public void EnumerationAndFormatting_DeclaredMembersOnly_UseCanonicalAliases()
	{
		var bits = BitVector32<Bits32>.FromRaw(new BitVector32(uint.MaxValue));
		var names = new List<Bits32>();
		foreach (var bit in bits.SetBitIndices)
		{
			names.Add(bit);
		}
		CollectionAssert.AreEqual(new[] { Bits32.First, Bits32.Last }, names);
		Assert.AreEqual("First,Last", bits.ToFlagsString());
		Assert.AreEqual("First / Last", bits.ToFlagsString(" / "));
		Assert.AreEqual(32, bits.Cardinality);
		Assert.AreEqual(-1, bits.ToRaw().Data);
		var unused = BitVector32<SmallBits>.FromRaw(new BitVector32(1 << 7));
		Assert.AreEqual(string.Empty, unused.ToFlagsString());
		Assert.AreEqual("First,Third", unused.ToFlagsString(stateFilter: false));
		Assert.AreEqual(1 << 7, unused.ToRaw().Data);
		Assert.AreEqual(string.Empty, new BitVector32<EmptyBits>().ToFlagsString());
	}

	[TestMethod]
	public void Access_InvalidValuesAndDomains_FailsOnDefaultAndRepeatedUse()
	{
		for (int attempt = 0; attempt < 2; attempt++)
		{
			var bits = new BitVector32<SmallBits>();
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => bits.Test(SmallBits.None));
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => bits.Set((SmallBits)1));
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => bits.Set(SmallBits.kNumberOf));
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => bits.Test((SmallBits)0x1_0000_0000L));
			Assert.ThrowsExactly<ArgumentException>(() => new BitVector32<FlagMasks>().Test(FlagMasks.First));
			Assert.ThrowsExactly<ArgumentException>(() => new BitVector32<Bits64>().Test(Bits64.First));
			Assert.ThrowsExactly<ArgumentException>(() => new BitVector64<LargeBits>().Test(LargeBits.First));
			Assert.ThrowsExactly<ArgumentException>(() => new BitVector32<InvalidBounds>().Test(InvalidBounds.First));
			Assert.ThrowsExactly<ArgumentException>(() => BitVector32<Bits64>.FromRaw(default));
		}
	}

	[TestMethod]
	public void LargerSet_SentinelsAndSparseMembers_PreservesNamedSearchAndCount()
	{
		var bits = new EnumBitSet<SmallBits>(SmallBits.None);
		bits.SetAll(true);
		Assert.AreEqual(3, bits.Cardinality);
		Assert.AreEqual(2, ((ICollection<SmallBits>)bits).Count);
		var members = new List<SmallBits>();
		foreach (var bit in bits)
		{
			members.Add(bit);
		}
		CollectionAssert.AreEqual(new[] { SmallBits.First, SmallBits.Third }, members);
		bits.Set(SmallBits.First, false);
		Assert.AreEqual(SmallBits.Third, bits.NextSetBit(SmallBits.First));
		Assert.AreEqual(1, bits.NextSetBitIndex(SmallBits.First));
		Assert.IsTrue(bits.Test(SmallBits.Third));
		bits.Clear();
		Assert.AreEqual(SmallBits.None, bits.NextSetBit(SmallBits.First));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => bits.Set((SmallBits)1, true));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => bits.Set(SmallBits.None, true));
		var large = new EnumBitSet<LargeBits>();
		large.Set(LargeBits.High, true);
		Assert.AreEqual(65, large.Length);
		Assert.IsTrue(large.Test(LargeBits.High));
	}
}
