﻿using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	partial class BitStream
	{
		public BitStream Stream<TEnum>(ref TEnum value, int bitCount, IEnumBitStreamer<TEnum> implementation)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(implementation != null);

			implementation.Stream(this, ref value, bitCount);

			return this;
		}

		#region Stream Values
		public BitStream StreamValue<T>(ref T value)
			where T : struct, IO.IBitStreamSerializable
		{
			if (IsReading)
				value = new T();

			value.Serialize(this);

			return this;
		}
		public BitStream StreamValue<T>(ref T value, Func<T> initializer)
			where T : struct, IO.IBitStreamSerializable
		{
			Contract.Requires(initializer != null);

			if (IsReading)
				value = initializer();

			value.Serialize(this);

			return this;
		}
		#endregion

		#region Stream Objects
		public BitStream StreamObject<T>(T value)
			where T : class, IO.IBitStreamSerializable
		{
			Contract.Requires(value != null);

			value.Serialize(this);

			return this;
		}
		public BitStream StreamObject<T>(ref T value, Func<T> initializer)
			where T : class, IO.IBitStreamSerializable
		{
			Contract.Requires(IsReading || value != null);
			Contract.Requires(initializer != null);

			if (IsReading)
				value = initializer();

			value.Serialize(this);

			return this;
		}
		#endregion

		#region Stream Methods
		public BitStream StreamMethods(Action<BitStream> read, Action<BitStream> write)
		{
			Contract.Requires(read != null);
			Contract.Requires(write != null);

				 if (IsReading) read(this);
			else if (IsWriting) write(this);

			return this;
		}
		public BitStream StreamMethods<T>(T context, Action<T, BitStream> read, Action<T, BitStream> write)
			where T : class
		{
			Contract.Requires(context != null);
			Contract.Requires(read != null);
			Contract.Requires(write != null);

				 if (IsReading) read(context, this);
			else if (IsWriting) write(context, this);

			return this;
		}
		#endregion

		#region Stream Array Values
		public BitStream StreamValueArray<T>(T[] values)
			where T : struct, IO.IBitStreamSerializable
		{
			Contract.Requires(values != null);

			for (int x = 0; x < values.Length; x++)
				StreamValue(ref values[x]);

			return this;
		}
		#endregion

		#region Stream Array Objects
		public BitStream StreamObjectArray<T>(T[] values, Func<T> initializer)
			where T : class, IO.IBitStreamSerializable
		{
			Contract.Requires(values != null);
			Contract.Requires(initializer != null);

			for (int x = 0; x < values.Length; x++)
				StreamObject(ref values[x], initializer);

			return this;
		}
		#endregion

		#region Stream Collection
		public BitStream StreamElements<T, TContext>(ICollection<T> list, int countBitSize,
			TContext ctxt, Func<TContext, T> ctor)
			where T : IO.IBitStreamSerializable
		{
			Contract.Requires(list != null);
			Contract.Requires(countBitSize <= Bits.kInt32BitCount);
			Contract.Requires(ctor != null);

			int count = list.Count;
			Stream(ref count, countBitSize);

			if (IsReading)
			{
				for (int x = 0; x < count; x++)
				{
					var t = ctor(ctxt);
					t.Serialize(this);
					list.Add(t);
				}
			}
			else if (IsWriting)
			{
				foreach (var obj in list)
					obj.Serialize(this);
			}

			return this;
		}

		public IO.BitStream StreamElements<T>(ICollection<T> list, int countBitSize)
			where T : IO.IBitStreamSerializable, new()
		{
			Contract.Requires(list != null);
			Contract.Requires(countBitSize <= Bits.kInt32BitCount);

			return StreamElements(list, countBitSize, (object)null, (nil) => new T());
		}
		#endregion
	};
}