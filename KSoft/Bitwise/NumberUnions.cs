using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace KSoft.Bitwise
{
	/// <summary>Unionized value of a UInt32 and a Single</summary>
	[StructLayout(LayoutKind.Explicit)]
	[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct SingleUnion
	{
		[SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		[FieldOffset(0)] public uint Integer;
		[FieldOffset(0)] public float Real;

		public SingleUnion(uint unsignedInteger)
		{
			this.Real = 0;
			this.Integer = unsignedInteger;
		}
		public SingleUnion(float real)
		{
			this.Integer = 0;
			this.Real = real;
		}
	};
	/// <summary>Unionized value of a UInt64 and a Double</summary>
	[StructLayout(LayoutKind.Explicit)]
	[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct DoubleUnion
	{
		[SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		[FieldOffset(0)] public ulong Integer;
		[FieldOffset(0)] public double Real;

		public DoubleUnion(ulong unsignedInteger)
		{
			this.Real = 0;
			this.Integer = unsignedInteger;
		}
		public DoubleUnion(double real)
		{
			this.Integer = 0;
			this.Real = real;
		}
	};

	/// <summary>Unionized value of a UInt64 and a UInt32</summary>
	[StructLayout(LayoutKind.Explicit)]
	[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct IntegerUnion
	{
		[FieldOffset(0)] public ulong u64;
		[FieldOffset(0)] public uint u32;

		IntegerUnion(ulong x64) : this()
		{
			this.u64 = x64;
		}
		IntegerUnion(uint x32) : this()
		{
			this.u32 = x32;
		}

		public static IntegerUnion FromUInt32(uint x32)
		{
			return new IntegerUnion(x32);
		}
		public static IntegerUnion FromUInt64(ulong x64)
		{
			return new IntegerUnion(x64);
		}
	};
}
