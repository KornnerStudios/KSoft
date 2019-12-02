using System;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	[System.Diagnostics.DebuggerDisplay("Bit/Length = {BitLength}/{Length}, Bit = {BitPosition}")]
	public partial class BitStream : IKSoftStream, IKSoftStreamModeable,
		IDisposable
	{
		static BitStream()
		{
			InitializeBitmaskLookUpTable(out kBitmaskLUT);
		}

		#region BaseStream
		/// <summary>Stream to push/pull bits to/from</summary>
		public Stream BaseStream { get; private set; }
		/// <summary>byte offset in <see cref="BaseStream"/> where we started streaming at</summary>
		long mStartPosition;
		/// <summary>byte offset in <see cref="BaseStream"/> to stop streaming at, or zero</summary>
		long mEndPosition;
		/// <summary>More efficient to use buffer Read/Writes than to do Read/WriteByte calls</summary>
		byte[] mIoBuffer = new byte[1];
		#endregion

		#region Cache
		/// <summary>current bit index in <see cref="mCache"/></summary>
		int mCacheBitIndex;
		/// <summary>Actual number of bits streamed to/form <see cref="mCache"/></summary>
		int mCacheBitsStreamedCount;

		/// <summary>Number of bits still left in <see cref="mCache"/> that can be read/written</summary>
		int CacheBitsRemaining	{ get {
			return kWordBitCount - mCacheBitIndex;
		} }
		#endregion

		#region IKSoftStream
		/// <summary>Owner of this stream</summary>
		public object Owner { get; set; }

		public object UserData { get; set; }

		/// <summary>Name for this bitstream, or an empty string</summary>
		public string StreamName			{ get; private set; }
		/// <returns><see cref="StreamName"/></returns>
		public override string ToString()	{ return StreamName; }
		#endregion

		#region IKSoftStreamModeable
		/// <remarks>0 means this stream is closed</remarks>
		public FileAccess StreamPermissions { get; private set; }

		FileAccess mStreamMode;
		/// <summary>Current data streaming state</summary>
		/// <remarks>Read or Write, not both</remarks>
		public FileAccess StreamMode { get { return mStreamMode; } set {
			if (value != mStreamMode)
			{
				// if we're switching to Read, flush then warm the cache
				if (value == FileAccess.Read)
				{
					FlushCache();
					FillCache();
				}

				mStreamMode = value;
			}
		} }
		public bool IsReading { get { return mStreamMode == FileAccess.Read; } }
		public bool IsWriting { get { return mStreamMode == FileAccess.Write; } }
		#endregion

		/// <summary>Access operations to throw exceptions on when they result in overflows</summary>
		/// <remarks>Bad read/writes will throw <see cref="EndOfStreamException"/>s</remarks>
		public FileAccess ThrowOnOverflow { get; set; }

		/// <summary>Do we own the base stream?</summary>
		/// <remarks>If we don't own the stream, when this object is disposed, the <see cref="BaseStream"/> won't be closed\disposed</remarks>
		public bool BaseStreamOwner { get; set; }

		/// <summary>Byte length of the bitstream or <see cref="BaseStream"/>'s Length</summary>
		/// <exception cref="NotSupportedException"><see cref="BaseStream"/> does not support seeking</exception>
		public long Length { get {
			return mEndPosition > 0
				? mEndPosition
				: BaseStream.Length;
		} }
		/// <summary>Number of bits in the bitstream</summary>
		/// <exception cref="NotSupportedException"><see cref="BaseStream"/> does not support seeking</exception>
		public long BitLength { get {
			return (Length - mStartPosition) * Bits.kByteBitCount;
		} }
		/// <summary>Current bit index within the bitstream</summary>
		/// <exception cref="NotSupportedException"><see cref="BaseStream"/> does not support seeking</exception>
		public long BitPosition	{ get {
			long position = ((BaseStream.Position - mStartPosition) * Bits.kByteBitCount) + mCacheBitIndex;
			// When reading we initialize the cache, meaning we've already advanced BaseStream.Position
			return IsWriting
				? position
				: position - mCacheBitsStreamedCount;
		} }

		bool IsEndOfStream { get {
			return CanSeek
				? BaseStream.Position >= Length
				: false;
		} }

		internal void SeekToStart()
		{
			Contract.Requires<InvalidOperationException>(CanSeek);

			FlushCache();
			mCacheBitsStreamedCount = 0;

			BaseStream.Seek(mStartPosition, SeekOrigin.Begin);
		}

		/// <summary>Constuct a new bitsream using an underlying <see cref="Stream"/> object</summary>
		/// <param name="baseStream">Underlying stream to read/write bits to</param>
		/// <param name="permissions">Access permissions for <paramref name="baseStream"/></param>
		/// <param name="startPos">Start of the bitstream in <paramref name="baseStream"/>, or -1 to use default value</param>
		/// <param name="endPos">End of the bitstream in <paramref name="baseStream"/>, or -1 to use default value</param>
		/// <param name="streamName">Optional name for this bitstream</param>
		/// <remarks>
		/// If <paramref name="baseStream"/> CanSeek, the default value for <paramref name="startPos"/> is the current Position of the stream.
		/// Else the default value is zero.
		///
		/// The default value for <paramref name="endPos"/> is zero. When it is zero, <paramref name="Stream"/>'s Length is always used at
		/// runtime. So it will acknowledge changes in the base stream length after the constructor finishes.
		/// </remarks>
		public BitStream(Stream baseStream, FileAccess permissions = FileAccess.ReadWrite,
			long startPos = TypeExtensions.kNone, long endPos = TypeExtensions.kNone,
			string streamName = "")
		{
			Contract.Requires<ArgumentNullException>(baseStream != null);
			Contract.Requires<ArgumentOutOfRangeException>(!baseStream.CanSeek || endPos <= baseStream.Length);
			Contract.Requires(permissions != 0);
			Contract.Requires<ArgumentNullException>(streamName != null);

			StreamPermissions = permissions;
			BaseStream = baseStream;
			StreamName = streamName;

			if (baseStream.CanSeek)
			{
				if (startPos >= 0)
				{
					mStartPosition = startPos;
					BaseStream.Seek(startPos, SeekOrigin.Begin);
				}
				else
					mStartPosition = baseStream.Position;

				if (endPos > 0)
					mEndPosition = endPos;
			}
			else
			{
				mStartPosition = 0;
				mEndPosition = 0;
			}
		}

		#region IDisposable Members
		public void Dispose()
		{
			if (BaseStream != null)
			{
				if(IsWriting)
					FlushCache();

				if(BaseStreamOwner)
					BaseStream.Dispose();

				BaseStream = null;
				StreamPermissions = 0;
			}
		}
		#endregion

		#region Stream-like interfaces
		public bool CanRead		{ get { return StreamPermissions.CanRead(); } }
		public bool CanWrite	{ get { return StreamPermissions.CanWrite(); } }
		public bool CanSeek		{ get { return BaseStream.CanSeek; } }

		public void Close()		{ this.Dispose(); }
		public void Flush()		{ this.FlushCache(); }
		#endregion

		#region Boolean
		public void Read(out bool value)
		{
			value = ReadBoolean();
		}
		public void Write(bool value)
		{
			WriteWord(value ? 1U : 0U, Bits.kBooleanBitCount);
		}
		#endregion

		#region Single
		public float ReadSingle()
		{
			uint data;
			Read(out data, Bits.kInt32BitCount);

			return Bitwise.ByteSwap.SingleFromUInt32(data);
		}
		public void Read(out float value)
		{
			value = ReadSingle();
		}
		public void Write(float value)
		{
			Write(Bitwise.ByteSwap.SingleToUInt32(value), Bits.kInt32BitCount);
		}
		#endregion

		#region Double
		public double ReadDouble()
		{
			long data;
			Read(out data, Bits.kInt64BitCount);

			return BitConverter.Int64BitsToDouble(data);
		}
		public void Read(out double value)
		{
			value = ReadDouble();
		}
		public void Write(double value)
		{
			Write(BitConverter.DoubleToInt64Bits(value), Bits.kInt32BitCount);
		}
		#endregion

		#region DateTime
		public DateTime ReadDateTime(int bitCount = Bits.kInt64BitCount)
		{
			Contract.Requires(bitCount <= Bits.kInt64BitCount);

			long time64;
			Read(out time64, bitCount);

			return Util.ConvertDateTimeFromUnixTime(time64);
		}
		public void Read(out DateTime value, int bitCount = Bits.kInt64BitCount)
		{
			Contract.Requires(bitCount <= Bits.kInt64BitCount);

			value = ReadDateTime(bitCount);
		}
		public void Write(DateTime value, int bitCount = Bits.kInt64BitCount)
		{
			Contract.Requires(bitCount <= Bits.kInt64BitCount);

			long time64 = Util.ConvertDateTimeToUnixTime(value);

			Write(time64, bitCount);
		}
		public BitStream Stream(ref DateTime value, int bitCount = Bits.kInt64BitCount)
		{
			Contract.Requires(bitCount <= Bits.kInt64BitCount);

				 if (IsReading) value = ReadDateTime(bitCount);
			else if (IsWriting) Write(value, bitCount);

			return this;
		}
		#endregion

		#region String
		// Verify that we have enough information to correctly stream a string
		static void ValidateStringStorageForStreaming(Memory.Strings.StringStorage s, int length)
		{
			// There are going to be issues if we try to read back a willy nilly char array string
			if (s.Type == Memory.Strings.StringStorageType.CharArray && !s.IsFixedLength && length <= 0)
				throw new InvalidDataException(string.Format("Provided string storage and length is invalid for Endian streaming: {0}, {1}",
					s.ToString(), length.ToString()));
		}

		/// <summary>Read a string using a <see cref="Memory.Strings.StringStorage"/> definition and a provided character length</summary>
		/// <param name="storage">Definition for the string's characteristics</param>
		/// <param name="length">Length, in characters, of the string.</param>
		/// <param name="maxLength">CString only: Optional maximum length of this specific string (exclusive of terminator)</param>
		/// <param name="prefixBitLength">Pascal only: Number of bits in the prefix count</param>
		/// <returns></returns>
		/// <remarks>
		/// Length can be non-positive if <paramref name="storage"/> defines or
		/// doesn't require an explicit character length. If you do provide the
		/// length, this operation will perform faster in some cases.
		/// </remarks>
		public string ReadString(Memory.Strings.StringStorage storage, int length = TypeExtensions.kNone,
			int maxLength = TypeExtensions.kNone, int prefixBitLength = TypeExtensions.kNone)
		{
			ValidateStringStorageForStreaming(storage, length);

			var sse = Text.StringStorageEncoding.TryAndGetStaticEncoding(storage);

			return sse.ReadString(this, length, maxLength, prefixBitLength);
		}
		/// <summary>Read a string using a <see cref="Text.StringStorageEncoding"/> definition and a provided character length</summary>
		/// <param name="encoding">Encoding to use for character streaming</param>
		/// <param name="length">Length, in characters, of the string.</param>
		/// <param name="maxLength">CString only: Optional maximum length of this specific string (exclusive of terminator)</param>
		/// <param name="prefixBitLength">Pascal only: Number of bits in the prefix count</param>
		/// <returns></returns>
		/// <remarks>
		/// Length can be non-positive if <paramref name="storage"/> defines or
		/// doesn't require an explicit character length. If you do provide the
		/// length, this operation will perform faster in some cases.
		/// </remarks>
		public string ReadString(Text.StringStorageEncoding encoding, int length = TypeExtensions.kNone,
			int maxLength = TypeExtensions.kNone, int prefixBitLength = TypeExtensions.kNone)
		{
			Contract.Requires(encoding != null);
			ValidateStringStorageForStreaming(encoding.Storage, length);

			return encoding.ReadString(this, length, maxLength, prefixBitLength);
		}

		/// <summary>Writes a string based on a <see cref="Memory.Strings.StringStorage"/> definition</summary>
		/// <param name="value">String value to writee. Null defaults to an empty string</param>
		/// <param name="storage">Definition for how we're streaming the string</param>
		/// <param name="maxLength">CString only: Optional maximum length of this specific string (exclusive of terminator)</param>
		public void Write(string value, Memory.Strings.StringStorage storage,
			int maxLength = TypeExtensions.kNone/*, int prefixBitLength = TypeExtensions.kNone*/)
		{
			var sse = Text.StringStorageEncoding.TryAndGetStaticEncoding(storage);
			sse.WriteString(this, value ?? string.Empty, maxLength,
				prefixBitLength: TypeExtensions.kNone);
		}
		/// <summary>Writes a string using a <see cref="Text.StringStorageEncoding"/></summary>
		/// <param name="value">String value to writee. Null defaults to an empty string</param>
		/// <param name="encoding">Encoding to use for character streaming</param>
		/// <param name="maxLength">CString only: Optional maximum length of this specific string (exclusive of terminator)</param>
		public void Write(string value, Text.StringStorageEncoding encoding,
			int maxLength = TypeExtensions.kNone/*, int prefixBitLength = TypeExtensions.kNone*/)
		{
			Contract.Requires(encoding != null);

			encoding.WriteString(this, value ?? string.Empty, maxLength,
				prefixBitLength: TypeExtensions.kNone);
		}

		/// <summary>Serializes a string based on a <see cref="Memory.Strings.StringStorage"/> definition</summary>
		/// <param name="value"></param>
		/// <param name="storage">Definition for how we're streaming the string</param>
		/// <param name="maxLength">CString only: Optional maximum length of this specific string (exclusive of terminator)</param>
		/// <returns></returns>
		public BitStream Stream(ref string value, Memory.Strings.StringStorage storage,
			int maxLength = TypeExtensions.kNone)
		{
				 if (IsReading) value = ReadString(storage, maxLength: maxLength);
			else if (IsWriting) Write(value, storage, maxLength: maxLength);

			return this;
		}
		/// <summary>Serializes a string using a <see cref="Text.StringStorageEncoding"/></summary>
		/// <param name="value"></param>
		/// <param name="encoding">Encoding to use for character streaming</param>
		/// <param name="maxLength">CString only: Optional maximum length of this specific string (exclusive of terminator)</param>
		/// <returns></returns>
		public BitStream Stream(ref string value, Text.StringStorageEncoding encoding,
			int maxLength = TypeExtensions.kNone)
		{
			Contract.Requires(encoding != null);

				 if (IsReading) value = ReadString(encoding, maxLength: maxLength);
			else if (IsWriting) Write(value, encoding, maxLength: maxLength);

			return this;
		}
		#endregion

		#region byte[]
		public void Read(byte[] buffer, int index, int count, int bitCount = Bits.kByteBitCount)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(index+count <= buffer.Length);
			Contract.Requires(bitCount <= Bits.kByteBitCount);
#if false // #TODO redo optimization
			if (mCacheBitIndex == 0 && bitCount == Bits.kByteBitCount && count >= kWordByteCount)
			{
				LowLevel.Data.ByteSwap.ReplaceBytes(buffer, index, mCache);
				index += sizeof(TWord); count -= sizeof(TWord);

				// #TODO: need to handle unaligned reads
				BaseStream.Read(buffer, index, count);
				mCacheBitsStreamedCount += count * Bits.kByteBitCount;
				FillCache();
			}
			else
#endif
			{
				for (int x = index; x < count; x++)
					Read(out buffer[x], bitCount);
			}
		}
		public void Write(byte[] buffer, int index, int count, int bitCount = Bits.kByteBitCount)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(index+count <= buffer.Length);
			Contract.Requires(bitCount <= Bits.kByteBitCount);
