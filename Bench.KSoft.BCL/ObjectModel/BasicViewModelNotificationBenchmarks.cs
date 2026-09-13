using System.ComponentModel;
using BenchmarkDotNet.Attributes;
using KSoft.ObjectModel;

namespace Bench.KSoft.BCL.ObjectModel;

[MemoryDiagnoser]
public class BasicViewModelNotificationBenchmarks
{
	private static readonly PropertyChangedEventArgs sEventArgs = new("Value");
	private readonly Model mModel = new();
	private int mNotificationCount;

	[GlobalSetup]
	public void Setup()
	{
		mModel.PropertyChanged += (_, _) => mNotificationCount++;
	}

	[Benchmark(Baseline = true)]
	public int NotifyByPropertyName()
	{
		mModel.NotifyByPropertyName();
		return mNotificationCount;
	}

	[Benchmark]
	public int NotifyWithCachedArgs()
	{
		mModel.NotifyWithCachedArgs(sEventArgs);
		return mNotificationCount;
	}

	private sealed class Model : BasicViewModel
	{
		public void NotifyByPropertyName() => OnPropertyChanged("Value");
		public void NotifyWithCachedArgs(PropertyChangedEventArgs eventArgs) => OnPropertyChangedCached(eventArgs);
	}
}
