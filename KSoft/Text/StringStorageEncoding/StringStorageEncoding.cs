using System;
using System.Collections.Generic;
using System.Text;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Text
{
	using Memory.Strings;

	/// <summary>Encapsulates an <see cref="Encoding"/> from a <see cref="StringStorage"/> definition</summary>
	/// <remarks>
	/// For <see cref="StringStorageType.CString"/> cases, the encoding does not check if there
	/// is an existing '\0' character in the user supplied strings. If you pass such strings to
	/// the encoding for streaming the results will be undefined. Oh and bad.
	/// </remarks>
	public sealed partial class StringStorageEncoding : System.Text.Encoding,
		IEquatable<StringStorageEncoding>, IEqualityComparer<StringStorageEncoding>,
		IComparer<StringStorageEncoding>, IComparable<StringStorageEncoding>
	{
		Encoding mBaseEncoding;
		#region Storage
		StringStorage mStorage;
		/// <summary>The string storage definition for this encoding</summary>
		public StringStorage Storage { get { return mStorage; } }
		#endregion
		Options mOptions;
		bool DontAlwaysFlush { get { return (mOptions & Options.DontAlwaysFlush) != 0; } }
		/// <summary>Number of bytes a null character consumes</summary>
		int mNullCharacterSize;
		/// <summary>
		/// Number of bytes used to store a fixed length character array using
		/// the <see cref="StringStorageType"/> defined in <see cref="storage"/>
		/// </summary>
		int mFixedLengthByteLength;

		#region Ctor
		/// <summary>Initialize an encoding for this library's methods of String Storages</summary>
		/// <param name="storage">Storage definition</param>
		/// <param name="options"><see cref="System.Text.Encoding"/> options</param>
		public StringStorageEncoding(StringStorage storage, Options options = 0)
		{
			bool use_bom = (options & Options.UseByteOrderMark) != 0;
			bool big_endian = storage.ByteOrder == Shell.EndianFormat.Big;
			bool throw_on_invalid = (options & Options.ThrowOnInvalidBytes) != 0;

			mNullCharacterSize = sizeof(byte); // The majority of encodings only require 1 byte for the null character
			switch (storage.WidthType)
			{
				case StringStorageWidthType.Ascii:	mBaseEncoding = Encoding.ASCII;
					break;
				case StringStorageWidthType.Unicode:mBaseEncoding = new UnicodeEncoding(big_endian, use_bom, throw_on_invalid);
					mNullCharacterSize = UnicodeEncoding.CharSize; // TODO: should we really do this?
					break;
				case StringStorageWidthType.UTF7:	mBaseEncoding = new UTF7Encoding(!throw_on_invalid);
					break;
				case StringStorageWidthType.UTF8:	mBaseEncoding = new UTF8Encoding(use_bom, throw_on_invalid);
					break;
				case StringStorageWidthType.UTF32:	mBaseEncoding = new UTF32Encoding(big_endian, use_bom, throw_on_invalid);
					mNullCharacterSize = 4 * sizeof(byte);
					break;

				default: throw new Debug.UnreachableException();
			}

			mFixedLengthByteLength = !storage.IsFixedLength
				? 0
				: GetMaxCleanByteCount(storage.FixedLength);

			mStorage = storage;
			mOptions = options;
		}

		public override object Clone()
		{
			return new StringStorageEncoding(mStorage, mOptions);
		}
		#endregion

		#region Implementation
		/// <summary>Calculates the number of bytes produced by encoding a set of characters from the specified character array</summary>
		/// <param name="chars">The character array containing the set of characters to encode</param>
		/// <param name="index">The index of the first character to encode</param>
		/// <param name="count">The number of characters to encode</param>
		/// <returns>The number of bytes produced by encoding the specified characters</returns>
		public override int GetByteCount(char[] chars, int index, int count)
		{
			// In case someone is trying to encode a string outside of the storage's bounds
			ClampCharCount(ref count);

			int byte_count = mBaseEncoding.GetByteCount(chars, index, count);

			byte_count = CalculateByteCount(byte_count); // Add our String Storage calculations

			return byte_count;
		}
		/// <summary>Encodes a set of characters from the specified character array into the specified byte array</summary>
		/// <param name="chars">The character array containing the set of characters to encode</param>
		/// <param name="charIndex">The index of the first character to encode</param>
		/// <param name="charCount">The number of characters to encode</param>
		/// <param name="bytes">The byte array to contain the resulting sequence of bytes</param>
		/// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes</param>
		/// <returns>The actual number of bytes written into bytes</returns>
		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			// In case someone is trying to encode a string outside of the storage's bounds
			ClampCharCount(ref charCount);

			// Add our String Storage calculations
			int bytes_written = EncodeStringStorageTypePrefixData(chars, charIndex, charCount, bytes, byteIndex);

			bytes_written += mBaseEncoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex + bytes_written);

			// Add our String Storage calculations
			bytes_written += EncodeStringStorageTypePostfixData(chars, charIndex, charCount, bytes, bytes_written);

			return bytes_written;
		}
		/// <summary>Calculates the number of characters produced by decoding a sequence of bytes from the specified byte array</summary>
		/// <param name="bytes">The byte array containing the sequence of bytes to decode</param>
		/// <param name="index">The index of the first byte to decode</param>
		/// <param name="count">The number of bytes to decode</param>
		/// <returns>The number of characters produced by decoding the specified sequence of bytes.</returns>
		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			count = CalculateCharByteCount(bytes, ref index, count); // Remove our String Storage calculations

			int char_count = mBaseEncoding.GetCharCount(bytes, index, count);

			return char_count;
		}
		/// <summary>Decodes a sequence of bytes from the specified byte array into the specified character array</summary>
		/// <param name="bytes">The byte array containing the sequence of bytes to decode</param>
		/// <param name="byteIndex">The index of the first byte to decode</param>
		/// <param name="byteCount">The number of bytes to decode</param>
		/// <param name="chars">The character array to contain the resulting set of characters.</param>
		/// <param name="charIndex">The index at which to start writing the resulting set of characters</param>
		/// <returns>The actual number of characters written into chars</returns>
		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			byteCount = CalculateCharByteCount(bytes, ref byteIndex, byteCount); // Remove our String Storage calculations

			int chars_written = mBaseEncoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);

			return chars_written;
		}
		/// <summary>Calculates the maximum number of bytes produced by encoding the specified number of characters</summary>
		/// <param name="charCount">The number of characters to encode</param>
		/// <returns>The maximum number of bytes produced by encoding the specified number of characters</returns>
		public override int GetMaxByteCount(int charCount)
		{
			int max_count = mBaseEncoding.GetMaxByteCount(charCount);

			max_count = CalculateByteCount(max_count); // Add our String Storage calculations

			return max_count;
		}
		/// <summary>
		/// Calculates the maximum number of bytes produced by encoding the specified number of characters WITHOUT
		/// THE EXTRA SURROGATE JIZZ
		/// </summary>
		/// <param name="charCount">The number of characters to encode</param>
		/// <returns></returns>
		public int GetMaxCleanByteCount(int charCount)
		{
			int max_count = mBaseEncoding.GetMaxByteCount(charCount);

			// NOTE: that GetMaxByteCount considers potential leftover surrogates from a previous decoder operation.
			// Because of the decoder, passing a value of 1 to the method retrieves 2 for a single-byte encoding,
			// such as ASCII. Your application should use the IsSingleByte property if this information is necessary.
			// ...That being said, it looks like they internally use a null character. So for streaming related cases,
			// we have to circumcise the fucking count.
			max_count -= mNullCharacterSize;

			return max_count;
		}
		/// <summary>Calculates the maximum number of characters produced by decoding the specified number of bytes</summary>
		/// <param name="byteCount">The number of bytes to decode</param>
		/// <returns>The maximum number of characters produced by decoding the specified number of bytes</returns>
		public override int GetMaxCharCount(int byteCount)
		{
			// For Pascal type strings, this will give a larger count
			// than usual, even for Max standards, since we can't
			// sneak a peak at the length prefix bytes
			byteCount = CalculateCharByteCount(byteCount); // Remove our String Storage calculations

			int max_count = mBaseEncoding.GetMaxCharCount(byteCount);

			return max_count;
		}
		// TODO: override?
		//public override string ToString()		{ return mBaseEncoding.ToString(); }
		#endregion

		#region overrides to baseEncoding
		public override string BodyName			{ get { return mBaseEncoding.BodyName; } }
		public override int CodePage			{ get { return mBaseEncoding.CodePage; } }
		public override string EncodingName		{ get { return mBaseEncoding.EncodingName; } }
		public override string HeaderName		{ get { return mBaseEncoding.HeaderName; } }
		public override bool IsBrowserDisplay	{ get { return mBaseEncoding.IsBrowserDisplay; } }
		public override bool IsBrowserSave		{ get { return mBaseEncoding.IsBrowserSave; } }
		public override bool IsMailNewsDisplay	{ get { return mBaseEncoding.IsMailNewsDisplay; } }
		public override bool IsMailNewsSave		{ get { return mBaseEncoding.IsMailNewsSave; } }
		public override bool IsSingleByte		{ get { return mBaseEncoding.IsSingleByte; } }
		public override string WebName			{ get { return mBaseEncoding.WebName; } }
		public override int WindowsCodePage		{ get { return mBaseEncoding.WindowsCodePage; } }

		/// <summary>Compares this to another object testing for equality</summary>
		/// <param name="obj"></param>
		/// <returns>
		/// True if both this object and <paramref name="obj"/> are equal.
		/// False if <paramref name="obj"/> is not a <see cref="StringStorageEncoding"/></returns>
		public override bool Equals(object value)
		{
			//return mBaseEncoding.Equals(value);
			if(value is StringStorageEncoding)
				return this.Equals(value as StringStorageEncoding);

			return false;
		}
		public override System.Text.Decoder GetDecoder()
		{
			return new Decoder(this);
		}
		public override System.Text.Encoder GetEncoder()
		{
			return new Encoder(this);
		}
		public override int GetHashCode()
		{
			return mBaseEncoding.GetHashCode();
		}
		public override byte[] GetPreamble()
		{
			return mBaseEncoding.GetPreamble();
		}
		public override bool IsAlwaysNormalized(NormalizationForm form)
		{
			return mBaseEncoding.IsAlwaysNormalized(form);
		}
		#endregion

		#region IEquatable<StringStorageEncoding> Members
		/// <summary>
		/// Compares this to another <see cref="StringStorageEncoding"/> object testing
		/// their underlying fields for equality
		/// </summary>
		/// <param name="obj">other <see cref="StringStorageEncoding"/> object</param>
		/// <returns>true if both this object and <paramref name="obj"/> are equal</returns>
		public bool Equals(StringStorageEncoding other)
		{
			return mOptions == other.mOptions &&
				mStorage.Equals(other.mStorage);
		}

		public bool Equals(StringStorageEncoding x, StringStorageEncoding y)
		{
			return x.Equals(y);
		}

		public int GetHashCode(StringStorageEncoding obj)
		{
			return obj.GetHashCode();
		}
		#endregion

		#region IComparer<StringStorageEncoding> Members
		/// <summary>Compare two <see cref="StringStorageEncoding"/> objects for similar underlying values</summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(StringStorageEncoding x, StringStorageEncoding y)
		{
			return x.CompareTo(y);
		}
		/// <summary>Compare this with another <see cref="StringStorageEncoding"/> object for similar underlying values</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(StringStorageEncoding other)
		{
			int cmp = mStorage.CompareTo(other.mStorage);

			if (cmp == 0)
				return ((int)mOptions) - ((int)other.mOptions);

			return cmp;
		}
		#endregion


		#region Static encodings
		internal static readonly StringStorageEncoding[] kStorageEncodingList;
		static StringStorageEncoding()
		{
			kStorageEncodingList = new StringStorageEncoding[StringStorage.kStorageTypesList.Length];
			for (int x = 0; x < kStorageEncodingList.Length; x++)
				kStorageEncodingList[x] = new StringStorageEncoding(StringStorage.kStorageTypesList[x]);
		}
		/// <summary>
		/// Try and get an existing <b>static</b> <see cref="StringStorageEncoding"/> instance
		/// based on a provided definition
		/// </summary>
		/// <param name="storageDesc">Storage to base the result on</param>
		/// <returns>
		/// If an instance is found with <paramref name="storageDesc"/>, a static based
		/// object will be returned. Otherwise, a new <see cref="StringStorageEncoding"/>
		/// object will be created using the definition.
		/// </returns>
		public static StringStorageEncoding TryAndGetStaticEncoding(StringStorage storageDesc)
		{
			Contract.Ensures(Contract.Result<StringStorageEncoding>() != null);

			StringStorageEncoding sse = Array.Find(kStorageEncodingList,
				x => x.mStorage.Equals(storageDesc));

			return sse ?? new StringStorageEncoding(storageDesc);
		}
		#endregion
	};
}