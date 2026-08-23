using System;
using System.Diagnostics.CodeAnalysis;

namespace KSoft.IO
{
	using EnumUtils = Reflection.EnumUtils;

	/// <summary>Utility for streaming enum types to/from bitstreams</summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	/// <typeparam name="TStreamType">Integer-type to stream the enum value as</typeparam>
	/// <typeparam name="TOptions">TBD</typeparam>
	/// <remarks>
	/// Streamers cache one typed method per stream type instead of compiling expression delegates for every closed enum
	/// streamer. Numeric casts between stream type and enum backing type are still handled by
	/// <see cref="Reflection.EnumValue{TEnum}"/>. Decode applies none-sentinel adjustment before optional bit swap;
	/// encode applies those options in the same order before writing.
	/// </remarks>
	[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Static initialization creates delegates, cached options, and the instance from one validated configuration.")]
	public class EnumBitStreamer<TEnum, TStreamType, TOptions> : EnumBitStreamerBase, IEnumBitStreamer<TEnum>
		where TEnum : struct, Enum
		where TStreamType : struct
		where TOptions : EnumBitStreamerOptions, new()
	{
		class MethodGenerationArgs
		{
			/// <summary>Integer type code to stream the enum value as</summary>
			public readonly TypeCode StreamTypeCode;
			public readonly bool StreamTypeIsSigned;

			public TOptions Options;

			void AssertStreamTypeIsValid(Type streamType)
			{
				if (!EnumUtils.TypeIsSupported(StreamTypeCode))
				{
					var message = string.Format(Util.InvariantCultureInfo, "{0} is an invalid stream type", streamType);

					throw new NotSupportedException(message);
				}
			}

			public MethodGenerationArgs()
			{
				Type enum_type = typeof(TEnum);
				Type stream_type = typeof(TStreamType);
				Type underlying_type = Enum.GetUnderlyingType(enum_type);

				// Preserve the single-type-parameter streamer shape by resolving the marker type to the enum's backing
				// type once per closed generic streamer.
				if (stream_type == typeof(EnumBinaryStreamerUseUnderlyingType))
				{
					stream_type = underlying_type;
				}

				EnumUtils.AssertTypeIsEnum(enum_type);
				EnumUtils.AssertUnderlyingTypeIsSupported(enum_type, underlying_type);
				StreamTypeCode = Type.GetTypeCode(stream_type);
				StreamTypeIsSigned = StreamTypeCode.IsSigned();
				AssertStreamTypeIsValid(stream_type);

				Options = new TOptions();

				if (Options.UseNoneSentinelEncoding)
				{
					// None-sentinel encoding maps an enum value of -1 to encoded zero by adding one before write and
					// subtracting one after read. Byte-sized stream types do not have enough room for that sentinel.
					if (stream_type == typeof(sbyte) || stream_type == typeof(byte))
					{
						throw new ArgumentException(
							"{0}: UseNoneSentinelEncoding can't operate on (s)byte types (StreamType)",
							enum_type.FullName);
					}
				}
				#region Options.BitSwap
				if (Options.BitSwap)
				{
					// Bit swapping is a bit-order transform for unsigned flag-style payloads. Signed stream types have
					// sign-extension semantics, so accepting them here would make the encoded bits ambiguous.
					if (StreamTypeIsSigned)
					{
						throw new ArgumentException(
							"{0}: Bit-swapping only makes sense on flags/unsigned types, but StreamType is signed",
							enum_type.FullName);
					}
				}
				else
				{
					if (Options.BitSwapGuardAgainstOneBit)
					{
						Debug.Trace.IO.TraceInformation("{0}'s {1} says we should guard against one bit cases, but not bitswap",
							enum_type.FullName, typeof(TOptions).FullName);
					}
				}
				#endregion
			}
		};

		/// <summary>Cached method for reading enum values</summary>
		static readonly ReadDelegate kRead;
		/// <summary>Cached method for writing enum values</summary>
		static readonly Action<IO.BitStream, TEnum, int> kWrite;
		static readonly bool kUseNoneSentinelEncoding;
		static readonly bool kSignExtend;
		static readonly bool kBitSwap;
		static readonly bool kBitSwapGuardAgainstOneBit;

		/// <summary>Object for referencing the streamer functionality as an instance instead of as a type</summary>
		public static readonly IEnumBitStreamer<TEnum> Instance;

		/// <summary>Initializes the <see cref="EnumBitStreamer{TEnum}"/> class by caching the IO methods.</summary>
		static EnumBitStreamer()
		{
			var generation_args = new MethodGenerationArgs();

			// Cache option values beside the read/write delegates so hot calls do not construct or inspect options.
			kUseNoneSentinelEncoding = generation_args.Options.UseNoneSentinelEncoding;
			kSignExtend = generation_args.Options.SignExtend;
			kBitSwap = generation_args.Options.BitSwap;
			kBitSwapGuardAgainstOneBit = generation_args.Options.BitSwapGuardAgainstOneBit;

			kRead = CreateReadMethod(generation_args.StreamTypeCode);
			kWrite = CreateWriteMethod(generation_args.StreamTypeCode);

			Instance = new EnumBitStreamer<TEnum, TStreamType, TOptions>();
		}

		#region Streamer delegates
		/// <summary>Signature for a method which reads a <typeparamref name="TEnum"/> value from a stream</summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="v">Value read from the stream</param>
		/// <param name="bitCount"></param>
		public delegate void ReadDelegate(IO.BitStream s, out TEnum v, int bitCount);

		static ReadDelegate CreateReadMethod(TypeCode streamTypeCode)
		{
			// Choose the exact BitStream read overload once. This replaces the old reflection/expression tree path while
			// still preserving the caller-selected stream width.
			return streamTypeCode switch
			{
				TypeCode.Byte => ReadByte,
				TypeCode.SByte => ReadSByte,
				TypeCode.UInt16 => ReadUInt16,
				TypeCode.Int16 => ReadInt16,
				TypeCode.UInt32 => ReadUInt32,
				TypeCode.Int32 => ReadInt32,
				TypeCode.UInt64 => ReadUInt64,
				TypeCode.Int64 => ReadInt64,
				_ => throw new NotSupportedException(),
			};
		}

		static Action<IO.BitStream, TEnum, int> CreateWriteMethod(TypeCode streamTypeCode)
		{
			// Choose the matching BitStream write overload once; each typed delegate only performs the option transforms
			// needed for that stream type and then converts through EnumValue<TEnum>.
			return streamTypeCode switch
			{
				TypeCode.Byte => WriteByte,
				TypeCode.SByte => WriteSByte,
				TypeCode.UInt16 => WriteUInt16,
				TypeCode.Int16 => WriteInt16,
				TypeCode.UInt32 => WriteUInt32,
				TypeCode.Int32 => WriteInt32,
				TypeCode.UInt64 => WriteUInt64,
				TypeCode.Int64 => WriteInt64,
				_ => throw new NotSupportedException(),
			};
		}

		// Unsigned stream types can participate in bit-swap. Signed stream types only apply sign extension through the
		// BitStream read overload and optional none-sentinel adjustment.
		static void ReadByte(IO.BitStream s, out TEnum v, int bitCount)
		{
			var value = DecodeUnsignedByte(s.ReadByte(bitCount), bitCount);
			v = Reflection.EnumValue<TEnum>.FromByte(value);
		}

		static void ReadSByte(IO.BitStream s, out TEnum v, int bitCount)
		{
			var value = DecodeSignedSByte(s.ReadSByte(bitCount, kSignExtend));
			v = Reflection.EnumValue<TEnum>.FromSByte(value);
		}

		static void ReadUInt16(IO.BitStream s, out TEnum v, int bitCount)
		{
			var value = DecodeUnsignedUInt16(s.ReadUInt16(bitCount), bitCount);
			v = Reflection.EnumValue<TEnum>.FromUInt16(value);
		}

		static void ReadInt16(IO.BitStream s, out TEnum v, int bitCount)
		{
			var value = DecodeSignedInt16(s.ReadInt16(bitCount, kSignExtend));
			v = Reflection.EnumValue<TEnum>.FromInt16(value);
		}

		static void ReadUInt32(IO.BitStream s, out TEnum v, int bitCount)
		{
			var value = DecodeUnsignedUInt32(s.ReadUInt32(bitCount), bitCount);
			v = Reflection.EnumValue<TEnum>.FromUInt32(value);
		}

		static void ReadInt32(IO.BitStream s, out TEnum v, int bitCount)
		{
			var value = DecodeSignedInt32(s.ReadInt32(bitCount, kSignExtend));
			v = Reflection.EnumValue<TEnum>.FromInt32(value);
		}

		static void ReadUInt64(IO.BitStream s, out TEnum v, int bitCount)
		{
			var value = DecodeUnsignedUInt64(s.ReadUInt64(bitCount), bitCount);
			v = Reflection.EnumValue<TEnum>.FromUInt64(value);
		}

		static void ReadInt64(IO.BitStream s, out TEnum v, int bitCount)
		{
			var value = DecodeSignedInt64(s.ReadInt64(bitCount, kSignExtend));
			v = Reflection.EnumValue<TEnum>.FromInt64(value);
		}

		static void WriteByte(IO.BitStream s, TEnum value, int bitCount)
		{
			s.Write(EncodeUnsignedByte(Reflection.EnumValue<TEnum>.ToByte(value), bitCount), bitCount);
		}

		static void WriteSByte(IO.BitStream s, TEnum value, int bitCount)
		{
			s.Write(EncodeSignedSByte(Reflection.EnumValue<TEnum>.ToSByte(value)), bitCount);
		}

		static void WriteUInt16(IO.BitStream s, TEnum value, int bitCount)
		{
			s.Write(EncodeUnsignedUInt16(Reflection.EnumValue<TEnum>.ToUInt16(value), bitCount), bitCount);
		}

		static void WriteInt16(IO.BitStream s, TEnum value, int bitCount)
		{
			s.Write(EncodeSignedInt16(Reflection.EnumValue<TEnum>.ToInt16(value)), bitCount);
		}

		static void WriteUInt32(IO.BitStream s, TEnum value, int bitCount)
		{
			s.Write(EncodeUnsignedUInt32(Reflection.EnumValue<TEnum>.ToUInt32(value), bitCount), bitCount);
		}

		static void WriteInt32(IO.BitStream s, TEnum value, int bitCount)
		{
			s.Write(EncodeSignedInt32(Reflection.EnumValue<TEnum>.ToInt32(value)), bitCount);
		}

		static void WriteUInt64(IO.BitStream s, TEnum value, int bitCount)
		{
			s.Write(EncodeUnsignedUInt64(Reflection.EnumValue<TEnum>.ToUInt64(value), bitCount), bitCount);
		}

		static void WriteInt64(IO.BitStream s, TEnum value, int bitCount)
		{
			s.Write(EncodeSignedInt64(Reflection.EnumValue<TEnum>.ToInt64(value)), bitCount);
		}

		// Keep the transform order from the generated expression delegates:
		// read raw bits -> subtract the none sentinel -> optionally bit-swap -> convert to TEnum.
		static byte DecodeUnsignedByte(byte value, int bitCount)
		{
			if (kUseNoneSentinelEncoding)
			{
				value = unchecked((byte)(value - 1));
			}

			return ApplyBitSwap(value, bitCount);
		}

		static sbyte DecodeSignedSByte(sbyte value)
		{
			if (kUseNoneSentinelEncoding)
			{
				value = unchecked((sbyte)(value - 1));
			}

			return value;
		}

		static ushort DecodeUnsignedUInt16(ushort value, int bitCount)
		{
			if (kUseNoneSentinelEncoding)
			{
				value = unchecked((ushort)(value - 1));
			}

			return ApplyBitSwap(value, bitCount);
		}

		static short DecodeSignedInt16(short value)
		{
			if (kUseNoneSentinelEncoding)
			{
				value = unchecked((short)(value - 1));
			}

			return value;
		}

		static uint DecodeUnsignedUInt32(uint value, int bitCount)
		{
			if (kUseNoneSentinelEncoding)
			{
				value = unchecked(value - 1);
			}

			return ApplyBitSwap(value, bitCount);
		}

		static int DecodeSignedInt32(int value)
		{
			if (kUseNoneSentinelEncoding)
			{
				value = unchecked(value - 1);
			}

			return value;
		}

		static ulong DecodeUnsignedUInt64(ulong value, int bitCount)
		{
			if (kUseNoneSentinelEncoding)
			{
				value = unchecked(value - 1);
			}

			return ApplyBitSwap(value, bitCount);
		}

		static long DecodeSignedInt64(long value)
		{
			if (kUseNoneSentinelEncoding)
			{
				value = unchecked(value - 1);
			}

			return value;
		}

		// Encode mirrors decode in the same order before the final write:
		// convert from TEnum -> add the none sentinel -> optionally bit-swap -> write raw bits.
		static byte EncodeUnsignedByte(byte value, int bitCount)
		{
			if (kUseNoneSentinelEncoding)
			{
				value = unchecked((byte)(value + 1));
			}

			return ApplyBitSwap(value, bitCount);
		}

		static sbyte EncodeSignedSByte(sbyte value)
		{
			if (kUseNoneSentinelEncoding)
			{
				value = unchecked((sbyte)(value + 1));
			}

			return value;
		}

		static ushort EncodeUnsignedUInt16(ushort value, int bitCount)
		{
			if (kUseNoneSentinelEncoding)
			{
				value = unchecked((ushort)(value + 1));
			}

			return ApplyBitSwap(value, bitCount);
		}

		static short EncodeSignedInt16(short value)
		{
			if (kUseNoneSentinelEncoding)
			{
				value = unchecked((short)(value + 1));
			}

			return value;
		}

		static uint EncodeUnsignedUInt32(uint value, int bitCount)
		{
			if (kUseNoneSentinelEncoding)
			{
				value = unchecked(value + 1);
			}

			return ApplyBitSwap(value, bitCount);
		}

		static int EncodeSignedInt32(int value)
		{
			if (kUseNoneSentinelEncoding)
			{
				value = unchecked(value + 1);
			}

			return value;
		}

		static ulong EncodeUnsignedUInt64(ulong value, int bitCount)
		{
			if (kUseNoneSentinelEncoding)
			{
				value = unchecked(value + 1);
			}

			return ApplyBitSwap(value, bitCount);
		}

		static long EncodeSignedInt64(long value)
		{
			if (kUseNoneSentinelEncoding)
			{
				value = unchecked(value + 1);
			}

			return value;
		}

		// BitStream uses a bit count, but Bits.BitSwap expects the zero-based index of the highest participating bit.
		// The one-bit guard preserves existing callers that requested a no-op for single-bit flag fields.
		static byte ApplyBitSwap(byte value, int bitCount)
		{
			if (!kBitSwap)
			{
				return value;
			}

			int start_bit_index = bitCount - 1;
			return kBitSwapGuardAgainstOneBit && start_bit_index == 0
				? value
				: Bits.BitSwap(value, start_bit_index);
		}

		static ushort ApplyBitSwap(ushort value, int bitCount)
		{
			if (!kBitSwap)
			{
				return value;
			}

			int start_bit_index = bitCount - 1;
			return kBitSwapGuardAgainstOneBit && start_bit_index == 0
				? value
				: Bits.BitSwap(value, start_bit_index);
		}

		static uint ApplyBitSwap(uint value, int bitCount)
		{
			if (!kBitSwap)
			{
				return value;
			}

			int start_bit_index = bitCount - 1;
			return kBitSwapGuardAgainstOneBit && start_bit_index == 0
				? value
				: Bits.BitSwap(value, start_bit_index);
		}

		static ulong ApplyBitSwap(ulong value, int bitCount)
		{
			if (!kBitSwap)
			{
				return value;
			}

			int start_bit_index = bitCount - 1;
			return kBitSwapGuardAgainstOneBit && start_bit_index == 0
				? value
				: Bits.BitSwap(value, start_bit_index);
		}
		#endregion

		#region Static interface
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="IO.BitStream"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="bitCount"></param>
		/// <returns>Value read from the stream</returns>
		public static TEnum Read(IO.BitStream s, int bitCount)
		{
			ArgumentNullException.ThrowIfNull(s);
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bitCount);

			kRead(s, out TEnum value, bitCount);

			return value;
		}
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="IO.BitStream"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="value">Value read from the stream</param>
		/// <param name="bitCount"></param>
		public static void Read(IO.BitStream s, out TEnum value, int bitCount)
		{
			ArgumentNullException.ThrowIfNull(s);
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bitCount);

			kRead(s, out value, bitCount);
		}
		/// <summary>Stream a <typeparamref name="TEnum"/> value to a <see cref="IO.BitStream"/></summary>
		/// <param name="s">Writer we're streaming to</param>
		/// <param name="value"></param>
		/// <param name="bitCount"></param>
		public static void Write(IO.BitStream s, TEnum value, int bitCount)
		{
			ArgumentNullException.ThrowIfNull(s);
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bitCount);

