using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace Bench.KSoft.BCL.Enum;

/// <summary>
/// Measures equality comparison after <see cref="global::KSoft.EnumComparer{TEnum}"/> has paid its static setup cost.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("Enum", "Comparer")]
public class EnumComparerEqualsSteadyStateBenchmarks
{
	private global::KSoft.EnumComparer<SteadyStateEnum> mKSoftComparer;
	private EqualityComparer<SteadyStateEnum> mDefaultComparer;

	[Params(SteadyStateEnum.BigValueNeg)]
	public SteadyStateEnum Left { get; set; }

	[Params(SteadyStateEnum.BigValuePos)]
	public SteadyStateEnum Right { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		mKSoftComparer = global::KSoft.EnumComparer.For<SteadyStateEnum>();
		mDefaultComparer = EqualityComparer<SteadyStateEnum>.Default;
		_ = mKSoftComparer.Equals(Left, Right);
		_ = mDefaultComparer.Equals(Left, Right);
	}

	[Benchmark(Baseline = true)]
	public bool KSoftEquals() =>
		mKSoftComparer.Equals(Left, Right);

	[Benchmark]
	public bool DefaultEquals() =>
		mDefaultComparer.Equals(Left, Right);
}

/// <summary>
/// Measures hash-code calculation after <see cref="global::KSoft.EnumComparer{TEnum}"/> has paid its static setup cost.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("Enum", "Comparer")]
public class EnumComparerHashCodeSteadyStateBenchmarks
{
	private global::KSoft.EnumComparer<SteadyStateEnum> mKSoftComparer;
	private EqualityComparer<SteadyStateEnum> mDefaultComparer;

	[Params(SteadyStateEnum.BigValueNeg)]
	public SteadyStateEnum Value { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		mKSoftComparer = global::KSoft.EnumComparer.For<SteadyStateEnum>();
		mDefaultComparer = EqualityComparer<SteadyStateEnum>.Default;
		_ = mKSoftComparer.GetHashCode(Value);
		_ = mDefaultComparer.GetHashCode(Value);
	}

	[Benchmark(Baseline = true)]
	public int KSoftGetHashCode() =>
		mKSoftComparer.GetHashCode(Value);

	[Benchmark]
	public int DefaultGetHashCode() =>
		mDefaultComparer.GetHashCode(Value);
}

/// <summary>
/// Measures ordering comparison after <see cref="global::KSoft.EnumComparer{TEnum}"/> has paid its static setup cost.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("Enum", "Comparer")]
public class EnumComparerCompareSteadyStateBenchmarks
{
	private global::KSoft.EnumComparer<SteadyStateEnum> mKSoftComparer;
	private Comparer<SteadyStateEnum> mDefaultComparer;

	[Params(SteadyStateEnum.BigValueNeg)]
	public SteadyStateEnum Left { get; set; }

	[Params(SteadyStateEnum.BigValuePos)]
	public SteadyStateEnum Right { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		mKSoftComparer = global::KSoft.EnumComparer.For<SteadyStateEnum>();
		mDefaultComparer = Comparer<SteadyStateEnum>.Default;
		_ = mKSoftComparer.Compare(Left, Right);
		_ = mDefaultComparer.Compare(Left, Right);
	}

	[Benchmark(Baseline = true)]
	public int KSoftCompare() =>
		mKSoftComparer.Compare(Left, Right);

	[Benchmark]
	public int DefaultCompare() =>
		mDefaultComparer.Compare(Left, Right);
}

/// <summary>
/// Measures equality comparison including first-touch static setup.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.ColdStart, launchCount: 16, warmupCount: 0, iterationCount: 1)]
[BenchmarkCategory("Enum", "Comparer", "FirstTouch")]
public class EnumComparerEqualsFirstTouchBenchmarks
{
	private KSoftEqualsEnum mKSoftLeft = KSoftEqualsEnum.BigValueNeg;
	private KSoftEqualsEnum mKSoftRight = KSoftEqualsEnum.BigValuePos;
	private DefaultEqualsEnum mDefaultLeft = DefaultEqualsEnum.BigValueNeg;
	private DefaultEqualsEnum mDefaultRight = DefaultEqualsEnum.BigValuePos;

	[Benchmark(Baseline = true)]
	public bool KSoftEquals() =>
		global::KSoft.EnumComparer.For<KSoftEqualsEnum>().Equals(mKSoftLeft, mKSoftRight);

	[Benchmark]
	public bool DefaultEquals() =>
		EqualityComparer<DefaultEqualsEnum>.Default.Equals(mDefaultLeft, mDefaultRight);
}

/// <summary>
/// Measures hash-code calculation including first-touch static setup.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.ColdStart, launchCount: 16, warmupCount: 0, iterationCount: 1)]
[BenchmarkCategory("Enum", "Comparer", "FirstTouch")]
public class EnumComparerHashCodeFirstTouchBenchmarks
{
	private KSoftHashCodeEnum mKSoftValue = KSoftHashCodeEnum.BigValueNeg;
	private DefaultHashCodeEnum mDefaultValue = DefaultHashCodeEnum.BigValueNeg;

	[Benchmark(Baseline = true)]
	public int KSoftGetHashCode() =>
		global::KSoft.EnumComparer.For<KSoftHashCodeEnum>().GetHashCode(mKSoftValue);

	[Benchmark]
	public int DefaultGetHashCode() =>
		EqualityComparer<DefaultHashCodeEnum>.Default.GetHashCode(mDefaultValue);
}

/// <summary>
/// Measures ordering comparison including first-touch static setup.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.ColdStart, launchCount: 16, warmupCount: 0, iterationCount: 1)]
[BenchmarkCategory("Enum", "Comparer", "FirstTouch")]
public class EnumComparerCompareFirstTouchBenchmarks
{
	private KSoftCompareEnum mKSoftLeft = KSoftCompareEnum.BigValueNeg;
	private KSoftCompareEnum mKSoftRight = KSoftCompareEnum.BigValuePos;
	private DefaultCompareEnum mDefaultLeft = DefaultCompareEnum.BigValueNeg;
	private DefaultCompareEnum mDefaultRight = DefaultCompareEnum.BigValuePos;

	[Benchmark(Baseline = true)]
	public int KSoftCompare() =>
		global::KSoft.EnumComparer.For<KSoftCompareEnum>().Compare(mKSoftLeft, mKSoftRight);

	[Benchmark]
	public int DefaultCompare() =>
		Comparer<DefaultCompareEnum>.Default.Compare(mDefaultLeft, mDefaultRight);
}

public enum SteadyStateEnum : long
{
	Zero,
	One,
	BigValueNeg = long.MinValue + 1,
	BigValuePos = long.MaxValue - 1,
}

enum KSoftEqualsEnum : long
{
	BigValueNeg = long.MinValue + 1,
	BigValuePos = long.MaxValue - 1,
}

enum DefaultEqualsEnum : long
{
	BigValueNeg = long.MinValue + 1,
	BigValuePos = long.MaxValue - 1,
}

enum KSoftHashCodeEnum : long
{
	BigValueNeg = long.MinValue + 1,
}

enum DefaultHashCodeEnum : long
{
	BigValueNeg = long.MinValue + 1,
}

enum KSoftCompareEnum : long
{
	BigValueNeg = long.MinValue + 1,
	BigValuePos = long.MaxValue - 1,
}

enum DefaultCompareEnum : long
{
	BigValueNeg = long.MinValue + 1,
	BigValuePos = long.MaxValue - 1,
}
