using System;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Bench.KSoft.BCL.Text;

[MemoryDiagnoser]
[BenchmarkCategory("Text", "Hex")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "CountingTextWriter owns no resources, and BenchmarkDotNet controls benchmark instance lifecycle.")]
public class UtilitiesBytesBenchmarks
{
	private byte[] mData = [];
	private string mHexString = "";
	private CountingTextWriter mWriter = new CountingTextWriter();

	[Params(16, 1024, 65536)]
	public int Length { get; set; }

	[GlobalSetup]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Benchmark input data does not require cryptographic randomness.")]
	public void GlobalSetup()
	{
		byte[] data = new byte[Length];
		Random random = new Random(42);
		random.NextBytes(data);

		mData = data;
		mHexString = Convert.ToHexString(data);
		mWriter = new CountingTextWriter();
	}

	[Benchmark]
	public byte[] ByteStringToArray() =>
		global::KSoft.Text.Util.ByteStringToArray(mHexString);

	[Benchmark]
	public long ByteArrayToStream()
	{
		global::KSoft.Text.Util.ByteArrayToStream(mData, mWriter);
		return mWriter.CharsWritten;
	}

	sealed class CountingTextWriter
		: TextWriter
	{
		public long CharsWritten { get; private set; }

		public override Encoding Encoding => Encoding.UTF8;

		public override void Write(ReadOnlySpan<char> buffer) =>
			CharsWritten += buffer.Length;

		public override void Write(string? value) =>
			CharsWritten += value?.Length ?? 0;
	}
}
