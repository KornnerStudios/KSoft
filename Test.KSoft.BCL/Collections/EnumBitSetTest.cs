using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Collections.Test;

[TestClass]
public sealed class EnumBitSetTest
{
	enum ImplicitBits { First, Second, Third }
	enum CountBits { First, Second, Third, kNumberOf }
	enum MaxBits { First, Second, Third, kMax }
	enum AliasBits { First, Alias = First, Third = 2 }
	enum WordBits { First, Fifth = 5, Last32 = 31, Next32, Last64 = 63, Next64, kNumberOf }
	enum LongBits : long { First, Second }
	enum UnsignedBits : ulong { First, Second }
	enum EmptyBits { }
	enum SingleBit { First }
	enum InvalidBoundBits { First, Second, kNumberOf = Second }
	enum ConflictingBoundBits { First, kNumberOf = 1, kMax = 2 }
	enum TooLargeBits : uint { Highest = int.MaxValue }
	[Flags]
	enum WideFlags : uint { All = uint.MaxValue }

	[TestMethod]
	public void Length_EmptyEnum_HasNoAddressableBits()
	{
		var bits = new EnumBitSet<EmptyBits>();

		Assert.AreEqual(0, bits.Length);
		Assert.AreEqual(0, bits.Cardinality);
		Assert.IsEmpty(bits);
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => bits[default] = true);
	}

	[TestMethod]
	public void Constructor_UnrepresentableExtentOrFlags_ThrowsBeforeStorageAllocation()
	{
		Assert.ThrowsExactly<ArgumentException>(() => new EnumBitSet<TooLargeBits>());
		Assert.ThrowsExactly<ArgumentException>(() => new EnumBitSet<WideFlags>());
	}

	[TestMethod]
	public void Length_SingleMember_ProvidesOneBit()
	{
		var bits = new EnumBitSet<SingleBit>();

		Assert.AreEqual(1, bits.Length);
		bits[SingleBit.First] = true;
		Assert.IsTrue(bits[SingleBit.First]);
		Assert.AreEqual(1, bits.Cardinality);
	}

	[TestMethod]
	public void Constructor_InvalidExclusiveBounds_ThrowsArgumentException()
	{
		Assert.ThrowsExactly<ArgumentException>(() => new EnumBitSet<InvalidBoundBits>());
		Assert.ThrowsExactly<ArgumentException>(() => new EnumBitSet<ConflictingBoundBits>());
	}

	[TestMethod]
	public void Length_EnumExtent_NotEncodedValueWidth()
	{
		var implicitBits = new EnumBitSet<ImplicitBits>();
		var countBits = new EnumBitSet<CountBits>();
		var maxBits = new EnumBitSet<MaxBits>();
		var aliases = new EnumBitSet<AliasBits>();

		Assert.AreEqual(3, implicitBits.Length);
		Assert.AreEqual(3, countBits.Length);
		Assert.AreEqual(3, maxBits.Length);
		Assert.AreEqual(3, aliases.Length);
		implicitBits[ImplicitBits.Third] = true;
		countBits[CountBits.Third] = true;
		maxBits[MaxBits.Third] = true;
		aliases[AliasBits.Third] = true;

		Assert.IsTrue(implicitBits.Get(ImplicitBits.Third));
		Assert.IsTrue(countBits.Get(CountBits.Third));
		Assert.IsTrue(maxBits.Get(MaxBits.Third));
		Assert.IsTrue(aliases.Get(AliasBits.Third));
		Assert.AreEqual(1, implicitBits.Cardinality);
		Assert.AreEqual(2, implicitBits.CardinalityZeros);
	}

	[TestMethod]
	public void Access_WordBoundaries_AllDeclaredIndicesFit()
	{
		var bits = CreateWordBits();

		Assert.AreEqual(65, bits.Length);
		Assert.AreEqual(6, bits.Cardinality);
		foreach (var bit in new[] { WordBits.First, WordBits.Fifth, WordBits.Last32, WordBits.Next32, WordBits.Last64, WordBits.Next64 })
		{
			Assert.IsTrue(bits[bit], bit.ToString());
		}
		Assert.AreEqual(WordBits.Next64, bits.NextSetBit(WordBits.Next64));
		Assert.AreEqual(64, bits.NextSetBitIndex(WordBits.Next64));
	}

	[TestMethod]
	[DataRow(Shell.EndianFormat.Big, Shell.EndianFormat.Big, "840000018000000180000000")]
	[DataRow(Shell.EndianFormat.Little, Shell.EndianFormat.Big, "010000840100008000000080")]
	[DataRow(Shell.EndianFormat.Big, Shell.EndianFormat.Little, "800000218000000100000001")]
	[DataRow(Shell.EndianFormat.Little, Shell.EndianFormat.Little, "210000800100008001000000")]
	public void SerializeWords_EndianStream_UsesThreeWords(
		Shell.EndianFormat byteOrder, Shell.EndianFormat bitOrder, string expectedHex)
	{
		// MSB-first indices 0,5,31; 32,63; 64 give words 84000001,80000001,80000000.
		var bits = CreateWordBits();
		using var buffer = new MemoryStream();
		using var stream = new IO.EndianStream(buffer, byteOrder);
		stream.StreamMode = FileAccess.Write;

		bits.SerializeWords(stream, bitOrder);

		CollectionAssert.AreEqual(Convert.FromHexString(expectedHex), buffer.ToArray());
		stream.Writer.Write((byte)0xCC);
		buffer.Position = 0;
		stream.StreamMode = FileAccess.Read;
		var read = new EnumBitSet<WordBits>();
		read.SerializeWords(stream, bitOrder);

		Assert.IsTrue(bits.Equals(read));
		Assert.AreEqual(6, read.Cardinality);
		Assert.AreEqual(12L, buffer.Position);
		Assert.AreEqual(0xCC, stream.Reader.ReadByte());
	}

	[TestMethod]
	[DataRow(Shell.EndianFormat.Big, "840000018000000180000000")]
	[DataRow(Shell.EndianFormat.Little, "800000218000000100000001")]
	public void SerializeWords_BitStream_UsesThreeWords(Shell.EndianFormat bitOrder, string expectedHex)
	{
		var bits = CreateWordBits();
		using var buffer = new MemoryStream();
		using (var stream = new IO.BitStream(buffer, FileAccess.Write))
		{
			stream.StreamMode = FileAccess.Write;
			bits.SerializeWords(stream, bitOrder);
		}
		CollectionAssert.AreEqual(Convert.FromHexString(expectedHex), buffer.ToArray());

		using var input = new MemoryStream(Convert.FromHexString(expectedHex));
		using var reader = new IO.BitStream(input, FileAccess.Read);
		reader.StreamMode = FileAccess.Read;
		var read = new EnumBitSet<WordBits>();
		read.SerializeWords(reader, bitOrder);

		Assert.IsTrue(bits.Equals(read));
		Assert.AreEqual(6, read.Cardinality);
		Assert.AreEqual(96, reader.BitPosition);
	}

	[TestMethod]
	public void SerializeWords_TruncatedThirdWord_ThrowsEndOfStream()
	{
		using var buffer = new MemoryStream(Convert.FromHexString("8400000180000001800000"));
		using var stream = new IO.EndianStream(buffer, Shell.EndianFormat.Big);
		stream.StreamMode = FileAccess.Read;
		var bits = new EnumBitSet<WordBits>();

		Assert.ThrowsExactly<EndOfStreamException>(() => bits.SerializeWords(stream));
	}

	[TestMethod]
	[DataRow((LongBits)0x1_0000_0000L)]
	[DataRow((LongBits)(-0x1_0000_0000L))]
	[DataRow((LongBits)(-1L))]
	[DataRow((LongBits)2L)]
	[DataRow((UnsignedBits)0x1_0000_0000UL)]
	[DataRow((UnsignedBits)ulong.MaxValue)]
	[DataRow((UnsignedBits)2UL)]
	public void Access_InvalidEnumValue_ThrowsWithoutMutation<TEnum>(TEnum invalid)
		where TEnum : struct, Enum
	{
		var bits = new EnumBitSet<TEnum>();
		bits[default] = true;

		AssertInvalid("bitIndex", () => _ = bits[invalid]);
		AssertInvalid("bitIndex", () => bits[invalid] = false);
		AssertInvalid("bitIndex", () => bits.Get(invalid));
		AssertInvalid("bitIndex", () => bits.Set(invalid, false));
		AssertInvalid("bitIndex", () => bits.Toggle(invalid));
		AssertInvalid("startBitIndex", () => bits.NextSetBitIndex(invalid));
		AssertInvalid("startBitIndex", () => bits.NextClearBitIndex(invalid));
		AssertInvalid("startBitIndex", () => bits.NextSetBit(invalid));
		AssertInvalid("startBitIndex", () => bits.NextClearBit(invalid));

		Assert.IsTrue(bits[default]);
		Assert.AreEqual(1, bits.Cardinality);
	}

	static EnumBitSet<WordBits> CreateWordBits()
	{
		var bits = new EnumBitSet<WordBits>();
		foreach (var bit in new[] { WordBits.First, WordBits.Fifth, WordBits.Last32, WordBits.Next32, WordBits.Last64, WordBits.Next64 })
		{
			bits[bit] = true;
		}
		return bits;
	}

	static void AssertInvalid(string parameter, Action action)
	{
		var error = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);
		Assert.AreEqual(parameter, error.ParamName);
	}
}
