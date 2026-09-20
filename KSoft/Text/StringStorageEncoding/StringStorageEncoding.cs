using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace KSoft.Text
{
	using Memory.Strings;

	/// <summary>Encodes and decodes binary string records described by <see cref="StringStorage"/>.</summary>
	/// <remarks>
	/// <para>Use KSoft stream string overloads accepting an explicit storage descriptor or encoding; ordinary BinaryReader/BinaryWriter string methods use CLR byte-counted framing.</para>
	/// <para>Prefixes and fixed fields count serialized storage units. A UTF-32 scalar can require two managed input characters.</para>
	/// <para>CString inputs must not contain embedded nulls; writers do not escape or reject them.</para>
	/// </remarks>
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	public sealed partial class StringStorageEncoding
		: System.Text.Encoding
		, IEquatable<StringStorageEncoding>, IEqualityComparer<StringStorageEncoding>
		, IComparer<StringStorageEncoding>, IComparable<StringStorageEncoding>
	{
		readonly Encoding mBaseEncoding;
		#region Storage
		readonly StringStorage mStorage;
		/// <summary>The string storage definition for this encoding</summary>
		public StringStorage Storage { get => mStorage; }
		#endregion
		bool HasCountedPayload => mStorage.HasLengthPrefix || mStorage.IsFixedLength;
		readonly Options mOptions;
		bool DontAlwaysFlush { get => (mOptions & Options.DontAlwaysFlush) != 0; }
		/// <summary>Number of bytes a null character consumes</summary>
		readonly int mNullCharacterSize;
		/// <summary>
		/// Number of bytes used to store a fixed length character array using
		/// the <see cref="StringStorageType"/> defined by <see cref="Storage"/>
		/// </summary>
		readonly int mFixedLengthByteLength;

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
					mNullCharacterSize = UnicodeEncoding.CharSize; // #REVIEW: should we really do this?
					break;
//				case StringStorageWidthType.UTF7:	mBaseEncoding = new UTF7Encoding(!throw_on_invalid);
//					break;
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
			Verify.Buffers.OffsetAndLengthWithinLength(chars, index, count);
			// In case someone is trying to encode a string outside of the storage's bounds
			count = ClampCharCount(chars.AsSpan(index, count));

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
		/// <remarks>Includes framing and truncates to fixed-field capacity, but does not initialize unused caller-owned padding. Stream writes serialize and pad the complete field instead.</remarks>
		/// <seealso cref="IO.EndianWriter.Write(ReadOnlySpan{char}, StringStorage)"/>
		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			Verify.Buffers.OffsetAndLengthWithinLength(chars, charIndex, charCount);
			Verify.Buffers.StartIndexWithinLength(bytes, byteIndex);
			// In case someone is trying to encode a string outside of the storage's bounds
			charCount = ClampCharCount(chars.AsSpan(charIndex, charCount));
			int serializedCount = 0;
			if (HasCountedPayload)
			{
				int byteCount = mBaseEncoding.GetByteCount(chars, charIndex, charCount);
				ValidateEncodedPayload(byteCount);
				serializedCount = GetSerializedCharacterCount(byteCount);
			}

			// Add our String Storage calculations
			int bytes_written = EncodeStringStorageTypePrefixData(serializedCount, bytes.AsSpan(byteIndex));

			bytes_written += mBaseEncoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex + bytes_written);

			// Add our String Storage calculations
			bytes_written += EncodeStringStorageTypePostfixData(
				bytes.AsSpan(byteIndex + bytes_written));

			return bytes_written;
		}
		/// <summary>Calculates the number of characters produced by decoding a sequence of bytes from the specified byte array</summary>
		/// <param name="bytes">The byte array containing the sequence of bytes to decode</param>
		/// <param name="index">The index of the first byte to decode</param>
		/// <param name="count">The number of bytes to decode</param>
		/// <returns>The number of characters produced by decoding the specified sequence of bytes.</returns>
		/// <inheritdoc cref="GetChars(byte[], int, int, char[], int)" path="/remarks"/>
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
		/// <remarks>Decodes the supplied record extent, not an unbounded stream. CString decoding validates and removes the final encoded null; it does not scan for the first null or trim fixed-field padding.</remarks>
		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			byteCount = CalculateCharByteCount(bytes, ref byteIndex, byteCount); // Remove our String Storage calculations

			int chars_written = mBaseEncoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);

			return chars_written;
		}
		/// <summary>Calculates the maximum number of bytes produced by encoding the specified number of characters</summary>
		/// <param name="charCount">The number of characters to encode</param>
		/// <returns>The maximum number of bytes produced by encoding the specified number of characters</returns>
		/// <remarks>A capacity estimate, not validation of a particular string or its Pascal prefix range. Use <see cref="GetByteCount(char[], int, int)"/> for actual input.</remarks>
		public override int GetMaxByteCount(int charCount)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(charCount);
			if (mStorage.IsFixedLength)
			{
				return mFixedLengthByteLength;
			}
			int max_count = mBaseEncoding.GetMaxByteCount(charCount);

			max_count = CalculateByteCount(max_count, validateLength: false); // Add our String Storage calculations

			return max_count;
		}
		/// <summary>Returns the base encoding's maximum byte count minus one encoded null unit.</summary>
		/// <param name="charCount">Storage-unit count for fixed-width encodings; input character count for a variable-width estimate.</param>
		/// <returns>Payload byte count without string framing.</returns>
		/// <remarks>The result is exact for fixed-width storage, but is only an upper bound for variable-width encodings.</remarks>
		public int GetMaxCleanByteCount(int charCount)
		{
			int max_count = mBaseEncoding.GetMaxByteCount(charCount);

			max_count -= mNullCharacterSize;

			return max_count;
		}
		/// <summary>Calculates the maximum number of characters produced by decoding the specified number of bytes</summary>
		/// <param name="byteCount">The number of bytes to decode</param>
		/// <returns>The maximum number of characters produced by decoding the specified number of bytes</returns>
		/// <remarks>Does not inspect or validate a record's contents. Use <see cref="GetCharCount(byte[], int, int)"/> for actual input.</remarks>
		public override int GetMaxCharCount(int byteCount)
		{
			// For Pascal type strings, this will give a larger count
			// than usual, even for Max standards, since we can't
			// sneak a peak at the length prefix bytes
			byteCount = CalculateCharByteCount(byteCount); // Remove our String Storage calculations

			int max_count = mBaseEncoding.GetMaxCharCount(byteCount);

			return max_count;
		}
		// #REVIEW: override?
		//public override string ToString()		{ return mBaseEncoding.ToString(); }
		#endregion

		#region overrides to baseEncoding
		public override string BodyName			=> mBaseEncoding.BodyName;
		public override int CodePage			=> mBaseEncoding.CodePage;
		public override string EncodingName		=> mBaseEncoding.EncodingName;
		public override string HeaderName		=> mBaseEncoding.HeaderName;
		public override bool IsBrowserDisplay	=> mBaseEncoding.IsBrowserDisplay;
		public override bool IsBrowserSave		=> mBaseEncoding.IsBrowserSave;
		public override bool IsMailNewsDisplay	=> mBaseEncoding.IsMailNewsDisplay;
		public override bool IsMailNewsSave		=> mBaseEncoding.IsMailNewsSave;
		public override bool IsSingleByte		=> mBaseEncoding.IsSingleByte;
		public override string WebName			=> mBaseEncoding.WebName;
		public override int WindowsCodePage		=> mBaseEncoding.WindowsCodePage;

		/// <summary>Compares this to another object testing for equality</summary>
		/// <param name="value">Object to compare</param>
		/// <returns>
		/// True if both this object and <paramref name="value"/> are equal.
		/// False if <paramref name="value"/> is not a <see cref="StringStorageEncoding"/></returns>
		public override bool Equals(object? value)
		{
			//return mBaseEncoding.Equals(value);
			if (value is StringStorageEncoding e)
			{
				return this.Equals(e);
			}

			return false;
		}
		/// <summary>Returns a decoder for complete string-storage records.</summary>
		/// <inheritdoc cref="GetEncoder()" path="/remarks"/>
		/// <seealso cref="GetChars(byte[], int, int, char[], int)"/>
		public override System.Text.Decoder GetDecoder() => new Decoder(this);
		/// <summary>Returns an encoder for complete string-storage records.</summary>
		/// <remarks>Framing is processed on every conversion call; this is not incremental framing across arbitrary record fragments. <see cref="Options.DontAlwaysFlush"/> controls only the underlying character-conversion state.</remarks>
		/// <seealso cref="GetBytes(char[], int, int, byte[], int)"/>
		public override System.Text.Encoder GetEncoder() => new Encoder(this);
		public override int GetHashCode() => mBaseEncoding.GetHashCode();
		public override byte[] GetPreamble() => mBaseEncoding.GetPreamble();
		public override bool IsAlwaysNormalized(NormalizationForm form) => mBaseEncoding.IsAlwaysNormalized(form);
		#endregion

		#region IEquatable<StringStorageEncoding> Members
		/// <summary>
		/// Compares this to another <see cref="StringStorageEncoding"/> object testing
		/// their underlying fields for equality
		/// </summary>
		/// <param name="other">Other <see cref="StringStorageEncoding"/> object</param>
		/// <returns>true if both this object and <paramref name="other"/> are equal</returns>
		public bool Equals(StringStorageEncoding? other)
		{
			return mOptions == other!.mOptions &&
				mStorage.Equals(other.mStorage);
		}

		public bool Equals(StringStorageEncoding? x, StringStorageEncoding? y) => x!.Equals(y);

		public int GetHashCode(StringStorageEncoding obj) => obj.GetHashCode();
		#endregion

		#region IComparer<StringStorageEncoding> Members
		/// <summary>Compare two <see cref="StringStorageEncoding"/> objects for similar underlying values</summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(StringStorageEncoding? x, StringStorageEncoding? y) => x!.CompareTo(y);
		/// <summary>Compare this with another <see cref="StringStorageEncoding"/> object for similar underlying values</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(StringStorageEncoding? other)
		{
			int cmp = mStorage.CompareTo(other!.mStorage);

			if (cmp == 0)
			{
				return ((int)mOptions) - ((int)other.mOptions);
			}

			return cmp;
		}
		#endregion


		#region Static encodings
		internal static readonly StringStorageEncoding[] kStorageEncodingList = EncodingArrayFromStorageArray(StringStorage.kStorageTypesList);
		static StringStorageEncoding[] EncodingArrayFromStorageArray(StringStorage[] storageArray)
		{
			var encodings = new StringStorageEncoding[storageArray.Length];
			for (int x = 0; x < encodings.Length; x++)
			{
				encodings[x] = new StringStorageEncoding(storageArray[x]);
			}

			return encodings;
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
			StringStorageEncoding? sse = Array.Find(kStorageEncodingList,
				x => x.mStorage.Equals(storageDesc));

			return sse ?? new StringStorageEncoding(storageDesc);
		}
		#endregion
	};
}
