﻿using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	/// <summary>
	/// Interface for using an <see cref="EnumBinaryStreamer{TEnum,TStreamType}"/>'s functionality via an instance object
	/// </summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	[Contracts.ContractClass(typeof(IEnumBitStreamerContract<>))]
	public interface IEnumBitStreamer<TEnum>
		where TEnum : struct, IComparable, IFormattable, IConvertible
	{
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="IO.BitStream"/></summary>
		/// <param name="s">Source stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		/// <returns>Value read from the stream</returns>
		TEnum Read(IO.BitStream s, int bitCount);
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="IO.BitStream"/></summary>
		/// <param name="s">Source stream</param>
		/// <param name="value">Value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		void Read(IO.BitStream s, out TEnum value, int bitCount);

		/// <summary>Stream a <typeparamref name="TEnum"/> value to a <see cref="IO.BitStream"/></summary>
		/// <param name="s">Target stream</param>
		/// <param name="value"></param>
		/// <param name="bitCount">Number of bits to write</param>
		void Write(IO.BitStream s, TEnum value, int bitCount);

		/// <summary>Serialize an <typeparamref name="TEnum"/> value to/from a <see cref="IO.BitStream"/></summary>
		/// <param name="s">Target/Source stream</param>
		/// <param name="value">Value read from the stream</param>
		/// <param name="bitCount">Number of bits to stream</param>
		void Stream(IO.BitStream s, ref TEnum value, int bitCount);
	};

	[Contracts.ContractClassFor(typeof(IEnumBitStreamer<>))]
	abstract class IEnumBitStreamerContract<TEnum> : IEnumBitStreamer<TEnum>
		where TEnum : struct, IComparable, IFormattable, IConvertible
	{
		public TEnum Read(IO.BitStream s, int bitCount)
		{
			Contract.Requires<ArgumentNullException>(s != null);
			Contract.Requires(bitCount > 0);

			throw new NotImplementedException();
		}
		public void Read(IO.BitStream s, out TEnum value, int bitCount)
		{
			Contract.Requires<ArgumentNullException>(s != null);
			Contract.Requires(bitCount > 0);

			throw new NotImplementedException();
		}
		public void Write(IO.BitStream s, TEnum value, int bitCount)
		{
			Contract.Requires<ArgumentNullException>(s != null);
			Contract.Requires(bitCount > 0);

			throw new NotImplementedException();
		}
		public void Stream(IO.BitStream s, ref TEnum value, int bitCount)
		{
			Contract.Requires<ArgumentNullException>(s != null);
			Contract.Requires(bitCount > 0);

			throw new NotImplementedException();
		}
	};
}