using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using Interop = System.Runtime.InteropServices;

namespace KSoft.Shell
{
	using BitEncoders = TypeExtensions.BitEncoders;

	/// <summary>Represents a processor definition</summary>
	[Collections.InitializeValueTypeComparer(typeof(Processor))]
	[Collections.InitializeValueTypeEqualityComparer(typeof(Processor))]
	[Interop.StructLayout(Interop.LayoutKind.Explicit)]
	[System.Diagnostics.DebuggerDisplay("IA = {InstructionSet}, WordSize = {ProcessorSize}, Endian = {ByteOrder}")]
	public struct Processor : 
		IComparer<Processor>, System.Collections.IComparer,
		IComparable<Processor>, IComparable,
		IEquatable<Processor>
	{
		#region Constants
		// nesting these into a static class makes them run before the struct's static ctor...
		// which, being a value type cctor, may not run when we want it
		static class Constants
		{
			public static readonly int kByteOrderShift = 0;
			public static readonly uint kByteOrderMask = BitEncoders.EndianFormat.BitmaskTrait;

			public static readonly int kProcessorSizeShift = kByteOrderShift + BitEncoders.EndianFormat.BitCountTrait;
			public static readonly uint kProcessorSizeMask = BitEncoders.ProcessorSize.BitmaskTrait;

			public static readonly int kInstructionSetShift = kProcessorSizeShift + BitEncoders.ProcessorSize.BitCountTrait;
			public static readonly uint kInstructionSetMask = BitEncoders.InstructionSet.BitmaskTrait;

			public static readonly int kBitCount = kInstructionSetShift + BitEncoders.InstructionSet.BitCountTrait;
			public static readonly uint kBitmask = Bits.BitCountToMask32(kBitCount);
		};

		/// <summary>Number of bits required to represent a bit-encoded representation of this value type</summary>
		/// <remarks>6 bits at last count</remarks>
		public static int BitCount { get { return Constants.kBitCount; } }
		public static uint Bitmask { get { return Constants.kBitmask; } }
		#endregion

		#region Internal Value
		[Interop.FieldOffset(0)] readonly uint mHandle;

		internal uint Handle { get { return mHandle; } }

		static void InitializeHandle(out uint handle,
			InstructionSet instSet, ProcessorSize procSize, EndianFormat byteOrder)
		{
			var encoder = new Bitwise.HandleBitEncoder();
			encoder.Encode32(byteOrder, BitEncoders.EndianFormat);
			encoder.Encode32(procSize, BitEncoders.ProcessorSize);
			encoder.Encode32(instSet, BitEncoders.InstructionSet);

			Contract.Assert(encoder.UsedBitCount == Processor.BitCount);

			handle = encoder.GetHandle32();
		}
		#endregion

		/// <summary>Construct a processor definition</summary>
		/// <param name="size">Instruction size of the processor</param>
		/// <param name="byteOrder">Default byte ordering of the processor</param>
		/// <param name="instructionSet">Instruction set of the processor</param>
		public Processor(ProcessorSize size, EndianFormat byteOrder, InstructionSet instructionSet)
		{
			InitializeHandle(out mHandle, instructionSet, size, byteOrder);
		}
		internal Processor(uint handle, int startBitIndex)
		{
			handle >>= startBitIndex;
			handle &= Constants.kBitmask;

			mHandle = handle;
		}

		#region Value properties
		/// <summary>The processor's instruction set</summary>
		public InstructionSet InstructionSet { get {
			return BitEncoders.InstructionSet.BitDecode(mHandle, Constants.kInstructionSetShift);
		} }
		/// <summary>The processor's instruction size</summary>
		public ProcessorSize ProcessorSize { get {
			return BitEncoders.ProcessorSize.BitDecode(mHandle, Constants.kProcessorSizeShift);
		} }
		/// <summary>The processor's byte ordering</summary>
		public EndianFormat ByteOrder { get {
			return BitEncoders.EndianFormat.BitDecode(mHandle, Constants.kByteOrderShift);
		} }
		#endregion

		#region Overrides
		/// <summary>See <see cref="Object.Equals"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if(obj is Processor)
				return this.mHandle == ((Processor)obj).mHandle;

			return false;
		}
		/// <summary>Returns a unique 32-bit identifier for this object based on its exposed properties</summary>
		/// <returns></returns>
		/// <see cref="Object.GetHashCode"/>
		public override int GetHashCode()
		{
			return (int)mHandle;
		}
		/// <summary>Returns a string representation of this object</summary>
		/// <returns>"[InstructionSet\tProcessorSize\tByteOrder]"</returns>
		public override string ToString()
		{
			Contract.Ensures(Contract.Result<string>() != null);

			return string.Format("[{0}\t{1}\t{2}]", 
				InstructionSet.ToString(), 
				ProcessorSize.ToString(), 
				ByteOrder.ToString()
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
			return this.mHandle == other.mHandle;
		}
		#endregion


		#region Util
		static int StaticCompare(Processor lhs, Processor rhs)
		{
			Contract.Assert(Processor.BitCount < Bits.kInt32BitCount,
				"Handle bits needs to be <= 31 (ie, sans sign bit) in order for this implementation of CompareTo to reasonably work");

			int lhs_data = (int)lhs.mHandle;
			int rhs_data = (int)rhs.mHandle;
			int result = lhs_data - rhs_data;

			return result;
		}
		#endregion

		static readonly Processor kUndefined = new Processor(uint.MaxValue, 0);
		/// <summary>Undefined processor definition</summary>
		/// <remarks>Only use for comparison operations, don't query value properties. Results will be...undefined</remarks>
		public static Processor Undefined		{ get { return kUndefined; } }

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