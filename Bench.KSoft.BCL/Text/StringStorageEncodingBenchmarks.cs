using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using KSoft.Memory.Strings;

namespace Bench.KSoft.BCL.Text;

public enum StringStorageStreamKind
{
	EndianWriter,
	BitStream,
}

[MemoryDiagnoser]
[BenchmarkCategory("Text", "StringStorage", "Write", "SteadyState")]
public class StringStorageEncodingBenchmarks
{
	private string mValue = "";
	private StringStorage mStorage;
	private global::KSoft.Text.StringStorageEncoding mEncoding = null!;
	private byte[] mOutputBuffer = [];

	[Params(16, 128, 1024)]
	public int CharacterCount { get; set; }

	[Params(StringStorageStreamKind.EndianWriter, StringStorageStreamKind.BitStream)]
	public StringStorageStreamKind StreamKind { get; set; }

	[GlobalSetup]
	public void GlobalSetup()
	{
		mValue = new string('A', CharacterCount);
		mStorage = new StringStorage(
			StringStorageWidthType.Unicode, StringStorageType.CString,
			checked((short)(CharacterCount + 1)));
		mEncoding = new global::KSoft.Text.StringStorageEncoding(
			mStorage, global::KSoft.Shell.EndianFormat.Big);
		mOutputBuffer = new byte[mEncoding.GetByteCount(mValue.AsSpan())];
	}

	[Benchmark(Baseline = true)]
	public long AllocatedBuffer()
	{
		using var output = new MemoryStream(mOutputBuffer, writable: true);
		var plan = mEncoding.GetEncodingPlan(mValue.AsSpan());
		byte[] buffer = new byte[plan.SerializedByteCount];
		int extent = mEncoding.EncodeString(mValue, buffer, plan, out _);
		if (StreamKind == StringStorageStreamKind.EndianWriter)
		{
			using var writer = new global::KSoft.IO.EndianWriter(
				output, global::KSoft.Shell.EndianFormat.Big);
			writer.BaseStream.Write(buffer.AsSpan(0, extent));
			return output.Position;
		}

		using var bits = new global::KSoft.IO.BitStream(output, FileAccess.Write)
		{
			StreamMode = FileAccess.Write,
		};
		bits.Write(buffer.AsSpan(0, extent));
		bits.Flush();
		return output.Position;
	}

	[Benchmark]
	public long StackOrPooledBuffer()
	{
		using var output = new MemoryStream(mOutputBuffer, writable: true);
		if (StreamKind == StringStorageStreamKind.EndianWriter)
		{
			using var writer = new global::KSoft.IO.EndianWriter(
				output, global::KSoft.Shell.EndianFormat.Big);
			writer.Write(mValue.AsSpan(), mEncoding);
			return output.Position;
		}

		using var bits = new global::KSoft.IO.BitStream(output, FileAccess.Write)
		{
			StreamMode = FileAccess.Write,
		};
		bits.Write(mValue, mEncoding);
		bits.Flush();
		return output.Position;
	}
}
