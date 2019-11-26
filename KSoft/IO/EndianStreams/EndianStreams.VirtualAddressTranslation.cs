﻿
namespace KSoft.IO
{
	partial class EndianReader
	{
		#region Read Pointer
		/// <summary>Read and translate a VA from the stream into a physical address</summary>
		/// <returns>Virtual address from the stream in physical address form</returns>
		public Values.PtrHandle ReadVirtualAddress()
		{
			VerifyVAT();

			return mVAT.ReadVirtualAsPhysicalAddress(this);
		}
		/// <summary>Read and translate a VA from the stream into a physical address</summary>
		/// <param name="physicalAddress">Virtual address from the stream in physical address form</param>
		public void ReadVirtualAddress(out Values.PtrHandle physicalAddress)
		{
			VerifyVAT();

			physicalAddress = mVAT.ReadVirtualAsPhysicalAddress(this);
		}
		#endregion
	};

	partial class EndianWriter
	{
		/// <summary>Mark a position as a VA</summary>
		/// <param name="ptrSize">Size of the mark (all zeros) to write</param>
		/// <returns>Position of the stream before the 'mark' was written</returns>
		/// <remarks>Up to caller to write VA value later</remarks>
		public Values.PtrHandle MarkVirtualAddress(Shell.ProcessorSize ptrSize)
		{
			var va = PositionPtr;

			switch (ptrSize)
			{
				case Shell.ProcessorSize.x32: Write(uint.MinValue); break;
				case Shell.ProcessorSize.x64: Write(ulong.MinValue); break;

				default:
					throw new Debug.UnreachableException(ptrSize.ToString());
			}

			return va;
		}
		/// <summary>Mark a position as a 32-bit VA</summary>
		/// <returns>Position of the stream before the 'mark' was written</returns>
		/// <remarks>Up to caller to write VA value later</remarks>
		public Values.PtrHandle MarkVirtualAddress32() { return MarkVirtualAddress(Shell.ProcessorSize.x32); }
		/// <summary>Mark a position as a 64-bit VA</summary>
		/// <returns>Position of the stream before the 'mark' was written</returns>
		/// <remarks>Up to caller to write VA value later</remarks>
		public Values.PtrHandle MarkVirtualAddress64() { return MarkVirtualAddress(Shell.ProcessorSize.x64); }

		#region Write Pointer
		/// <summary>Write a physical address, translating it into a VA first, to the stream</summary>
		/// <param name="physicalAddress">Physical address to be translated into virtual address</param>
		public void WriteVirtualAddress(Values.PtrHandle physicalAddress)
		{
			VerifyVAT();

			mVAT.WritePhysicalAsVirtualAddress(this, physicalAddress);
		}
		#endregion
	};

	partial class EndianStream
	{
		public void StreamVirtualAddress(ref Values.PtrHandle physicalAddress)
		{
				 if (IsReading) Reader.ReadVirtualAddress(out physicalAddress);
			else if (IsWriting) Writer.WriteVirtualAddress(physicalAddress);
		}
	};
}