using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using KSoft.Memory.Strings;

namespace Bench.KSoft.BCL.Text;

public enum StringStorageReadFraming
{
	FixedCString,
	ExplicitCString,
	Pascal,
	FixedCharArray,
}

[MemoryDiagnoser]
[BenchmarkCategory("Text", "StringStorage", "Read", "SteadyState")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the benchmark lifecycle and invokes GlobalCleanup.")]
public class StringStorageReadBenchmarks
{
	private StringStorage mStorage;
	private global::KSoft.Text.StringStorageEncoding mEncoding = null!;
	private string mExpected = "";
	private byte[] mRecord = [];
	private int mSerializedUnitSize;
	private MemoryStream mEndianInput = null!;
	private global::KSoft.IO.EndianReader mEndianReader = null!;

	[Params(16, 128, 1024)]
	public int CharacterCount { get; set; }

	[Params(StringStorageStreamKind.EndianReader, StringStorageStreamKind.BitStream)]
	public StringStorageStreamKind StreamKind { get; set; }

	[Params(
		StringStorageReadFraming.FixedCString,
		StringStorageReadFraming.ExplicitCString,
		StringStorageReadFraming.Pascal,
		StringStorageReadFraming.FixedCharArray)]
	public StringStorageReadFraming Framing { get; set; }

	[GlobalSetup]
	public void GlobalSetup()
	{
		int payloadCharacterCount = Framing is
			StringStorageReadFraming.FixedCString or StringStorageReadFraming.FixedCharArray
				? CharacterCount / 2
				: CharacterCount;
		mExpected = new string('A', payloadCharacterCount);
		mStorage = Framing switch
		{
			StringStorageReadFraming.FixedCString => new StringStorage(
				StringStorageWidthType.Unicode, StringStorageType.CString,
				checked((short)(CharacterCount + 1))),
			StringStorageReadFraming.ExplicitCString => StringStorage.CStringUnicode,
			StringStorageReadFraming.Pascal => new StringStorage(
				StringStorageWidthType.Unicode, StringStorageLengthPrefix.Int32),
			StringStorageReadFraming.FixedCharArray => new StringStorage(
				StringStorageWidthType.Unicode, StringStorageType.CharArray,
				checked((short)CharacterCount)),
			_ => throw new InvalidOperationException(Framing.ToString()),
		};
		mEncoding = new global::KSoft.Text.StringStorageEncoding(
			mStorage, global::KSoft.Shell.EndianFormat.Big);
		mRecord = mEncoding.GetBytes(mExpected);
		mSerializedUnitSize = mEncoding.GetMaxCleanByteCount(1);
		mEndianInput = new MemoryStream(mRecord, writable: false);
		mEndianReader = new global::KSoft.IO.EndianReader(
			mEndianInput, global::KSoft.Shell.EndianFormat.Big)
		{
			BaseStreamOwner = false,
		};
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		mEndianReader.Dispose();
		mEndianInput.Dispose();
	}

	[Benchmark(Baseline = true)]
	public string AllocatedBuffer()
	{
		// Reconstruct the former payload-array acquisition while sharing the current
		// framing decoder, so timings compare buffer strategies rather than decoder logic.
		if (StreamKind == StringStorageStreamKind.EndianReader)
		{
			mEndianInput.Position = 0;
			return ReadWithAllocatedBuffer(mEndianReader);
		}

		using var input = new MemoryStream(mRecord, writable: false);
		using var bits = new global::KSoft.IO.BitStream(input, FileAccess.Read)
		{
			StreamMode = FileAccess.Read,
		};
		return ReadWithAllocatedBuffer(bits);
	}

	[Benchmark]
	public string StackOrPooledBuffer()
	{
		if (StreamKind == StringStorageStreamKind.EndianReader)
		{
			mEndianInput.Position = 0;
			int byteCount = GetPayloadOrRecordByteCount(mEndianReader);
			return mEncoding.ReadKnownPayload(
				mEndianReader, byteCount, GetKnownPayloadKind());
		}

		using var input = new MemoryStream(mRecord, writable: false);
		using var bits = new global::KSoft.IO.BitStream(input, FileAccess.Read)
		{
			StreamMode = FileAccess.Read,
		};
		int bitByteCount = GetPayloadOrRecordByteCount(bits);
		return mEncoding.ReadKnownPayload(bits, bitByteCount, GetKnownPayloadKind());
	}

	private string ReadWithAllocatedBuffer(global::KSoft.IO.EndianReader reader)
	{
		int byteCount = GetPayloadOrRecordByteCount(reader);
		int initiallyRequiredByteCount = Framing == StringStorageReadFraming.ExplicitCString
			? byteCount - mSerializedUnitSize
			: byteCount;
		if (reader.BaseStream.CanSeek &&
			initiallyRequiredByteCount > reader.BaseStream.Length - reader.BaseStream.Position)
		{
			throw new EndOfStreamException("The string payload is incomplete.");
		}

		byte[] buffer = new byte[byteCount];
		if (Framing == StringStorageReadFraming.ExplicitCString)
		{
			int payloadByteCount = byteCount - mSerializedUnitSize;
			reader.BaseStream.ReadExactly(buffer.AsSpan(0, payloadByteCount));
			reader.BaseStream.ReadExactly(buffer.AsSpan(payloadByteCount));
		}
		else
		{
			reader.BaseStream.ReadExactly(buffer);
		}
		return mEncoding.DecodeKnownPayload(buffer, GetKnownPayloadKind());
	}

	private string ReadWithAllocatedBuffer(global::KSoft.IO.BitStream bits)
	{
		int byteCount = GetPayloadOrRecordByteCount(bits);
		byte[] buffer = new byte[byteCount];
		if (Framing == StringStorageReadFraming.ExplicitCString)
		{
			int payloadByteCount = byteCount - mSerializedUnitSize;
			bits.Read(buffer.AsSpan(0, payloadByteCount));
			bits.Read(buffer.AsSpan(payloadByteCount));
		}
		else
		{
			bits.Read(buffer);
		}
		return mEncoding.DecodeKnownPayload(buffer, GetKnownPayloadKind());
	}

	private int GetPayloadOrRecordByteCount(global::KSoft.IO.EndianReader reader)
	{
		return Framing == StringStorageReadFraming.Pascal
			? checked(reader.ReadInt32() * sizeof(char))
			: mRecord.Length;
	}

	private int GetPayloadOrRecordByteCount(global::KSoft.IO.BitStream bits)
	{
		return Framing == StringStorageReadFraming.Pascal
			? checked(bits.ReadInt32() * sizeof(char))
			: mRecord.Length;
	}

	private global::KSoft.Text.StringStorageEncoding.KnownPayloadKind GetKnownPayloadKind()
	{
		return Framing switch
		{
			StringStorageReadFraming.FixedCString =>
				global::KSoft.Text.StringStorageEncoding.KnownPayloadKind.FixedCString,
			StringStorageReadFraming.ExplicitCString =>
				global::KSoft.Text.StringStorageEncoding.KnownPayloadKind.ExplicitCString,
			StringStorageReadFraming.Pascal =>
				global::KSoft.Text.StringStorageEncoding.KnownPayloadKind.Complete,
			StringStorageReadFraming.FixedCharArray =>
				global::KSoft.Text.StringStorageEncoding.KnownPayloadKind.CharArray,
			_ => throw new InvalidOperationException(Framing.ToString()),
		};
	}
}