			kWrite(s, value, bitCount);
		}

		/// <summary>Serialize a <typeparamref name="TEnum"/> value using an <see cref="IO.BitStream"/></summary>
		/// <param name="s">Stream we're using for serialization</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="bitCount"></param>
		public static void Stream(IO.BitStream s, ref TEnum value, int bitCount)
		{
			ArgumentNullException.ThrowIfNull(s);
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bitCount);

				 if (s.IsReading) { Read(s, out value, bitCount); }
			else if (s.IsWriting) { Write(s, value, bitCount); }
		}
		#endregion

		#region IEnumBitStreamer<TEnum> Members
		TEnum IEnumBitStreamer<TEnum>.Read(IO.BitStream s, int bitCount)					{ return Read(s, bitCount); }
		void IEnumBitStreamer<TEnum>.Read(IO.BitStream s, out TEnum value, int bitCount)	{ Read(s, out value, bitCount); }
		void IEnumBitStreamer<TEnum>.Write(IO.BitStream s, TEnum value, int bitCount)		{ Write(s, value, bitCount); }
		void IEnumBitStreamer<TEnum>.Stream(IO.BitStream s, ref TEnum value, int bitCount)	{ Stream(s, ref value, bitCount); }
		#endregion
	};

	/// <summary>Utility for streaming enum types to/from bitstreams</summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	/// <typeparam name="TStreamType">Integer-type to stream the enum value as</typeparam>
	/// <remarks>Uses the default options in <see cref="EnumBitStreamerOptions"/></remarks>
	public class EnumBitStreamer<TEnum, TStreamType> : EnumBitStreamer<TEnum, TStreamType, EnumBitStreamerOptions>
		where TEnum : struct, Enum
		where TStreamType : struct
	{
	};

	/// <summary>Utility for streaming enum types to/from bitstreams</summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	/// <remarks>Implicitly uses the Enum's underlying type for the stream type</remarks>
	public sealed class EnumBitStreamer<TEnum> : EnumBitStreamer<TEnum, EnumBinaryStreamerUseUnderlyingType>
		where TEnum : struct, Enum
	{
	};

	public sealed class EnumBitStreamerWithOptions<TEnum, TOptions>
		: EnumBitStreamer<TEnum, EnumBinaryStreamerUseUnderlyingType, TOptions>
		where TEnum : struct, Enum
		where TOptions : EnumBitStreamerOptions, new()
	{
	};
}
