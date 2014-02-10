using System;
using System.Runtime.InteropServices;

namespace KSoft.Bitwise
{
	/// <summary>Unionized value of a UInt32 and a Single</summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct SingleUnion
	{
		[FieldOffset(0)] public uint Integer;
		[FieldOffset(0)] public float Real;

		public SingleUnion(uint integer)
		{
			this.Real = 0;
			this.Integer = integer;
		}
		public SingleUnion(float real)
		{
			this.Integer = 0;
			this.Real = real;
		}
	};
	/// <summary>Unionized value of a UInt64 and a Double</summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct DoubleUnion
	{
		[FieldOffset(0)] public ulong Integer;
		[FieldOffset(0)] public double Real;

		public DoubleUnion(ulong integer)
		{
			this.Real = 0;
			this.Integer = integer;
		}
		public DoubleUnion(double real)
		{
			this.Integer = 0;
			this.Real = real;
		}
	};
}