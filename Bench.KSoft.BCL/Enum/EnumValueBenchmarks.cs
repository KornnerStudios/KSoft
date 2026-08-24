using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace Bench.KSoft.BCL.Enums;

/// <summary>
/// Measures the hot path after <see cref="global::KSoft.Reflection.EnumValue{TEnum}"/> has paid its static setup cost.
/// </summary>
/// <remarks>
/// Bit-vector enum-index helpers run in this mode after first use. The baseline keeps the legacy constrained
/// <see cref="IConvertible.ToInt32(IFormatProvider)"/> call shape so per-call boxing is visible beside the cached
/// delegate path.
/// </remarks>
[MemoryDiagnoser]
[BenchmarkCategory("Enum", "Conversion")]
public class EnumValueSteadyStateBenchmarks
{
	private ConversionEnum mValue = ConversionEnum.Member3;

	[Benchmark(Baseline = true)]
	public int IConvertibleToInt32() =>
		ToInt32ViaIConvertible(mValue);

	[Benchmark]
	public int EnumValueToInt32() =>
		global::KSoft.Reflection.EnumValue<ConversionEnum>.ToInt32(mValue);

	static int ToInt32ViaIConvertible<TEnum>(TEnum value)
		where TEnum : struct, System.Enum, IConvertible =>
		value.ToInt32(null);

	enum ConversionEnum
	{
		Member0,
		Member1,
		Member2,
		Member3,
	}
}

/// <summary>
/// Measures the first touch of <see cref="global::KSoft.Reflection.EnumValue{TEnum}"/> for a closed enum type.
/// </summary>
/// <remarks>
/// BenchmarkDotNet normally warms up methods before measurement, which hides the converter delegate generation cost.
/// This cold-start job intentionally keeps that one-time allocation and compilation overhead in the measured operation.
/// </remarks>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.ColdStart, launchCount: 16, warmupCount: 0, iterationCount: 1)]
[BenchmarkCategory("Enum", "Conversion", "FirstTouch")]
public class EnumValueFirstTouchBenchmarks
{
	private IConvertibleEnum mIConvertibleValue = IConvertibleEnum.Member3;
	private EnumValueEnum mEnumValue = EnumValueEnum.Member3;

	[Benchmark(Baseline = true)]
	public int IConvertibleToInt32() =>
		ToInt32ViaIConvertible(mIConvertibleValue);

	[Benchmark]
	public int EnumValueToInt32() =>
		global::KSoft.Reflection.EnumValue<EnumValueEnum>.ToInt32(mEnumValue);

	static int ToInt32ViaIConvertible<TEnum>(TEnum value)
		where TEnum : struct, System.Enum, IConvertible =>
		value.ToInt32(null);

	enum IConvertibleEnum
	{
		Member0,
		Member1,
		Member2,
		Member3,
	}

	enum EnumValueEnum
	{
		Member0,
		Member1,
		Member2,
		Member3,
	}
}
