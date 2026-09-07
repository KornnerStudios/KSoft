using System;
using System.IO;
using System.Text;

// #TODO fix CA warnings
#pragma warning disable IDE0011 // Use braces

namespace KSoft.IO
{
	/// <summary>A binary stream with the ability to read data in different endian formats</summary>
	/// <remarks>For stream character encoding, when no explicit encoding is provided, <see cref="System.Text.UTF8Encoding"/> is assumed</remarks>
	public sealed partial class EndianReader : BinaryReader, IKSoftBinaryStream, IKSoftEndianStream
	{
		#region Ctor
		/// <summary>Create a new binary reader which respects the endian format of the underlying stream's bytes</summary>
		/// <param name="input">Base stream to use as input</param>
		/// <param name="encoding">Character encoding to use. If null, <see cref="System.Text.UTF8Encoding"/> is assumed</param>
		/// <param name="byteOrder">Endian format for how we interpret the stream's bytes</param>
		/// <param name="streamOwner">Owner object of this stream, or null</param>
		/// <param name="name">Special name to associate with this stream</param>
		public EndianReader(Stream input, Encoding encoding,
			Shell.EndianFormat byteOrder, object? streamOwner = null, string? name = null)
			: base(Util.ThrowIfNull(input), Util.ThrowIfNull(encoding), leaveOpen: true)
		{
			BaseStreamOwner = true;
			BaseAddress = Values.PtrHandle.Null32;

			ByteOrder = byteOrder;
			Owner = streamOwner;
			mStringEncoding = encoding;

			StreamName = name ?? "(unnamed)";

			// If the stream is a different endian than the runtime, data will
			// be byte swapped of course
			//this.mRequiresByteSwap = Shell.Platform.Environment.ProcessorType.ByteOrder != byteOrder;
			mRequiresByteSwap = !byteOrder.IsSameAsRuntime();
		}

		/// <summary>Create a new binary reader which respects the endian format of the underlying stream's bytes</summary>
		/// <param name="input">Base stream to use as input</param>
		/// <param name="byteOrder">Endian format for how we interpret the stream's bytes</param>
		/// <param name="streamOwner">Owner object of this stream, or null</param>
		/// <param name="name">Special name to associate with this stream</param>
		/// <remarks>Defaults to <see cref="System.Text.UTF8Encoding"/> for the string encoding</remarks>
		public EndianReader(Stream input, Shell.EndianFormat byteOrder,
			object? streamOwner = null, string? name = null) : this(input, new UTF8Encoding(), byteOrder, streamOwner!, name!)
		{
		}
		/// <summary>Create a new binary reader which uses the environment's endian format</summary>
		/// <param name="input">Base stream to use as input</param>
		/// <remarks>
		/// Default endian format is set from <see cref="Shell.Platform.Environment"/>.
		/// <see cref="Owner"/> is set to <c>null</c>
		/// </remarks>
		public EndianReader(Stream input) : this(input, new UTF8Encoding(),
			Shell.Platform.Environment.ProcessorType.ByteOrder)
		{
		}
		#endregion

		public IDisposable ReadSignatureWithByteSwapSupport(uint expectedSignature, out uint actualSignature)
		{
			var signature = this.ReadUInt32();
			if (signature != expectedSignature)
			{
				var signatureInverted = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(signature);
				if (signatureInverted == expectedSignature)
				{
					actualSignature = signatureInverted;
					return this.BeginEndianSwitch();
				}

				throw new SignatureMismatchException(this.BaseStream, expectedSignature, signature);
			}

			actualSignature = signature;
			return Util.NullDisposable;
		}

		public IDisposable ReadSignatureWithByteSwapSupport(uint expectedSignature)
		{
#pragma warning disable IDE0059 // Unnecessary assignment of a value
			return ReadSignatureWithByteSwapSupport(expectedSignature, out uint actualSignature);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
		}

		#region Pad
		public void Pad(int byteCount)
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(byteCount);

			BaseStream.Seek(sizeof(byte) * byteCount, SeekOrigin.Current);
		}

