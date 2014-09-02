﻿using System;
using System.Collections.Generic;
using System.Text;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using Expr = System.Linq.Expressions.Expression;
using Interop = System.Runtime.InteropServices;

namespace KSoft.Values
{
	// http://www.ietf.org/rfc/rfc4122.txt
	// useful reference: http://grepcode.com/file/repository.grepcode.com/java/root/jdk/openjdk/6-b14/java/util/UUID.java

	public enum UuidVersion
	{
		TimeBased,
		/// <summary>DCE Security, with embedded POSIX UIDs</summary>
		DCE,
		/// <summary>Name-based, with MD5</summary>
		NameBasedMd5,
		/// <summary>(Pseudo-)Randomly generated</summary>
		Random,
		/// <summary>Name-based, with SHA1</summary>
		NameBasedSha1,

		/// <remarks>4 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	public enum UuidVariant
	{
		/// <summary>Network Computing System backward compatibility</summary>
		NCS,
		/// <summary>Leach-Salz</summary>
		Standard,
		/// <summary>GUID; Microsoft Component Object Model backward compatibility</summary>
		Microsoft,
		/// <summary>Reserved for future definition</summary>
		Reserved,

		/// <remarks>3 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	[Interop.StructLayout(Interop.LayoutKind.Explicit, Size=KGuid.kSizeOf)]
	[Interop.ComVisible(true)]
	[Serializable]
	public struct KGuid : IO.IEndianStreamable,
		IComparable, IComparable<KGuid>, IComparable<Guid>,
		System.Collections.IComparer, IComparer<KGuid>,
		IEquatable<KGuid>, IEqualityComparer<KGuid>, IEquatable<Guid>
	{
		#region Constants
		public const int kSizeOf = sizeof(int) + (sizeof(short) * 2) + (sizeof(byte) * 8);

		const int kVersionBitCount = 4;
		const int kVersionBitShift = Bits.kInt16BitCount - kVersionBitCount;

		const int kVariantBitCount = 3;
		const int kVariantBitShift = Bits.kByteBitCount - kVariantBitCount;

		/// <summary>
		/// Guid format is 32 digits: 00000000000000000000000000000000
		/// </summary>
		public const string kFormatNoStyle = "N";
		/// <summary>
		/// Guid format is 32 digits separated by hyphens: 00000000-0000-0000-0000-000000000000
		/// </summary>
		public const string kFormatHyphenated = "D";
		#endregion

		#region Guid Accessors
		// nesting these into a static class makes them run before the struct's static ctor...
		// which, being a value type cctor, may not run when we want it
		/// <summary><see cref="System.Guid"/> internal accessors</summary>
		static class SysGuid
		{
			const string kData1Name = "_a";
			public static readonly Func<Guid, int> GetData1;
			public static readonly Reflection.Util.ValueTypeMemberSetterDelegate<Guid, int> SetData1;

			const string kData2Name = "_b";
			public static readonly Func<Guid, short> GetData2;
			public static readonly Reflection.Util.ValueTypeMemberSetterDelegate<Guid, short> SetData2;

			const string kData3Name = "_c";
			public static readonly Func<Guid, short> GetData3;
			public static readonly Reflection.Util.ValueTypeMemberSetterDelegate<Guid, short> SetData3;

			public static readonly Func<Guid, byte>[] GetData4;
			public static readonly Reflection.Util.ValueTypeMemberSetterDelegate<Guid, byte>[] SetData4;

			static SysGuid()
			{
				GetData1 = Reflection.Util.GenerateMemberGetter			<Guid, int>		(kData1Name);
				SetData1 = Reflection.Util.GenerateValueTypeMemberSetter<Guid, int>		(kData1Name);
				GetData2 = Reflection.Util.GenerateMemberGetter			<Guid, short>	(kData2Name);
				SetData2 = Reflection.Util.GenerateValueTypeMemberSetter<Guid, short>	(kData2Name);
				GetData3 = Reflection.Util.GenerateMemberGetter			<Guid, short>	(kData3Name);
				SetData3 = Reflection.Util.GenerateValueTypeMemberSetter<Guid, short>	(kData3Name);

				string[] kData4Names = { "_d", "_e", "_f", "_g", "_h", "_i", "_j", "_k", };
				GetData4 = new Func<Guid, byte>[kData4Names.Length];
				SetData4 = new Reflection.Util.ValueTypeMemberSetterDelegate<Guid, byte>[kData4Names.Length];

				for (int x = 0; x < kData4Names.Length; x++)
				{
					GetData4[x] = Reflection.Util.GenerateMemberGetter			<Guid, byte>(kData4Names[x]);
					SetData4[x] = Reflection.Util.GenerateValueTypeMemberSetter	<Guid, byte>(kData4Names[x]);
				}
			}
		};

		public int Data1 { get { return SysGuid.GetData1(mData); } }
		public int Data2 { get { return SysGuid.GetData2(mData); } }
		public int Data3 { get { return SysGuid.GetData3(mData); } }
		public long Data4 { get {
			byte	d = SysGuid.GetData4[0](mData), e = SysGuid.GetData4[1](mData),
					f = SysGuid.GetData4[2](mData), g = SysGuid.GetData4[3](mData), 
					h = SysGuid.GetData4[4](mData), i = SysGuid.GetData4[5](mData),
					j = SysGuid.GetData4[6](mData), k = SysGuid.GetData4[7](mData);
			long result;

			result =  d; result <<= Bits.kByteBitCount;
			result |= e; result <<= Bits.kByteBitCount;
			result |= f; result <<= Bits.kByteBitCount;
			result |= g; result <<= Bits.kByteBitCount;
			result |= h; result <<= Bits.kByteBitCount;
			result |= i; result <<= Bits.kByteBitCount;
			result |= j; result <<= Bits.kByteBitCount;
			result = k;

			return result;
		} }
		#endregion

		[Interop.FieldOffset(0)] Guid mData;
		[Interop.FieldOffset(0)] ulong mDataHi;
		[Interop.FieldOffset(8)] ulong mDataLo;

		public Guid ToGuid() { return mData; }

		public long MostSignificantBits { get {
			long result = SysGuid.GetData1(mData);
			result <<= Bits.kInt32BitCount;

			result <<= Bits.kInt16BitCount;
			result |= (uint)SysGuid.GetData2(mData);

			result <<= Bits.kInt16BitCount;
			result |= (uint)SysGuid.GetData3(mData);

			return (long)result;
		} }
		public long LeastSignificantBits { get {
			long result = Data4;

			return result;
		} }

		#region Version and Variant
		public UuidVersion Version { get {
			return (UuidVersion)(SysGuid.GetData3(mData) >> kVersionBitShift);
		} }

		public UuidVariant Variant { get {
			int raw = SysGuid.GetData4[0](mData) >> kVariantBitShift;

			// Special condition due to the 'type' bits starting in the right-most (ie, MSB) bits,
			// plus for NCS and Standard the lower two bits are documented in RFC as being 'don't care'
			if ((raw >> 2) == 0)
				return UuidVariant.NCS;
			else if ((raw >> 2) == 1)
				return UuidVariant.Standard;
			else if (raw == 6)
				return UuidVariant.Microsoft;
			else // raw == 7
				return UuidVariant.Reserved;
		} }
		#endregion

		#region TimeBased properties
		public long Timestamp { get {
			Contract.Requires<InvalidOperationException>(Version == UuidVersion.TimeBased, 
				"Tried to get the Timestamp of a non-time-based GUID");

			ulong msb = (ulong)MostSignificantBits;
			ulong result = (msb & 0xFFF) << 48;
			result |= ((msb >> 16) & 0xFFFF) << 32;
			result |= msb >> 32;

			return (long)result;
		} }

		public int ClockSequence { get {
			Contract.Requires<InvalidOperationException>(Version == UuidVersion.TimeBased, 
				"Tried to get the ClockSequence of a non-time-based GUID");

			// NOTE: While the Variant field is 3-bits, both the Java and RFC implementations
			// seem to lob the two MSB off, instead of 0x1F
			int hi = SysGuid.GetData4[0](mData) & 0x3F;
			int lo = SysGuid.GetData4[1](mData);

			return (hi << 8) | lo;
		} }

		public long Node { get {
			Contract.Requires<InvalidOperationException>(Version == UuidVersion.TimeBased, 
				"Tried to get the Node of a non-time-based GUID");

			long result = 0;

			for (int x = 2; x < SysGuid.GetData4.Length; x++, result <<= Bits.kByteBitCount)
				result |= SysGuid.GetData4[x](mData);

			return result;
		} }
		#endregion

		#region Ctor
		public KGuid(Guid guid)	{ mDataHi=mDataLo=0; mData = guid; }
		public KGuid(byte[] b)	{ mDataHi=mDataLo=0; mData = new Guid(b); }
		public KGuid(string g)	{ mDataHi=mDataLo=0; mData = new Guid(g); }
		public KGuid(long msb, long lsb)
		{
			mData = Guid.Empty;
			mDataHi = (ulong)msb;
			mDataLo = (ulong)lsb;
		}

		public KGuid(int a, short b, short c, byte[] d)
		{
			mDataHi=mDataLo=0;
			mData = new Guid(a,b,c,d);
		}
		public KGuid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
		{
			mDataHi=mDataLo=0;
			mData = new Guid(a,b,c,d,e,f,g,h,i,j,k);
		}
		public KGuid(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
		{
			mDataHi=mDataLo=0;
			mData = new Guid(a,b,c,d,e,f,g,h,i,j,k);
		}
		#endregion

		#region ToString
		/// <see cref="Guid.ToString()"/>
		public override string ToString()								{ return mData.ToString(); }
		/// <see cref="Guid.ToString(string)"/>
		public string ToString(string format)							{ return mData.ToString(format); }
		/// <see cref="Guid.ToString(string, IFormatProvider)"/>
		public string ToString(string format, IFormatProvider provider)	{ return mData.ToString(format, provider); }

		/// <summary>
		/// 32 digits: 00000000000000000000000000000000
		/// </summary>
		/// <returns></returns>
		internal string ToStringNoStyle()								{ return mData.ToString(kFormatNoStyle); }
		/// <summary>
		/// 32 digits separated by hyphens: 00000000-0000-0000-0000-000000000000
		/// </summary>
		/// <returns></returns>
		internal string ToStringHyphenated()							{ return mData.ToString(kFormatHyphenated); }
		#endregion

		#region IEndianStreamable Members
		public void Read(IO.EndianReader s)
		{
			SysGuid.SetData1(ref mData, s.ReadInt32());
			SysGuid.SetData2(ref mData, s.ReadInt16());
			SysGuid.SetData3(ref mData, s.ReadInt16());

			foreach (var data4 in SysGuid.SetData4)
				data4(ref mData, s.ReadByte());
		}

		public void Write(IO.EndianWriter s)
		{
			int data1 = SysGuid.GetData1(mData);
			short data2 = SysGuid.GetData2(mData);
			short data3 = SysGuid.GetData3(mData);

			s.Write(data1);
			s.Write(data2);
			s.Write(data3);

			foreach (var data4 in SysGuid.GetData4)
				s.Write(data4(mData));
		}
		#endregion

		#region IComparable Members
		public int CompareTo(object obj)
		{
				 if (obj == null)	return 1;
			else if (obj is KGuid)	return CompareTo((KGuid)obj);
			else if (obj is Guid)	return CompareTo((Guid)obj);

			throw new InvalidCastException(obj.GetType().ToString());
		}
		public int CompareTo(KGuid other)	{ return mData.CompareTo(other.mData); }
		public int CompareTo(Guid other)	{ return mData.CompareTo(other); }
		#endregion

		#region IEquatable Members
		public override bool Equals(object obj)
		{
				 if (obj is KGuid)	return Equals(this, (KGuid)obj);
			else if (obj is Guid)	return Equals((Guid)obj);

			return false;
		}

		public bool Equals(KGuid x, KGuid y)
		{
			// We don't compare using mData. System.Guid's Equals implementation compares each individual A...K field
			
			return
				x.mDataHi == y.mDataHi &&
				x.mDataLo == y.mDataLo;
		}
		public bool Equals(KGuid other)			{ return Equals(this, other); }
		public bool Equals(Guid other)			{ return mData == other; }

		public override int GetHashCode()	{ return mData.GetHashCode(); }
		public int GetHashCode(KGuid obj)	{ return obj.GetHashCode(); }

		public static bool operator ==(KGuid a, KGuid b)
		{
			// We don't compare using mData. System.Guid's Equals implementation compares each individual A...K field
			
			return
				a.mDataHi == b.mDataHi &&
				a.mDataLo == b.mDataLo;
		}
		public static bool operator !=(KGuid a, KGuid b)
		{
			return !(a == b);
		}
		#endregion

		#region IComparer<KGuid> Members
		int System.Collections.IComparer.Compare(object x, object y)
		{
			if (x == y)		return 0;
			if (x == null)	return -1;
			if (y == null)	return 1;

			if (x is KGuid)
			{
				if (y is KGuid)
					return ((KGuid)x).CompareTo((KGuid)y);
				if (y is Guid)
					return ((KGuid)x).CompareTo((Guid)y);
			}
			else if(x is Guid && y is KGuid)
				return -((KGuid)y).CompareTo((Guid)x);

			throw new InvalidCastException(x.GetType().ToString());
		}		

		public int Compare(KGuid x, KGuid y)
		{
			return x.CompareTo(y);
		}
		#endregion

		public static readonly KGuid Empty = new KGuid();

		public static KGuid NewGuid()
		{
			return new KGuid(Guid.NewGuid());
		}

		#region Parse
		public static KGuid Parse(string input)						{ return new KGuid(Guid.Parse(input)); }
		public static KGuid ParseExact(string input, string format)	{ return new KGuid(Guid.ParseExact(input, format)); }
		/// <summary>
		/// 32 digits: 00000000000000000000000000000000
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		internal static KGuid ParseExactNoStyle(string input)		{ return new KGuid(Guid.ParseExact(input, kFormatNoStyle)); }
		/// <summary>
		/// 32 digits separated by hyphens: 00000000-0000-0000-0000-000000000000
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		internal static KGuid ParseExactHyphenated(string input)	{ return new KGuid(Guid.ParseExact(input, kFormatHyphenated)); }

		public static bool TryParse(string input, out KGuid result)
		{
			Guid guid;
			if (Guid.TryParse(input, out guid))
			{
				result = new KGuid(guid);
				return true;
			}

			result = Empty;
			return false;
		}
		public static bool TryParseExact(string input, string format, out KGuid result)
		{
			Guid guid;
			if (Guid.TryParseExact(input, format, out guid))
			{
				result = new KGuid(guid);
				return true;
			}

			result = Empty;
			return false;
		}
		internal static bool TryParseExactNoStyle(string input, out KGuid result)
		{
			return TryParseExact(input, kFormatNoStyle, out result);
		}
		internal static bool TryParseExactHyphenated(string input, out KGuid result)
		{
			return TryParseExact(input, kFormatHyphenated, out result);
		}
		#endregion

		#region Byte Utils
		public byte[] ToByteArray()	{ return mData.ToByteArray(); }

		public void ToByteBuffer(byte[] buffer, int index = 0)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>((index+kSizeOf) <= buffer.Length);

			Bitwise.ByteSwap.ReplaceBytes(buffer, index, SysGuid.GetData1(mData)); index += sizeof(int);
			Bitwise.ByteSwap.ReplaceBytes(buffer, index, SysGuid.GetData2(mData)); index += sizeof(short);
			Bitwise.ByteSwap.ReplaceBytes(buffer, index, SysGuid.GetData3(mData)); index += sizeof(short);
			for (int x = 0; x < 8; x++, index++)
				buffer[x] = SysGuid.GetData4[x](mData);
		}
		#endregion
	};
}