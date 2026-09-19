using System;

namespace KSoft.Text
{
	partial class StringStorageEncoding
	{
		/// <summary>Additional options for constructing the <see cref="StringStorageEncoding"/></summary>
		[Flags]
		public enum Options : byte
		{
//			/// <summary>No additional encoding options</summary>
//			/// <remarks>Options will thus default to "false"</remarks>
//			None = 0,

			/// <summary>Expose the base encoding's byte-order preamble.</summary>
			/// <seealso cref="System.Text.Encoding.GetPreamble()"/>
			/// <remarks>String-storage conversions do not insert the preamble into records.</remarks>
			UseByteOrderMark = 1 << 0,
			/// <summary>Use exception fallbacks for invalid Unicode input instead of replacement fallbacks.</summary>
			/// <remarks>Ignored for <see cref="Memory.Strings.StringStorageWidthType.Ascii"/>, which retains replacement fallback.</remarks>
			ThrowOnInvalidBytes = 1 << 1,
			/// <summary>Honor the caller's flush argument instead of always flushing the underlying encoder/decoder.</summary>
			/// <seealso cref="StringStorageEncoding.GetEncoder()"/>
			/// <seealso cref="StringStorageEncoding.GetDecoder()"/>
			DontAlwaysFlush = 1 << 2,

			/// <summary>All options are enabled</summary>
			kAll = UseByteOrderMark | ThrowOnInvalidBytes | DontAlwaysFlush
		};
	};
}