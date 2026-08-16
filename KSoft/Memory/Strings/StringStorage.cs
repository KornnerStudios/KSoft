using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Memory.Strings
{
	/// <summary>String storage definition</summary>
	public readonly struct StringStorage : //IO.IEndianStreamable,
		IEquatable<StringStorage>, IEqualityComparer<StringStorage>,
		IComparer<StringStorage>, IComparable<StringStorage>,
		System.Collections.IComparer, IComparable
	{
		#region WidthType
		readonly StringStorageWidthType mWidthType;
		/// <summary>Character serialization width/encoding type</summary>
		public StringStorageWidthType WidthType => mWidthType;
		#endregion

		#region Type
		readonly StringStorageType mType;
		/// <summary>Character serialization format method</summary>
		public StringStorageType Type => mType;
		#endregion

		#region ByteOrder
		readonly Shell.EndianFormat mByteOrder;
		/// <summary>Endian byte order of the character storage</summary>
		/// <remarks>Affects both the wide-characters and any length prefixes written</remarks>
		public Shell.EndianFormat ByteOrder => mByteOrder;
		#endregion

		#region LengthPrefix
		readonly StringStorageLengthPrefix mLengthPrefix;
		/// <summary>Length prefix size</summary>
		public StringStorageLengthPrefix LengthPrefix => mLengthPrefix;

		public bool HasLengthPrefix => mType.UsesLengthPrefix();
		#endregion

		#region FixedLength
		readonly short mFixedLength;
		/// <summary>Fixed string serialization length</summary>
		/// <remarks>Set to '0' when no specified fixed length</remarks>
		public short FixedLength => mFixedLength;

		/// <summary>Does the storage use a fixed length character array</summary>
		/// <remarks>
		/// Ignored in <see cref="StringStorageType.Clr"/> cases
		///
		/// For <see cref="StringStorageType.CharArray"/> cases, the full fixed length
		/// buffer can be used, but for <see cref="StringStorageType.CString"/> cases
		/// the <see cref="FixedLength"/> will be 1 less due to null termination
		/// </remarks>
		public bool IsFixedLength => mFixedLength != 0 && !HasLengthPrefix;
		#endregion

		[Contracts.ContractInvariantMethod]
		readonly void ObjectInvariant()	{ Contract.Invariant(mFixedLength >= 0); }

		#region Ctor
		static void ValidateFixedLengthStorage(StringStorageWidthType widthType, StringStorageType type, short fixedLength)
		{
			if (type.UsesLengthPrefix())
			{
				throw new ArgumentException("Use ctor with StringStorageLengthPrefix instead", nameof(type));
			}

			ArgumentOutOfRangeException.ThrowIfNegative(fixedLength);
			if (fixedLength != 0 && widthType.IsVariableWidth())
			{
				throw new ArgumentException("Can't use a variable width encoding with fixed buffers!", nameof(fixedLength));
			}
		}

		/// <summary>Construct a new string storage definition</summary>
		/// <param name="widthType">Width size of a single character of this string definition</param>
		/// <param name="type">Storage method for this string definition</param>
		/// <param name="byteOrder"></param>
		/// <param name="fixedLength">The storage fixed length (in characters) of this string definition</param>
		public StringStorage(StringStorageWidthType widthType, StringStorageType type,
			Shell.EndianFormat byteOrder = Shell.EndianFormat.Little, short fixedLength = 0)
		{
			ValidateFixedLengthStorage(widthType, type, fixedLength);

			mWidthType = widthType;
			mType = type;
			mByteOrder = byteOrder;
			mLengthPrefix = StringStorageLengthPrefix.None;
			mFixedLength = fixedLength;

			kHashCode = CalculateHashCode(mWidthType, mType, mByteOrder, mLengthPrefix, mFixedLength);
		}
		/// <summary>Construct a new string storage definition (in <see cref="Shell.EndianFormat.Little"/> byte order)</summary>
		/// <param name="widthType">Width size of a single character of this string definition</param>
		/// <param name="type">Storage method for this string definition</param>
		/// <param name="fixedLength">The storage fixed length (in characters) of this string definition</param>
		public StringStorage(StringStorageWidthType widthType, StringStorageType type, short fixedLength) :
			this(widthType, type, Shell.EndianFormat.Little, fixedLength)
		{
		}
		/// <summary>Construct a new Pascal string storage definition</summary>
		/// <param name="widthType">Width size of a single character of this string definition</param>
		/// <param name="prefix">Length prefix size</param>
		/// <param name="byteOrder"></param>
		public StringStorage(StringStorageWidthType widthType, StringStorageLengthPrefix prefix,
			Shell.EndianFormat byteOrder = Shell.EndianFormat.Little)
		{
			mWidthType = widthType;
			mType = StringStorageType.Pascal;
			mByteOrder = byteOrder;
			mLengthPrefix = prefix;
			mFixedLength = 0;

			kHashCode = CalculateHashCode(mWidthType, mType, mByteOrder, mLengthPrefix, mFixedLength);
		}
		#endregion

		#region IEndianStreamable Members
#if false // #TODO
		public void Read(IO.EndianReader s)
		{
			mWidthType = (StringStorageWidthType)s.ReadByte();
			mType = (StringStorageType)s.ReadByte();
			mByteOrder = (Shell.EndianFormat)s.ReadByte();
			s.Seek(sizeof(byte));
			mFixedLength = s.ReadInt16();
			s.Seek(sizeof(ushort));
		}

		public void Write(IO.EndianWriter s)
		{
			s.Write((byte)mWidthType);
			s.Write((byte)mType);
			s.Write((byte)mByteOrder);
			s.Write(byte.MinValue);
			s.Write(mFixedLength);
			s.Write(ushort.MinValue);
		}
#endif
		#endregion

		#region GetHashCode
		static int CalculateHashCode(StringStorageWidthType widthType, StringStorageType type, Shell.EndianFormat byteOrder,
			StringStorageLengthPrefix prefix,
			short fixedLength)
		{
			var encoder = new Bitwise.HandleBitEncoder();
			encoder.Encode32(widthType, TypeExtensions.BitEncoders.StringStorageWidthType);
			encoder.Encode32(type, TypeExtensions.BitEncoders.StringStorageType);
			encoder.Encode32(byteOrder, TypeExtensions.BitEncoders.EndianFormat);

			if (type.UsesLengthPrefix())
			{
				encoder.Encode32(prefix, TypeExtensions.BitEncoders.StringStorageLengthPrefix);
			}
			else if (fixedLength != 0)
			{
				encoder.Encode32((uint)fixedLength, 0x7FFF);
			}

			return (int)encoder.GetHandle32();
		}
		readonly int kHashCode;
		/// <summary>Returns the hash code for this instance</summary>
		/// <returns>All of this definition's fields bit-encoded into an integer</returns>
		public override int GetHashCode()	{ return kHashCode; }
		#endregion

		#region IEquatable<StringStorage> Members
		/// <summary>Compares this to another <see cref="StringStorage"/> object testing their underlying fields for equality</summary>
		/// <param name="obj">other <see cref="StringStorage"/> object</param>
		/// <returns>true if both this object and <paramref name="obj"/> are equal</returns>
		public bool Equals(StringStorage other)					{ return kHashCode == other.kHashCode; }

		public bool Equals(StringStorage x, StringStorage y)	{ return x.kHashCode == y.kHashCode; }

		public int GetHashCode(StringStorage obj)				{ return obj.GetHashCode(); }

		/// <summary>Compares this to another object testing for equality</summary>
		/// <param name="obj"></param>
		/// <returns>
		/// True if both this object and <paramref name="obj"/> are equal.
		/// False if <paramref name="obj"/> is not a <see cref="StringStorage"/></returns>
		public override bool Equals(object obj)
		{
			if (obj is StringStorage s)
			{
				return this.Equals(s);
			}

			return false;
		}
		#endregion

		#region IComparer<StringStorage> Members
		/// <summary>Compare two <see cref="StringStorage"/> objects for similar serializer values</summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		/// <see cref="int CompareTo(StringStorage)"/>
		public int Compare(StringStorage x, StringStorage y) => x.CompareTo(y);
		/// <summary>Compare this with another <see cref="StringStorage"/> object for similar serializer values</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		/// <remarks>
		/// Compares the object fields in the following order:
		/// <see cref="StringStorage.Type"/>,
		/// <see cref="StringStorage.WidthType"/>,
		/// <see cref="StringStorage.ByteOrder"/>,
		/// <see cref="StringStorage.LengthPrefix"/>,
		/// <see cref="StringStorage.FixedLength"/>
		/// </remarks>
		public int CompareTo(StringStorage other)
		{
			if (mType == other.mType)
			{
				if (mWidthType == other.mWidthType)
				{
					if (mByteOrder == other.mByteOrder)
					{
						if (mLengthPrefix == other.mLengthPrefix)
						{
							return mFixedLength - other.mFixedLength;
						}
						else
						{
							return ((int)mLengthPrefix) - ((int)other.mLengthPrefix);
						}
					}
					else
					{
						return ((int)mByteOrder) - ((int)other.mByteOrder);
					}
				}
				else
				{
					return ((int)mWidthType) - ((int)other.mWidthType);
				}
			}
			else
			{
				return ((int)mType) - ((int)other.mType);
			}
		}

		/// <summary></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		/// <see cref="int Compare(StringStorage, StringStorage)"/>
		public int Compare(object x, object y) => Compare((StringStorage)x, (StringStorage)y);
		/// <summary></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		/// <see cref="int CompareTo(StringStorage)"/>
		public int CompareTo(object obj) => CompareTo((StringStorage)obj);
		#endregion

		#region Operators
		public static bool operator ==(StringStorage a, StringStorage b) => a.Equals(b);
		public static bool operator !=(StringStorage a, StringStorage b) => !(a == b);
		#endregion


		#region CString
		static readonly StringStorage kCStringAscii = new(StringStorageWidthType.Ascii, StringStorageType.CString);
		/// <summary>Get a storage definition for a regular CString format ASCII string</summary>
		public static StringStorage CStringAscii { get { return kCStringAscii; } }

		static readonly StringStorage kCStringUTF8 = new(StringStorageWidthType.UTF8, StringStorageType.CString);
		/// <summary>Get a storage definition for a regular CString format ASCII string</summary>
		public static StringStorage CStringUtf8 { get { return kCStringUTF8; } }

		static readonly StringStorage kCStringUnicode = new(StringStorageWidthType.Unicode, StringStorageType.CString);
		/// <summary>Get a storage definition for a CString format Unicode string</summary>
		public static StringStorage CStringUnicode { get { return kCStringUnicode; } }

		static readonly StringStorage kCStringUnicodeBE = new(StringStorageWidthType.Unicode, StringStorageType.CString, Shell.EndianFormat.Big);
		/// <summary>Get a storage definition for a CString format Unicode string (big endian)</summary>
		public static StringStorage CStringUnicodeBigEndian { get { return kCStringUnicodeBE; } }
		#endregion

		#region String
		static readonly StringStorage kStringAscii = new(StringStorageWidthType.Ascii, StringStorageType.CharArray);
		/// <summary>Get a storage definition for a string of ASCII characters</summary>
		/// <remarks>This is a <see cref="StringStorageType.CharArray"/> which doesn't specify a fixed length.</remarks>
		public static StringStorage AsciiString { get { return kStringAscii; } }

		static readonly StringStorage kStringUTF8 = new(StringStorageWidthType.UTF8, StringStorageType.CharArray);
		/// <summary>Get a storage definition for a string of ASCII characters</summary>
		/// <remarks>This is a <see cref="StringStorageType.CharArray"/> which doesn't specify a fixed length.</remarks>
		public static StringStorage Utf8String { get { return kStringUTF8; } }

		static readonly StringStorage kStringUnicode = new(StringStorageWidthType.Unicode, StringStorageType.CharArray);
		/// <summary>Get a storage definition for a string of Unicode characters</summary>
		/// <remarks>This is a <see cref="StringStorageType.CharArray"/> which doesn't specify a fixed length.</remarks>
		public static StringStorage UnicodeString { get { return kStringUnicode; } }

		static readonly StringStorage kStringUnicodeBE = new(StringStorageWidthType.Unicode, StringStorageType.CharArray, Shell.EndianFormat.Big);
		/// <summary>Get a storage definition for a string of Unicode characters (big endian)</summary>
		/// <remarks>This is a <see cref="StringStorageType.CharArray"/> which doesn't specify a fixed length.</remarks>
		public static StringStorage UnicodeStringBigEndian { get { return kStringUnicodeBE; } }
		#endregion

		internal static readonly StringStorage[] kStorageTypesList = [
			// Ascii
			kCStringAscii,
			/* Clr */ new(StringStorageWidthType.Ascii, StringStorageLengthPrefix.Int7),
			kStringAscii,

			// Unicode
			kCStringUnicode,
			/* Clr */ new(StringStorageWidthType.Unicode, StringStorageLengthPrefix.Int7),
			kStringUnicode,

			// UTF8
			kCStringUTF8,
			/* Clr */ new(StringStorageWidthType.UTF8, StringStorageLengthPrefix.Int7),
			kStringUTF8,

			// Unicode-BE
			kCStringUnicodeBE,
			/* Clr */ new(StringStorageWidthType.Unicode, StringStorageLengthPrefix.Int7, Shell.EndianFormat.Big),
			kStringUnicodeBE,
		];
	};
}
