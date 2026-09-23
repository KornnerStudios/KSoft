using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using KSoft.Collections;

namespace Bench.KSoft.BCL.Collections;

/// <summary>Valid flag access and property-style copies, separating validation cost from the typed facade.</summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class EnumBitVector32Benchmarks
{
	enum FlagBits { First, Last = 31, kNumberOf }
	FlagBits mBit = FlagBits.Last;
	bool mValue = true;
	BitVector32 mRaw;
	BitVector32<FlagBits> mTyped;

	[GlobalSetup]
	public void Setup()
	{
		mRaw.Set(mBit, mValue);
		mTyped.Set(mBit, mValue);
	}

	[Benchmark(Baseline = true), BenchmarkCategory("Access")]
	public bool RawAccess() { mRaw.Set(mBit, mValue); return mRaw.Test(mBit); }

	[Benchmark, BenchmarkCategory("Access")]
	public bool RawWithSameValidation()
	{
		mRaw[EnumBitTraits<FlagBits>.ToIndex(mBit)] = mValue;
		return mRaw[EnumBitTraits<FlagBits>.ToIndex(mBit)];
	}

	[Benchmark, BenchmarkCategory("Access")]
	public bool TypedAccess() { mTyped.Set(mBit, mValue); return mTyped.Test(mBit); }

	[Benchmark(Baseline = true), BenchmarkCategory("Copy")]
	public int RawCopy() { var copy = mRaw; copy.Set(mBit, mValue); return copy.Data; }

	[Benchmark, BenchmarkCategory("Copy")]
	public int TypedWith() => mTyped.With(mBit, mValue).ToRaw().Data;
}

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class EnumBitVector64Benchmarks
{
	enum FlagBits { First, Last = 63, kNumberOf }
	FlagBits mBit = FlagBits.Last;
	bool mValue = true;
	BitVector64 mRaw;
	BitVector64<FlagBits> mTyped;

	[GlobalSetup]
	public void Setup()
	{
		mRaw.Set(mBit, mValue);
		mTyped.Set(mBit, mValue);
	}

	[Benchmark(Baseline = true), BenchmarkCategory("Access")]
	public bool RawAccess() { mRaw.Set(mBit, mValue); return mRaw.Test(mBit); }

	[Benchmark, BenchmarkCategory("Access")]
	public bool TypedAccess() { mTyped.Set(mBit, mValue); return mTyped.Test(mBit); }

	[Benchmark(Baseline = true), BenchmarkCategory("Copy")]
	public long RawCopy() { var copy = mRaw; copy.Set(mBit, mValue); return copy.Data; }

	[Benchmark, BenchmarkCategory("Copy")]
	public long TypedWith() => mTyped.With(mBit, mValue).ToRaw().Data;
}

[MemoryDiagnoser]
public class EnumBitVectorLargeSetBenchmarks
{
	enum FlagBits { First, Last = 64, kNumberOf }
	FlagBits mBit = FlagBits.Last;
	BitSet mRaw = null!;
	EnumBitSet<FlagBits> mTyped = null!;

	[GlobalSetup]
	public void Setup()
	{
		mRaw = new BitSet(65);
		mTyped = new EnumBitSet<FlagBits>();
		mRaw.Set(mBit);
		mTyped.Set(mBit, true);
	}

	[Benchmark(Baseline = true)]
	public bool RawAccess() { mRaw.Set(mBit); return mRaw.Test(mBit); }

	[Benchmark]
	public bool TypedAccess() { mTyped.Set(mBit, true); return mTyped.Test(mBit); }
}

/// <summary>First operational use in fresh processes, including existing enum-converter initialization.</summary>
/// <remarks>Record first-invocation managed bytes explicitly: MemoryDiagnoser's subsequent pass sees initialized statics. Timing includes JIT and setup; the byte count covers the invoking thread.</remarks>
[SimpleJob(RunStrategy.ColdStart, launchCount: 8, warmupCount: 0, iterationCount: 1)]
public class EnumBitVectorFirstTouchBenchmarks
{
	enum RawBits { First, Last = 31 }
	enum TypedBits { First, Last = 31 }
	RawBits mRawBit = RawBits.Last;
	TypedBits mTypedBit = TypedBits.Last;
	long mFirstUseAllocated = -1;

	[Benchmark(Baseline = true)]
	public bool RawFirstUse()
	{
		long before = GC.GetAllocatedBytesForCurrentThread();
		bool result = new BitVector32().Set(mRawBit).Test(mRawBit);
		if (mFirstUseAllocated < 0) mFirstUseAllocated = GC.GetAllocatedBytesForCurrentThread() - before;
		return result;
	}

	[Benchmark]
	public bool TypedFirstUse()
	{
		long before = GC.GetAllocatedBytesForCurrentThread();
		bool result = new BitVector32<TypedBits>().Set(mTypedBit).Test(mTypedBit);
		if (mFirstUseAllocated < 0) mFirstUseAllocated = GC.GetAllocatedBytesForCurrentThread() - before;
		return result;
	}

	[GlobalCleanup]
	public void ReportFirstUseAllocation() =>
		Console.WriteLine(FormattableString.Invariant($"First operation managed bytes on invoking thread: {mFirstUseAllocated}"));
}