		public void Pad8()	{ Pad(sizeof(byte)); }
		public void Pad16()	{ Pad(sizeof(ushort)); }
		public void Pad24()	{ Pad(sizeof(ushort)+sizeof(byte)); }
		public void Pad32()	{ Pad(sizeof(uint)); }
		public void Pad64()	{ Pad(sizeof(ulong)); }
		public void Pad128(){ Pad(sizeof(ulong)*2); }
		#endregion

		// #REVIEW: #DOTNET10 BinaryReader gains an inherited ReadExactly(Span<byte>) overload on .NET 10;
		// on net9.0 today, explicitly raw-byte callers can already reach exact-read semantics via
		// Stream.ReadExactly through BaseStream. No EndianReader-level exact-read wrapper is needed now.

		#region Read group tag
		// #VITA_KEEP: tag helpers preserve KSoft character/group-tag ordering semantics.
		/// <summary>Reads a tag id (four character code)</summary>
		/// <param name="tag">Destination to populate</param>
		public void ReadTag32(Span<char> tag)
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(tag.Length, 4, nameof(tag));

			tag[0] = (char)base.ReadByte();
			tag[1] = (char)base.ReadByte();
			tag[2] = (char)base.ReadByte();
			tag[3] = (char)base.ReadByte();

			// Explicitly check for Little endian since this is
			// a character array and not a primitive integer
			if (ByteOrder == Shell.EndianFormat.Little)
			{
				tag.Slice(0, 4).Reverse();
			}
		}

		/// <summary>Reads a tag id (eight character code)</summary>
		/// <param name="tag">Destination to populate</param>
		public void ReadTag64(Span<char> tag)
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(tag.Length, 8, nameof(tag));

			tag[0] = (char)base.ReadByte();
			tag[1] = (char)base.ReadByte();
			tag[2] = (char)base.ReadByte();
			tag[3] = (char)base.ReadByte();
			tag[4+0] = (char)base.ReadByte();
			tag[4+1] = (char)base.ReadByte();
			tag[4+2] = (char)base.ReadByte();
			tag[4+3] = (char)base.ReadByte();

			// Explicitly check for Little endian since this is
			// a character array and not a primitive integer
			if (ByteOrder == Shell.EndianFormat.Little)
			{
				tag.Slice(0, 4).Reverse();
				tag.Slice(4, 4).Reverse();
			}
		}

		/// <summary>Reads a tag id (four character code)</summary>
		/// <returns></returns>
		/// <seealso cref="BinaryReader.ReadUInt32()"/>
		public uint ReadTagUInt32()
		{
			uint value = base.ReadUInt32();
			if (mRequiresByteSwap)
			{
				value = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
			}

			return value;
		}

		/// <summary>Reads a tag id (eight character code)</summary>
		/// <returns></returns>
		/// <seealso cref="BinaryReader.ReadUInt64()"/>
		public ulong ReadTagUInt64()
		{
			ulong value = base.ReadUInt64();
			if (mRequiresByteSwap)
			{
				value = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
			}

			return value;
		}
		#endregion

		// #TODO: generate with T4
		#region Read numerics
		// #VITA_KEEP: 24/40-bit game-format widths have no BinaryPrimitives equivalent.
		/// <summary>Reads a signed 24-bit integer</summary>
		/// <returns></returns>
		/// <remarks>Sign extends the value read from the stream</remarks>
		public int ReadInt24()
		{
			int value = base.ReadByte() | (base.ReadByte() << 8) | (base.ReadByte() << 16);
			if(mRequiresByteSwap) Bitwise.ByteSwap.SwapInt24(ref value);

			value = Bits.SignExtend(value, 24);
			return value;
		}

		/// <summary>Reads an unsigned 24-bit integer</summary>
		/// <returns></returns>
		public uint ReadUInt24()
		{
			uint value = (uint)(base.ReadByte() | (base.ReadByte() << 8) | (base.ReadByte() << 16));
			if(mRequiresByteSwap) Bitwise.ByteSwap.SwapUInt24(ref value);

			return value;
		}

		/// <summary>Reads an unsigned 24-bit integer</summary>
		/// <returns></returns>
		public ulong ReadUInt40()
		{
			var value =
				((ulong)base.ReadByte()) |
				((ulong)base.ReadByte() << 8) |
				((ulong)base.ReadByte() << 16) |
				((ulong)base.ReadByte() << 24) |
				((ulong)base.ReadByte() << 32)
				;
			if(mRequiresByteSwap) Bitwise.ByteSwap.SwapUInt40(ref value);

			return value;
		}
		#endregion

		#region Read string
		/// <summary>Read a string using a <see cref="Memory.Strings.StringStorage"/> definition and a provided character length</summary>
		/// <param name="storage">Definition for the string's characteristics</param>
		/// <param name="length">Length, in characters, of the string.</param>
		/// <returns></returns>
		/// <remarks>
		/// Length can be non-positive if <paramref name="storage"/> defines or
		/// doesn't require an explicit character length. If you do provide the
		/// length, this operation will perform faster in some cases.
		/// </remarks>
		public string ReadString(Memory.Strings.StringStorage storage, int length)
		{
			Verify.StringStorage.ForStreaming(storage, length);

			var sse = Text.StringStorageEncoding.TryAndGetStaticEncoding(storage);

			return sse.ReadString(this, length);
		}
		/// <summary>
		/// Read a string using a <see cref="Memory.Strings.StringStorage"/> definition.
		/// String length defaults to <see cref="Memory.Strings.StringStorage.FixedLegnth"/>
		/// </summary>
		/// <param name="storage">Definition for the string's characteristics</param>
		/// <returns></returns>
		public string ReadString(Memory.Strings.StringStorage storage)
		{
			return ReadString(storage, storage.FixedLength);
		}

		/// <summary>Read a string using a <see cref="Memory.Strings.StringStorage"/> encoding and a provided character length</summary>
		/// <param name="encoding">The encoding to use for character streaming</param>
		/// <param name="length">Length, in characters, of the string.</param>
		/// <returns></returns>
		/// <remarks>
		/// Length can be non-positive if <paramref name="encoding"/>'s storage defines
		/// or doesn't require an explicit character length. If you do provide the
		/// length, this operation will perform faster in some cases.
		/// </remarks>
		public string ReadString(Text.StringStorageEncoding encoding, int length)
		{
			ArgumentNullException.ThrowIfNull(encoding);
			Verify.StringStorage.ForStreaming(encoding.Storage, length);

			return encoding.ReadString(this, length);
		}
		/// <summary>
		/// Read a string using a <see cref="Memory.Strings.StringStorage"/> encoding.
		/// String length defaults to <see cref="Memory.Strings.StringStorage.FixedLegnth"/>
		/// </summary>
		/// <param name="encoding">The encoding to use for character streaming</param>
		/// <returns></returns>
		public string ReadString(Text.StringStorageEncoding encoding)
		{
			ArgumentNullException.ThrowIfNull(encoding);

			return ReadString(encoding, encoding.Storage.FixedLength);
		}
		#endregion

		#region Read Pointer
		/// <summary>Read a pointer value from the stream with no postprocessing to the result</summary>
		/// <param name="addressSize">Size of the pointer we're to read</param>
		/// <returns></returns>
		public Values.PtrHandle ReadRawPointer(Shell.ProcessorSize addressSize)
		{
			var ptr = new Values.PtrHandle(addressSize);

			if (!ptr.Is64bit)	ptr.u32 = ReadUInt32();
			else				ptr.u64 = ReadUInt64();

			return ptr;
		}
		/// <summary>Read a pointer value from the stream with no postprocessing to the result</summary>
		/// <param name="ptrHandle">Handle to stream the value into</param>
		public void ReadRawPointer(ref Values.PtrHandle ptrHandle)
		{
			if (!ptrHandle.Is64bit)	ptrHandle.u32 = ReadUInt32();
			else					ptrHandle.u64 = ReadUInt64();
		}

		/// <summary>Read a pointer value from the stream</summary>
		/// <param name="addressSize">Size of the pointer we're to read</param>
		/// <returns>
		/// A handle that encapsulates the pointer value and its size. After the value
		/// is read from the stream, <see cref="BaseAddress"/> is subtracted.
		/// </returns>
		/// <remarks>
		/// Be sure to set <see cref="BaseAddress"/> to the proper value so you get
		/// a correct relative-virtual-address. Otherwise, set <see cref="BaseAddress"/>
		/// to zero to get the pure pointer value.
		/// </remarks>
		public Values.PtrHandle ReadPointer(Shell.ProcessorSize addressSize)
		{
			var ptr = new Values.PtrHandle(addressSize);

			if (!ptr.Is64bit)
			{
				ptr.u32 = ReadUInt32();

				if (ptr.u32 != 0)
					ptr.u32 -= BaseAddress.u32;
			}
			else
			{
				ptr.u64 = ReadUInt64();

				if (ptr.u64 != 0)
					ptr.u64 -= BaseAddress.u64;
			}

			return ptr;
		}
		/// <summary>Read a pointer value from the stream</summary>
		/// <param name="ptrHandle">Handle to stream the value into</param>
		/// <remarks>
		/// After the value is read from the stream, <see cref="BaseAddress"/> is subtracted.
		///
		/// Be sure to set <see cref="BaseAddress"/> to the proper value so you get
		/// a correct relative-virtual-address. Otherwise, set <see cref="BaseAddress"/>
		/// to zero to get the pure pointer value.
		/// </remarks>
		public void ReadPointer(ref Values.PtrHandle ptrHandle)
		{
			if (!ptrHandle.Is64bit)
			{
				ptrHandle.u32 = ReadUInt32();

				if (ptrHandle.u32 != 0)
					ptrHandle.u32 -= BaseAddress.u32;
			}
			else
			{
				ptrHandle.u64 = ReadUInt64();

				if (ptrHandle.u64 != 0)
					ptrHandle.u64 -= BaseAddress.u64;
			}
		}

		/// <summary>Read a pointer value from the stream (size inherited from <see cref="BaseAddress"/>)</summary>
		/// <returns>
		/// A handle that encapsulates the pointer value and its size. After the value
		/// is read from the stream, <see cref="BaseAddress"/> is subtracted.
		/// </returns>
		/// <remarks>
		/// Be sure to set <see cref="BaseAddress"/> to the proper value so you get
		/// a correct relative-virtual-address. Otherwise, set <see cref="BaseAddress"/>
		/// to zero to get the pure pointer value.
		/// </remarks>
		public Values.PtrHandle ReadPointerViaBaseAddress()
		{
			return ReadPointer(BaseAddress.Size);
		}
		/// <summary>Read a pointer value from the stream (size inherited from <see cref="BaseAddress"/>)</summary>
		/// <param name="ptrHandle">Handle to initialize and stream the value into</param>
		/// <remarks>
		/// After the value is read from the stream, <see cref="BaseAddress"/> is subtracted.
		///
		/// Be sure to set <see cref="BaseAddress"/> to the proper value so you get
		/// a correct relative-virtual-address. Otherwise, set <see cref="BaseAddress"/>
		/// to zero to get the pure pointer value.
		/// </remarks>
		public void ReadPointerViaBaseAddress(out Values.PtrHandle ptrHandle)
		{
			ptrHandle = new Values.PtrHandle(BaseAddress.Size);

			ReadPointer(ref ptrHandle);
		}
		#endregion

		/// <summary>Read out an Int32 7 bits at a time. The high bit of the byte when on means to continue reading more bytes.</summary>
		/// <returns></returns>
		public new int Read7BitEncodedInt() { return base.Read7BitEncodedInt(); }

		public DateTime ReadDateTime(bool isUnixTime = false)
		{
			long binary = ReadInt64();

			return isUnixTime
				? Util.ConvertDateTimeFromUnixTime(binary)
				: DateTime.FromBinary(binary);
		}

		public TEnum Read<TEnum>(IEnumEndianStreamer<TEnum> implementation)
			where TEnum : struct, Enum
		{
			ArgumentNullException.ThrowIfNull(implementation);

			return implementation.Read(this);
		}

		public void ReadFixedArray(Span<bool> values)
		{
			for (int x = 0; x < values.Length; x++)
				values[x] = ReadBoolean();
		}
	};
}
