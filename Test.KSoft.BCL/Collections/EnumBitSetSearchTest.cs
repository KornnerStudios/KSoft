using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Collections.Test;

[TestClass]
public sealed class EnumBitSetSearchTest
{
	public enum DenseBits
	{
		None = -1,
		B00, B01, B02, B03, B04, B05, B06, B07,
		B08, B09, B10, B11, B12, B13, B14, B15,
		B16, B17, B18, B19, B20, B21, B22, B23,
		B24, B25, B26, B27, B28, B29, B30, B31,
		B32, B33, B34, B35, B36, B37, B38, B39,
		B40, B41, B42, B43, B44, B45, B46, B47,
		B48, B49, B50, B51, B52, B53, B54, B55,
		B56, B57, B58, B59, B60, B61, B62, B63,
		B64,
		kNumberOf,
	}

	enum SparseBits { None = -1, First, NearBoundary = 31, Boundary, Far = 256, Last = 4096, kNumberOf }

	[TestMethod]
	[DataRow(DenseBits.B00, DenseBits.B00)]
	[DataRow(DenseBits.B01, DenseBits.B31)]
	[DataRow(DenseBits.B31, DenseBits.B31)]
	[DataRow(DenseBits.B32, DenseBits.B32)]
	[DataRow(DenseBits.B33, DenseBits.B64)]
	[DataRow(DenseBits.B64, DenseBits.B64)]
	public void Search_DenseDomain_IsInclusiveAcrossWordBoundaries(DenseBits start, DenseBits expected)
	{
		foreach (bool state in new[] { false, true })
		{
			var bits = new EnumBitSet<DenseBits>(DenseBits.None);
			bits.SetAll(!state);
			foreach (var bit in new[] { DenseBits.B00, DenseBits.B31, DenseBits.B32, DenseBits.B64 })
			{
				bits.Set(bit, state);
			}

			Assert.AreEqual(expected, state ? bits.NextSetBit(start) : bits.NextClearBit(start));
			bits.Set(DenseBits.B64, !state);
			Assert.AreEqual(DenseBits.None, state ? bits.NextSetBit(DenseBits.B64) : bits.NextClearBit(DenseBits.B64));
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => bits.NextSetBit(DenseBits.kNumberOf));
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => bits.NextClearBit(DenseBits.None));
		}
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public void Search_SparseDomain_SkipsMatchingGapsAndStopsAtExhaustion(bool state)
	{
		var bits = new EnumBitSet<SparseBits>(SparseBits.None);
		bits.SetAll(state);
		foreach (var bit in new[] { SparseBits.First, SparseBits.NearBoundary, SparseBits.Boundary, SparseBits.Far })
		{
			bits.Set(bit, !state);
		}

		Assert.AreEqual(1, state ? bits.NextSetBitIndex(SparseBits.First) : bits.NextClearBitIndex(SparseBits.First));
		foreach (var start in new[] { SparseBits.First, SparseBits.NearBoundary, SparseBits.Boundary, SparseBits.Far, SparseBits.Last })
		{
			Assert.AreEqual(SparseBits.Last, state ? bits.NextSetBit(start) : bits.NextClearBit(start));
		}
		bits.Set(SparseBits.Last, !state);
		Assert.AreEqual(SparseBits.None, state ? bits.NextSetBit(SparseBits.First) : bits.NextClearBit(SparseBits.First));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => bits.NextSetBit((SparseBits)1));
	}
}
