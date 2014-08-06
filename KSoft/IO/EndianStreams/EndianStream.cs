using System;
using System.IO;
using System.Text;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	public sealed partial class EndianStream : IKSoftBinaryStream, IKSoftStreamModeable, IKSoftStreamWithVirtualBuffer
	{
		public Stream BaseStream { get; private set; }
		public EndianReader Reader { get; private set; }
		public EndianWriter Writer { get; private set; }

		#region IKSoftStream
		/// <summary>Owner of this stream</summary>
		public object Owner
		{
			get { return Reader != null ? Reader.Owner : Writer.Owner; }
			set {
				if (Reader != null) Reader.Owner = value;
				if (Writer != null) Writer.Owner = value;
			}
		}

		public object UserData
		{
			get { return Reader != null ? Reader.UserData : Writer.UserData; }
			set {
				if (Reader != null) Reader.UserData = value;
				if (Writer != null) Writer.UserData = value;
			}
		}

		/// <summary>Name of the underlying stream this object is interfacing with</summary>
		/// <remarks>So if this endian stream is interfacing with a file, this will be it's name</remarks>
		public string StreamName { get {
				 if (IsReading) return Reader.StreamName;
			else if (IsWriting) return Writer.StreamName;
			else throw new Debug.UnreachableException(StreamMode.ToString());
		} }
		#endregion

		#region IKSoftBinaryStream
		/// <summary>Base address used for simulating pointers in the stream</summary>
		/// <remarks>Default value is <see cref="Data.PtrHandle.Null32"/></remarks>
		public Values.PtrHandle BaseAddress
		{
			get { return Reader != null ? Reader.BaseAddress : Writer.BaseAddress; }
			set {
				if (Reader != null) Reader.BaseAddress = value;
				if (Writer != null) Writer.BaseAddress = value;
			}
		}

		#region Seek
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to the beginning of the stream</summary>
		/// <param name="offset">Offset to seek to</param>
		public void Seek32(uint offset)						{ Seek(offset, SeekOrigin.Begin); }
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to <paramref name="origin"/></summary>
		/// <param name="offset">Offset to seek to</param>
		/// <param name="origin">Origin to base seek operation</param>
		public void Seek32(uint offset, SeekOrigin origin)	{ Seek(offset, origin); }
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to the beginning of the stream</summary>
		/// <param name="offset">Offset to seek to</param>
		public void Seek32(int offset)						{ Seek(offset, SeekOrigin.Begin); }
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to <paramref name="origin"/></summary>
		/// <param name="offset">Offset to seek to</param>
		/// <param name="origin">Origin to base seek operation</param>
		public void Seek32(int offset, SeekOrigin origin)	{ Seek(offset, origin); }

		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to the beginning of the stream</summary>
		/// <param name="offset">Offset to seek to</param>
		public void Seek(long offset)						{ Seek(offset, SeekOrigin.Begin); }
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to <paramref name="origin"/></summary>
		/// <param name="offset">Offset to seek to</param>
		/// <param name="origin">Origin to base seek operation</param>
		public void Seek(long offset, SeekOrigin origin)	{ BaseStream.Seek(offset, origin); }
		#endregion

		/// <summary>Align the stream's position by a certain page boundry</summary>
		/// <param name="alignmentBit">log2 size of the alignment (ie, 1&lt;&lt;bit)</param>
		/// <returns>True if any alignment had to be performed, false if otherwise</returns>
		public bool AlignToBoundry(int alignmentBit)
		{
				 if (IsReading) return Reader.AlignToBoundry(alignmentBit);
			else if (IsWriting)	return Writer.AlignToBoundry(alignmentBit);
			else throw new Debug.UnreachableException(StreamMode.ToString());
		}

		#region VirtualAddressTranslation
		/// <summary>Initialize the VAT with a specific handle size and initial table capacity</summary>
		/// <param name="vaSize">Handle size</param>
		/// <param name="translationCapacity">The initial table capacity</param>
		public void VirtualAddressTranslationInitialize(Shell.ProcessorSize vaSize, int translationCapacity = 0)
		{
			if (Reader != null) Reader.VirtualAddressTranslationInitialize(vaSize, translationCapacity);
			if (Writer != null) Writer.VirtualAddressTranslationInitialize(vaSize, translationCapacity);
		}
		/// <summary>Push a PA into to the VAT table, setting the current PA in the process</summary>
		/// <param name="physicalAddress">PA to push and to use as the VAT's current address</param>
		public void VirtualAddressTranslationPush(Values.PtrHandle physicalAddress)
		{
			if (Reader != null) Reader.VirtualAddressTranslationPush(physicalAddress);
			if (Writer != null) Writer.VirtualAddressTranslationPush(physicalAddress);
		}
		/// <summary>Push the stream's position (as a physical address) into the VAT table</summary>
		public void VirtualAddressTranslationPushPosition()
		{
			if (Reader != null) Reader.VirtualAddressTranslationPushPosition();
			if (Writer != null) Writer.VirtualAddressTranslationPushPosition();
		}
		/// <summary>Increase the current address (PA) by a relative offset</summary>
		/// <param name="relativeOffset">Offset, relative to the current address</param>
		public void VirtualAddressTranslationIncrease(Values.PtrHandle relativeOffset)
		{
			if (Reader != null) Reader.VirtualAddressTranslationIncrease(relativeOffset);
			if (Writer != null) Writer.VirtualAddressTranslationIncrease(relativeOffset);
		}
		/// <summary>Pop and return the current address (PA) in the VAT table</summary>
		/// <returns>The VAT's current address value before this call</returns>
		public Values.PtrHandle VirtualAddressTranslationPop()
		{
			var result = BaseAddress.Is64bit 
				? Values.PtrHandle.InvalidHandle64 
				: Values.PtrHandle.InvalidHandle32;

			if (Reader != null) result = Reader.VirtualAddressTranslationPop();
			if (Writer != null) result = Writer.VirtualAddressTranslationPop();

			return result;
		}
		#endregion

		#region PositionPtr
		/// <summary>Get the current position as a <see cref="Data.PtrHandle"/></summary>
		/// <param name="ptrSize">Pointer size to use for the result handle</param>
		/// <returns></returns>
		public Values.PtrHandle GetPositionPtr(Shell.ProcessorSize ptrSize)
		{
			return new Values.PtrHandle(ptrSize, (ulong)BaseStream.Position);
		}
		/// <summary>Current position as a <see cref="Data.PtrHandle"/></summary>
		/// <remarks>Pointer traits\info is inherited from <see cref="BaseAddress"/></remarks>
		public Values.PtrHandle PositionPtr { get {
			return new Values.PtrHandle(BaseAddress, (ulong)BaseStream.Position);
		} }
		#endregion
		#endregion

		#region IKSoftStreamModeable
		public FileAccess StreamPermissions { get; private set; }
		public FileAccess StreamMode { get; set; }
		public bool IsReading { get { return StreamMode == FileAccess.Read; } }
		public bool IsWriting { get { return StreamMode == FileAccess.Write; } }
		#endregion

		#region IKSoftStreamWithVirtualBuffer Members
		public long VirtualBufferStart { get; set; }
		public long VirtualBufferLength { get; set; }
		#endregion

		#region IKSoftEndianStream
		/// <summary>The assumed byte order of the stream</summary>
		/// <remarks>Use <see cref="ChangeByteOrder"/> to properly change this property</remarks>
		public Shell.EndianFormat ByteOrder { get {
			return Reader != null ? Reader.ByteOrder : Writer.ByteOrder;
		} }

		/// <summary>Change the order in which bytes are ordered to/from the stream</summary>
		/// <param name="newOrder">The new byte order to switch to</param>
		/// <remarks>If <paramref name="newOrder"/> is the same as <see cref="ByteOrder"/> nothing will happen</remarks>
		public void ChangeByteOrder(Shell.EndianFormat newOrder)
		{
			if (Reader != null) Reader.ChangeByteOrder(newOrder);
			if (Writer != null) Writer.ChangeByteOrder(newOrder);
		}

		/// <summary>Convenience class for C# "using" statements where we want to temporarily inverse the current byte order</summary>
		class EndianFormatSwitchBlock : IDisposable
		{
			readonly IDisposable mReaderSwitch, mWriterSwitch;

			/// <summary></summary>
			/// <param name="s"></param>
			public EndianFormatSwitchBlock(EndianStream s)
			{
				if (s.Reader != null) mReaderSwitch = s.Reader.BeginEndianSwitch();
				if (s.Writer != null) mWriterSwitch = s.Writer.BeginEndianSwitch();
			}

			#region IDisposable Members
			public void Dispose()
			{
				if (mReaderSwitch != null) mReaderSwitch.Dispose();
				if (mWriterSwitch != null) mWriterSwitch.Dispose();
			}
			#endregion
		};

		/// <summary>Convenience method for C# "using" statements. Temporarily inverts the current byte order which is used for read/writes.</summary>
		/// <returns>Object which when Disposed will return this stream to its original <see cref="Shell.EndianFormat"/> state</returns>
		public IDisposable BeginEndianSwitch()
		{
			return new EndianFormatSwitchBlock(this);
		}
		/// <summary>Convenience method for C# "using" statements. Temporarily inverts the current byte order which is used for read/writes.</summary>
		/// <param name="switchTo">Byte order to switch to</param>
		/// <returns>Object which when Disposed will return this stream to its original <see cref="Shell.EndianFormat"/> state</returns>
		/// <remarks>
		/// If <paramref name="switchTo"/> is the same as <see cref="EndianStream.ByteOrder"/>
		/// then no actual object state changes will happen. However, this construct 
		/// will continue to be usable and will Dispose of properly with no error
		/// </remarks>
		public IDisposable BeginEndianSwitch(Shell.EndianFormat switchTo)
		{
			if (switchTo == this.ByteOrder)
				return Util.NullDisposable;

			return new EndianFormatSwitchBlock(this);
		}
		#endregion

		/// <summary>Do we own the base stream?</summary>
		/// <remarks>If we don't own the stream, when this object is disposed, the <see cref="BaseStream"/> won't be closed\disposed</remarks>
		public bool BaseStreamOwner
		{
			get { return Reader != null ? Reader.BaseStreamOwner : Writer.BaseStreamOwner; }
			set {
				if (Reader != null) Reader.BaseStreamOwner = value;
				if (Writer != null) Writer.BaseStreamOwner = value;
			}
		}

		public bool CanRead		{ get { return Reader != null; } }
		public bool CanWrite	{ get { return Writer != null; } }

		#region Ctor
		public EndianStream(Stream baseStream, FileAccess permissions = FileAccess.ReadWrite)
		{
			Contract.Requires<ArgumentNullException>(baseStream != null);

			BaseStream = baseStream;
			StreamPermissions = permissions;
			StreamMode = 0;

			if (baseStream.CanRead && permissions.CanRead())
				Reader = new EndianReader(baseStream);
			if (baseStream.CanWrite && permissions.CanWrite())
				Writer = new EndianWriter(baseStream);
		}

		public EndianStream(Stream baseStream, Encoding encoding, 
			Shell.EndianFormat byteOrder,
			object streamOwner = null, string name = null, FileAccess permissions = FileAccess.ReadWrite)
		{
			Contract.Requires<ArgumentNullException>(baseStream != null);
			Contract.Requires<ArgumentNullException>(encoding != null);

			BaseStream = baseStream;
			StreamPermissions = permissions;
			StreamMode = 0;

			if (baseStream.CanRead && permissions.CanRead())
				Reader = new EndianReader(baseStream, encoding, byteOrder, streamOwner, name);
			if (baseStream.CanWrite && permissions.CanWrite())
				Writer = new EndianWriter(baseStream, encoding, byteOrder, streamOwner, name);
		}

		public EndianStream(Stream baseStream,
			Shell.EndianFormat byteOrder,
			object streamOwner = null, string name = null, FileAccess permissions = FileAccess.ReadWrite)
		{
			Contract.Requires<ArgumentNullException>(baseStream != null);

			BaseStream = baseStream;
			StreamPermissions = permissions;
			StreamMode = 0;

			if (baseStream.CanRead && permissions.CanRead())
				Reader = new EndianReader(baseStream, byteOrder, streamOwner, name);
			if (baseStream.CanWrite && permissions.CanWrite())
				Writer = new EndianWriter(baseStream, byteOrder, streamOwner, name);
		}
		#endregion

		#region IDisposable Members
		public void Dispose()
		{
			// NOTE: we intentionally don't call BaseStream's Dispose
			// Reader/Writer should call it, if needed

			if (Reader != null)
			{
				Reader.Dispose();
				Reader = null;
			}
			if (Writer != null)
			{
				Writer.Dispose();
				Writer = null;
			}
		}
		#endregion

		public void Close()
		{
			this.Dispose();
		}

		#region Pad
		public EndianStream Pad(int byteCount)
		{
			Contract.Requires(byteCount > 0);

				 if (IsReading) Reader.Pad(byteCount);
			else if (IsWriting) Writer.Pad(byteCount);

			return this;
		}
		public EndianStream Pad8()
		{
				 if (IsReading) Reader.Pad8();
			else if (IsWriting) Writer.Pad8();

			return this;
		}
		public EndianStream Pad16()
		{
				 if (IsReading) Reader.Pad16();
			else if (IsWriting) Writer.Pad16();

			return this;
		}
		public EndianStream Pad24()
		{
				 if (IsReading) Reader.Pad24();
			else if (IsWriting) Writer.Pad24();

			return this;
		}
		public EndianStream Pad32()
		{
				 if (IsReading) Reader.Pad32();
			else if (IsWriting) Writer.Pad32();

			return this;
		}
		public EndianStream Pad64()
		{
				 if (IsReading) Reader.Pad64();
			else if (IsWriting) Writer.Pad64();

			return this;
		}
		public EndianStream Pad128()
		{
				 if (IsReading) Reader.Pad128();
			else if (IsWriting) Writer.Pad128();

			return this;
		}
		#endregion

		public EndianStream Stream(ref bool value)
		{
				 if (IsReading) value = Reader.ReadBoolean();
			else if (IsWriting) Writer.Write(value);

			return this;
		}

		#region Stream (buffers)
		public EndianStream Stream(byte[] value, int index, int count)
		{
			Contract.Requires(value != null);
			Contract.Requires(index >= 0 && index < value.Length);
			Contract.Requires(count >= 0 && count <= value.Length);
			Contract.Requires((index+count) <= value.Length);

				 if (IsReading) Reader.Read(value, index, count);
			else if (IsWriting) Writer.Write(value, index, count);

			return this;
		}
		public EndianStream Stream(byte[] value, int count)
		{
			Contract.Requires(value != null);
			Contract.Requires(count >= 0 && count <= value.Length);

				 if (IsReading) Reader.Read(value, count);
			else if (IsWriting) Writer.Write(value, count);

			return this;
		}
		public EndianStream Stream(byte[] value)
		{
			Contract.Requires(value != null);

				 if (IsReading) Reader.Read(value, value.Length);
			else if (IsWriting) Writer.Write(value, value.Length);

			return this;
		}
		public EndianStream Stream(char[] value, int index, int count)
		{
			Contract.Requires(value != null);
			Contract.Requires(index >= 0 && index < value.Length);
			Contract.Requires(count >= 0 && count <= value.Length);
			Contract.Requires((index+count) <= value.Length);

				 if (IsReading) Reader.Read(value, index, count);
			else if (IsWriting) Writer.Write(value, index, count);

			return this;
		}
		public EndianStream Stream(char[] value, int count)
		{
			Contract.Requires(value != null);
			Contract.Requires(count >= 0 && count <= value.Length);

				 if (IsReading) Reader.Read(value, count);
			else if (IsWriting) Writer.Write(value, count);

			return this;
		}
		public EndianStream Stream(char[] value)
		{
			Contract.Requires(value != null);

				 if (IsReading) Reader.Read(value, value.Length);
			else if (IsWriting) Writer.Write(value, value.Length);

			return this;
		}
		#endregion

		#region Stream group tag
		char[] mTagScratchBuffer = new char[8];

		public EndianStream StreamTag(ref uint value)
		{
				 if (IsReading) value = Reader.ReadTagUInt32();
			else if (IsWriting) Writer.WriteTag32(value);

			return this;
		}
		// Tag always appears in big-endian order in the stream
		public EndianStream StreamTagBigEndian(ref uint value)
		{
			if (IsReading)
			{
				Reader.ReadTag32(mTagScratchBuffer);
				value = Values.GroupTagData32.ToUInt(mTagScratchBuffer);
			}
			else if (IsWriting)
			{
				Values.GroupTagData32.FromUInt(value, mTagScratchBuffer);
				Writer.WriteTag32(mTagScratchBuffer);
			}

			return this;
		}

		public EndianStream StreamTag(ref ulong value)
		{
				 if (IsReading) value = Reader.ReadTagUInt64();
			else if (IsWriting) Writer.WriteTag64(value);

			return this;
		}
		#endregion

		#region Stream numerics
		public EndianStream StreamInt24(ref int value)
		{
				 if (IsReading) value = Reader.ReadInt24();
			else if (IsWriting) Writer.WriteInt24(value);

			return this;
		}
		public EndianStream StreamUInt24(ref uint value)
		{
				 if (IsReading) value = Reader.ReadUInt24();
			else if (IsWriting) Writer.WriteUInt24(value);

			return this;
		}
		public EndianStream StreamUInt40(ref ulong value)
		{
				 if (IsReading) value = Reader.ReadUInt40();
			else if (IsWriting) Writer.WriteUInt40(value);

			return this;
		}
		#endregion

		#region Stream strings
		public EndianStream Stream(ref string value)
		{
				 if (IsReading) value = Reader.ReadString();
			else if (IsWriting) Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref string value, Memory.Strings.StringStorage storage)
		{
				 if (IsReading) value = Reader.ReadString(storage);
			else if (IsWriting) Writer.Write(value, storage);

			return this;
		}
		public EndianStream Stream(ref string value, Memory.Strings.StringStorage storage, int length)
		{
				 if (IsReading) value = Reader.ReadString(storage, length);
			else if (IsWriting) Writer.Write(value, storage);

			return this;
		}

		public EndianStream Stream(ref string value, Text.StringStorageEncoding encoding)
		{
			Contract.Requires(encoding != null);

				 if (IsReading) value = Reader.ReadString(encoding);
			else if (IsWriting) Writer.Write(value, encoding);

			return this;
		}
		public EndianStream Stream(ref string value, Text.StringStorageEncoding encoding, int length)
		{
			Contract.Requires(encoding != null);

				 if (IsReading) value = Reader.ReadString(encoding, length);
			else if (IsWriting) Writer.Write(value, encoding);

			return this;
		}
		#endregion

		#region Stream Pointer
		public EndianStream StreamRawPointer(ref Values.PtrHandle value)
		{
				 if (IsReading) Reader.ReadRawPointer(ref value);
			else if (IsWriting) Writer.WriteRawPointer(value);

			return this;
		}
		public EndianStream StreamRawPointer(ref Values.PtrHandle value, Shell.ProcessorSize addressSize)
		{
				 if (IsReading) value = Reader.ReadRawPointer(addressSize);
			else if (IsWriting) Writer.WriteRawPointer(value);

			return this;
		}

		public EndianStream StreamPointer(ref Values.PtrHandle value)
		{
				 if (IsReading) Reader.ReadPointer(ref value);
			else if (IsWriting) Writer.WritePointer(value);

			return this;
		}
		public EndianStream StreamPointer(ref Values.PtrHandle value, Shell.ProcessorSize addressSize)
		{
				 if (IsReading) value = Reader.ReadPointer(addressSize);
			else if (IsWriting) Writer.WritePointer(value);

			return this;
		}

		public EndianStream StreamPointerViaBaseAddress(ref Values.PtrHandle value)
		{
				 if (IsReading) value = Reader.ReadPointerViaBaseAddress();
			else if (IsWriting) Writer.WritePointer(value);

			return this;
		}
		#endregion

		public EndianStream Stream7BitEncodedInt(ref int value)
		{
				 if (IsReading) value = Reader.Read7BitEncodedInt();
			else if (IsWriting) Writer.Write7BitEncodedInt(value);

			return this;
		}

		public EndianStream Stream(ref DateTime value, bool isUnixTime = false)
		{
				 if (IsReading) value = Reader.ReadDateTime(isUnixTime);
			else if (IsWriting) Writer.Write(value, isUnixTime);

			return this;
		}

		public EndianStream Stream<TEnum>(ref TEnum value, IEnumEndianStreamer<TEnum> implementation)
			where TEnum : struct
		{
			Contract.Requires(implementation != null);

			implementation.Stream(this, ref value);

			return this;
		}

		#region Stream Values
		public EndianStream StreamValue<T>(ref T value)
			where T : struct, IO.IEndianStreamable
		{
			if (IsReading)
			{
				value = new T();
				value.Read(Reader);
			}
			else if (IsWriting) value.Write(Writer);

			return this;
		}
		public EndianStream StreamValue<T>(ref T value, Func<T> initializer)
			where T : struct, IO.IEndianStreamable
		{
			Contract.Requires(initializer != null);

			if (IsReading)
			{
				value = initializer();
				value.Read(Reader);
			}
			else if (IsWriting) value.Write(Writer);

			return this;
		}

		public EndianStream Stream<T>(ref T value)
			where T : struct, IO.IEndianStreamSerializable
		{
			if (IsReading)
				value = new T();

			value.Serialize(this);

			return this;
		}
		#endregion

		#region Stream Objects
		public EndianStream StreamObject<T>(T value)
			where T : class, IO.IEndianStreamable
		{
			Contract.Requires(value != null);

				 if (IsReading) value.Read(Reader);
			else if (IsWriting) value.Write(Writer);

			return this;
		}
		public EndianStream StreamObject<T>(ref T value, Func<T> initializer)
			where T : class, IO.IEndianStreamable
		{
			Contract.Requires(IsReading || value != null);
			Contract.Requires(initializer != null);

			if (IsReading)
			{
				value = initializer();
				value.Read(Reader);
			}
			else if (IsWriting) value.Write(Writer);

			return this;
		}

		public EndianStream Stream<T>(T value)
			where T : class, IO.IEndianStreamSerializable
		{
			Contract.Requires(value != null);

			value.Serialize(this);

			return this;
		}
		public EndianStream Stream<T>(ref T value, Func<T> initializer)
			where T : class, IO.IEndianStreamSerializable
		{
			Contract.Requires(IsReading || value != null);
			Contract.Requires(initializer != null);

			if (IsReading)
				value = initializer();

			value.Serialize(this);

			return this;
		}
		#endregion

		#region Stream Value Methods
		public delegate void ReadValueDelegate<T>(EndianReader r, out T value);

		public EndianStream StreamValueMethods<T>(ref T value, ReadValueDelegate<T> read, Action<EndianWriter, T> write)
		{
			Contract.Requires(read != null);
			Contract.Requires(write != null);

				 if (IsReading) read (Reader, out value);
			else if (IsWriting) write(Writer, value);

			return this;
		}
		#endregion

		public EndianStream StreamObjectMethods<T>(T obj, Action<EndianReader, T> read, Action<EndianWriter, T> write)
			where T : class
		{
			Contract.Requires(obj != null);
			Contract.Requires(read != null);
			Contract.Requires(write != null);

				 if (IsReading) read (Reader, obj);
			else if (IsWriting) write(Writer, obj);

			return this;
		}

		#region Stream Methods
		public EndianStream StreamMethods(Action<EndianReader> read, Action<EndianWriter> write)
		{
			Contract.Requires(read != null);
			Contract.Requires(write != null);

				 if (IsReading) read(Reader);
			else if (IsWriting) write(Writer);

			return this;
		}
		public EndianStream StreamMethods<T>(T context, Action<T, EndianReader> read, Action<T, EndianWriter> write)
			where T : class
		{
			Contract.Requires(context != null);
			Contract.Requires(read != null);
			Contract.Requires(write != null);

				 if (IsReading) read(context, Reader);
			else if (IsWriting) write(context, Writer);

			return this;
		}
		#endregion

		#region Stream Array Values
		public delegate EndianStream StreamArrayValueDelegate<T>(ref T value);

		public EndianStream StreamArray<T>(T[] values)
			where T : struct, IO.IEndianStreamSerializable
		{
			Contract.Requires(values != null);

			for (int x = 0; x < values.Length; x++)
				Stream(ref values[x]);

			return this;
		}

		public EndianStream StreamArrayInt32<T>(ref T[] values)
			where T : struct, IO.IEndianStreamSerializable
		{
			Contract.Requires(IsReading || values != null);

			bool reading = IsReading;

			int count = reading ? 0 : values.Length;
			Stream(ref count);
			if (reading)
				values = new T[count];

			for (int x = 0; x < count; x++)
				Stream(ref values[x]);

			return this;
		}
		public EndianStream StreamArrayInt32<T>(ref T[] values, StreamArrayValueDelegate<T> streamFunc)
			where T : struct
		{
			Contract.Requires(IsReading || values != null);
			Contract.Requires(streamFunc != null);

			bool reading = IsReading;

			int count = reading ? 0 : values.Length;
			Stream(ref count);
			if (reading)
				values = new T[count];

			for (int x = 0; x < count; x++)
				streamFunc(ref values[x]);

			return this;
		}
		#endregion

		#region Stream Array Objects
		public EndianStream StreamArray<T>(T[] values, Func<T> initializer)
			where T : class, IO.IEndianStreamSerializable
		{
			Contract.Requires(values != null);
			Contract.Requires(initializer != null);

			for (int x = 0; x < values.Length; x++)
				Stream(ref values[x], initializer);

			return this;
		}

		public EndianStream StreamArrayInt32<T>(ref T[] values, Func<T> initializer)
			where T : class, IO.IEndianStreamSerializable
		{
			Contract.Requires(IsReading || values != null);
			Contract.Requires(initializer != null);

			bool reading = IsReading;

			int count = reading ? 0 : values.Length;
			Stream(ref count);
			if (reading)
				values = new T[count];

			for (int x = 0; x < count; x++)
				Stream(ref values[x], initializer);

			return this;
		}
		#endregion

		#region Stream Array Methods
		public delegate void ReadArrayDelegate<T>(EndianReader r, ref T[] array);
		public delegate void WriteArrayDelegate<T>(EndianWriter r, T[] array);

		public EndianStream StreamArrayMethods<T>(ref T[] array, ReadArrayDelegate<T> read, WriteArrayDelegate<T> write)
		{
			Contract.Requires(array != null);
			Contract.Requires(read != null);
			Contract.Requires(write != null);

				 if (IsReading) read (Reader, ref array);
			else if (IsWriting) write(Writer, array);

			return this;
		}
		#endregion

		#region Stream signature
		public EndianStream StreamSignature(byte signature)
		{
				 if (IsReading) SignatureMismatchException.Assert(Reader, signature);
			else if (IsWriting) Writer.Write(signature);

			return this;
		}
		public EndianStream StreamSignature(ushort signature)
		{
				 if (IsReading) SignatureMismatchException.Assert(Reader, signature);
			else if (IsWriting) Writer.Write(signature);

			return this;
		}
		public EndianStream StreamSignature(uint signature)
		{
				 if (IsReading) SignatureMismatchException.Assert(Reader, signature);
			else if (IsWriting) Writer.Write(signature);

			return this;
		}
		public EndianStream StreamSignature(ulong signature)
		{
				 if (IsReading) SignatureMismatchException.Assert(Reader, signature);
			else if (IsWriting) Writer.Write(signature);

			return this;
		}
		public EndianStream StreamSignature(string signature, Memory.Strings.StringStorage storage)
		{
			Contract.Requires(!string.IsNullOrEmpty(signature));

				 if (IsReading) SignatureMismatchException.Assert(Reader, signature, storage);
			else if (IsWriting) Writer.Write(signature, storage);

			return this;
		}
		public EndianStream StreamSignature(string signature, Text.StringStorageEncoding encoding)
		{
			Contract.Requires(!string.IsNullOrEmpty(signature));
			Contract.Requires(encoding != null);

				 if (IsReading) SignatureMismatchException.Assert(Reader, signature, encoding);
			else if (IsWriting) Writer.Write(signature, encoding);

			return this;
		}
		#endregion

		#region Stream version
		public EndianStream StreamVersion(byte version)
		{
				 if (IsReading) VersionMismatchException.Assert(Reader, version);
			else if (IsWriting) Writer.Write(version);

			return this;
		}
		public EndianStream StreamVersion(ushort version)
		{
				 if (IsReading) VersionMismatchException.Assert(Reader, version);
			else if (IsWriting) Writer.Write(version);

			return this;
		}
		public EndianStream StreamVersion(uint version)
		{
				 if (IsReading) VersionMismatchException.Assert(Reader, version);
			else if (IsWriting) Writer.Write(version);

			return this;
		}
		public EndianStream StreamVersion(ulong version)
		{
				 if (IsReading) VersionMismatchException.Assert(Reader, version);
			else if (IsWriting) Writer.Write(version);

			return this;
		}
		#endregion

		[System.Diagnostics.Conditional("TRACE")]
		public void TraceAndDebugPosition(ref long position)
		{
			if (IsReading)
				position = BaseStream.Position;
			else if (IsWriting)
				Contract.Assert(position == BaseStream.Position);
		}
	};
}