using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Shell
{
	/// <summary>Represents a processor definition</summary>
	/// <remarks>Currently only this class can define the various processor definitions</remarks>
	[System.Diagnostics.DebuggerDisplay("IA = {InstructionSet}, WordSize = {ProcessorSize}, Endian = {ByteOrder}")]
	public struct Processor : 
		IComparer<Processor>, System.Collections.IComparer,
		IComparable<Processor>, IComparable,
		IEquatable<Processor>
	{
		#region InstructionSet
		InstructionSet mInstructionSet;
		/// <summary>The processor's instruction set</summary>
		public InstructionSet InstructionSet	{ get { return mInstructionSet; } }
		#endregion

		#region ProcessorSize
		ProcessorSize mProcessorSize;
		/// <summary>The processor's instruction size</summary>
		public ProcessorSize ProcessorSize		{ get { return mProcessorSize; } }
		#endregion

		#region ByteOrder
		EndianFormat mByteOrder;
		/// <summary>The processor's byte ordering</summary>
		public EndianFormat ByteOrder			{ get { return mByteOrder; } }
		#endregion

		/// <summary>Construct a processor definition</summary>
		/// <param name="size">Instruction size of the processor</param>
		/// <param name="byteOrder">Default byte ordering of the processor</param>
		/// <param name="instructionSet">Instruction set of the processor</param>
		Processor(ProcessorSize size, EndianFormat byteOrder, InstructionSet instructionSet)
		{
			mInstructionSet = instructionSet;
			mProcessorSize = size;
			mByteOrder = byteOrder;
		}

		#region Overrides
		/// <summary>See <see cref="Object.Equals"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if(obj is Processor)
				return Processor.StaticEquals(this, (Processor)obj);

			return false;
		}
		/// <summary>Returns a unique 32-bit identifier for this object based on it's exposed properties</summary>
		/// <returns></returns>
		/// <see cref="Object.GetHashCode"/>
		public override int GetHashCode()
		{
			const int kInstructionSetShift = 16;
			const int kProcessorSizeShift = 8;
			const int kByteOrderShift = 0;

			uint code = (
					(((uint)mInstructionSet) & 0xFF) << kInstructionSetShift |
					(((uint)mProcessorSize) & 0xFF) << kProcessorSizeShift |
					(((uint)mByteOrder) & 0xFF) << kByteOrderShift
				);

			return unchecked((int)code);
		}
		/// <summary>Returns a string representation of this object</summary>
		/// <returns>"[InstructionSet\tProcessorSize\tByteOrder]"</returns>
		public override string ToString()
		{
			Contract.Ensures(Contract.Result<string>() != null);

			return string.Format("[{0}\t{1}\t{2}]", 
				mInstructionSet.ToString(), 
				mProcessorSize.ToString(), 
				mByteOrder.ToString()
				);
		}
		#endregion

		#region IComparer<Processor> Members
		/// <summary>See <see cref="IComparer{T}.Compare"/></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(Processor x, Processor y)
		{
			return Processor.StaticCompare(x, y);
		}
		/// <summary>See <see cref="IComparer{T}.Compare"/></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		int System.Collections.IComparer.Compare(object x, object y)
		{
			Processor _x; Debug.TypeCheck.CastValue(x, out _x);
			Processor _y; Debug.TypeCheck.CastValue(y, out _y);

			return Processor.StaticCompare(_x, _y);
		}
		#endregion

		#region IComparable<Processor> Members
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(Processor other)
		{
			return Processor.StaticCompare(this, other);
		}
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		int IComparable.CompareTo(object obj)
		{
			Processor _obj; Debug.TypeCheck.CastValue(obj, out _obj);

			return Processor.StaticCompare(this, _obj);
		}
		#endregion

		#region IEquatable<Processor> Members
		/// <summary>See <see cref="IEquatable{T}.Equals"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(Processor other)
		{
			return Processor.Equals(this, other);
		}
		#endregion

		/// <summary></summary>
		/// <param name="encoder"></param>
		/// <remarks>6 bits</remarks>
		public void HandleEncode(ref Bitwise.HandleBitEncoder encoder)
		{
			encoder.Encode32(mInstructionSet, TypeExtensions.BitEncoders.InstructionSet);
			encoder.Encode32(mProcessorSize, TypeExtensions.BitEncoders.ProcessorSize);
			encoder.Encode32(mByteOrder, TypeExtensions.BitEncoders.EndianFormat);
		}

		public void HandleDecode(ref Bitwise.HandleBitEncoder encoder)
		{
			encoder.Decode32(out mInstructionSet, TypeExtensions.BitEncoders.InstructionSet);
			encoder.Decode32(out mProcessorSize, TypeExtensions.BitEncoders.ProcessorSize);
			encoder.Decode32(out mByteOrder, TypeExtensions.BitEncoders.EndianFormat);
		}


		#region Util
		static bool StaticEquals(Processor lhs, Processor rhs)
		{
			return lhs.mInstructionSet == rhs.mInstructionSet &&
				lhs.mProcessorSize == rhs.mProcessorSize &&
				lhs.mByteOrder == rhs.mByteOrder;
		}

		static int StaticCompare(Processor lhs, Processor rhs)
		{
			int lhs_data = (int)lhs.mInstructionSet, rhs_data = (int)rhs.mInstructionSet;
			int result = lhs_data - rhs_data;

			if (result == 0)
			{
				lhs_data = (int)lhs.mProcessorSize; rhs_data = (int)rhs.mProcessorSize;
				result = lhs_data - rhs_data;

				if (result == 0)
				{
					lhs_data = (int)lhs.mByteOrder; rhs_data = (int)rhs.mByteOrder;
					result = lhs_data - rhs_data;
				}
			}

			return result;
		}
		#endregion

		#region Intel
		static readonly Processor kIntelx86 = new Processor(ProcessorSize.x32, EndianFormat.Little, InstructionSet.Intel);
		/// <summary>Intel's x86 processor definition</summary>
		public static Processor Intelx86		{ get { return kIntelx86; } }

		static readonly Processor kIntelx64 = new Processor(ProcessorSize.x64, EndianFormat.Little, InstructionSet.Intel);
		/// <summary>Intel's x64 processor definition</summary>
		public static Processor Intelx64		{ get { return kIntelx64; } }
		#endregion

		#region PowerPc
		static readonly Processor kPowerPc32 = new Processor(ProcessorSize.x32, EndianFormat.Big, InstructionSet.PPC);
		/// <summary>IBM's PowerPC 32-bit processor definition</summary>
		public static Processor PowerPc32		{ get { return kPowerPc32; } }

		static readonly Processor kPowerPc64 = new Processor(ProcessorSize.x64, EndianFormat.Big, InstructionSet.PPC);
		/// <summary>IBM's PowerPC 64-bit processor definition</summary>
		public static Processor PowerPc64		{ get { return kPowerPc64; } }

		static readonly Processor kPowerPcXenon = new Processor(ProcessorSize.x32, EndianFormat.Big, InstructionSet.PPC);
		/// <summary>IBM's PowerPC (Xenon) processor definition</summary>
		/// <remarks>
		/// Why is there a special Xenon definition? Because if I recall correctly, the Xenon is a 64-bit processor however 
		/// its instruction set is treated as if it were 32-bit. So we may adjust the definition system later on for these 
		/// types of processor cases.
		/// </remarks>
		public static Processor PowerPcXenon	{ get { return kPowerPcXenon; } }
		#endregion
	};
}