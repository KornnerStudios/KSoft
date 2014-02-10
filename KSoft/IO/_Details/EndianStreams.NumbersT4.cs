using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	partial class EndianReader
	{
		/// <summary>Reads a unsigned 16-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadUInt16()"/>
		public override ushort ReadUInt16()
		{
			var value = base.ReadUInt16();
			return !mRequiresByteSwap 
				? value 
				: Bitwise.ByteSwap.SwapUInt16(value);
		}

		/// <summary>Reads a signed 16-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadInt16()"/>
		public override short ReadInt16()
		{
			var value = base.ReadInt16();
			return !mRequiresByteSwap 
				? value 
				: Bitwise.ByteSwap.SwapInt16(value);
		}

		/// <summary>Reads a unsigned 32-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadUInt32()"/>
		public override uint ReadUInt32()
		{
			var value = base.ReadUInt32();
			return !mRequiresByteSwap 
				? value 
				: Bitwise.ByteSwap.SwapUInt32(value);
		}

		/// <summary>Reads a signed 32-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadInt32()"/>
		public override int ReadInt32()
		{
			var value = base.ReadInt32();
			return !mRequiresByteSwap 
				? value 
				: Bitwise.ByteSwap.SwapInt32(value);
		}

		/// <summary>Reads a uint64-precision number</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadUInt64()"/>
		public override ulong ReadUInt64()
		{
			var value = base.ReadUInt64();
			return !mRequiresByteSwap 
				? value 
				: Bitwise.ByteSwap.SwapUInt64(value);
		}

		/// <summary>Reads a unsigned 64-bit integer</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadInt64()"/>
		public override long ReadInt64()
		{
			var value = base.ReadInt64();
			return !mRequiresByteSwap 
				? value 
				: Bitwise.ByteSwap.SwapInt64(value);
		}

		/// <summary>Reads a single-precision number</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadSingle()"/>
		public override float ReadSingle()
		{
			var value = base.ReadSingle();
			return !mRequiresByteSwap 
				? value 
				: Bitwise.ByteSwap.SwapSingle(value);
		}

		/// <summary>Reads a double-precision number</summary>
		/// <returns></returns>
		/// <seealso cref="System.IO.BinaryReader.ReadDouble()"/>
		public override double ReadDouble()
		{
			var value = base.ReadDouble();
			return !mRequiresByteSwap 
				? value 
				: Bitwise.ByteSwap.SwapDouble(value);
		}

	};

	partial class EndianWriter
	{
		/// <summary>Writes a unsigned 16-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(ushort)"/>
		public override void Write(ushort value)
		{
			base.Write(!mRequiresByteSwap 
				? value 
				: Bitwise.ByteSwap.SwapUInt16(value));
		}

		/// <summary>Writes a signed 16-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(short)"/>
		public override void Write(short value)
		{
			base.Write(!mRequiresByteSwap 
				? value 
				: Bitwise.ByteSwap.SwapInt16(value));
		}

		/// <summary>Writes a unsigned 32-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(uint)"/>
		public override void Write(uint value)
		{
			base.Write(!mRequiresByteSwap 
				? value 
				: Bitwise.ByteSwap.SwapUInt32(value));
		}

		/// <summary>Writes a signed 32-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(int)"/>
		public override void Write(int value)
		{
			base.Write(!mRequiresByteSwap 
				? value 
				: Bitwise.ByteSwap.SwapInt32(value));
		}

		/// <summary>Writes a uint64-precision number</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(ulong)"/>
		public override void Write(ulong value)
		{
			base.Write(!mRequiresByteSwap 
				? value 
				: Bitwise.ByteSwap.SwapUInt64(value));
		}

		/// <summary>Writes a unsigned 64-bit integer</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(long)"/>
		public override void Write(long value)
		{
			base.Write(!mRequiresByteSwap 
				? value 
				: Bitwise.ByteSwap.SwapInt64(value));
		}

		/// <summary>Writes a single-precision number</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(float)"/>
		public override void Write(float value)
		{
			base.Write(!mRequiresByteSwap 
				? value 
				: Bitwise.ByteSwap.SwapSingle(value));
		}

		/// <summary>Writes a double-precision number</summary>
		/// <param name="value"></param>
		/// <seealso cref="System.IO.BinaryWriter.Write(double)"/>
		public override void Write(double value)
		{
			base.Write(!mRequiresByteSwap 
				? value 
				: Bitwise.ByteSwap.SwapDouble(value));
		}

	};

	partial class EndianStream
	{
		public EndianStream Stream(ref byte value)
		{
				 if (IsReading) value = Reader.ReadByte();
			else if (IsWriting) Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref sbyte value)
		{
				 if (IsReading) value = Reader.ReadSByte();
			else if (IsWriting) Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref ushort value)
		{
				 if (IsReading) value = Reader.ReadUInt16();
			else if (IsWriting) Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref short value)
		{
				 if (IsReading) value = Reader.ReadInt16();
			else if (IsWriting) Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref uint value)
		{
				 if (IsReading) value = Reader.ReadUInt32();
			else if (IsWriting) Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref int value)
		{
				 if (IsReading) value = Reader.ReadInt32();
			else if (IsWriting) Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref ulong value)
		{
				 if (IsReading) value = Reader.ReadUInt64();
			else if (IsWriting) Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref long value)
		{
				 if (IsReading) value = Reader.ReadInt64();
			else if (IsWriting) Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref float value)
		{
				 if (IsReading) value = Reader.ReadSingle();
			else if (IsWriting) Writer.Write(value);

			return this;
		}

		public EndianStream Stream(ref double value)
		{
				 if (IsReading) value = Reader.ReadDouble();
			else if (IsWriting) Writer.Write(value);

			return this;
		}

	};
}