using System;
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

	/// <remarks>Caveat emptor: has a static ctor</remarks>
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
		#endregion

		#region Guid Accessors
		static readonly Func<Guid, int> kGetData1;
		static readonly Reflection.Util.ValueTypeMemberSetterDelegate<Guid, int> kSetData1;
		static readonly Func<Guid, short> kGetData2;
		static readonly Reflection.Util.ValueTypeMemberSetterDelegate<Guid, short> kSetData2;
		static readonly Func<Guid, short> kGetData3;
		static readonly Reflection.Util.ValueTypeMemberSetterDelegate<Guid, short> kSetData3;
		static readonly Func<Guid, byte>[] kGetData4;
		static readonly Reflection.Util.ValueTypeMemberSetterDelegate<Guid, byte>[] kSetData4;

		static KGuid()
		{
			const string	kData1Name = "_a", 
							kData2Name = "_b",
							kData3Name = "_c";

			kGetData1 = Reflection.Util.GenerateMemberGetter			<Guid, int>		(kData1Name);
			kSetData1 = Reflection.Util.GenerateValueTypeMemberSetter	<Guid, int>		(kData1Name);
			kGetData2 = Reflection.Util.GenerateMemberGetter			<Guid, short>	(kData2Name);
			kSetData2 = Reflection.Util.GenerateValueTypeMemberSetter	<Guid, short>	(kData2Name);
			kGetData3 = Reflection.Util.GenerateMemberGetter			<Guid, short>	(kData3Name);
			kSetData3 = Reflection.Util.GenerateValueTypeMemberSetter	<Guid, short>	(kData3Name);

			string[] kData4Names = { "_d", "_e", "_f", "_g", "_h", "_i", "_j", "_k", };
			kGetData4 = new Func<Guid, byte>[kData4Names.Length];
			kSetData4 = new Reflection.Util.ValueTypeMemberSetterDelegate<Guid, byte>[kData4Names.Length];

			for (int x = 0; x < kData4Names.Length; x++)
			{
				kGetData4[x] = Reflection.Util.GenerateMemberGetter			<Guid, byte>(kData4Names[x]);
				kSetData4[x] = Reflection.Util.GenerateValueTypeMemberSetter<Guid, byte>(kData4Names[x]);
			}
		}

		public int Data1 { get { return kGetData1(mData); } }
		public int Data2 { get { return kGetData2(mData); } }
		public int Data3 { get { return kGetData3(mData); } }
		public long Data4 { get {
			byte	d = kGetData4[0](mData), e = kGetData4[1](mData), f = kGetData4[2](mData), g = kGetData4[3](mData), 
					h = kGetData4[4](mData), i = kGetData4[5](mData), j = kGetData4[6](mData), k = kGetData4[7](mData);
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
			long result = kGetData1(mData);
			result <<= Bits.kInt32BitCount;

			result <<= Bits.kInt16BitCount;
			result |= (uint)kGetData2(mData);

			result <<= Bits.kInt16BitCount;
			result |= (uint)kGetData3(mData);

			return (long)result;
		} }
		public long LeastSignificantBits { get {
			long result = Data4;

			return result;
		} }

		#region Version and Variant
		public UuidVersion Version { get {
			return (UuidVersion)(kGetData3(mData) >> kVersionBitShift);
		} }

		public UuidVariant Variant { get {
			int raw = kGetData4[0](mData) >> kVariantBitShift;

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
			int hi = kGetData4[0](mData) & 0x3F;
			int lo = kGetData4[1](mData);

			return (hi << 8) | lo;
		} }

		public long Node { get {
			Contract.Requires<InvalidOperationException>(Version == UuidVersion.TimeBased, 
				"Tried to get the Node of a non-time-based GUID");

			long result = 0;

			for (int x = 2; x < kGetData4.Length; x++, result <<= Bits.kByteBitCount)
				result |= kGetData4[x](mData);

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

		internal string ToStringNoStyle()								{ return mData.ToString("N"); }
		internal string ToStringHyphenated()							{ return mData.ToString("D"); }
		#endregion

		#region IEndianStreamable Members
		public void Read(IO.EndianReader s)
		{
			kSetData1(ref mData, s.ReadInt32());
			kSetData2(ref mData, s.ReadInt16());
			kSetData3(ref mData, s.ReadInt16());

			foreach (var data4 in kSetData4)
				data4(ref mData, s.ReadByte());
		}

		public void Write(IO.EndianWriter s)
		{
			int data1 = kGetData1(mData);
			short data2 = kGetData2(mData);
			short data3 = kGetData3(mData);

			s.Write(data1);
			s.Write(data2);
			s.Write(data3);

			foreach (var data4 in kGetData4)
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
		internal static KGuid ParseExactNoStyle(string input)		{ return new KGuid(Guid.ParseExact(input, "N")); }
		/// <summary>
		/// 32 digits separated by hyphens: 00000000-0000-0000-0000-000000000000
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		internal static KGuid ParseExactHyphenated(string input)	{ return new KGuid(Guid.ParseExact(input, "D")); }

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
			return TryParseExact(input, "N", out result);
		}
		internal static bool TryParseExactHyphenated(string input, out KGuid result)
		{
			return TryParseExact(input, "D", out result);
		}
		#endregion

		#region Byte Utils
		public byte[] ToByteArray()	{ return mData.ToByteArray(); }

		public void ToByteBuffer(byte[] buffer, int index = 0)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>((index+kSizeOf) <= buffer.Length);

			Bitwise.ByteSwap.ReplaceBytes(buffer, index, kGetData1(mData)); index += sizeof(int);
			Bitwise.ByteSwap.ReplaceBytes(buffer, index, kGetData2(mData)); index += sizeof(short);
			Bitwise.ByteSwap.ReplaceBytes(buffer, index, kGetData3(mData)); index += sizeof(short);
			for (int x = 0; x < 8; x++, index++)
				buffer[x] = kGetData4[x](mData);
		}
		#endregion
	};
}