#if false // #TODO redo optimization
			if (mCacheBitIndex == 0 && bitCount == Bits.kByteBitCount)
			{
				// #TODO: need to handle unaligned writes
				BaseStream.Write(buffer, index, count);
				mCacheBitsStreamedCount += count * Bits.kByteBitCount;
			}
			else
#endif
			{
				for (int x = index; x < count; x++)
					Write(buffer[x], bitCount);
			}
		}
		public BitStream Stream(byte[] buffer, int index, int count, int bitCount = Bits.kByteBitCount)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(index+count <= buffer.Length);
			Contract.Requires(bitCount <= Bits.kByteBitCount);

				 if (IsReading) Read( buffer, index, count, bitCount);
			else if (IsWriting) Write(buffer, index, count, bitCount);

			return this;
		}

		public byte[] ReadBytes(int byteCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(byteCount >= 0);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			byte[] buffer = new byte[byteCount];

			if (byteCount > 0)
				Read(buffer, 0, byteCount);

			return buffer;
		}
		public byte[] Read(byte[] buffer, int bitCount = Bits.kByteBitCount)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires(bitCount <= Bits.kByteBitCount);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			if (buffer.Length > 0)
				Read(buffer, 0, buffer.Length, bitCount);

			return buffer;
		}
		public void Write(byte[] buffer, int bitCount = Bits.kByteBitCount)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires(bitCount <= Bits.kByteBitCount);

			if (buffer.Length > 0)
				Write(buffer, 0, buffer.Length, bitCount);
		}
		#endregion
	};
}