using System;

namespace KSoft.Memory.Strings
{
	/// <summary>The types of storages supported for string characters</summary>
	public enum StringStorageWidthType : byte
	{
		/// <summary>String character storage is single byte based</summary>
		Ascii,
		/// <summary>String character storage is UTF-16</summary>
		/// <remarks>http://en.wikipedia.org/wiki/UTF-16/UCS-2</remarks>
		Unicode,

		/// <summary>UTF-7. Not fully tested.</summary>
		/// <remarks>http://en.wikipedia.org/wiki/UTF-7</remarks>
		UTF7,
		/// <summary>UTF-8. Not fully tested.</summary>
		/// <remarks>http://en.wikipedia.org/wiki/UTF-8</remarks>
		UTF8,
		/// <summary>UTF-32 (32-bit code points). Not fully tested.</summary>
		/// <remarks>http://en.wikipedia.org/wiki/UTF32</remarks>
		UTF32,

		/// <remarks>3 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)]
		kNumberOf,
	};

    /// <summary>The storage method types supported for string data</summary>
	public enum StringStorageType : byte
	{
		/// <summary>Zero-terminated character string</summary>
		CString,
		/// <summary>Character string with a length pre-fix header</summary>
		/// <remarks>This type will never acknowledge fixed length status</remarks>
		Pascal,
		/// <summary>Standard array of characters, nothing more</summary>
		CharArray,

		/// <remarks>2 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)]
		kNumberOf,
	};

	public enum StringStorageLengthPrefix : byte
	{
		/// <remarks>This is what the CLR uses</remarks>
		/// <see cref="Encoded7BitInt"/>
		Int7,

		/// <summary>1-byte length prefix</summary>
		Int8,
		/// <summary>2-byte length prefix</summary>
		Int16,
		/// <summary>4-byte length prefix</summary>
		Int32,

		/// <remarks>2 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)]
		kNumberOf,

		/// <summary>Convenience member to save a bit. You shouldn't need to use this unless your internal code</summary>
		None = Int7,
	};
}