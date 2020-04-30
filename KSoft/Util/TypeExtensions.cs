using System;
using System.Text;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	using MS = Memory.Strings;

	/// <summary>Extension methods for types in this assembly</summary>
	public static partial class TypeExtensions
	{
		public const double kRadiansPerDegree = Math.PI / 180.0;
		public const double kDegreesPerRadian = 180 / Math.PI;

		#region NONE extensions
		public const int kNone = -1;
		/// <summary>The default string representation of a value which has a 'None' state</summary>
		public const string kNoneDisplayString = "NONE";

		public const sbyte kNoneInt8 = kNone;
		public const short kNoneInt16 = kNone;
		public const int kNoneInt32 = kNone;
		public const long kNoneInt64 = kNone;

		[Contracts.Pure]
		public static bool IsNone(this int value)
		{
			return value == kNoneInt32;
		}
		[Contracts.Pure]
		public static bool IsNotNone(this int value)
		{
			return value != kNoneInt32;
		}
		[Contracts.Pure]
		public static bool IsNoneOrPositive(this int value)
		{
			return value >= kNoneInt32;
		}

		[Contracts.Pure]
		public static bool IsNone(this long value)
		{
			return value == kNoneInt64;
		}
		[Contracts.Pure]
		public static bool IsNotNone(this long value)
		{
			return value != kNoneInt64;
		}
		[Contracts.Pure]
		public static bool IsNoneOrPositive(this long value)
		{
			return value >= kNoneInt64;
		}
		#endregion

		#region Fluent
		// Based on http://stackoverflow.com/questions/271398/what-are-your-favorite-extension-methods-for-c-codeplex-com-extensionoverflow/3842545#3842545

		[Contracts.Pure]
		[System.Diagnostics.DebuggerStepThrough]
		public static TRet NullOr<T, TRet>(this T theObj, Func<T, TRet> func, TRet elseValue = default(TRet))
			where T : class
		{
			Contract.Requires/*<ArgumentNullException>*/(func != null);

			return theObj != null ? func(theObj) : elseValue;
		}
		#endregion

		#region Enum Bit Encoders
		public static class BitEncoders
		{
			// KSoft

			// KSoft.Memory.Strings
			public static readonly EnumBitEncoder32<MS.StringStorageWidthType>
				StringStorageWidthType = new EnumBitEncoder32<MS.StringStorageWidthType>();
			public static readonly EnumBitEncoder32<MS.StringStorageType>
				StringStorageType = new EnumBitEncoder32<MS.StringStorageType>();
			public static readonly EnumBitEncoder32<MS.StringStorageLengthPrefix>
				StringStorageLengthPrefix = new EnumBitEncoder32<MS.StringStorageLengthPrefix>();

			// KSoft.Shell
			public static readonly EnumBitEncoder32<Shell.EndianFormat>
				EndianFormat = new EnumBitEncoder32<Shell.EndianFormat>();
			public static readonly EnumBitEncoder32<Shell.ProcessorSize>
				ProcessorSize = new EnumBitEncoder32<Shell.ProcessorSize>();
			public static readonly EnumBitEncoder32<Shell.ProcessorWordSize>
				ProcessorWordSize = new EnumBitEncoder32<Shell.ProcessorWordSize>();
			public static readonly EnumBitEncoder32<Shell.InstructionSet>
				InstructionSet = new EnumBitEncoder32<Shell.InstructionSet>();
			public static readonly EnumBitEncoder32<Shell.PlatformType>
				PlatformType = new EnumBitEncoder32<Shell.PlatformType>();
		};
		#endregion

		#region Collections
		[Contracts.Pure]
		public static Collections.TreeTraversalDirection GetOrder(this Collections.TreeTraversalDirection dir)
		{
			return dir & Collections.TreeTraversalDirection.kOrderMask;
		}
		[Contracts.Pure]
		public static Collections.TreeTraversalDirection GetDirections(this Collections.TreeTraversalDirection dir)
		{
			return dir & Collections.TreeTraversalDirection.kDirMask;
		}
		[Contracts.Pure]
		public static bool HasFlag(this Collections.TreeTraversalOrders orders, Collections.TreeTraversalOrders flag)
		{
			return (orders & flag) == flag;
		}
		#endregion

		#region IO
		public static /*IDisposable*/IO.IKSoftStreamOwnerBookmark EnterOwnerBookmark(this IO.IKSoftStream stream,
			object newOwner = null)
		{
			return new IO.IKSoftStreamOwnerBookmark(stream, newOwner);
		}
		public static /*IDisposable*/IO.IKSoftStreamUserDataBookmark EnterUserDataBookmark(this IO.IKSoftStream stream,
			object newUserData = null)
		{
			return new IO.IKSoftStreamUserDataBookmark(stream, newUserData);
		}
		/// <summary>Temporarily enter a new data streaming state</summary>
		/// <param name="newMode"></param>
		/// <returns></returns>
		public static IO.IKSoftStreamModeBookmark EnterStreamModeBookmark(IO.IKSoftStreamModeable stream,
			System.IO.FileAccess newMode)
		{
			Contract.Requires(stream.StreamMode != 0, "Current mode is unset!");
			Contract.Requires(newMode != 0, "New mode is unset!");

			return new IO.IKSoftStreamModeBookmark(stream, newMode);
		}

		#region IKSoftStreamWithVirtualBuffer
		/// <summary>Begin the concept of a virtual buffer</summary>
		/// <param name="bufferLength">Virtual buffer's byte length</param>
		/// <returns></returns>
		public static IO.IKSoftStreamWithVirtualBufferCleanup EnterVirtualBuffer(this IO.IKSoftStreamWithVirtualBuffer stream,
			long bufferLength)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bufferLength > 0);

			stream.VirtualBufferStart = stream.BaseStream.Position;
			stream.VirtualBufferLength = bufferLength;

			return new IO.IKSoftStreamWithVirtualBufferCleanup(stream);
		}
		/// <summary>Begin the concept of a virtual buffer</summary>
		/// <returns></returns>
		public static IO.IKSoftStreamWithVirtualBufferCleanup EnterVirtualBuffer(this IO.IKSoftStreamWithVirtualBuffer stream)
		{
			return new IO.IKSoftStreamWithVirtualBufferCleanup(stream);
		}
		/// <summary>Temporarily bookmark this stream's VirtualBuffer properties</summary>
		/// <returns></returns>
		public static IO.IKSoftStreamWithVirtualBufferBookmark EnterVirtualBufferBookmark(this IO.IKSoftStreamWithVirtualBuffer stream)
		{
			return new IO.IKSoftStreamWithVirtualBufferBookmark(stream);
		}
		/// <summary>
		/// Temporarily bookmark this stream's VirtualBuffer properties and begin the concept of a virtual buffer
		/// </summary>
		/// <returns></returns>
		public static IO.IKSoftStreamWithVirtualBufferAndBookmark EnterVirtualBufferWithBookmark(this IO.IKSoftStreamWithVirtualBuffer stream,
			long bufferLength)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bufferLength > 0);

			return new IO.IKSoftStreamWithVirtualBufferAndBookmark(stream, bufferLength);
		}
		#endregion

		public static int Read(this byte[] value, IO.EndianReader s)						{ return s.Read(value, 0, value.Length); }
		public static void Write(this byte[] value, IO.EndianWriter s)						{ s.Write(value, 0, value.Length); }
		public static int Read(this byte[] value, IO.EndianReader s, int index, int count)	{ return s.Read(value, index, count); }
		public static void Write(this byte[] value, IO.EndianWriter s, int index, int count){ s.Write(value, index, count); }

		public static void Read(this IO.EndianReader s, out string value)													{ value = s.ReadString(); }
		public static void Write(this string value, IO.EndianWriter s)														{ s.Write(value); }
		public static void Read(this IO.EndianReader s, out string value, Memory.Strings.StringStorage storage, int length)	{ value = s.ReadString(storage, length); }
		// no Write(length) override
		public static void Read(this IO.EndianReader s, out string value, Memory.Strings.StringStorage storage)				{ value = s.ReadString(storage); }
		public static void Write(this string value, IO.EndianWriter s, Memory.Strings.StringStorage storage)				{ s.Write(value, storage); }
		public static void Read(this IO.EndianReader s, out string value, Text.StringStorageEncoding encoding, int length)	{ value = s.ReadString(encoding, length); }
		// no Write(length) override
		public static void Read(this IO.EndianReader s, out string value, Text.StringStorageEncoding encoding)				{ value = s.ReadString(encoding); }
		public static void Write(this string value, IO.EndianWriter s, Text.StringStorageEncoding encoding)					{ s.Write(value, encoding); }

		public static void Read(this IO.EndianReader s, out Guid value, bool respectEndian = true)
		{
			if (respectEndian)
			{
				uint a = s.ReadUInt32();
				ushort b = s.ReadUInt16();
				ushort c = s.ReadUInt16();
				byte d = s.ReadByte();
				byte e = s.ReadByte();
				byte f = s.ReadByte();
				byte g = s.ReadByte();
				byte h = s.ReadByte();
				byte i = s.ReadByte();
				byte j = s.ReadByte();
				byte k = s.ReadByte();
				value = new Guid(a, b, c, d,e,f,g,h,i,j,k);
			}
			else
				value = new Guid(s.ReadBytes(16));
		}
		public static void Write(this Guid value, IO.EndianWriter s, bool respectEndian = true)
		{
			byte[] data = value.ToByteArray();

			if (respectEndian)
			{
				Bitwise.ByteSwap.SwapInt32(data, 0);
				Bitwise.ByteSwap.SwapInt16(data, sizeof(uint));
				Bitwise.ByteSwap.SwapInt16(data, sizeof(uint)+sizeof(ushort));
			}

			s.Write(data);
		}

		#region IEndianStreamable
		/// <summary>
		/// Read a serializable value type from an endian stream
		/// </summary>
		/// <typeparam name="T">Value type implementing <see cref="IO.IEndianStreamable"/></typeparam>
		/// <param name="s"></param>
		/// <param name="value"></param>
		public static void ReadObject<T>(this IO.EndianReader s, out T value)
			where T : struct, IO.IEndianStreamable
		{
			value = new T();
			value.Read(s);
		}
		/// <summary>
		/// Write a serializable value type to an endian stream
		/// </summary>
		/// <typeparam name="T">Value type implementing <see cref="IO.IEndianStreamable"/></typeparam>
		/// <param name="s"></param>
		/// <param name="value"></param>
		public static void WriteObject<T>(this IO.EndianWriter s, ref T value)
			where T : struct, IO.IEndianStreamable
		{
			value.Write(s);
		}
		/// <summary>
		/// Read a serializable object from an endian stream
		/// </summary>
		/// <typeparam name="T">Reference type implementing <see cref="IO.IEndianStreamable"/></typeparam>
		/// <param name="s"></param>
		/// <param name="theObj"></param>
		public static void ReadObject<T>(this IO.EndianReader s, T theObj)
			where T : class, IO.IEndianStreamable
		{
			theObj.Read(s);
		}
		/// <summary>
		/// Write a serializable object to an endian stream
		/// </summary>
		/// <typeparam name="T">Reference type implementing <see cref="IO.IEndianStreamable"/></typeparam>
		/// <param name="s"></param>
		/// <param name="theObj"></param>
		public static void WriteObject<T>(this IO.EndianWriter s, T theObj)
			where T : class, IO.IEndianStreamable
		{
			theObj.Write(s);
		}
		#endregion
		#endregion

		#region Memory.Strings
		/// <summary>Get the encoding implementation based off the storage width type</summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static Encoding GetEncoding(this MS.StringStorageWidthType type)
		{
			Contract.Ensures(Contract.Result<Encoding>() != null);

			switch(type)
			{
				case MS.StringStorageWidthType.Ascii:	return Encoding.ASCII;
				case MS.StringStorageWidthType.Unicode:	return Encoding.Unicode;
				case MS.StringStorageWidthType.UTF7:	return Encoding.UTF7;
				case MS.StringStorageWidthType.UTF8:	return Encoding.UTF8;
				case MS.StringStorageWidthType.UTF32:	return Encoding.UTF32;

				default: throw new Debug.UnreachableException(type.ToString());
			}
		}

		/// <summary>
		/// Get the <see cref="StringStorageWidthType"/> from a <see cref="System.Text.Encoding"/>
		/// implementation
		/// </summary>
		/// <param name="enc">Instance of an encoding whose type we'll use to determine the storage type</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static MS.StringStorageWidthType FromEncoding(Encoding enc)
		{
			Contract.Requires<ArgumentNullException>(enc != null);

			if		(enc is ASCIIEncoding)				return MS.StringStorageWidthType.Ascii;
			else if (enc is UnicodeEncoding)			return MS.StringStorageWidthType.Unicode;
			else if (enc is UTF7Encoding)				return MS.StringStorageWidthType.UTF7;
			else if (enc is UTF8Encoding)				return MS.StringStorageWidthType.UTF8;
			else if (enc is UTF32Encoding)				return MS.StringStorageWidthType.UTF32;
			else if (enc is Text.StringStorageEncoding)	return (enc as Text.StringStorageEncoding).Storage.WidthType;
			else										throw new Debug.UnreachableException(enc.GetType().ToString());
		}

		/// <summary>Does this string type use a length prefix when serialized?</summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static bool UsesLengthPrefix(this MS.StringStorageType type)
		{
			return type == MS.StringStorageType.Pascal;
		}

		/// <summary>Does this width type support variable length characters (code points)?</summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static bool IsVariableWidth(this MS.StringStorageWidthType type)
		{
			return	type == MS.StringStorageWidthType.UTF7 ||
					type == MS.StringStorageWidthType.UTF8;
		}
		#endregion

		#region Shell
		public static int GetBitCount(Shell.ProcessorSize value)
		{
			Contract.Ensures(Contract.Result<int>() >= -1);

			switch (value)
			{
			case Shell.ProcessorSize.x32:
				return Bits.kInt32BitCount;
			case Shell.ProcessorSize.x64:
				return Bits.kInt64BitCount;

			default:
				return -1;
			}
		}

		public static int GetByteCount(Shell.ProcessorSize value)
		{
			Contract.Ensures(Contract.Result<int>() >= -1);

			switch (value)
			{
			case Shell.ProcessorSize.x32:
				return sizeof(int);
			case Shell.ProcessorSize.x64:
				return sizeof(long);

			default:
				return -1;
			}
		}

		public static int GetBitCount(Shell.ProcessorWordSize value)
		{
			Contract.Ensures(Contract.Result<int>() > 0);

			int shift = (int)value;
			return 8 << shift;
		}

		public static int GetByteCount(Shell.ProcessorWordSize value)
		{
			Contract.Ensures(Contract.Result<int>() > 0);

			int mul = (int)value + 1;
			return 8 * mul;
		}
		#endregion

		#region Shell.EndianFormat
		/// <summary>Is the byte order the same as the current runtime?</summary>
		/// <param name="ef"></param>
		/// <returns>True if the runtime byte order is the same as this</returns>
		/// <see cref="Shell.Platform.Environment"/>
		[Contracts.Pure]
		public static bool IsSameAsRuntime(this Shell.EndianFormat ef)
		{
			return ef == Shell.Platform.Environment.ProcessorType.ByteOrder;
		}

		/// <summary>Get the inverse of this byte order</summary>
		/// <param name="ef"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static Shell.EndianFormat Invert(this Shell.EndianFormat ef)
		{
			return ef == Shell.EndianFormat.Little ? Shell.EndianFormat.Big : Shell.EndianFormat.Little;
		}
		#endregion
	};
}
