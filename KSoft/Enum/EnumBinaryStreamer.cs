using System;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	using EnumUtils = Reflection.EnumUtils;

	/// <summary>Base type for enum binary streamers.</summary>
	public abstract class EnumBinaryStreamerBase
	{
	};

	#region IEnumBinaryStreamer
	/// <summary>
	/// Interface for using an <see cref="EnumBinaryStreamer{TEnum,TStreamType}"/>'s functionality via an instance object
	/// </summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	public interface IEnumBinaryStreamer<TEnum>
		where TEnum : struct, Enum
	{
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="BinaryReader"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <returns>Value read from the stream</returns>
		TEnum Read(BinaryReader s);
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="BinaryReader"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="value">Value read from the stream</param>
		void Read(BinaryReader s, out TEnum value);

		/// <summary>Stream a <typeparamref name="TEnum"/> value to a <see cref="BinaryWriter"/></summary>
		/// <param name="s">Writer we're streaming to</param>
		/// <param name="value"></param>
		void Write(BinaryWriter s, TEnum value);
	};
	#endregion

	#region IEnumEndianStreamer
	/// <summary>
	/// Interface for using an <see cref="EnumBinaryStreamer{TEnum,TStreamType}"/>'s functionality via an instance object
	/// </summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	public interface IEnumEndianStreamer<TEnum> : IEnumBinaryStreamer<TEnum>
		where TEnum : struct, Enum
	{
		void Stream(IO.EndianStream s, ref TEnum value);
	};
	#endregion

	public static class EnumBinaryStreamer
	{
		#region IEnumBinaryStreamer
		public static IEnumBinaryStreamer<TEnum> ForBinary<TEnum, TStreamType>()
			where TEnum : struct, Enum
			where TStreamType : struct
		{
			Contract.Ensures(Contract.Result<IEnumBinaryStreamer<TEnum>>() != null);

			return EnumBinaryStreamer<TEnum, TStreamType>.Instance;
		}
		public static IEnumBinaryStreamer<TEnum> ForBinary<TEnum>()
			where TEnum : struct, Enum
		{
			Contract.Ensures(Contract.Result<IEnumBinaryStreamer<TEnum>>() != null);

			return EnumBinaryStreamer<TEnum>.Instance;
		}
		#endregion

		#region IEnumEndianStreamer
		public static IEnumEndianStreamer<TEnum> For<TEnum, TStreamType>()
			where TEnum : struct, Enum
			where TStreamType : struct
		{
			Contract.Ensures(Contract.Result<IEnumEndianStreamer<TEnum>>() != null);

			return EnumBinaryStreamer<TEnum, TStreamType>.Instance;
		}
		public static IEnumEndianStreamer<TEnum> For<TEnum>()
			where TEnum : struct, Enum
		{
			Contract.Ensures(Contract.Result<IEnumEndianStreamer<TEnum>>() != null);

			return EnumBinaryStreamer<TEnum>.Instance;
		}
		#endregion
	};

	/// <summary>Utility for streaming enum types to/from binary streams</summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	/// <typeparam name="TStreamType">Integer-type to stream the enum value as</typeparam>
	/// <remarks>
	/// Streamers cache one typed method per stream type instead of compiling expression delegates for every closed enum
	/// streamer. Numeric casts between stream type and enum backing type are still handled by
	/// <see cref="Reflection.EnumValue{TEnum}"/> so signed/unsigned wrap behavior stays centralized.
	/// </remarks>
	public class EnumBinaryStreamer<TEnum, TStreamType> : EnumBinaryStreamerBase, IEnumEndianStreamer<TEnum>
		where TEnum : struct, Enum
		where TStreamType : struct
	{
		class MethodGenerationArgs
		{
			/// <summary>Integer type code to stream the enum value as</summary>
			public readonly TypeCode StreamTypeCode;

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

				// Check if the user wants us to always use the underlying type
				if (stream_type == typeof(EnumBinaryStreamerUseUnderlyingType))
				{
					stream_type = underlying_type;
				}

				EnumUtils.AssertTypeIsEnum(enum_type);
				EnumUtils.AssertUnderlyingTypeIsSupported(enum_type, underlying_type);

				StreamTypeCode = Type.GetTypeCode(stream_type);
				AssertStreamTypeIsValid(stream_type);
			}
		};

		/// <summary>Cached method for reading enum values</summary>
		static readonly ReadDelegate kRead;
		/// <summary>Cached method for writing enum values</summary>
		static readonly Action<BinaryWriter, TEnum> kWrite;

		/// <summary>Object for referencing the streamer functionality as an instance instead of as a type</summary>
		public static readonly IEnumEndianStreamer<TEnum> Instance;

		/// <summary>Initializes the <see cref="EnumBinaryStreamer{TEnum}"/> class by caching the IO methods.</summary>
		static EnumBinaryStreamer()
		{
			var generation_args = new MethodGenerationArgs();
			kRead = CreateReadMethod(generation_args.StreamTypeCode);
			kWrite = CreateWriteMethod(generation_args.StreamTypeCode);

			Instance = new EnumBinaryStreamer<TEnum, TStreamType>();
		}

		#region Streamer delegates
		/// <summary>Signature for a method which reads a <typeparamref name="TEnum"/> value from a stream</summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="v">Value read from the stream</param>
		public delegate void ReadDelegate(BinaryReader s, out TEnum v);

		static ReadDelegate CreateReadMethod(TypeCode streamTypeCode)
		{
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

		static Action<BinaryWriter, TEnum> CreateWriteMethod(TypeCode streamTypeCode)
		{
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

		static void ReadByte(BinaryReader s, out TEnum v)	{ v = Reflection.EnumValue<TEnum>.FromByte(s.ReadByte()); }
		static void ReadSByte(BinaryReader s, out TEnum v)	{ v = Reflection.EnumValue<TEnum>.FromSByte(s.ReadSByte()); }
		static void ReadUInt16(BinaryReader s, out TEnum v)	{ v = Reflection.EnumValue<TEnum>.FromUInt16(s.ReadUInt16()); }
		static void ReadInt16(BinaryReader s, out TEnum v)	{ v = Reflection.EnumValue<TEnum>.FromInt16(s.ReadInt16()); }
		static void ReadUInt32(BinaryReader s, out TEnum v)	{ v = Reflection.EnumValue<TEnum>.FromUInt32(s.ReadUInt32()); }
		static void ReadInt32(BinaryReader s, out TEnum v)	{ v = Reflection.EnumValue<TEnum>.FromInt32(s.ReadInt32()); }
		static void ReadUInt64(BinaryReader s, out TEnum v)	{ v = Reflection.EnumValue<TEnum>.FromUInt64(s.ReadUInt64()); }
		static void ReadInt64(BinaryReader s, out TEnum v)	{ v = Reflection.EnumValue<TEnum>.FromInt64(s.ReadInt64()); }

		static void WriteByte(BinaryWriter s, TEnum value)	{ s.Write(Reflection.EnumValue<TEnum>.ToByte(value)); }
		static void WriteSByte(BinaryWriter s, TEnum value)	{ s.Write(Reflection.EnumValue<TEnum>.ToSByte(value)); }
		static void WriteUInt16(BinaryWriter s, TEnum value)	{ s.Write(Reflection.EnumValue<TEnum>.ToUInt16(value)); }
		static void WriteInt16(BinaryWriter s, TEnum value)	{ s.Write(Reflection.EnumValue<TEnum>.ToInt16(value)); }
		static void WriteUInt32(BinaryWriter s, TEnum value)	{ s.Write(Reflection.EnumValue<TEnum>.ToUInt32(value)); }
		static void WriteInt32(BinaryWriter s, TEnum value)	{ s.Write(Reflection.EnumValue<TEnum>.ToInt32(value)); }
		static void WriteUInt64(BinaryWriter s, TEnum value)	{ s.Write(Reflection.EnumValue<TEnum>.ToUInt64(value)); }
		static void WriteInt64(BinaryWriter s, TEnum value)	{ s.Write(Reflection.EnumValue<TEnum>.ToInt64(value)); }
		#endregion

		#region Static interface
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="BinaryReader"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <returns>Value read from the stream</returns>
		public static TEnum Read(BinaryReader s)
		{
			ArgumentNullException.ThrowIfNull(s);

			kRead(s, out TEnum value);

			return value;
		}
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="BinaryReader"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="value">Value read from the stream</param>
		public static void Read(BinaryReader s, out TEnum value)
		{
			ArgumentNullException.ThrowIfNull(s);

			kRead(s, out value);
		}
		/// <summary>Stream a <typeparamref name="TEnum"/> value to a <see cref="BinaryWriter"/></summary>
		/// <param name="s">Writer we're streaming to</param>
		/// <param name="value"></param>
		public static void Write(BinaryWriter s, TEnum value)
		{
			ArgumentNullException.ThrowIfNull(s);

			kWrite(s, value);
		}

		/// <summary>Serialize a <typeparamref name="TEnum"/> value using an <see cref="IO.EndianStream"/></summary>
		/// <param name="s">Stream we're using for serialization</param>
		/// <param name="value">Value to serialize</param>
		public static void Stream(IO.EndianStream s, ref TEnum value)
		{
			ArgumentNullException.ThrowIfNull(s);

				 if (s.IsReading) { Read(s.Reader, out value); }
			else if (s.IsWriting) { Write(s.Writer, value); }
		}
		#endregion

		#region IEnumEndianStreamer<TEnum> Members
		TEnum IEnumBinaryStreamer<TEnum>.Read(BinaryReader s)						{ return Read(s); }
		void IEnumBinaryStreamer<TEnum>.Read(BinaryReader s, out TEnum value)		{ Read(s, out value); }
		void IEnumBinaryStreamer<TEnum>.Write(BinaryWriter s, TEnum value)			{ Write(s, value); }
		void IEnumEndianStreamer<TEnum>.Stream(IO.EndianStream s, ref TEnum value)	{ Stream(s, ref value); }
		#endregion
	};

	public struct EnumBinaryStreamerUseUnderlyingType {};

	/// <summary>Utility for streaming enum types to/from binary streams</summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	/// <remarks>Implicitly uses the Enum's underlying type for the stream type</remarks>
	public sealed class EnumBinaryStreamer<TEnum> : EnumBinaryStreamer<TEnum, EnumBinaryStreamerUseUnderlyingType>
		where TEnum : struct, Enum
	{
	};
}
