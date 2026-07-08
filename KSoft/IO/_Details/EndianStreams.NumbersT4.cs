using System;
using System.Buffers.Binary;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	partial class EndianReader
	{
		/// <summary>Reads a unsigned 16-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadUInt16()"/>
		public override ushort ReadUInt16()
		{
			// #VITA_SHIM: BinaryPrimitives reads the stream byte order directly without byte-swap wrappers.
			Span<byte> bytes = stackalloc byte[sizeof(ushort)];
			BaseStream.ReadExactly(bytes);
			return ByteOrder == Shell.EndianFormat.Little
				? BinaryPrimitives.ReadUInt16LittleEndian(bytes)
				: BinaryPrimitives.ReadUInt16BigEndian(bytes);
		}

		/// <summary>Reads a signed 16-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadInt16()"/>
		public override short ReadInt16()
		{
			// #VITA_SHIM: BinaryPrimitives reads the stream byte order directly without byte-swap wrappers.
			Span<byte> bytes = stackalloc byte[sizeof(short)];
			BaseStream.ReadExactly(bytes);
			return ByteOrder == Shell.EndianFormat.Little
				? BinaryPrimitives.ReadInt16LittleEndian(bytes)
				: BinaryPrimitives.ReadInt16BigEndian(bytes);
		}

		/// <summary>Reads a unsigned 32-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadUInt32()"/>
		public override uint ReadUInt32()
		{
			// #VITA_SHIM: BinaryPrimitives reads the stream byte order directly without byte-swap wrappers.
			Span<byte> bytes = stackalloc byte[sizeof(uint)];
			BaseStream.ReadExactly(bytes);
			return ByteOrder == Shell.EndianFormat.Little
				? BinaryPrimitives.ReadUInt32LittleEndian(bytes)
				: BinaryPrimitives.ReadUInt32BigEndian(bytes);
		}

		/// <summary>Reads a signed 32-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadInt32()"/>
		public override int ReadInt32()
		{
			// #VITA_SHIM: BinaryPrimitives reads the stream byte order directly without byte-swap wrappers.
			Span<byte> bytes = stackalloc byte[sizeof(int)];
			BaseStream.ReadExactly(bytes);
			return ByteOrder == Shell.EndianFormat.Little
				? BinaryPrimitives.ReadInt32LittleEndian(bytes)
				: BinaryPrimitives.ReadInt32BigEndian(bytes);
		}

		/// <summary>Reads a unsigned 64-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadUInt64()"/>
		public override ulong ReadUInt64()
		{
			// #VITA_SHIM: BinaryPrimitives reads the stream byte order directly without byte-swap wrappers.
			Span<byte> bytes = stackalloc byte[sizeof(ulong)];
			BaseStream.ReadExactly(bytes);
			return ByteOrder == Shell.EndianFormat.Little
				? BinaryPrimitives.ReadUInt64LittleEndian(bytes)
				: BinaryPrimitives.ReadUInt64BigEndian(bytes);
		}

		/// <summary>Reads a signed 64-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadInt64()"/>
		public override long ReadInt64()
		{
			// #VITA_SHIM: BinaryPrimitives reads the stream byte order directly without byte-swap wrappers.
			Span<byte> bytes = stackalloc byte[sizeof(long)];
			BaseStream.ReadExactly(bytes);
			return ByteOrder == Shell.EndianFormat.Little
				? BinaryPrimitives.ReadInt64LittleEndian(bytes)
				: BinaryPrimitives.ReadInt64BigEndian(bytes);
		}

		/// <summary>Reads a single-precision number</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadSingle()"/>
		public override float ReadSingle()
		{
			// #VITA_SHIM: BinaryPrimitives reads the stream byte order directly without byte-swap wrappers.
			Span<byte> bytes = stackalloc byte[sizeof(float)];
			BaseStream.ReadExactly(bytes);
			return ByteOrder == Shell.EndianFormat.Little
				? BinaryPrimitives.ReadSingleLittleEndian(bytes)
				: BinaryPrimitives.ReadSingleBigEndian(bytes);
		}

		/// <summary>Reads a double-precision number</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadDouble()"/>
		public override double ReadDouble()
		{
			// #VITA_SHIM: BinaryPrimitives reads the stream byte order directly without byte-swap wrappers.
			Span<byte> bytes = stackalloc byte[sizeof(double)];
			BaseStream.ReadExactly(bytes);
			return ByteOrder == Shell.EndianFormat.Little
				? BinaryPrimitives.ReadDoubleLittleEndian(bytes)
				: BinaryPrimitives.ReadDoubleBigEndian(bytes);
		}


		#region ReadFixedArray
		public byte[] ReadFixedArray(byte[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				array[x] = ReadByte();
			}

			return array;
		}
		public byte[] ReadFixedArray(byte[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			return ReadFixedArray(array, 0, array.Length);
		}

		public sbyte[] ReadFixedArray(sbyte[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<sbyte[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				array[x] = ReadSByte();
			}

			return array;
		}
		public sbyte[] ReadFixedArray(sbyte[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<sbyte[]>() != null);

			return ReadFixedArray(array, 0, array.Length);
		}

		public ushort[] ReadFixedArray(ushort[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<ushort[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				array[x] = ReadUInt16();
			}

			return array;
		}
		public ushort[] ReadFixedArray(ushort[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<ushort[]>() != null);

			return ReadFixedArray(array, 0, array.Length);
		}

		public short[] ReadFixedArray(short[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<short[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				array[x] = ReadInt16();
			}

			return array;
		}
		public short[] ReadFixedArray(short[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<short[]>() != null);

			return ReadFixedArray(array, 0, array.Length);
		}

		public uint[] ReadFixedArray(uint[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<uint[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				array[x] = ReadUInt32();
			}

			return array;
		}
		public uint[] ReadFixedArray(uint[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<uint[]>() != null);

			return ReadFixedArray(array, 0, array.Length);
		}

		public int[] ReadFixedArray(int[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<int[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				array[x] = ReadInt32();
			}

			return array;
		}
		public int[] ReadFixedArray(int[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<int[]>() != null);

			return ReadFixedArray(array, 0, array.Length);
		}

		public ulong[] ReadFixedArray(ulong[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<ulong[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				array[x] = ReadUInt64();
			}

			return array;
		}
		public ulong[] ReadFixedArray(ulong[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<ulong[]>() != null);

			return ReadFixedArray(array, 0, array.Length);
		}

		public long[] ReadFixedArray(long[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<long[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				array[x] = ReadInt64();
			}

			return array;
		}
		public long[] ReadFixedArray(long[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<long[]>() != null);

			return ReadFixedArray(array, 0, array.Length);
		}

		public float[] ReadFixedArray(float[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<float[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				array[x] = ReadSingle();
			}

			return array;
		}
		public float[] ReadFixedArray(float[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<float[]>() != null);

			return ReadFixedArray(array, 0, array.Length);
		}

		public double[] ReadFixedArray(double[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<double[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				array[x] = ReadDouble();
			}

			return array;
		}
		public double[] ReadFixedArray(double[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<double[]>() != null);

			return ReadFixedArray(array, 0, array.Length);
		}

		#endregion
	};

	partial class EndianWriter
	{
		/// <summary>Writes a unsigned 16-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(ushort)"/>
		public override void Write(ushort value)
		{
			// #VITA_SHIM: BinaryPrimitives writes the stream byte order directly without byte-swap wrappers.
			Span<byte> bytes = stackalloc byte[sizeof(ushort)];
			if (ByteOrder == Shell.EndianFormat.Little)
			{
				BinaryPrimitives.WriteUInt16LittleEndian(bytes, value);
			}
			else
			{
				BinaryPrimitives.WriteUInt16BigEndian(bytes, value);
			}
			base.Write(bytes);
		}

		/// <summary>Writes a signed 16-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(short)"/>
		public override void Write(short value)
		{
			// #VITA_SHIM: BinaryPrimitives writes the stream byte order directly without byte-swap wrappers.
			Span<byte> bytes = stackalloc byte[sizeof(short)];
			if (ByteOrder == Shell.EndianFormat.Little)
			{
				BinaryPrimitives.WriteInt16LittleEndian(bytes, value);
			}
			else
			{
				BinaryPrimitives.WriteInt16BigEndian(bytes, value);
			}
			base.Write(bytes);
		}

		/// <summary>Writes a unsigned 32-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(uint)"/>
		public override void Write(uint value)
		{
			// #VITA_SHIM: BinaryPrimitives writes the stream byte order directly without byte-swap wrappers.
			Span<byte> bytes = stackalloc byte[sizeof(uint)];
			if (ByteOrder == Shell.EndianFormat.Little)
			{
				BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
			}
			else
			{
				BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
			}
			base.Write(bytes);
		}

		/// <summary>Writes a signed 32-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(int)"/>
		public override void Write(int value)
		{
			// #VITA_SHIM: BinaryPrimitives writes the stream byte order directly without byte-swap wrappers.
			Span<byte> bytes = stackalloc byte[sizeof(int)];
			if (ByteOrder == Shell.EndianFormat.Little)
			{
				BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
			}
			else
			{
				BinaryPrimitives.WriteInt32BigEndian(bytes, value);
			}
			base.Write(bytes);
		}

		/// <summary>Writes a unsigned 64-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(ulong)"/>
		public override void Write(ulong value)
		{
			// #VITA_SHIM: BinaryPrimitives writes the stream byte order directly without byte-swap wrappers.
			Span<byte> bytes = stackalloc byte[sizeof(ulong)];
			if (ByteOrder == Shell.EndianFormat.Little)
			{
				BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
			}
			else
			{
				BinaryPrimitives.WriteUInt64BigEndian(bytes, value);
			}
			base.Write(bytes);
		}

		/// <summary>Writes a signed 64-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(long)"/>
		public override void Write(long value)
		{
			// #VITA_SHIM: BinaryPrimitives writes the stream byte order directly without byte-swap wrappers.
			Span<byte> bytes = stackalloc byte[sizeof(long)];
			if (ByteOrder == Shell.EndianFormat.Little)
			{
				BinaryPrimitives.WriteInt64LittleEndian(bytes, value);
			}
			else
			{
				BinaryPrimitives.WriteInt64BigEndian(bytes, value);
			}
			base.Write(bytes);
		}

		/// <summary>Writes a single-precision number</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(float)"/>
		public override void Write(float value)
		{
			// #VITA_SHIM: BinaryPrimitives writes the stream byte order directly without byte-swap wrappers.
			Span<byte> bytes = stackalloc byte[sizeof(float)];
			if (ByteOrder == Shell.EndianFormat.Little)
			{
				BinaryPrimitives.WriteSingleLittleEndian(bytes, value);
			}
			else
			{
				BinaryPrimitives.WriteSingleBigEndian(bytes, value);
			}
			base.Write(bytes);
		}

		/// <summary>Writes a double-precision number</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(double)"/>
		public override void Write(double value)
		{
			// #VITA_SHIM: BinaryPrimitives writes the stream byte order directly without byte-swap wrappers.
			Span<byte> bytes = stackalloc byte[sizeof(double)];
			if (ByteOrder == Shell.EndianFormat.Little)
			{
				BinaryPrimitives.WriteDoubleLittleEndian(bytes, value);
			}
			else
			{
				BinaryPrimitives.WriteDoubleBigEndian(bytes, value);
			}
			base.Write(bytes);
		}


		#region WriteFixedArray
		public byte[] WriteFixedArray(byte[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				Write(array[x]);
			}

			return array;
		}
		public byte[] WriteFixedArray(byte[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			return WriteFixedArray(array, 0, array.Length);
		}

		public sbyte[] WriteFixedArray(sbyte[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<sbyte[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				Write(array[x]);
			}

			return array;
		}
		public sbyte[] WriteFixedArray(sbyte[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<sbyte[]>() != null);

			return WriteFixedArray(array, 0, array.Length);
		}

		public ushort[] WriteFixedArray(ushort[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<ushort[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				Write(array[x]);
			}

			return array;
		}
		public ushort[] WriteFixedArray(ushort[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<ushort[]>() != null);

			return WriteFixedArray(array, 0, array.Length);
		}

		public short[] WriteFixedArray(short[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<short[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				Write(array[x]);
			}

			return array;
		}
		public short[] WriteFixedArray(short[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<short[]>() != null);

			return WriteFixedArray(array, 0, array.Length);
		}

		public uint[] WriteFixedArray(uint[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<uint[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				Write(array[x]);
			}

			return array;
		}
		public uint[] WriteFixedArray(uint[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<uint[]>() != null);

			return WriteFixedArray(array, 0, array.Length);
		}

		public int[] WriteFixedArray(int[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<int[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				Write(array[x]);
			}

			return array;
		}
		public int[] WriteFixedArray(int[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<int[]>() != null);

			return WriteFixedArray(array, 0, array.Length);
		}

		public ulong[] WriteFixedArray(ulong[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<ulong[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				Write(array[x]);
			}

			return array;
		}
		public ulong[] WriteFixedArray(ulong[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<ulong[]>() != null);

			return WriteFixedArray(array, 0, array.Length);
		}

		public long[] WriteFixedArray(long[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<long[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				Write(array[x]);
			}

			return array;
		}
		public long[] WriteFixedArray(long[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<long[]>() != null);

			return WriteFixedArray(array, 0, array.Length);
		}

		public float[] WriteFixedArray(float[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<float[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				Write(array[x]);
			}

			return array;
		}
		public float[] WriteFixedArray(float[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<float[]>() != null);

			return WriteFixedArray(array, 0, array.Length);
		}

		public double[] WriteFixedArray(double[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<double[]>() != null);

			for (int x = startIndex, end = startIndex+length; x < end; x++)
			{
				Write(array[x]);
			}

			return array;
		}
		public double[] WriteFixedArray(double[] array)
		{
			Contract.Requires(array != null);
			Contract.Ensures(Contract.Result<double[]>() != null);

			return WriteFixedArray(array, 0, array.Length);
		}

		#endregion
	};

	partial class EndianStream
	{
		public EndianStream Stream(ref byte value)
		{
				 if (IsReading) { value = Reader.ReadByte(); }
			else if (IsWriting) { Writer.Write(value); }

			return this;
		}

		public EndianStream Stream(ref sbyte value)
		{
				 if (IsReading) { value = Reader.ReadSByte(); }
			else if (IsWriting) { Writer.Write(value); }

			return this;
		}

		public EndianStream Stream(ref ushort value)
		{
				 if (IsReading) { value = Reader.ReadUInt16(); }
			else if (IsWriting) { Writer.Write(value); }

			return this;
		}

		public EndianStream Stream(ref short value)
		{
				 if (IsReading) { value = Reader.ReadInt16(); }
			else if (IsWriting) { Writer.Write(value); }

			return this;
		}

		public EndianStream Stream(ref uint value)
		{
				 if (IsReading) { value = Reader.ReadUInt32(); }
			else if (IsWriting) { Writer.Write(value); }

			return this;
		}

		public EndianStream Stream(ref int value)
		{
				 if (IsReading) { value = Reader.ReadInt32(); }
			else if (IsWriting) { Writer.Write(value); }

			return this;
		}

		public EndianStream Stream(ref ulong value)
		{
				 if (IsReading) { value = Reader.ReadUInt64(); }
			else if (IsWriting) { Writer.Write(value); }

			return this;
		}

		public EndianStream Stream(ref long value)
		{
				 if (IsReading) { value = Reader.ReadInt64(); }
			else if (IsWriting) { Writer.Write(value); }

			return this;
		}

		public EndianStream Stream(ref float value)
		{
				 if (IsReading) { value = Reader.ReadSingle(); }
			else if (IsWriting) { Writer.Write(value); }

			return this;
		}

		public EndianStream Stream(ref double value)
		{
				 if (IsReading) { value = Reader.ReadDouble(); }
			else if (IsWriting) { Writer.Write(value); }

			return this;
		}


		#region StreamFixedArray
		public EndianStream StreamFixedArray(byte[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (IsReading) { Reader.ReadFixedArray(array, startIndex, length); }
			else if (IsWriting) { Writer.WriteFixedArray(array, startIndex, length); }

			return this;
		}
		public EndianStream StreamFixedArray(byte[] array)
		{
			Contract.Requires(array != null);

			return StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(sbyte[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (IsReading) { Reader.ReadFixedArray(array, startIndex, length); }
			else if (IsWriting) { Writer.WriteFixedArray(array, startIndex, length); }

			return this;
		}
		public EndianStream StreamFixedArray(sbyte[] array)
		{
			Contract.Requires(array != null);

			return StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(ushort[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (IsReading) { Reader.ReadFixedArray(array, startIndex, length); }
			else if (IsWriting) { Writer.WriteFixedArray(array, startIndex, length); }

			return this;
		}
		public EndianStream StreamFixedArray(ushort[] array)
		{
			Contract.Requires(array != null);

			return StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(short[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (IsReading) { Reader.ReadFixedArray(array, startIndex, length); }
			else if (IsWriting) { Writer.WriteFixedArray(array, startIndex, length); }

			return this;
		}
		public EndianStream StreamFixedArray(short[] array)
		{
			Contract.Requires(array != null);

			return StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(uint[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (IsReading) { Reader.ReadFixedArray(array, startIndex, length); }
			else if (IsWriting) { Writer.WriteFixedArray(array, startIndex, length); }

			return this;
		}
		public EndianStream StreamFixedArray(uint[] array)
		{
			Contract.Requires(array != null);

			return StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(int[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (IsReading) { Reader.ReadFixedArray(array, startIndex, length); }
			else if (IsWriting) { Writer.WriteFixedArray(array, startIndex, length); }

			return this;
		}
		public EndianStream StreamFixedArray(int[] array)
		{
			Contract.Requires(array != null);

			return StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(ulong[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (IsReading) { Reader.ReadFixedArray(array, startIndex, length); }
			else if (IsWriting) { Writer.WriteFixedArray(array, startIndex, length); }

			return this;
		}
		public EndianStream StreamFixedArray(ulong[] array)
		{
			Contract.Requires(array != null);

			return StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(long[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (IsReading) { Reader.ReadFixedArray(array, startIndex, length); }
			else if (IsWriting) { Writer.WriteFixedArray(array, startIndex, length); }

			return this;
		}
		public EndianStream StreamFixedArray(long[] array)
		{
			Contract.Requires(array != null);

			return StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(float[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (IsReading) { Reader.ReadFixedArray(array, startIndex, length); }
			else if (IsWriting) { Writer.WriteFixedArray(array, startIndex, length); }

			return this;
		}
		public EndianStream StreamFixedArray(float[] array)
		{
			Contract.Requires(array != null);

			return StreamFixedArray(array, 0, array.Length);
		}

		public EndianStream StreamFixedArray(double[] array, int startIndex, int length)
		{
			Contract.Requires(array != null);
			Contract.Requires(startIndex >= 0);
			Contract.Requires(length >= 0);

				 if (IsReading) { Reader.ReadFixedArray(array, startIndex, length); }
			else if (IsWriting) { Writer.WriteFixedArray(array, startIndex, length); }

			return this;
		}
		public EndianStream StreamFixedArray(double[] array)
		{
			Contract.Requires(array != null);

			return StreamFixedArray(array, 0, array.Length);
		}

		#endregion
	};
}